using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using log4net;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using RestSharp;
using TagFileHarvester.Interfaces;
using TagFileHarvester.Models;
using VSS.ConfigurationStore;

namespace TagFileHarvester.Implementation
{
  public static class DictionaryExtensions
  {
    public static void RemoveAll<TKey, TValue>(this Dictionary<TKey, TValue> dic,
      Func<TValue, bool> predicate)
    {
      var keys = dic.Keys.Where(k => predicate(dic[k])).ToList();
      foreach (var key in keys) dic.Remove(key);
    }
  }

  public class FileRepository : IFileRepository
  {
    private static readonly GenericConfiguration config = new GenericConfiguration(new NullLoggerFactory());
    public static ILog Log;
    private static string tccSynchFolder;
    private static readonly string tccBaseUrl = config.GetValueString("TCCBaseURL");
    private static readonly string tccUserName = config.GetValueString("TCCUserName");
    private static readonly string tccPassword = config.GetValueString("TCCPassword");
    private static readonly string tccOrganization = config.GetValueString("TCCOrganization");
    private static readonly string INVALID_TICKET_ERRORID = "NOT_AUTHENTICATED";
    private static readonly string FILE_NOT_FOUND = "WRONG_PATH";
    private static readonly string INVALID_TICKET_MESSAGE = "You have not authenticated, use login action";
    private static readonly object ticketLock = new object();
    private static string ticket = string.Empty;

    private static readonly ConcurrentDictionary<string, List<TagFile>> cache =
      new ConcurrentDictionary<string, List<TagFile>>();

    public FileRepository()
    {
      tccSynchFolder = string.Format("{0}/{1}", OrgsHandler.tccSynchMachineFolder,
        OrgsHandler.TCCSynchProductionDataFolder);
    }

    private string Ticket
    {
      get
      {
        lock (ticketLock)
        {
          if (string.IsNullOrEmpty(ticket)) Login();
          return ticket;
        }
      }
      set
      {
        lock (ticketLock)
        {
          ticket = value;
        }
      }
    }

    public bool IsAnythingInCahe(Organization org)
    {
      var keys = cache.Keys.Where(k => k.Contains(org.shortName)).ToList();
      foreach (var key in keys)
        if (cache[key].Any())
          return true;
      return false;
    }

    public void CleanCache(Organization org)
    {
      Log.DebugFormat("Cleaning cache for org {0}", org.shortName);
      var keys = cache.Keys.Where(k => k.Contains(org.shortName)).ToList();
      foreach (var key in keys) cache[key].Clear();
    }

    public List<Organization> ListOrganizations()
    {
      Log.Debug("ListOrganizations");
      List<Organization> orgs = null;
      try
      {
        var fileSpaceParams = new GetFileSpacesParams { filter = "otherorgs" };
        var result = ExecuteRequest(Ticket, Method.GET, "/tcc/GetFileSpaces", fileSpaceParams,
          typeof(GetFileSpacesResult));
        var filespacesResult = result as GetFileSpacesResult;
        if (filespacesResult != null)
        {
          if (filespacesResult.success)
          {
            if (filespacesResult.filespaces != null)
            {
              //Only return orgs with Trimble Synchronizer Data folders
              var synchOrgFilespaces = (from f in filespacesResult.filespaces
                                        where string.Compare(f.shortname, OrgsHandler.tccSynchFilespaceShortName, true) == 0
                                        select f).ToList();
              if (synchOrgFilespaces.Count > 0)
                orgs =
                  synchOrgFilespaces.Select(
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
            else
            {
              Log.Warn("No organizations returned from ListOrganizations");
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
          Log.Error("Null result from ListOrganizations");
        }
      }
      catch (Exception ex)
      {
        Log.Error("Failed to get list of TCC organizations", ex);
      }

      return orgs;
    }

    public List<string> ListFolders(Organization org, out bool fromCache)
    {
      Log.DebugFormat("ListFolders: org={0} {1}", org.shortName, org.filespaceId);
      //Get top level list of folders from root
      if (IsAnythingInCahe(org))
      {
        Log.DebugFormat("ListFolders: Found something in cache for org {0}, building folder list from it",
          org.shortName);
        var keys = cache.Keys.Where(k => k.Contains(org.shortName + "$")).ToList();
        fromCache = true;
        return keys.Select(key => key.Substring(key.LastIndexOf('$') + 1)).ToList();
      }

      fromCache = false;
      return GetFolders(org, "/");
    }


    public void RemoveObsoleteFilesFromCache(Organization org, List<TagFile> files)
    {
      Log.DebugFormat(
        "Removing obsolete {0} files from cache for org {1}", files.Count, org.shortName);
      var keys = cache.Keys.Where(k => k.Contains(org.shortName + "$")).ToList();
      foreach (var key in keys)
        cache[key].RemoveAll(t => files.Any(f => f.fullName == t.fullName && f.createdUTC == t.createdUTC));
    }


    public List<TagFile> ListFiles(Organization org, string path)
    {
      Log.DebugFormat("ListFiles: org={0} {1}, path={2}", org.shortName, org.filespaceId, path);

      //The tag files are located in a directory structure of the form: /<machine name>/Machine Control Data/.Production-Data/<subfolder>/*.tag
      //If a machine is in a workgroup then folder structure is: /<workgroup name>/<machine name>/Machine Control Data/.Production-Data/<subfolder>/*.tag

      //Avoid big recursive search which is very slow in TCC. Look for the TCC synch folder which will be 
      //directly under the given folder if it is a machine folder or two levels down for work group.
      List<TagFile> files = null;

      Log.DebugFormat("Called by {0}; Cache contens is {1}", org.shortName,
        cache.Select(i => i.Key + " : " + i.Value.Count.ToString() + " ")
          .DefaultIfEmpty(" ")
          .Aggregate((prev, next) => prev + next));

      //If we have something in cache - retrieve it
      if (cache.ContainsKey(GetCacheKey(org, path)))
        if (cache[GetCacheKey(org, path)].Any())
        {
          var cachedFiles = cache[GetCacheKey(org, path)];
          Log.DebugFormat("ListFiles: Found {0} number of files in cache. Sending them back", cachedFiles.Count());
          //Remove obsolete files

          return cachedFiles.OrderBy(f => f.createdUTC).ToList();
        }

      try
      {
        //First see if synch folder exists at machine level
        var syncFolder = string.Format("{0}/{1}", path, tccSynchFolder);
        if (FolderExists(org.filespaceId, syncFolder))
        {
          files = GetFiles(org, syncFolder);
        }
        else
        {
          Log.DebugFormat("Folders {0} does not exists for org {1}, executing recursive search", syncFolder,
            org.shortName);
          //Get next level of folders and repeat to see if it's a work group
          var folders = GetFolders(org, path);
          if (folders == null)
          {
            Log.DebugFormat("No folders returned for org {0} path {1}", org.shortName, path);
            return new List<TagFile>();
          }

          foreach (var folder in folders)
          {
            syncFolder = string.Format("{0}{1}/{2}", path, folder, tccSynchFolder);
            if (FolderExists(org.filespaceId, syncFolder))
            {
              if (files == null)
                files = GetFiles(org, syncFolder);
              else
                files.AddRange(GetFiles(org, syncFolder));
              Log.InfoFormat("Got files in folder {0} for org {1}. Total file number: {2}", syncFolder, org.shortName,
                files.Count);
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.ErrorFormat("Failed to get list of TCC tag files for path {0}: {1}", path, ex.Message);
        return null;
      }
      //Order files by date and return a maximum of maxNumberFiles

      if (files == null)
      {
        Log.DebugFormat("No files returned for org {0} path {1}", org.shortName, path);
        return new List<TagFile>();
      }

      if (OrgsHandler.CacheEnabled)
      {
        //Update cache here
        if (!cache.ContainsKey(GetCacheKey(org, path)))
          cache.GetOrAdd(GetCacheKey(org, path), new List<TagFile>());


        //Add files to cache
        Log.DebugFormat(
          "Adding new {0} files to cache", files.Count());
        cache[GetCacheKey(org, path)].AddRange(files);
      }


      return files;
    }


    public Stream GetFile(Organization org, string fullName)
    {
      Log.DebugFormat("GetFile: org={0} {1}, fullName={2}", org.shortName, org.filespaceId, fullName);
      try
      {
        var getFileParams = new GetFileParams
        {
          filespaceid = org.filespaceId,
          path = fullName
        };
        var result = ExecuteRequest(Ticket, Method.GET, "/tcc/GetFile", getFileParams, typeof(ApiResult), true);
        if (result != null)
        {
          //If it has failed we get an error id and message
          if (result is ApiResult)
            CheckForInvalidTicket(result as ApiResult, "GetFile");
          else
            return new MemoryStream(result);
        }
        else
        {
          Log.ErrorFormat("Null result from GetFile for org {0} file {1}", org.shortName, fullName);
        }
      }
      catch (Exception ex)
      {
        Log.Error("Failed to get TCC file", ex);
      }

      return null;
    }

    public bool MoveFile(Organization org, string srcFullName, string dstFullName)
    {
      Log.DebugFormat("MoveFile: org={0} {1}, srcFullName={2}, dstFullName={3}", org.shortName, org.filespaceId,
        srcFullName, dstFullName);
      try
      {
        if (!FolderExists(org.filespaceId, dstFullName.Substring(0, dstFullName.LastIndexOf("/")) + "/"))
        {
          var mkdirParams = new MkDir
          {
            filespaceid = org.filespaceId,
            force = true,
            path = dstFullName.Substring(0, dstFullName.LastIndexOf("/")) + "/"
          };
          var resultCreate = ExecuteRequest(Ticket, Method.GET, "/tcc/MkDir", mkdirParams, typeof(RenResult));
          if (resultCreate == null)
          {
            Log.ErrorFormat("Can not create folder for org {0} folder {1}", org.shortName,
              dstFullName.Substring(0, dstFullName.LastIndexOf("/")) + "/");
            return false;
          }
        }

        var renParams = new RenParams
        {
          filespaceid = org.filespaceId,
          path = srcFullName,
          newfilespaceid = org.filespaceId,
          newPath = dstFullName,
          merge = false,
          replace = true
        };
        var result = ExecuteRequest(Ticket, Method.GET, "/tcc/Ren", renParams, typeof(RenResult));
        var renResult = result as RenResult;
        if (renResult != null)
        {
          if (renResult.success || renResult.errorid.Contains("INVALID_OPERATION_FILE_IS_LOCKED")) return true;
          CheckForInvalidTicket(renResult, "MoveFile");
        }
        else
        {
          Log.ErrorFormat("Null result from MoveFile for org {0} file {1}", org.shortName, srcFullName);
        }
      }
      catch (Exception ex)
      {
        Log.ErrorFormat("Failed to move TCC file for org {0} file {1}: {2}", org.shortName, srcFullName, ex.Message);
      }

      return false;
    }


    private string GetCacheKey(Organization org, string path)
    {
      return string.Format("{0}${1}", org.shortName, path);
    }

    private List<string> GetFolders(Organization org, string path)
    {
      Log.DebugFormat("GetFolders: org={0} {1}, path={2}", org.shortName, org.filespaceId, path);
      List<string> folders = null;
      try
      {
        //Get list of folders one level down from path
        var dirParams = new DirParams
        {
          filespaceid = org.filespaceId,
          path = path,
          recursive = false,
          filterfolders = true,
          filemasklist = "*.tag"
        };
        var result = ExecuteRequest(Ticket, Method.GET, "/tcc/Dir", dirParams, typeof(DirResult));
        var dirResult = result as DirResult;
        if (dirResult != null)
        {
          if (dirResult.success)
          {
            if (dirResult.entries != null)
            {
              var folderEntries = (from d in dirResult.entries
                                   where d.isFolder && !d.leaf
                                   select d).ToList();

              folders = new List<string>();

              foreach (var folderEntry in folderEntries)
              {
                string lPath;
                if (path == "/")
                  lPath = string.Format("{0}{1}", path, folderEntry.entryName);
                else
                  lPath = string.Format("{0}/{1}", path, folderEntry.entryName);

                var lastChanged = GetLastChangedTime(org.filespaceId, lPath);

                if (OrgsHandler.FilenameDumpEnabled)
                  Log.DebugFormat("Dumping Dir {0} : {1} : {2}", org.filespaceId, lPath, lastChanged);

                folders.Add($"/{folderEntry.entryName}");
              }

              if (!folders.Any()) Log.WarnFormat("No folders found for org {0}", org.shortName);
            }
            else
            {
              Log.WarnFormat("No folders returned from GetFolders for org {0}", org.shortName);
              folders = new List<string>();
            }
          }
          else
          {
            CheckForInvalidTicket(dirResult, "GetFolders");
          }
        }
        else
        {
          Log.ErrorFormat("Null result from GetFolders for org {0}", org.shortName);
        }
      }
      catch (Exception ex)
      {
        Log.ErrorFormat("Failed to get list of TCC folders: {0}", ex.Message);
      }

      return folders;
    }

    private ApiResult DeleteFolder(Organization org, string folderPath)
    {
      var delParams = new DelParams
      {
        filespaceid = org.filespaceId,
        path = folderPath,
        recursive = "no"
      };

      return ExecuteRequest(Ticket, Method.DELETE, "/tcc/Del", delParams, typeof(ApiResult));
    }

    private DateTime GetLastChangedTime(string filespaceId, string path)
    {
      Log.DebugFormat("GetLastChangedTime: filespaceId={0}, path={1}", filespaceId, path);

      try
      {
        var lastDirChangeParams = new LastDirChangeParams
        {
          filespaceid = filespaceId,
          path = path,
          recursive = true
        };
        var result = ExecuteRequest(Ticket, Method.GET, "/tcc/LastDirChange", lastDirChangeParams,
          typeof(LastDirChangeResult));
        var lastDirChangeResult = result as LastDirChangeResult;
        if (lastDirChangeResult != null)
        {
          if (lastDirChangeResult.success) return lastDirChangeResult.lastUpdatedDateTime;
          CheckForInvalidTicket(lastDirChangeResult, "GetLastChangedTime");
        }
        else
        {
          Log.ErrorFormat("Null result from GetLastChangedTime for filespaceId={0}, path={1}", filespaceId, path);
        }
      }
      catch (Exception ex)
      {
        Log.ErrorFormat("Failed to get last time tag files added to folder: {0}", ex.Message);
      }

      return DateTime.MinValue;
    }

    private List<TagFile> GetFiles(Organization org, string syncFolder)
    {
      Log.DebugFormat("Found synch folder for {0}", syncFolder);

      // Check whether the sync folder has any empty tag files folders and delete them 
      // if their life length exceeds the life span set in the configuration file.
      CheckForEmptyTagFileFolders(org, syncFolder);

      var dirParams = new DirParams
      {
        filespaceid = org.filespaceId,
        path = syncFolder,
        recursive = true,
        filterfolders = true,
        filemasklist = "*.tag"
      };
      var result = ExecuteRequest(Ticket, Method.GET, "/tcc/Dir", dirParams, typeof(DirResult));
      var dirResult = result as DirResult;
      if (dirResult != null)
      {
        if (dirResult.success)
        {
          var files = new List<TagFile>();
          GetTagFiles(dirResult, files, syncFolder);
          return files;
        }

        CheckForInvalidTicket(dirResult, "GetFiles");
      }
      else
      {
        Log.ErrorFormat("Null result from GetFiles for {0}", syncFolder);
      }

      return null;
    }

    private void CheckForEmptyTagFileFolders(Organization org, string path)
    {
      Log.DebugFormat("CheckForEmptyTagFileFolders: org={0} {1}, rootFolder={2}", org.shortName, org.filespaceId, path);

      List<string> folders = null;

      try
      {
        //Get list of folders one level down from path
        var dirParams = new DirParams
        {
          filespaceid = org.filespaceId,
          path = path,
          recursive = false,
          filterfolders = false
        };

        var result = ExecuteRequest(Ticket, Method.GET, "/tcc/Dir", dirParams, typeof(DirResult));
        var dirResult = result as DirResult;

        if (dirResult != null)
        {
          if (dirResult.success)
          {
            if (dirResult.entries != null)
            {
              var folderEntries = (from d in dirResult.entries
                                   where d.isFolder && d.leaf
                                   select d).ToList();

              folders = new List<string>();

              foreach (var folderEntry in folderEntries) folders.Add($"/{folderEntry.entryName}");

              if (!folders.Any())
                Log.WarnFormat("No tag file folders found for org {0}, root folder={1}", org.shortName, path);
            }
            else
            {
              Log.WarnFormat(
                "No tag file folders returned from CheckForEmptyTagFileFolders for org {0}, root folder={1}",
                org.shortName, path);
              folders = new List<string>();
            }
          }
          else
          {
            CheckForInvalidTicket(dirResult, "CheckForEmptyTagFileFolders");
          }
        }
        else
        {
          Log.ErrorFormat(
            "Null result from CheckForEmptyTagFileFolders on getting tag file folders for org {0}, root folder={1}",
            org.shortName, path);
        }
      }
      catch (Exception ex)
      {
        Log.ErrorFormat("Failed to get list of TCC tag files folders: {0}, root folder={1}", ex.Message, path);
      }

      if (folders == null)
        return;

      foreach (var folder in folders)
      {
        var lPath = $"{path}{folder}";

        var lastChanged = GetLastChangedTime(org.filespaceId, lPath);

        Log.InfoFormat("Checking folder {0} for deletion. Last changed date={1}, today={2}, folder's age={3} days",
          lPath, lastChanged, DateTime.UtcNow, (DateTime.UtcNow - lastChanged).Days);

        if ((DateTime.UtcNow - lastChanged).Days >= OrgsHandler.TagFilesFolderLifeSpanInDays)
          if (ListFiles(org, lPath).ToList().Count == 0)
          {
            Log.InfoFormat("Deleting folder {0}", lPath);

            var delResult = DeleteFolder(org, lPath);
            if (!delResult.success) CheckForInvalidTicket(delResult, "DeleteFolder");
          }
      }
    }

    private bool FolderExists(string filespaceId, string folder)
    {
      Log.DebugFormat("Searching for folder {0}", folder);
      try
      {
        var getFileAttrParams = new GetFileAttributesParams
        {
          filespaceid = filespaceId,
          path = folder
        };
        var result = ExecuteRequest(Ticket, Method.GET, "/tcc/GetFileAttributes", getFileAttrParams,
          typeof(GetFileAttributesResult));
        var getFileAttrResult = result as GetFileAttributesResult;
        if (getFileAttrResult != null)
        {
          if (getFileAttrResult.success) return true;
          CheckForInvalidTicket(getFileAttrResult, "FolderExists");
          return getFileAttrResult.success;
        }
      }
      catch (Exception ex)
      {
        Log.ErrorFormat("Failed to get TCC file attributes: {0}", ex.Message);
      }

      return false;
    }

    private void GetTagFiles(DirResult entry, List<TagFile> files, string path)
    {
      if (entry.isFolder)
      {
        if (!entry.leaf && entry.entries != null)
          foreach (var e in entry.entries)
            GetTagFiles(e, files, path);
      }
      else
      {
        if (OrgsHandler.FilenameDumpEnabled) Log.DebugFormat("Dumping {0} : {1}", entry.entryName, entry.createTime);

        if (entry.leaf)
          files.Add(new TagFile
          {
            fullName = $"{path}/{entry.entryName}",
            createdUTC = entry.createTime
          });
      }
    }

    private void Login()
    {
      Log.InfoFormat("Logging in to TCC: user={0}, org={1}", tccUserName, tccOrganization);
      try
      {
        var loginParams = new LoginParams
        {
          username = tccUserName,
          orgname = tccOrganization,
          password = tccPassword,
          mode = "noredirect",
          forcegmt = true
        };
        var result = ExecuteRequest(ticket, Method.POST, "/tcc/Login", loginParams, typeof(LoginResult));
        var loginResult = result as LoginResult;
        if (loginResult != null)
        {
          if (loginResult.success)
            ticket = loginResult.ticket;
          else
            Log.ErrorFormat("Failed to login to TCC: errorId={0}, reason={1}", loginResult.errorid, loginResult.reason);
        }
        else
        {
          Log.Error("Null result from Login");
        }
      }
      catch (Exception ex)
      {
        Log.ErrorFormat("Failed to login to TCC: {0}", ex.Message);
      }
    }

    private void CheckForInvalidTicket(ApiResult result, string what)
    {
      //Check for expired/invalid ticket
      if (!result.success)
      {
        if (result.errorid == INVALID_TICKET_ERRORID && result.message == INVALID_TICKET_MESSAGE)
          Ticket = null;
        else
          Log.WarnFormat("{0} failed: errorid={1}, message={2}", what, result.errorid, result.message);
      }
    }

    private dynamic ExecuteRequest(string token, Method method, string contractPath, object requestData,
      Type expectedResultType, bool returnRaw = false)
    {
      if (string.IsNullOrEmpty(tccBaseUrl))
        throw new Exception("Configuration Error - no TCC url specified");

      var client = new RestClient(tccBaseUrl);
      client.Timeout = (int)OrgsHandler.TCCRequestTimeout.TotalMilliseconds;
      client.ReadWriteTimeout = (int)OrgsHandler.TCCRequestTimeout.TotalMilliseconds;
      var request = new RestRequest(contractPath, method);
      request.RequestFormat = DataFormat.Json;
      request.AddHeader("Accept", returnRaw ? "application/octet-stream" : "application/json");
      if (!string.IsNullOrEmpty(ticket))
        request.AddQueryParameter("Ticket", token);

      var properties = from p in requestData.GetType().GetFields()
                       where p.GetValue(requestData) != null
                       select new { p.Name, Value = p.GetValue(requestData) };
      foreach (var p in properties)
        if (method == Method.GET)
          request.AddQueryParameter(p.Name, p.Value.ToString());
        else
          request.AddParameter(p.Name, p.Value, ParameterType.GetOrPost);

      IRestResponse response = null;

      var timestamp1 = DateTime.UtcNow;
      var reqTask = client.ExecuteTaskAsync(request);
      if (!reqTask.Wait(OrgsHandler.TCCRequestTimeout))
      {
        Log.WarnFormat("TCC Request ran out of time for completion for request {0}", contractPath);
        return null;
      }

      response = reqTask.Result;
      var timestamp2 = DateTime.UtcNow;

      if (response.ResponseStatus == ResponseStatus.Completed)
      {
        Log.DebugFormat("Response Status: StatusCode={0}, StatusDescription={1}, data size={2} at {3:0.00} KB/sec",
          response.StatusCode, response.StatusDescription, response.Content.Length,
          response.Content.Length / 1024.0 / (timestamp2 - timestamp1).TotalMilliseconds * 1000.0);

        if (returnRaw && response.ContentType == "application/octet-stream")
          return response.RawBytes;
        dynamic result = JsonConvert.DeserializeObject(response.Content, expectedResultType);
        if (result == null)
          Log.WarnFormat("Can not execute request TCC response. Details: {0}", response.ErrorMessage);
        return result;
      }

      Log.ErrorFormat(
        "Failed to get response from TCC: ResponseStatus={0}, StatusCode={1}, StatusDescription={2}, ErrorMessage={3}",
        response.ResponseStatus, response.StatusCode, response.StatusDescription, response.ErrorMessage);
      return null;
    }

    public class TagFile
    {
      public DateTime createdUTC;
      public string fullName;
    }
  }
}