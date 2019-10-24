﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.DataOcean.Client.Models;
using VSS.DataOcean.Client.ResultHandling;
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.DataOcean.Client
{
  /// <summary>
  /// This is a client which is used to send requests to the data ocean. It uses GracefulWebRequest which uses HttpClient.
  /// It is a replacement for TCC file access so the public interface mimics that.
  /// </summary>
  public class DataOceanClient : IDataOceanClient
  {
    private readonly ILogger<DataOceanClient> Log;
    private readonly IWebRequest _gracefulClient;
    private readonly string _dataOceanBaseUrl;
    private readonly int _uploadWaitInterval;
    private readonly double _uploadTimeout;

    /// <summary>
    /// Client for sending requests to the data ocean.
    /// </summary>
    public DataOceanClient(IConfigurationStore configuration, ILoggerFactory logger, IWebRequest gracefulClient)
    {
      Log = logger.CreateLogger<DataOceanClient>();
      _gracefulClient = gracefulClient;

      const string DATA_OCEAN_URL_KEY = "DATA_OCEAN_URL";
      _dataOceanBaseUrl = configuration.GetValueString(DATA_OCEAN_URL_KEY);

      if (string.IsNullOrEmpty(_dataOceanBaseUrl))
      {
        throw new ArgumentException($"Missing environment variable {DATA_OCEAN_URL_KEY}");
      }

      _uploadWaitInterval = configuration.GetValueInt("DATA_OCEAN_UPLOAD_WAIT_MILLSECS", 1000); //Millisecs
      _uploadTimeout = configuration.GetValueDouble("DATA_OCEAN_UPLOAD_TIMEOUT_MINS", 5); //minutes

      Log.LogInformation($"{DATA_OCEAN_URL_KEY}={_dataOceanBaseUrl}");
    }

    /// <summary>
    /// Determines if the folder path exists.
    /// </summary>
    public async Task<bool> FolderExists(string path, IDictionary<string, string> customHeaders)
    {
      Log.LogDebug($"FolderExists: {path}");

      var folder = await GetFolderMetadata(path, true, customHeaders);
      return folder != null;
    }

    /// <summary>
    /// Determines if the file exists.
    /// </summary>
    public async Task<bool> FileExists(string filename, IDictionary<string, string> customHeaders)
    {
      Log.LogDebug($"FileExists: {filename}");

      var result = await GetFileMetadata(filename, customHeaders);
      return result != null;
    }

    /// <summary>
    /// Makes the folder structure. Can partially exist already i.e. parent folder. 
    /// </summary>
    public async Task<bool> MakeFolder(string path, IDictionary<string, string> customHeaders)
    {
      Log.LogDebug($"MakeFolder: {path}");

      var folder = await GetFolderMetadata(path, false, customHeaders);
      return folder != null;
    }

    /// <summary>
    /// Saves the file.
    /// </summary>
    public async Task<bool> PutFile(string path, string filename, Stream contents, IDictionary<string, string> customHeaders = null)
    {
      var fullName = Path.Combine(path, filename);
      Log.LogDebug($"PutFile: {fullName}");

      var success = false;
      var parentFolder = await GetFolderMetadata(path, true, customHeaders);

      //Delete any existing file. To avoid 2 traversals just try it anyway without checking for existance.
      await DeleteFile(fullName, customHeaders);

      //1. Create the file
      var createResult = await CreateFile(filename, parentFolder?.Id, customHeaders);
      var newFile = createResult?.File;

      if (newFile != null)
      {
        //2. Upload the file
        await _gracefulClient.ExecuteRequestAsStreamContent(newFile.DataOceanUpload.Url, HttpMethod.Put, customHeaders, contents);

        //3. Monitor status of upload until done
        var route = $"/api/files/{newFile.Id}";
        var endJob = DateTime.Now + TimeSpan.FromMinutes(_uploadTimeout);
        var done = false;

        while (!done && DateTime.Now <= endJob)
        {
          if (_uploadWaitInterval > 0)
          {
            await Task.Delay(_uploadWaitInterval);
          }

          //TODO: This could be a scheduler job, polled for by the caller, if the upload is too slow.
          var getResult = await GetData<DataOceanFileResult>(route, null, customHeaders);
          var status = getResult.File.Status.ToUpper();

          success = string.CompareOrdinal(status, "AVAILABLE") == 0;
          done = success || string.CompareOrdinal(status, "UPLOAD_FAILED") == 0;
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
    /// 
    public async Task<bool> DeleteFile(string fullName, IDictionary<string, string> customHeaders)
    {
      Log.LogDebug($"DeleteFile: {fullName}");

      var result = await GetFileMetadata(fullName, customHeaders);
      if (result != null)
      {
        var route = $"/api/files/{result.Id}";
        await _gracefulClient.ExecuteRequest($"{_dataOceanBaseUrl}{route}", null, customHeaders, HttpMethod.Delete);
        return true;
      }

      return false;
    }

    /// <summary>
    /// Gets the id of the lowest level folder metadata in the path 
    /// </summary>
    /// 
    public async Task<Guid?> GetFolderId(string path, IDictionary<string, string> customHeaders)
    {
      Log.LogDebug($"GetFolderId: {path}");

      var folder = await GetFolderMetadata(path, true, customHeaders);
      return folder?.Id;
    }

    /// <summary>
    /// Gets the file id
    /// </summary>
    /// 
    public async Task<Guid?> GetFileId(string fullName, IDictionary<string, string> customHeaders)
    {
      Log.LogDebug($"GetFileId: {fullName}");

      var result = await GetFileMetadata(fullName, customHeaders);
      return result?.Id;
    }

    /// <summary>
    /// Gets the file contents
    /// </summary>
    /// 
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
        response = await _gracefulClient.ExecuteRequestAsStreamContent(downloadUrl, HttpMethod.Get, customHeaders,
          null, null, result.Multifile ? 0 : 3);
      }
      catch (HttpRequestException ex)
      {
        //If tile does not exist DataOcean returns 403
        if (!result.Multifile ||
            !(string.CompareOrdinal(ex.Message.Substring(0, 3), "403") == 0 || string.Compare(ex.Message.Substring(0, 9), "Forbidden", StringComparison.OrdinalIgnoreCase) == 0))
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

    /// <summary>
    /// Gets the file metadata.
    /// </summary>
    private async Task<DataOceanFile> GetFileMetadata(string fullName, IDictionary<string, string> customHeaders)
    {
      Log.LogDebug($"{nameof(GetFileMetadata)}: {fullName}");

      var path = Path.GetDirectoryName(fullName)?.Replace(Path.DirectorySeparatorChar, DataOceanUtil.PathSeparator);
      var parentFolder = await GetFolderMetadata(path, true, customHeaders);

      var result = await BrowseFile(Path.GetFileName(fullName), parentFolder?.Id, customHeaders);
      var count = result?.Files?.Count;

      if (count == 1)
      {
        return result.Files[0];
      }
      if (count == 0)
      {
        Log.LogInformation($"File {fullName} not found");
      }
      if (count > 1)
      {
        Log.LogWarning($"Multiple copies of file {fullName} found - returning latest");
        return result.Files.OrderByDescending(f => f.UpdatedAt).First();
      }

      return null;
    }

    /// <summary>
    /// Gets the lowest level folder metadata in the path. Creates it unless it this is purely a query and therefore must exist.
    /// </summary>
    private async Task<DataOceanDirectory> GetFolderMetadata(string path, bool mustExist, IDictionary<string, string> customHeaders)
    {
      Log.LogDebug($"GetFolderMetadata: {path}");

      //NOTE: DataOcean requires / regardless of OS. However we construct the path and split using DataOceanUtil.PathSeparator.
      //This is merely a convenience as DataOcean doesn't use paths but a hierarchy of folders above the file, linked using parent_id.
      //We traverse this hierarchy. The only place it matters is the multifile structure for tiles. This is contained in the multifile
      //download url so we don't have to worry about it.
      var parts = path.Split(DataOceanUtil.PathSeparator);
      DataOceanDirectory folder = null;
      Guid? parentId = null;
      var creatingPath = false;

      for (var i = 0; i < parts.Length; i++)
      {
        if (string.IsNullOrEmpty(parts[i])) { continue; }

        //Once we know part of the path doesn't exist we can shortcut browsing to check for existance
        int? count = 0;

        if (!creatingPath)
        {
          var result = await BrowseFolder(parts[i], parentId, customHeaders);
          count = result?.Directories?.Count;

          if (count == 1)
          {
            folder = result.Directories[0];
            parentId = folder.Id;
          }
        }

        if (count == 0)
        {
          if (mustExist) { return null; }

          folder = (await CreateDirectory(parts[i], parentId, customHeaders)).Directory;
          parentId = folder.Id;
          creatingPath = true;
        }
        else if (count > 1)
        {
          Log.LogWarning($"Duplicate folders {parts[i]} in path {path}");
          return null;
        }
      }

      //Folders in path already exist or have been created successfully
      return folder;
    }

    /// <summary>
    /// Gets the requested folder metadata at the specified level i.e. with the requested parent.
    /// </summary>
    private Task<BrowseDirectoriesResult> BrowseFolder(string folderName, Guid? parentId, IDictionary<string, string> customHeaders) => BrowseItem<BrowseDirectoriesResult>(folderName, parentId, true, customHeaders);

    /// <summary>
    /// Gets the requested file metadata at the specified level i.e. with the requested parent.
    /// </summary>
    private Task<BrowseFilesResult> BrowseFile(string fileName, Guid? parentId, IDictionary<string, string> customHeaders) => BrowseItem<BrowseFilesResult>(fileName, parentId, false, customHeaders);

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
    private Task<DataOceanDirectoryResult> CreateDirectory(string directoryName, Guid? parentId, IDictionary<string, string> customHeaders)
    {
      var message = new CreateDirectoryMessage
      {
        Directory = new DataOceanDirectory
        {
          Name = directoryName,
          ParentId = parentId
        }
      };

      return CreateItem<CreateDirectoryMessage, DataOceanDirectoryResult>(message, "/api/directories", customHeaders);
    }

    /// <summary>
    /// Creates a DataOcean file.
    /// </summary>
    private Task<DataOceanFileResult> CreateFile(string filename, Guid? parentId, IDictionary<string, string> customHeaders)
    {
      var message = new CreateFileMessage
      {
        File = new DataOceanFile
        {
          Name = filename,
          ParentId = parentId,
          Multifile = false,
          RegionPreferences = new List<string> { DataOceanUtil.RegionalPreferences.US1 }
        }
      };

      return CreateItem<CreateFileMessage, DataOceanFileResult>(message, "/api/files", customHeaders);
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
    private async Task<U> CreateItem<T, U>(T message, string route, IDictionary<string, string> customHeaders)
    {
      var payload = JsonConvert.SerializeObject(message);
      Log.LogDebug($"CreateItem: route={route}, message={payload}");

      using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(payload)))
      {
        var result = await _gracefulClient.ExecuteRequest<U>($"{_dataOceanBaseUrl}{route}", ms, customHeaders, HttpMethod.Post);
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

      var query = $"{_dataOceanBaseUrl}{route}";
      if (queryParameters != null)
      {
        query = QueryHelpers.AddQueryString(query, queryParameters);
      }
      var result = await _gracefulClient.ExecuteRequest<T>(query, null, customHeaders, HttpMethod.Get);
      Log.LogDebug($"GetData: result={(result == null ? "null" : JsonConvert.SerializeObject(result))}");
      return result;
    }
  }
}
