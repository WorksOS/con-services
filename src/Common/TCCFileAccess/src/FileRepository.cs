using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies;
using VSS.TCCFileAccess.Models;

namespace VSS.TCCFileAccess
{
  /// <summary>
  /// File access class to talk to TCC. This class is thread safe 
  /// </summary>
  /// <seealso cref="IFileRepository" />
  /// TODO add resiliency with Polly. Include CheckForInvalidTicket into resiliency logic.
  public class FileRepository : IFileRepository
  {
    public string tccBaseUrl { get; }
    public string tccUserName { get; }
    public string tccPassword { get; }
    public string tccOrganization { get; }

    private const string INVALID_TICKET_ERRORID = "NOT_AUTHENTICATED";
    private const string INVALID_TICKET_MESSAGE = "You have not authenticated, use login action";

    //Reinvalidate tcc ticket every 30 min
    private static DateTime lastLoginTimestamp;


    private readonly ILogger<FileRepository> Log;
    private readonly ILoggerFactory logFactory;
    private readonly IConfigurationStore configStore;

    private static string ticket = String.Empty;
    private static object ticketLockObj = new object();

    /// <summary>
    /// The file cache - contains byte array for PNGs as a value and a full filename (path to the PNG) as a key. 
    /// It should persist across session so static. This class is thread-safe.
    /// Caching the tile files improves performance as downloading files from TCC is expensive.
    /// </summary>
    private static readonly MemoryCache fileCache =
      new MemoryCache(new MemoryCacheOptions()
      {
        CompactOnMemoryPressure = true,
        ExpirationScanFrequency = TimeSpan.FromMinutes(10)
      });

    /// <summary>
    /// The cache lookup class to support fast keys lookup in cache by filename, not the full path to the PNG as there are a lot of tiles per a file.
    /// It folder structure in TCC is /customeruid/projectuid/filename_generatedsuffix$.DXF_Tiles$/zoom/ytile/xtile.png
    /// </summary>
    private CacheLookup cacheLookup = new CacheLookup();

    private string Ticket
    {
      get
      {
        lock (ticketLockObj)
        {
          if (!string.IsNullOrEmpty(ticket) && lastLoginTimestamp >= DateTime.UtcNow.AddMinutes(-30)) return ticket;
          ticket = Login().Result;
          lastLoginTimestamp = DateTime.UtcNow;
          return ticket;
        }
      }
    }

    public FileRepository(IConfigurationStore configuration, ILoggerFactory logger)
    {
      tccBaseUrl = configuration.GetValueString("TCCBASEURL");
      tccUserName = configuration.GetValueString("TCCUSERNAME");
      tccPassword = configuration.GetValueString("TCCPWD");
      tccOrganization = configuration.GetValueString("TCCORG");
      if (string.IsNullOrEmpty(tccBaseUrl) || string.IsNullOrEmpty(tccUserName) || 
          string.IsNullOrEmpty(tccPassword) || string.IsNullOrEmpty(tccOrganization))
      {
        throw new Exception("Missing environment variable TCCBASEURL, TCCUSERNAME, TCCPWD or TCCORG");
      }
      if (!tccBaseUrl.ToLower().StartsWith("http"))
      {
        throw new Exception($"Invalid TCC URL {tccBaseUrl}");
      }
      logFactory = logger;
      Log = logger.CreateLogger<FileRepository>();
      configStore = configuration;
      Log.LogInformation($"TCCBASEURL={tccBaseUrl}");
    }

    public async Task<List<Organization>> ListOrganizations()
    {
      Log.LogDebug("ListOrganizations");
      List<Organization> orgs = null;
      try
      {
        GetFileSpacesParams fileSpaceParams = new GetFileSpacesParams { };
        var filespacesResult = await ExecuteRequest<GetFileSpacesResult>(Ticket, "GetFileSpaces", fileSpaceParams);
        if (filespacesResult != null)
        {
          if (filespacesResult.success)
          {
            if (filespacesResult.filespaces != null)
            {
              if (filespacesResult.filespaces.Any())
              {
                orgs =
                  filespacesResult.filespaces.Select(
                      filespace =>
                        new Organization
                        {
                          filespaceId = filespace.filespaceId,
                          shortName = filespace.orgShortname,
                          orgId = filespace.orgId,
                          orgDisplayName = filespace.orgDisplayName,
                          orgTitle = filespace.shortname
                        })
                    .ToList();
              }
            }
            else
            {
              Log.LogWarning("No organizations returned from ListOrganizations");
              orgs = new List<Organization>();
            }
          }
          else
          {
            CheckForInvalidTicket(filespacesResult, "ListOrganizations");
          }
        }
        else
        {
          Log.LogError("Null result from ListOrganizations");
        }
      }
      catch (Exception ex)
      {
        Log.LogError("Failed to get list of TCC organizations", ex);
      }
      return orgs;
    }

    public async Task<PutFileResponse> PutFile(Organization org, string path, string filename, Stream contents,
      long sizeOfContents)
    {
      Log.LogDebug("PutFile: org={0}", org.shortName);
      return await PutFileEx(org.filespaceId, path, filename, contents, sizeOfContents);
    }

    public async Task<bool> PutFile(string filespaceId, string path, string filename, Stream contents,
      long sizeOfContents)
    {
      var result = await PutFileEx(filespaceId, path, filename, contents, sizeOfContents);
      if (!result.success)
      {
        CheckForInvalidTicket(result, "PutFile");
      }
      return result.success;
    }

    public async Task<PutFileResponse> PutFileEx(string filespaceId, string path, string filename, Stream contents,
      long sizeOfContents)
    {
      Log.LogDebug("PutFileEx: filespaceId={0}, fullName={1} {2}", filespaceId, path, filename);

      //NOTE: for this to work in TCC the path must exist otherwise TCC either gives an error or creates the file as the folder name
      PutFileRequest sendFileParams = new PutFileRequest()
      {
        filespaceid = filespaceId,
        path = path,//WebUtility.UrlEncode(path),
        replace = true,
        commitUpload = true,
        filename = filename//WebUtility.UrlEncode(filename)
      };
      if (String.IsNullOrEmpty(tccBaseUrl))
        throw new Exception("Configuration Error - no TCC url specified");

      var gracefulClient = new GracefulWebRequest(logFactory, configStore);
      var (requestString, headers) = FormRequest(sendFileParams, "PutFile");

      headers.Add("X-File-Name", WebUtility.UrlEncode(filename));
      headers.Add("X-File-Size", sizeOfContents.ToString());
      headers.Add("X-FileType", "");

      PutFileResponse result = default(PutFileResponse);
      try
      {
        result = await gracefulClient.ExecuteRequest<PutFileResponse>(requestString, contents, headers, "PUT");
      }
      catch (WebException webException)
      {
        using (var response = webException.Response)
        {
          Log.LogWarning(
            $"Can not execute request TCC request with error {webException.Status} and {webException.Message}. {GetStringFromResponseStream(response)}");
        }
      }
      catch (Exception e)
      {
        Log.LogWarning("Can not execute request TCC response. Details: {0} {1}", e.Message, e.StackTrace);
      }
      return result;
    }

    /// <summary>
    /// Gets the file. The resulting stream should be disposed after read completed
    /// </summary>
    /// <param name="org">The org.</param>
    /// <param name="fullName">The full name.</param>
    /// <returns></returns>
    public async Task<Stream> GetFile(Organization org, string fullName)
    {
      Log.LogDebug("GetFile: org={0} {1}, fullName={2}", org.shortName, org.filespaceId, fullName);

      return await GetFileEx(org.filespaceId, fullName);
    }

    /// <summary>
    /// Gets the file. The resulting stream should be disposed after read completed
    /// </summary>
    /// <param name="filespaceId">The file space ID.</param>
    /// <param name="fullName">The full name.</param>
    /// <returns></returns>
    public async Task<Stream> GetFile(string filespaceId, string fullName)
    {
      Log.LogDebug("GetFile: filespaceId={0}, fullName={1}", filespaceId, fullName);

      return await GetFileEx(filespaceId, fullName);
    }

    private async Task<Stream> GetFileEx(string filespaceId, string fullName)
    {
      byte[] file = null;
      bool cacheable = TCCFile.FileCacheable(fullName);
      if (cacheable)
      {
        Log.LogDebug("Trying to extract from cache {0} with cache size {1}", fullName,fileCache.Count);
        if (fileCache.TryGetValue(fullName, out file))
        {
          Log.LogDebug("Serving TCC tile request from cache {0}", fullName);
          if (file.Length == 0)
          {
            Log.LogDebug("Serving TCC tile request from cache empty tile");
            return null;
          }
          return new MemoryStream(file);
        }
      }

      GetFileParams getFileParams = new GetFileParams
      {
        filespaceid = filespaceId,
        path = fullName//WebUtility.UrlEncode(fullName)
      };

      if (string.IsNullOrEmpty(tccBaseUrl))
      {
        throw new Exception("Configuration Error - no TCC url specified");
      }

      var gracefulClient = new GracefulWebRequest(logFactory, configStore);
      var (requestString, headers) = FormRequest(getFileParams, "GetFile");

      try
      {
        if (!cacheable)
        {
          using (var responseStream = await gracefulClient.ExecuteRequestAsStreamContent(requestString, "GET", headers, retries: 0))
          {
            responseStream.Position = 0;
            file = new byte[responseStream.Length];
            responseStream.Read(file, 0, file.Length);
            return new MemoryStream(file);
          }
        }

        using (var responseStream = await gracefulClient.ExecuteRequestAsStreamContent(requestString, "GET", headers))
        {
          Log.LogDebug("Adding TCC tile request to cache {0}", fullName);
          responseStream.Position = 0;
          file = new byte[responseStream.Length];
          responseStream.Read(file, 0, file.Length);
          fileCache.Set(fullName, file, DateTimeOffset.MaxValue);
          Log.LogDebug("About to extract file name for {0}", fullName);
          var baseFileName = TCCFile.ExtractFileNameFromTileFullName(fullName);
          Log.LogDebug("Extracted file name is {0}", baseFileName);
          cacheLookup.AddFile(baseFileName, fullName);
          return new MemoryStream(file);
        }
      }
      catch (WebException webException)
      {
        using (var response = webException.Response)
        {
          Log.LogWarning(
            $"Can not execute request TCC request with error {webException.Status} and {webException.Message}. {GetStringFromResponseStream(response)}");
        }
        //let's cache the response anyway but for a limited time
        fileCache.Set(fullName, new byte[0], DateTimeOffset.UtcNow.AddHours(12));
      }
      catch (Exception e)
      {
        Log.LogWarning("Can not execute request TCC response. Details: {0} {1}", e.Message, e.StackTrace);
      }
      return null;
    }

    public async Task<bool> MoveFile(Organization org, string srcFullName, string dstFullName)
    {
      Log.LogDebug("MoveFile: org={0} {1}, srcFullName={2}, dstFullName={3}", org.shortName, org.filespaceId,
        srcFullName, dstFullName);
      try
      {
        var dstPath = dstFullName.Substring(0, dstFullName.LastIndexOf("/")) + "/";
        if (!await FolderExists(org.filespaceId, dstPath))
        {
          var resultCreate = await MakeFolder(org.filespaceId, dstPath);
          if (!resultCreate)
          {
            Log.LogError("Can not create folder for org {0} folder {1}", org.shortName,
              dstPath);
            return false;
          }
        }

        RenParams renParams = new RenParams
        {
          filespaceid = org.filespaceId,
          path = srcFullName,//WebUtility.UrlEncode(srcFullName),
          newfilespaceid = org.filespaceId,
          newPath = dstFullName,//WebUtility.UrlEncode(dstFullName),
          merge = false,
          replace = true
        };
        var renResult = await ExecuteRequest<RenResult>(Ticket, "Ren", renParams);
        if (renResult != null)
        {
          if (renResult.success || renResult.errorid.Contains("INVALID_OPERATION_FILE_IS_LOCKED"))
          {
            return true;
          }
          CheckForInvalidTicket(renResult, "MoveFile");
        }
        else
        {
          Log.LogError("Null result from MoveFile for org {0} file {1}", org.shortName, srcFullName);
        }
      }
      catch (Exception ex)
      {
        Log.LogError("Failed to move TCC file for org {0} file {1}: {2}", org.shortName, srcFullName,
          ex.Message);
      }
      return false;
    }


    public Task<bool> CopyFile(string filespaceId, string srcFullName, string dstFullName)
    {
      return CopyFile(filespaceId, filespaceId, srcFullName, dstFullName);
    }

    public async Task<bool> CopyFile(string srcFilespaceId, string dstFilespaceId, string srcFullName, string dstFullName)
    {
      Log.LogDebug(
        $"CopyFile: srcFilespaceId={srcFilespaceId}, srcFilespaceId={srcFilespaceId}, dstFilespaceId={dstFilespaceId} srcFullName={srcFullName}, dstFullName={dstFullName}");
      try
      {
        var dstPath = dstFullName.Substring(0, dstFullName.LastIndexOf("/")) + "/";
        if (!await FolderExists(dstFilespaceId, dstPath))
        {
          var resultCreate = await MakeFolder(dstFilespaceId, dstPath);
          if (!resultCreate)
          {
            Log.LogError("Can not create folder for filespaceId {dstFilespaceId} folder {dstPath}");
            return false;
          }
        }

        CopyParams copyParams = new CopyParams
        {
          filespaceid = srcFilespaceId,
          path = srcFullName, //WebUtility.UrlEncode(srcFullName),
          newfilespaceid = dstFilespaceId,
          newPath = dstFullName,//WebUtility.UrlEncode(dstFullName),
          merge = false,
          replace = true//Not sure if we want true or false here
        };
        var copyResult = await ExecuteRequest<ApiResult>(Ticket, "Copy", copyParams);
        if (copyResult != null)
        {
          if (copyResult.success || copyResult.errorid.Contains("INVALID_OPERATION_FILE_IS_LOCKED"))
          {
            return true;
          }
          CheckForInvalidTicket(copyResult, "CopyFile");
        }
        else
        {
          Log.LogError($"Null result from CopyFile for filespaceId {srcFilespaceId} file {srcFullName}");
        }
      }
      catch (Exception ex)
      {
        Log.LogError($"Failed to copy TCC file for srcFilespaceId={srcFilespaceId}, srcFilespaceId={srcFilespaceId}, dstFilespaceId={dstFilespaceId} srcFullName={srcFullName}, dstFullName={dstFullName} error:{ex.Message}");
      }
      return false;
    }

    public async Task<DirResult> GetFolders(Organization org, DateTime lastModifiedUTC, string path)
    {
      Log.LogDebug("GetFolders: org={0} {1}, lastModfiedUTC={2}, path={3}", org.shortName, org.filespaceId,
        lastModifiedUTC, path);
      try
      {
        //Get list of folders one level down from path
        DirParams dirParams = new DirParams
        {
          filespaceid = org.filespaceId,
          path = path,//WebUtility.UrlEncode(path),
          recursive = false,
          filterfolders = true,
        };
        var dirResult = await ExecuteRequest<DirResult>(Ticket, "Dir", dirParams);
        if (dirResult != null)
        {
          if (dirResult.success)
            return dirResult;
          CheckForInvalidTicket(dirResult, "GetFolders");
        }
        else
        {
          Log.LogError("Null result from GetFolders for org {0}", org.shortName);
        }
      }
      catch (Exception ex)
      {
        Log.LogError("Failed to get list of TCC folders: {0}", ex.Message);
      }
      return null;
    }

    public async Task<DirResult> GetFileList(string filespaceId, string path, string fileMasks=null)
    {
      Log.LogDebug("GetFileList: filespaceId={0}, path={1}, fileMask={2}", filespaceId, path, fileMasks);
      try
      {
        //Get list of files one level down from path
        DirParams dirParams = new DirParams
        {
          filespaceid = filespaceId,
          path = path,//WebUtility.UrlEncode(path),
          recursive = false,
          filterfolders = false,
        };
        if (!string.IsNullOrEmpty(fileMasks))
          dirParams.filemasks = fileMasks;
        var dirResult = await ExecuteRequest<DirResult>(Ticket, "Dir", dirParams);
        if (dirResult != null)
        {
          if (dirResult.success)
            return dirResult;
          CheckForInvalidTicket(dirResult, "GetFileList");
        }
        else
        {
          Log.LogError("Null result from GetFileList for filespaceId {0}", filespaceId);
        }
      }
      catch (Exception ex)
      {
        Log.LogError("Failed to get list of TCC files: {0}", ex.Message);
      }
      return null;
    }

    public async Task<DateTime> GetLastChangedTime(string filespaceId, string path)
    {
      Log.LogDebug("GetLastChangedTime: filespaceId={0}, path={1}", filespaceId, path);

      try
      {
        LastDirChangeParams lastDirChangeParams = new LastDirChangeParams
        {
          filespaceid = filespaceId,
          path = path,//WebUtility.UrlEncode(path),
          recursive = true
        };
        var lastDirChangeResult =
          await ExecuteRequest<LastDirChangeResult>(Ticket, "LastDirChange", lastDirChangeParams);
        if (lastDirChangeResult != null)
        {
          if (lastDirChangeResult.success)
          {
            return lastDirChangeResult.lastUpdatedDateTime;
          }
          CheckForInvalidTicket(lastDirChangeResult, "GetLastChangedTime");
        }
        else
        {
          Log.LogError("Null result from GetLastChangedTime for filespaceId={0}, path={1}", filespaceId,
            path);
        }
      }
      catch (Exception ex)
      {
        Log.LogError("Failed to get last time tag files added to folder: {0}", ex.Message);
      }
      return DateTime.MinValue;
    }

    public async Task<bool> FolderExists(string filespaceId, string folder)
    {
      return await PathExists(filespaceId, folder);
    }

    public async Task<bool> FileExists(string filespaceId, string filename)
    {
      object obj = null;
      if (TCCFile.FileCacheable(filename))
        if (fileCache.TryGetValue(filename, out obj)) return true;
      return await PathExists(filespaceId, filename);
    }

    private async Task<bool> PathExists(string filespaceId, string path)
    {
      Log.LogDebug("Searching for file or folder {0}", path);
      try
      {
        GetFileAttributesParams getFileAttrParams = new GetFileAttributesParams
        {
          filespaceid = filespaceId,
          path = path,//WebUtility.UrlEncode(path)
        };
        var getFileAttrResult =
          await ExecuteRequestWithAllowedError<GetFileAttributesResult>(Ticket, "GetFileAttributes", getFileAttrParams);
        if (getFileAttrResult != null)
        {
          if (getFileAttrResult.success)
          {
            return true;
          }
          CheckForInvalidTicket(getFileAttrResult, "PathExists", false); //don't log "file does not exist"
          return getFileAttrResult.success;
        }
      }
      catch (Exception ex)
      {
        Log.LogError("Failed to get TCC file attributes: {0}", ex.Message);
      }
      return false;
    }


    public async Task<bool> DeleteFolder(string filespaceId, string path)
    {
      return await DeleteFileEx(filespaceId, path, true);
    }

    public async Task<bool> DeleteFile(string filespaceId, string fullName)
    {
      return await DeleteFileEx(filespaceId, fullName, false);
    }

    public async Task<bool> DeleteFileEx(string filespaceId, string fullName, bool isFolder)
    {
      Log.LogDebug("DeleteFileEx: filespaceId={0}, fullName={1}", filespaceId, fullName);
      try
      {
        DeleteFileParams deleteParams = new DeleteFileParams
        {
          filespaceid = filespaceId,
          path = fullName,//WebUtility.UrlEncode(fullName),
          recursive = isFolder
        };
        var deleteResult = await ExecuteRequest<DeleteFileResult>(Ticket, "Del", deleteParams);
        if (deleteResult != null)
        {
          if (deleteResult.success)
          {
            return true;
          }
          CheckForInvalidTicket(deleteResult, "DeleteFile");
          return deleteResult.success;
        }
      }
      catch (Exception ex)
      {
        Log.LogError("Failed to delete file: {0}", ex.Message);
      }
      return false;
    }

    public async Task<bool> MakeFolder(string filespaceId, string path)
    {
      Log.LogDebug("MakeFolder: filespaceId={0}, path={1}", filespaceId, path);
      try
      {
        MkDir mkDirParams = new MkDir
        {
          filespaceid = filespaceId,
          path = path,//WebUtility.UrlEncode(path),
          force = true
        };
        var mkDirResult = await ExecuteRequest<MkDirResult>(Ticket, "MkDir", mkDirParams);
        if (mkDirResult != null)
        {
          if (mkDirResult.success)
          {
            return true;
          }
          CheckForInvalidTicket(mkDirResult, "MakeFolder");
          return mkDirResult.success;
        }
      }
      catch (Exception ex)
      {
        Log.LogError("Failed to make directory: {0}", ex.Message);
      }
      return false;
    }

    private async Task<string> Login()
    {
      Log.LogInformation("Logging in to TCC: user={0}, org={1}", tccUserName, tccOrganization);
      try
      {
        LoginParams loginParams = new LoginParams
        {
          username = tccUserName,
          orgname = tccOrganization,
          password = tccPassword,
          mode = "noredirect",
          forcegmt = true
        };
        var loginResult = await ExecuteRequest<LoginResult>(ticket, "Login", loginParams);
        if (loginResult != null)
        {
          if (loginResult.success)
          {
            return loginResult.ticket;
          }
          Log.LogError("Failed to login to TCC: errorId={0}, reason={1}", loginResult.errorid,
            loginResult.reason);
        }
        else
        {
          Log.LogError("Null result from Login");
        }
        return null;
      }
      catch (Exception ex)
      {
        Log.LogError("Failed to login to TCC: {0}", ex.Message);
        return null;
      }
    }

    private void CheckForInvalidTicket(ApiResult result, string what, bool logWarning = true)
    {
      //Check for expired/invalid ticket
      if (!result.success)
      {
        if (result.errorid == INVALID_TICKET_ERRORID && result.message == INVALID_TICKET_MESSAGE)
        {
          ticket = null;
        }
        else if (logWarning)
        {
          Log.LogWarning("{0} failed: errorid={1}, message={2}", what, result.errorid, result.message);
        }
      }
    }


    private (string, Dictionary<string, string>) FormRequest(object request, string endpoint, string token = null)
    {
      var requestString = $"{tccBaseUrl}/tcc/{endpoint}?";
      var headers = new Dictionary<string, string>();
      /*var properties = from p in request.GetType().GetRuntimeFields()
                       where p.GetValue(request) != null
                       select new { p.Name, Value = p.GetValue(request) };*/
      var dProperties = request.GetType().GetRuntimeFields().Where(p => p.GetValue(request) != null).Select(p=> new { p.Name, Value = p.GetValue(request) })
        .ToDictionary(d => d.Name, v => v.Value.ToString());
      dProperties.Add("ticket", token ?? Ticket);

//      foreach (var p in properties)
      {
        requestString += new System.Net.Http.FormUrlEncodedContent(dProperties)
          .ReadAsStringAsync().Result;

        //$"&{p.Name}={p.Value.ToString()}";
      }
      return (requestString, headers);
    }

    private async Task<T> ExecuteRequest<T>(string token, string contractPath, object requestData)
    {
      if (String.IsNullOrEmpty(tccBaseUrl))
        throw new Exception("Configuration Error - no TCC url specified");

      var gracefulClient = new GracefulWebRequest(logFactory, configStore);
      var (requestString, headers) = FormRequest(requestData, contractPath, token);

      headers.Add("Content-Type", "application/json");
      var result = default(T);
      try
      {
        result = await gracefulClient.ExecuteRequest<T>(requestString, method: "GET", customHeaders: headers);
      }
      catch (WebException webException)
      {
        using (var response = webException.Response)
        {
          Log.LogWarning(
            $"Can not execute request TCC request with error {webException.Status} and {webException.Message}. {GetStringFromResponseStream(response)}");
        }
      }
      catch (Exception e)
      {
        Log.LogWarning("Can not execute request TCC response. Details: {0} {1}", e.Message, e.StackTrace);
      }
      return result;
    }

    private async Task<T> ExecuteRequestWithAllowedError<T>(string token, string contractPath, object requestData)
      where T : ApiResult, new()
    {
      const string FILE_DOES_NOT_EXIST_ERROR =
        "{\"errorid\":\"FILE_DOES_NOT_EXIST\",\"message\":\"File does not exist\",\"success\":false}";

      if (String.IsNullOrEmpty(tccBaseUrl))
        throw new Exception("Configuration Error - no TCC url specified");

      var gracefulClient = new GracefulWebRequest(logFactory, configStore);
      var (requestString, headers) = FormRequest(requestData, contractPath, token);

      headers.Add("Content-Type", "application/json");
      T result = default(T);
      try
      {
        result = await gracefulClient.ExecuteRequest<T>(requestString, method: "GET", customHeaders:headers, retries: 0, suppressExceptionLogging: true);
      }
      catch (WebException webException)
      {
        using (var response = webException.Response)
        {
          string tccError = GetStringFromResponseStream(response);
          if (tccError == FILE_DOES_NOT_EXIST_ERROR)
          {
            var tccErrorResult = JsonConvert.DeserializeObject<ApiResult>(FILE_DOES_NOT_EXIST_ERROR);

            result = new T
            {
              success = tccErrorResult.success,
              errorid = tccErrorResult.errorid,
              message = tccErrorResult.message
            };
          }
          else
          {
            Log.LogWarning(
              $"Can not execute request TCC request with error {webException.Status} and {webException.Message}. {tccError}");
          }
        }
      }
      catch (Exception e)
      {
        Log.LogWarning("Can not execute request TCC response. Details: {0} {1}", e.Message, e.StackTrace);
      }
      return result;
    }

    private string GetStringFromResponseStream(WebResponse response)
    {
      using (var readStream = response.GetResponseStream())
      {

        if (readStream != null)
        {
          var reader = new StreamReader(readStream, Encoding.UTF8);
          var responseString = reader.ReadToEnd();
          return responseString;
        }
        return string.Empty;
      }
    }

    #region Tile Rendering

    public async Task<string> CreateFileJob(string filespaceId, string path)
    {
      Log.LogDebug("CreateFileJob: filespaceId={0}, path={1}", filespaceId, path);
      try
      {
        CreateFileJobParams jobParams = new CreateFileJobParams
        {
          filespaceid = filespaceId,
          path = path,//WebUtility.UrlEncode(path),
          type = "GEOFILEINFO",
          forcerender = false
        };
        var jobResult = await ExecuteRequest<CreateFileJobResult>(Ticket, "CreateFileJob", jobParams);
        if (jobResult != null)
        {
          if (jobResult.success)
          {
            //This assumes that we're about to (re)generate tiles for the file
            //therefore clear the cache for this file if cached tiles exist.
            var filenames = cacheLookup.RetrieveCacheKeysExact(path);
            if (filenames != null)
            {
              Log.LogDebug($"Removing files for {path} from cachelookup and dropping cache");
              filenames.ForEach(s => fileCache.Remove(s));
              filenames.Clear();
              cacheLookup.DropCacheKeys(path);
            }

            return jobResult.jobId;
          }
          CheckForInvalidTicket(jobResult, "CreateFileJob");
          return null;
        }
      }
      catch (Exception ex)
      {
        Log.LogError("Failed to create file job: {0}", ex.Message);
      }
      return null;
    }

    public async Task<CheckFileJobStatusResult> CheckFileJobStatus(string jobId)
    {
      Log.LogDebug("CheckFileJobStatus: jobId={0}", jobId);
      try
      {
        CheckFileJobStatusParams statusParams = new CheckFileJobStatusParams
        {
          jobid = jobId
        };
        var statusResult = await ExecuteRequest<CheckFileJobStatusResult>(Ticket, "CheckFileJobStatus", statusParams);
        if (statusResult != null)
        {
          if (statusResult.success)
          {
            return statusResult;
          }
          CheckForInvalidTicket(statusResult, "CheckFileJobStatus");
          return null;
        }
      }
      catch (Exception ex)
      {
        Log.LogError("Failed to check file job status: {0}", ex.Message);
      }
      return null;
    }

    public async Task<GetFileJobResultResult> GetFileJobResult(string fileId)
    {
      Log.LogDebug("GetFileJobResult: fileId={0}", fileId);
      try
      {
        GetFileJobResultParams resultParams = new GetFileJobResultParams
        {
          fileid = fileId
        };
        var resultResult = await ExecuteRequest<GetFileJobResultResult>(Ticket, "GetFileJobResult", resultParams);
        //TODO: Check if graceful request works here. It's a stream of bytes returned which we want to process as text
        //(see ApiCallBase.ProcessResponseAsText)
        return resultResult;

      }
      catch (Exception ex)
      {
        Log.LogError("Failed to get file job result: {0}", ex.Message);
      }
      return null;
    }

    public async Task<string> ExportToWebFormat(string srcFilespaceId, string srcPath,
      string dstFilespaceId, string dstPath, int zoomLevel)
    {
      Log.LogDebug("ExportToWebFormat: srcFilespaceId={0}, srcPath={1}, dstFilespaceId={2}, dstPath={3}, zoomLevel={4}",
        srcFilespaceId, srcPath, dstFilespaceId, dstPath, zoomLevel);
      try
      {
        ExportToWebFormatParams exportParams = new ExportToWebFormatParams
        {
          sourcefilespaceid = srcFilespaceId,
          sourcepath = srcPath,
          destfilespaceid = dstFilespaceId,
          destpath = dstPath,
          format = "GoogleMaps",
          numzoomlevels = 1,
          maxzoomlevel = zoomLevel,
          imageformat = "png"
        };
        var exportResult = await ExecuteRequest<ExportToWebFormatResult>(Ticket, "ExportToWebFormat", exportParams);
        if (exportResult != null)
        {
          if (exportResult.success)
          {
            return exportResult.jobId;
          }
          CheckForInvalidTicket(exportResult, "ExportToWebFormat");
          return null;
        }
      }
      catch (Exception ex)
      {
        Log.LogError("Failed to export to web format: {0}", ex.Message);
      }
      return null;
    }

    public async Task<string> CheckExportJob(string jobId)
    {
      Log.LogDebug("CheckExportJob: jobId={0}", jobId);
      try
      {
        CheckExportJobParams checkParams = new CheckExportJobParams
        {
          jobid = jobId
        };
        var checkResult = await ExecuteRequest<CheckExportJobResult>(Ticket, "CheckExportJob", checkParams);
        if (checkResult != null)
        {
          if (checkResult.success)
          {
            return checkResult.status;
          }
          CheckForInvalidTicket(checkResult, "CheckExportJob");
          return null;
        }
      }
      catch (Exception ex)
      {
        Log.LogError("Failed to check export job status: {0}", ex.Message);
      }
      return null;
    }

    #endregion
  }
}
