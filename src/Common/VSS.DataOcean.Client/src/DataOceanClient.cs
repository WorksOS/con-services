using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using VSS.DataOcean.Client.Models;
using VSS.DataOcean.Client.ResultHandling;
using VSS.ConfigurationStore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.DataOcean.Client
{
  /// <summary>
  /// This is a client which is used to send requests to the data ocean. It uses GracefulWebRequest which uses HttpClient.
  /// It is a replacement for TCC file access so the public interface mimics that.
  /// </summary>
  public class DataOceanClient : IDataOceanClient
  {
    private const string DATA_OCEAN_URL_KEY = "DATA_OCEAN_URL";
    private const string DATA_OCEAN_UPLOAD_TIMEOUT_KEY = "DATA_OCEAN_UPLOAD_TIMEOUT_MINS";
    private const string DATA_OCEAN_UPLOAD_WAIT_KEY = "DATA_OCEAN_UPLOAD_WAIT_MILLSECS";

    private readonly ILogger<DataOceanClient> Log;
    private readonly IWebRequest gracefulClient;
    private readonly string dataOceanBaseUrl;
    private readonly int uploadWaitInterval;
    private readonly int uploadTimeout; 
    /// <summary>
    /// Client for sending requests to the data ocean.
    /// </summary>
    public DataOceanClient(IConfigurationStore configuration, ILoggerFactory logger, IWebRequest gracefulClient)
    {
      Log = logger.CreateLogger<DataOceanClient>();
      this.gracefulClient = gracefulClient;

      dataOceanBaseUrl = configuration.GetValueString(DATA_OCEAN_URL_KEY);
      if (string.IsNullOrEmpty(dataOceanBaseUrl))
      {
        throw new ArgumentException($"Missing environment variable {DATA_OCEAN_URL_KEY}");
      }
      uploadWaitInterval = configuration.GetValueInt(DATA_OCEAN_UPLOAD_WAIT_KEY, 1000);//Millisecs
      uploadTimeout = configuration.GetValueInt(DATA_OCEAN_UPLOAD_TIMEOUT_KEY, 5);//minutes
      Log.LogInformation($"{DATA_OCEAN_URL_KEY}={dataOceanBaseUrl}");
    }

    #region DataOcean public
    /// <summary>
    /// Determines if the folder path exists.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    public async Task<bool> FolderExists(string path, IDictionary<string, string> customHeaders)
    {
      Log.LogDebug($"FolderExists: {path}");

      var folder = await GetFolderMetadata(path, true, customHeaders);
      return folder != null;
    }

    /// <summary>
    /// Determines if the file exists.
    /// </summary>
    /// <param name="filename">The full path and file name</param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    public async Task<bool> FileExists(string filename, IDictionary<string, string> customHeaders)
    {
      Log.LogDebug($"FileExists: {filename}");

      var result = await GetFileMetadata(filename, customHeaders);
      return result != null;
    }

    /// <summary>
    /// Makes the folder structure. Can partially exist already i.e. parent folder. 
    /// </summary>
    /// <param name="path">The folder structure to create</param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    public async Task<bool> MakeFolder(string path, IDictionary<string, string> customHeaders)
    {
      Log.LogDebug($"MakeFolder: {path}");

      var folder = await GetFolderMetadata(path, false, customHeaders);
      return folder != null;
    }

    /// <summary>
    /// Saves the file.
    /// </summary>
    /// <param name="path">Where to save it</param>
    /// <param name="filename">The name of the file</param>
    /// <param name="contents">The contents of the file</param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    public async Task<bool> PutFile(string path, string filename, Stream contents, IDictionary<string, string> customHeaders=null)
    {
      Log.LogDebug($"PutFile: {Path.Combine(path,filename)}");

      var success = false;
      var parentFolder = await GetFolderMetadata(path, true, customHeaders);
      //1. Create the file
      var createResult = await CreateFile(filename, parentFolder?.Id, customHeaders);
      var newFile = createResult?.File;
      if (newFile != null)
      {
        //2. Upload the file
        HttpContent result = await gracefulClient.ExecuteRequestAsStreamContent(newFile.DataOceanUpload.Url, HttpMethod.Put, customHeaders, contents, null, 3, false);

        //3. Monitor status of upload until done
        var route = $"/api/files/{newFile.Id}";
        DateTime endJob = DateTime.Now + TimeSpan.FromMinutes(uploadTimeout);
        bool done = false;
        while (!done && DateTime.Now <= endJob)
        {
          if (uploadWaitInterval > 0) await Task.Delay(uploadWaitInterval);
          //TODO: This could be a scheduler job, polled for by the caller, if the upload is too slow.
          var getResult = await GetData<DataOceanFile>(route, null, customHeaders);
          var status = getResult.Status.ToUpper();
          success = string.Compare(status, "AVAILABLE", true) == 0;
          done = success || status == "UPLOAD_FAILED";
        }

        if (!done)
        {
          Log.LogDebug($"PutFile timed out: {path}/{filename}");
        }
        else if (!success)
        {
          Log.LogDebug($"PutFile failed: {path}/{filename}");
        }
      
      }
      
      Log.LogDebug($"PutFile: success={success}");
      return success;
    }

    /// <summary>
    /// Deletes the file.
    /// </summary>
    /// <param name="fullName"></param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    public async Task<bool> DeleteFile(string fullName, IDictionary<string, string> customHeaders)
    {
      Log.LogDebug($"DeleteFile: {fullName}");

      var result = await GetFileMetadata(fullName, customHeaders);
      if (result != null)
      {
        var route = $"/api/files/{result.Id}";
        await gracefulClient.ExecuteRequest($"{dataOceanBaseUrl}{route}", null, customHeaders, HttpMethod.Delete, null, 3, false);
      }

      return true;
    }

    /// <summary>
    /// Gets the id of the lowest level folder metadata in the path 
    /// </summary>
    /// <param name="path"></param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    public async Task<Guid?> GetFolderId(string path, IDictionary<string, string> customHeaders)
    {
      Log.LogDebug($"GetFolderId: {path}");

      var folder = await GetFolderMetadata(path, true, customHeaders);
      return folder?.Id;
    }

    /// <summary>
    /// Gets the file id
    /// </summary>
    /// <param name="fullName"></param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    public async Task<Guid?> GetFileId(string fullName, IDictionary<string, string> customHeaders)
    {
      Log.LogDebug($"GetFileId: {fullName}");

      var result = await GetFileMetadata(fullName, customHeaders);
      return result?.Id;
    }

    /// <summary>
    /// Gets the file contents
    /// </summary>
    /// <param name="fullName"></param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    public async Task<Stream> GetFile(string fullName, IDictionary<string, string> customHeaders)
    {
      Log.LogDebug($"GetFile: {fullName}");

      //1. Get the download url
      string tileFolderAndFileName = null;
      string nameForMetadata = fullName;
      if (fullName.Contains(DataOceanFileUtil.GENERATED_TILE_FOLDER_SUFFIX))
      {
        tileFolderAndFileName = DataOceanFileUtil.ExtractTileNameFromTileFullName(fullName);
        nameForMetadata = fullName.Substring(0, fullName.Length - tileFolderAndFileName.Length);
      }

      var result = await GetFileMetadata(nameForMetadata, customHeaders);
      if (result == null)
      {
        Log.LogWarning($"Failed to find file {fullName}");
        return null;
      }
      var downloadUrl = result.DataOceanDownload.Url;
      //PNG tiles files and tiles.json metadata file are in a DataOcean multifile
      if (result.Multifile)
      {
        if (string.IsNullOrEmpty(tileFolderAndFileName))
        {
          Log.LogError("Getting a multifile other than tiles is not implemented");
          return null;
        }
        tileFolderAndFileName = tileFolderAndFileName.Substring(1);//Skip leading / as it's in the URL already
        downloadUrl = downloadUrl.Replace("{path}", tileFolderAndFileName);
      }
      //2. Download the file
      HttpContent response = null;
      try
      {
        response = await gracefulClient.ExecuteRequestAsStreamContent(downloadUrl, HttpMethod.Get, customHeaders, 
          null, null, 3, false);
      }
      catch (HttpRequestException ex)
      {
        //If tile does not exist DataOcean returns 403
        if (result.Multifile && string.Compare(ex.Message, "403 access denied", true) != 0)
        {
          throw;
        }
      }
      //Check if anything returned. File may not exist.
      if (response == null)
        return null;
      using (var responseStream = await response.ReadAsStreamAsync())
      {
        responseStream.Position = 0;
        byte[] file = new byte[responseStream.Length];
        responseStream.Read(file, 0, file.Length);
        return new MemoryStream(file);
      }
    }
    #endregion

    #region DataOcean private

    /// <summary>
    /// Gets the file metadata.
    /// </summary>
    /// <param name="fullName"></param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    private async Task<DataOceanFile> GetFileMetadata(string fullName, IDictionary<string, string> customHeaders)
    {
      Log.LogDebug($"GetFile: {fullName}");

      var path = Path.GetDirectoryName(fullName);
      var parentFolder = await GetFolderMetadata(path, true, customHeaders);

      var filename = Path.GetFileName(fullName);
      var result = await BrowseFile(filename, parentFolder?.Id, customHeaders);
      if (result?.Files?.Count == 1)
      {
        return result.Files[0];
      }

      return null;
    }

    /// <summary>
    /// Gets the lowest level folder metadata in the path. Creates it unless it this is purely a query and therefore must exist.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="mustExist"></param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    private async Task<DataOceanDirectory> GetFolderMetadata(string path, bool mustExist, IDictionary<string, string> customHeaders)
    {
      Log.LogDebug($"GetFolderMetadata: {path}");

      var parts = path.Split(Path.DirectorySeparatorChar);
      DataOceanDirectory folder = null;
      for (var i = 0; i < parts.Length; i++)
      {
        if (!string.IsNullOrEmpty(parts[i]))
        {
          var result = await BrowseFolder(parts[i], folder?.ParentId, customHeaders);
          var count = result?.Directories?.Count;
          if (count == 1)
          {
            folder = result.Directories[0];
          }
          else if (count == 0)
          {
            if (mustExist)
            {
              return null;
            }
            else
            {
              folder = (await CreateDirectory(parts[i], folder?.ParentId, customHeaders)).Directory;
            }
          }
          else //count > 1
          {
            //Should we throw an exception here?
            Log.LogWarning($"Duplicate folders {parts[i]} in path {path}");
            return null;
          }
        }
      }
      //Folders in path already exist or have been created successfully
      return folder;
    }


    /// <summary>
    /// Gets the requested folder metadata at the specified level i.e. with the requested parent.
    /// </summary>
    /// <param name="folderName">Folder name</param>
    /// <param name="parentId">DataOcean ID of the parent folder</param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    private Task<BrowseDirectoriesResult> BrowseFolder(string folderName, Guid? parentId, IDictionary<string, string> customHeaders)
    {
      return BrowseItem<BrowseDirectoriesResult>(folderName, parentId, true, customHeaders);
    }

    /// <summary>
    /// Gets the requested file metadata at the specified level i.e. with the requested parent.
    /// </summary>
    /// <param name="fileName">File name</param>
    /// <param name="parentId">>DataOcean ID of the parent folder</param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    private Task<BrowseFilesResult> BrowseFile(string fileName, Guid? parentId, IDictionary<string, string> customHeaders)
    {
      return BrowseItem<BrowseFilesResult>(fileName, parentId, false, customHeaders);
    }

    /// <summary>
    /// Gets the requested folder or file metadata at the specified level i.e. with the requested parent.
    /// </summary>
    /// <typeparam name="T">The type of item, folder or file</typeparam>
    /// <param name="name">Folder or file name</param>
    /// <param name="parentId">DataOcean ID of the parent folder</param>
    /// <param name="isFolder">True if gettig a folder otherwise false</param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    private Task<T> BrowseItem<T>(string name, Guid? parentId, bool isFolder, IDictionary<string, string> customHeaders)
    {
      Log.LogDebug($"BrowseItem: name={name}, parentId={parentId}");

      IDictionary<string, string> queryParameters = new Dictionary<string, string>();
      queryParameters.Add("name", name);
      queryParameters.Add("owner", "true");
      if (parentId.HasValue)
      {
        queryParameters.Add("parent_id", parentId.Value.ToString());
      }

      var suffix = isFolder ? "directories" : "files";
      return GetData<T>($"/api/browse/{suffix}", queryParameters, customHeaders);
    }

    /// <summary>
    /// Creates a DataOcean directory.
    /// </summary>
    /// <param name="name">The directory name</param>
    /// <param name="parentId">DataOcean ID of the parent directory</param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    private Task<CreateDirectoryResult> CreateDirectory(string name, Guid? parentId, IDictionary<string, string> customHeaders)
    {
      var message = new CreateDirectoryMessage
      {
        Directory = new DataOceanDirectory
        {
          Name = name,
          ParentId = parentId
        }
      };

      return CreateItem<CreateDirectoryMessage, CreateDirectoryResult>(message, "/api/directories", customHeaders);
    }

    /// <summary>
    /// Creates a DataOcean file.
    /// </summary>
    /// <param name="name">The file name</param>
    /// <param name="parentId">DataOcean ID of the parent directory</param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    private Task<CreateFileResult> CreateFile(string name, Guid? parentId, IDictionary<string, string> customHeaders)
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
      return CreateItem<CreateFileMessage, CreateFileResult>(message, "/api/files", customHeaders);
    }

    /// <summary>
    /// Creates a DataOcean directory or file.
    /// </summary>
    /// <typeparam name="T">The type of data sent</typeparam>
    /// <typeparam name="U">The type of data returned</typeparam>
    /// <param name="message">The message payload</param>
    /// <param name="route">The route for the request</param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    private async Task<U> CreateItem<T,U>(T message, string route, IDictionary<string, string> customHeaders)
    {
      var payload = JsonConvert.SerializeObject(message);
      Log.LogDebug($"CreateItem: route={route}, message={payload}");

      using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(payload)))
      {
        var result = await gracefulClient.ExecuteRequest<U>($"{dataOceanBaseUrl}{route}", ms, customHeaders, HttpMethod.Post, null, 3, false);
        Log.LogDebug($"CreateItem: result={JsonConvert.SerializeObject(result)}");
        return result;
      }
    }

   /// <summary>
   /// Gets a DataOcean item.
   /// </summary>
   /// <typeparam name="T">The type of item to get</typeparam>
   /// <param name="route">The route for the request</param>
   /// <param name="queryParameters">Query parameters for the request</param>
   /// <param name="customHeaders"></param>
   /// <returns></returns>
    private async Task<T> GetData<T>(string route, IDictionary<string, string> queryParameters, IDictionary<string, string> customHeaders)
    {
      Log.LogDebug($"GetData: route={route}, queryParameters={JsonConvert.SerializeObject(queryParameters)}");

      var query = $"{dataOceanBaseUrl}{route}";
      if (queryParameters != null)
      {
        query = QueryHelpers.AddQueryString(query, queryParameters);
      }
      var result = await gracefulClient.ExecuteRequest<T>(query, null, customHeaders, HttpMethod.Get, null, 3, false);
      Log.LogDebug($"GetData: result={(result == null ? "null" : JsonConvert.SerializeObject(result))}");
      return result;
    }

    #endregion




  }
}
