using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
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
    private readonly ILogger<DataOceanClient> _log;
    private readonly IWebRequest _gracefulClient;
    private readonly string _dataOceanBaseUrl;
    private readonly int _uploadWaitInterval;
    private readonly double _uploadTimeout;
    
    private readonly DataOceanFolderCache _dataOceanFolderCache;

    /// <summary>
    /// Client for sending requests to the data ocean.
    /// </summary>
    public DataOceanClient(IConfigurationStore configuration, ILoggerFactory logger, IWebRequest gracefulClient, IMemoryCache memoryCache)
    {
      _log = logger.CreateLogger<DataOceanClient>();
      _gracefulClient = gracefulClient;

      const string DATA_OCEAN_ROOT_FOLDER_ID_KEY = "DATA_OCEAN_ROOT_FOLDER_ID";
      var dataOceanRootFolderId = configuration.GetValueString(DATA_OCEAN_ROOT_FOLDER_ID_KEY);
      if (string.IsNullOrEmpty(dataOceanRootFolderId))
        throw new ArgumentException($"Missing environment variable {DATA_OCEAN_ROOT_FOLDER_ID_KEY}");
      _dataOceanFolderCache = new DataOceanFolderCache(memoryCache, dataOceanRootFolderId);

      const string DATA_OCEAN_URL_KEY = "DATA_OCEAN_URL";
      _dataOceanBaseUrl = configuration.GetValueString(DATA_OCEAN_URL_KEY);

      if (string.IsNullOrEmpty(_dataOceanBaseUrl))
        throw new ArgumentException($"Missing environment variable {DATA_OCEAN_URL_KEY}");

      _uploadWaitInterval = configuration.GetValueInt("DATA_OCEAN_UPLOAD_WAIT_MILLSECS", 1000);
      _uploadTimeout = configuration.GetValueDouble("DATA_OCEAN_UPLOAD_TIMEOUT_MINS", 5);

      _log.LogInformation($"{nameof(DataOceanClient)} {DATA_OCEAN_URL_KEY}={_dataOceanBaseUrl}");
    }

    /// <summary>
    /// Determines if the folder path exists.
    /// </summary>
    public async Task<bool> FolderExists(string path, IDictionary<string, string> customHeaders)
    {
      _log.LogDebug($"{nameof(FolderExists)}: {path}");

      var folder = await GetFolderMetadata(path, true, customHeaders);
      return folder != null;
    }

    /// <summary>
    /// Determines if the file exists.
    /// </summary>
    public async Task<bool> FileExists(string filename, IDictionary<string, string> customHeaders)
    {
      _log.LogDebug($"{nameof(FileExists)}: {filename}");

      var result = await GetFileMetadata(filename, customHeaders);
      return result != null;
    }

    /// <summary>
    /// Makes the folder structure. Can partially exist already i.e. parent folder. 
    /// </summary>
    public async Task<bool> MakeFolder(string path, IDictionary<string, string> customHeaders)
    {
      _log.LogDebug($"{nameof(MakeFolder)}: {path}");

      var folder = await GetFolderMetadata(path, false, customHeaders);
      return folder != null;
    }


    /// <summary>
    /// Uploads the file to Data Ocean, will upsert if necessary.
    /// </summary>
    public async Task<bool> PutFile(string path, string filename, Stream contents, IDictionary<string, string> customHeaders = null)
    {
      var fullName = Path.Combine(path, filename);
      _log.LogDebug($"{nameof(PutFile)}: {fullName}");

      var success = false;
      var parentFolder = await GetFolderMetadata(path, true, customHeaders);

      //Delete any existing file. To avoid 2 traversals just try it anyway without checking for existence.
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
          _log.LogDebug($"{nameof(PutFile)} timed out: {path}/{filename}");
        }
        else if (!success)
        {
          _log.LogDebug($"{nameof(PutFile)} failed: {path}/{filename}");
        }
      }

      _log.LogDebug($"{nameof(PutFile)}: success={success}");
      return success;
    }

    /// <summary>
    /// Deletes the file.
    /// </summary>
    public async Task<bool> DeleteFile(string fullName, IDictionary<string, string> customHeaders)
    {
      _log.LogDebug($"{nameof(DeleteFile)}: {fullName}");

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
    public async Task<Guid?> GetFolderId(string path, IDictionary<string, string> customHeaders)
    {
      _log.LogDebug($"{nameof(GetFolderId)}: {path}");

      var folder = await GetFolderMetadata(path, true, customHeaders);
      return folder?.Id;
    }

    /// <summary>
    /// Gets the file id
    /// </summary>
    public async Task<Guid?> GetFileId(string fullName, IDictionary<string, string> customHeaders)
    {
      _log.LogDebug($"{nameof(GetFileId)}: {fullName}");

      var result = await GetFileMetadata(fullName, customHeaders);
      return result?.Id;
    }

    /// <summary>
    /// Gets the file contents
    /// </summary>
    public async Task<Stream> GetFile(string fullName, IDictionary<string, string> customHeaders)
    {
      _log.LogDebug($"{nameof(GetFile)}: {fullName}");

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
        _log.LogWarning($"{nameof(GetFile)} Failed to find file {fullName}");
        return null;
      }
      var downloadUrl = result.DataOceanDownload.Url;
      //PNG tiles files and tiles.json metadata file are in a DataOcean multifile
      if (result.Multifile)
      {
        if (string.IsNullOrEmpty(tileFolderAndFileName))
        {
          _log.LogError($"{nameof(GetFile)} Getting a multi-file other than tiles is not implemented");
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
      _log.LogDebug($"{nameof(GetFileMetadata)}: {fullName}");

      var path = Path.GetDirectoryName(fullName)?.Replace(Path.DirectorySeparatorChar, DataOceanUtil.PathSeparator);
      var parentFolder = await GetFolderMetadata(path, true, customHeaders);

      var result = await BrowseFile(Path.GetFileName(fullName), parentFolder?.Id, customHeaders);
      var count = result?.Files?.Count;

      if (count == 1) return result.Files[0];

      if (count == 0) _log.LogInformation($"File {fullName} not found");

      if (count > 1)
      {
        _log.LogWarning($"{nameof(GetFileMetadata)} Multiple copies of file {fullName} found - returning latest");
        return result.Files.OrderByDescending(f => f.UpdatedAt).First();
      }

      return null;
    }

    /// <summary>
    /// Gets the lowest level folder metadata in the path. Creates it unless it this is purely a query and therefore must exist.
    ///       //NOTE: DataOcean requires / regardless of OS. However we construct the path and split using DataOceanUtil.PathSeparator.
    ///   This is merely a convenience as DataOcean doesn't use paths but a hierarchy of folders above the file, linked using parent_id.
    ///   We traverse this hierarchy. The only place it matters is the multi-file structure for tiles. This is contained in the multi-file
    ///   download url so we don't have to worry about it.
    /// </summary>
    private async Task<DataOceanDirectory> GetFolderMetadata(string path, bool mustExist, IDictionary<string, string> customHeaders)
    {
      _log.LogDebug($"{nameof(GetFolderMetadata)}: path: {path}, mustExist: {mustExist}");

      // part1 is dataOcean root folder, 1 more level is required
      var parts = path.Split(DataOceanUtil.PathSeparator);
      parts = parts.Where(p => !string.IsNullOrEmpty(p)).ToArray();
      if (parts.Length < 2)
      {
        _log.LogError($"{nameof(GetFolderMetadata)} Not enough parts in folder path. path {path}");
        return null;
      }

      DataOceanDirectory folder = null;
      var parentId = parts[0];
      var creatingPath = false;
      _dataOceanFolderCache._cache.TryGetValue(parts[0], out DataOceanFolderPath currentDataOceanFolderPath);
      if (currentDataOceanFolderPath == null)
      {
        _log.LogError($"{nameof(GetFolderMetadata)} Unable to retrieve root cache. dataOceanRootFolderId {parts[0]}");
        return null;
      }

      for (var i = 1; i < parts.Length; i++)
      {
        if (currentDataOceanFolderPath.Nodes.TryGetValue(parts[i], out var retrievedCurrentDataOceanFolderPath))
        {
          currentDataOceanFolderPath = retrievedCurrentDataOceanFolderPath;
          folder = new DataOceanDirectory() { Id = Guid.Parse(currentDataOceanFolderPath.DataOceanFolderId), Name = parts[i], ParentId = Guid.Parse(parentId) };
          _log.LogDebug($"{nameof(GetFolderMetadata)}: found cached folder. parts[i]: {parts[i]}, Id: {currentDataOceanFolderPath.DataOceanFolderId}, ParentId: {parentId}");
          parentId = currentDataOceanFolderPath.DataOceanFolderId;
        }
        else
        {
          int? directoriesCount = 0;
          if (!creatingPath)
          {
            var result = await BrowseFolder(parts[i], Guid.Parse(parentId), customHeaders);
            directoriesCount = result?.Directories?.Count;

            if (directoriesCount == 1)
            {
              folder = result.Directories[0];
              parentId = folder.Id.ToString();
              currentDataOceanFolderPath = CacheCreateNode(currentDataOceanFolderPath, parentId, parts[i]);
              _log.LogDebug($"{nameof(GetFolderMetadata)}: create cache for existing folder. parts[i]: {parts[i]}, Id: {currentDataOceanFolderPath.DataOceanFolderId}, ParentId: {parentId}");
            }
          }

          if (directoriesCount == 0)
          {
            if (mustExist) return null;

            folder = (await CreateDirectory(parts[i], Guid.Parse(parentId), customHeaders)).Directory;
            parentId = folder.Id.ToString();
            currentDataOceanFolderPath = CacheCreateNode(currentDataOceanFolderPath, parentId, parts[i]);
            _log.LogDebug($"{nameof(GetFolderMetadata)}: create cache for created folder. parts[i]: {parts[i]}, Id: {currentDataOceanFolderPath.DataOceanFolderId}, ParentId: {parentId}");
            creatingPath = true;
          }
          else if (directoriesCount > 1)
          {
            _log.LogWarning($"{nameof(GetFolderMetadata)} Duplicate folders {parts[i]} in path {path}");
            return null;
          }
        }
      }

      //Folders in path already exist or have been created successfully
      return folder;
    }

    private DataOceanFolderPath CacheCreateNode(DataOceanFolderPath currentDataOceanFolderPath, string parentId, string part)
    {
      var folderPath = new DataOceanFolderPath(parentId, new Dictionary<string, DataOceanFolderPath>());
      currentDataOceanFolderPath.Nodes.Add(part, folderPath);
      if (!currentDataOceanFolderPath.Nodes.TryGetValue(part, out var retrievedCurrentDataOceanFolderPath))
      {
        _log.LogError($"{nameof(GetFolderMetadata)} Unable to add existing folder path in cache. part {part}");
        return null;
      }
      return retrievedCurrentDataOceanFolderPath;
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
    private Task<T> BrowseItem<T>(string itemName, Guid? parentId, bool isFolder, IDictionary<string, string> customHeaders)
    {
      _log.LogDebug($"{nameof(BrowseItem)} ({(isFolder ? "FOLDER" : "FILE")}): name={itemName}, parentId={parentId}");

      var queryParameters = new Dictionary<string, string>
      {
        { "name", itemName },
        { "owner", "true" }
      };

      if (parentId.HasValue) queryParameters.Add("parent_id", parentId.Value.ToString());

      return GetData<T>($"/api/browse/{(isFolder ? "keyset_directories" : "keyset_files")}", queryParameters, customHeaders);
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
    private async Task<U> CreateItem<T, U>(T message, string route, IDictionary<string, string> customHeaders)
    {
      var payload = JsonConvert.SerializeObject(message);
      _log.LogDebug($"{nameof(CreateItem)}: route={route}, message={payload}");

      using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(payload)))
      {
        var result = await _gracefulClient.ExecuteRequest<U>($"{_dataOceanBaseUrl}{route}", ms, customHeaders, HttpMethod.Post);
        _log.LogDebug($"{nameof(CreateItem)}: result={JsonConvert.SerializeObject(result)}");
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
    private async Task<T> GetData<T>(string route, IDictionary<string, string> queryParameters, IDictionary<string, string> customHeaders)
    {
      _log.LogDebug($"{nameof(GetData)}: route={route}, queryParameters={JsonConvert.SerializeObject(queryParameters)}");

      var query = $"{_dataOceanBaseUrl}{route}";
      if (queryParameters != null)
      {
        query = QueryHelpers.AddQueryString(query, queryParameters);
      }
      var result = await _gracefulClient.ExecuteRequest<T>(query, null, customHeaders, HttpMethod.Get);
      _log.LogDebug($"{nameof(GetData)}: result={(result == null ? "null" : JsonConvert.SerializeObject(result))}");
      return result;
    }
  }
}
