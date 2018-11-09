using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.DataOcean.Client.Models;
using VSS.DataOcean.Client.ResultHandling;
using VSS.TRex.HttpClients.Constants;

namespace VSS.DataOcean.Client
{
  /// <summary>
  /// This is a named HttpClient which is used to send requests to the data ocean.
  /// </summary>
  public class DataOceanClient
  {

    private ILogger<DataOceanClient> _logger;
    private HttpClient _client;

    /// <summary>
    /// Typed HttpClient for sending requests to the data ocean.
    /// </summary>
    /// <param name="client">Inner http client</param>
    public DataOceanClient(HttpClient client, ILogger<DataOceanClient> logger)
    {
      _client = client;
      _logger = logger;
    }

    #region DataOcean public
    public async Task<bool> FolderExists(string folder)
    {
      _logger.LogDebug($"FolderExists: {folder}");

      var parentId = await GetParentId(folder);
      return parentId.HasValue;
    }

    public async Task<bool> FileExists(string filename)
    {
      _logger.LogDebug($"FileExists: {filename}");

      var path = Path.GetDirectoryName(filename);
      var parentId = await GetParentId(path);
      if (parentId.HasValue)
      {
        filename = Path.GetFileName(filename);
        var result = await BrowseFile(filename, parentId.Value);
        return result?.Files?.Count == 1;
      }
   
      return false;
    }

    public async Task<bool> MakeFolder(string path)
    {
      _logger.LogDebug($"MakeFolder: {path}");

      var parts = path.Split(Path.DirectorySeparatorChar);
      Guid? parentId = null;
      for (var i = 0; i < parts.Length; i++)
      {
        if (!string.IsNullOrEmpty(parts[i]))
        {
          var getResult = await BrowseFolder(parts[i], parentId);
          var count = getResult?.Directories?.Count;
          if (count == 1)
          {
            parentId = getResult.Directories[0].ParentId;
          }
          else if (count == 0)
          {
            var createResult = await CreateDirectory(parts[i], parentId);
            return createResult.HasValue;
          }
          else
          {
            //Should we throw an exception here?
           _logger.LogWarning($"Duplicate folders {parts[i]} in path {path}");
            return false;
          }
        }
      }
      //Folders in path already exist
      return parentId.HasValue;
    }

    public async Task<bool> PutFile(string path, string filename, Stream contents)
    {
      _logger.LogDebug($"PutFile: {path}/{filename}");

      bool success = false;
      var parentId = await GetParentId(path);
      if (parentId.HasValue)
      {
        var createResult = await CreateFile(filename, parentId.Value);
        if (createResult != null)
        {
          var uploadResult = await _client.PutAsync(createResult.DataOceanUpload.Url, new StreamContent(contents));
          _logger.LogDebug($"Upload {filename} returned {uploadResult.StatusCode}");
          if (uploadResult.IsSuccessStatusCode)
          {
            var route = $"/api/files/{createResult.Id}";
            DateTime now = DateTime.Now;
            DateTime endJob = now + TimeSpan.FromMinutes(5);//TODO: make configurable
            int waitInterval = 1000;//TODO: make configurable
            bool done = false;
            while (!done && now <= endJob)
            {
              if (waitInterval > 0) await Task.Delay(waitInterval);
              //TODO: This could be a scheduler job, polled for by the caller, if the upload is too slow.
              var getResult = await GetData<DataOceanFile>(route, null);
              var status = getResult.Status.ToUpper();
              success = status == "AVAILABLE";
              done = success || status == "UPLOAD_FAILED";
            }

            if (!done)
            {
              _logger.LogDebug($"Timeout for PutFile: {path}/{filename}");
            }
            else if (!success)
            {
              _logger.LogDebug($"PutFile failed: {path}/{filename}");
            }
          }

        }
      }
      _logger.LogDebug($"PutFile: success={success}");

      return success;
    }
    #endregion

    #region DataOcean private
    private async Task<Guid?> GetParentId(string folder)
    {
      _logger.LogDebug($"GetParentId: folder={folder}");

      var parts = folder.Split(Path.DirectorySeparatorChar);
      Guid? parentId = null;
      for (var i = 0; i < parts.Length; i++)
      {
        if (!string.IsNullOrEmpty(parts[i]))
        {
          var result = await BrowseFolder(parts[i], parentId);
          if (result?.Directories?.Count != 1)
            return null;
          parentId = result.Directories[0].ParentId;
        }
      }
      return parentId;
    }

    private async Task<BrowseDirectoriesResult> BrowseFolder(string folderName, Guid? parentId = null)
    {
      return await BrowseItem<BrowseDirectoriesResult>(folderName, parentId);
    }

    private async Task<BrowseFilesResult> BrowseFile(string fileName, Guid? parentId = null)
    {
      return await BrowseItem<BrowseFilesResult>(fileName, parentId);
    }

    private async Task<T> BrowseItem<T>(string name, Guid? parentId = null)
    {
      _logger.LogDebug($"BrowseItem: name={name}, parentId={parentId}");

      IDictionary<string, string> queryParameters = new Dictionary<string, string>();
      queryParameters.Add("name", name);
      queryParameters.Add("owner", "true");
      if (parentId.HasValue)
      {
        queryParameters.Add("parent_id", parentId.Value.ToString());
      }

      return await GetData<T>("/api/browse", queryParameters);
    }



    private async Task<Guid?> CreateDirectory(string name, Guid? parentId)
    {
      var message = new CreateDirectoryMessage
      {
        Directory = new DataOceanDirectory
        {
          Name = name,
          ParentId = parentId
        }
      };

      var result = await CreateItem<CreateDirectoryMessage, DataOceanDirectory>(message, "/api/directories");
      return result?.Id;
    }

    private async Task<DataOceanFile> CreateFile(string name, Guid? parentId)
    {
      var message = new CreateFileMessage
      {
        File = new DataOceanFile
        {
          Name = name,
          ParentId = parentId,
          Multifile = false,
          RegionPreferences = new List<string> { "us1"}
        }
      };
      var result = await CreateItem<CreateFileMessage, DataOceanFile>(message, "/api/files");
      return result;
    }

    private async Task<U> CreateItem<T,U>(T message, string route)
    {
      _logger.LogDebug($"CreateItem: route={route}, message={JsonConvert.SerializeObject(message)}");

      var response = await PostMessage(message, route);

      if (response.IsSuccessStatusCode)
      {
        var contentString = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<U>(contentString);
        _logger.LogDebug($"CreateItem: result={JsonConvert.SerializeObject(result)}");

        return result;
      }
 
      _logger.LogInformation($"Failed to create item in data ocean: HttpStatusCode={response.StatusCode}");
      return default(U);
    }


    /// <summary>
    /// Get data from data ocean
    /// </summary>
    /// <returns></returns>
    private async Task<T> GetData<T>(string route, IDictionary<string, string> queryParameters)
    {
      _logger.LogDebug($"GetData: roue={route}, queryParameters={JsonConvert.SerializeObject(queryParameters)}");

      try
      {
        var query = QueryHelpers.AddQueryString($"{_client.BaseAddress}{route}", queryParameters);
        var responseString = await _client.GetStringAsync(query);
        var result = JsonConvert.DeserializeObject<T>(responseString);
        _logger.LogDebug($"GetData: result={JsonConvert.SerializeObject(result)}");

        return result;
      }
      catch (HttpRequestException ex)
      {
        _logger.LogError($"The following error occured connecting to data ocean API {ex}");
        throw;
      }
    }
    #endregion

    #region Http Client
    /// <summary>
    /// Post a message to data ocean
    /// </summary>
    /// <param name="message">Message to send</param>
    /// <returns></returns>
    public async Task<HttpResponseMessage> PostMessage<T>(T message, string route)
    {
      _logger.LogDebug($"PostMessage: route={route}, message={JsonConvert.SerializeObject(message)}");

      try
      {
        StringContent requestContent = new StringContent(JsonConvert.SerializeObject(message));
        requestContent.Headers.ContentType.MediaType = MediaTypes.JSON;
        return await _client.PostAsync($"{_client.BaseAddress}{route}", requestContent);
      }
      catch (HttpRequestException ex)
      {
        _logger.LogError($"The following error occured connecting to data ocean API {ex}");
        throw;
      }
    }

    /// <summary>
    /// Get message from data ocean
    /// </summary>
    /// <returns></returns>
    public async Task<HttpResponseMessage> GetMessage(string route, IDictionary<string,string> queryParameters)
    {
      _logger.LogDebug($"GetMessage: route={route}, queryParameters={JsonConvert.SerializeObject(queryParameters)}");

      try
      {
        var query = QueryHelpers.AddQueryString($"{_client.BaseAddress}{route}", queryParameters);
        return await _client.GetAsync(query);
      }
      catch (HttpRequestException ex)
      {
        _logger.LogError($"The following error occured connecting to data ocean API {ex}");
        throw;
      }
    }


    #endregion


  }
}
