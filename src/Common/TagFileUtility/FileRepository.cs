using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net;
using Newtonsoft.Json;
using RestSharp;
using TagFileHarvester.Models;
using TagFileUtility.Models;

namespace TagFileUtility
{

  public class FileRepository 
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private static string tccSynchFolder;  
    private static readonly string tccBaseUrl = ConfigurationManager.AppSettings["TCCBaseURL"];
    private readonly string tccUserName;// = ConfigurationManager.AppSettings["TCCUserName"];
    private readonly string tccPassword;// = ConfigurationManager.AppSettings["TCCPassword"];
    private readonly string tccOrganization;// = ConfigurationManager.AppSettings["TCCOrganization"];
    private static readonly string INVALID_TICKET_ERRORID = "NOT_AUTHENTICATED";
    private static readonly string FILE_NOT_FOUND = "WRONG_PATH";
    private static readonly string INVALID_TICKET_MESSAGE = "You have not authenticated, use login action";
    private static readonly object ticketLock = new object();
    private static string ticket = String.Empty;

    private static readonly string tccSynchMachineFolder = ConfigurationManager.AppSettings["TCCSynchMachineControlFolder"];
    private static readonly string tccSynchProductionDataFolder = ConfigurationManager.AppSettings["TCCSynchProductionDataFolder"];
    private static readonly string tccSynchFilespaceShortName = ConfigurationManager.AppSettings["TCCSynchFilespaceShortName"];
    private static readonly TimeSpan tccRequestTimeout = TimeSpan.Parse(ConfigurationManager.AppSettings["TCCRequestTimeout"]);

    public FileRepository(string tccUserName, string tccOrganization, string tccPassword)
    {
      this.tccUserName = tccUserName;
      this.tccOrganization = tccOrganization;
      this.tccPassword = tccPassword;

      tccSynchFolder = string.Format("{0}/{1}", tccSynchMachineFolder, tccSynchProductionDataFolder);
    }

    private string Ticket
    {
      get
      {
        lock (ticketLock)
        {
          if (string.IsNullOrEmpty(ticket))
          {
            Login();
          }
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

    public List<Organization> ListOrganizations()
    {
      Log.Debug("ListOrganizations");
      List<Organization> orgs = null;
      try
      { 
        GetFileSpacesParams fileSpaceParams = new GetFileSpacesParams {filter = "myorg"};//NOTE: Different to TagFileHarvester since logged in as org user 
        var result = ExecuteRequest(Ticket, Method.GET, "/tcc/GetFileSpaces", fileSpaceParams,
            typeof (GetFileSpacesResult));
        GetFileSpacesResult filespacesResult = result as GetFileSpacesResult;
        if (filespacesResult != null)
        {
          if (filespacesResult.success)
          {
            if (filespacesResult.filespaces != null)
            {
              //Only return orgs with Trimble Synchronizer Data folders
              var synchOrgFilespaces = (from f in filespacesResult.filespaces
                                        where String.Compare(f.shortname, tccSynchFilespaceShortName, true) == 0
                                        select f).ToList();
              if (synchOrgFilespaces.Count > 0)
              {
                orgs =
                    synchOrgFilespaces.Select(
                        filespace =>
                            new Organization {filespaceId = filespace.filespaceId, shortName = filespace.orgShortname, orgId = filespace.orgId, orgDisplayName = filespace.orgDisplayName, orgTitle = filespace.shortname})
                        .ToList();
                }
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

    public List<string> ListFolders(Organization org)
    {
      Log.DebugFormat("ListFolders: org={0} {1}", org.shortName, org.filespaceId);
      //Get top level list of folders from root
      return GetFolders(org, "/");
    }

    public List<TagFile> ListFiles(Organization org, string path)
    {
      Log.DebugFormat("ListFiles: org={0} {1}, path={2}", org.shortName, org.filespaceId, path);

      //The tag files are located in a directory structure of the form: /<machine name>/Machine Control Data/.Production-Data/<subfolder>/*.tag
      //If a machine is in a workgroup then folder structure is: /<workgroup name>/<machine name>/Machine Control Data/.Production-Data/<subfolder>/*.tag

      //Avoid big recursive search which is very slow in TCC. Look for the TCC synch folder which will be 
      //directly under the given folder if it is a machine folder or two levels down for work group.
      List<TagFile> files = null;

      Log.DebugFormat("Called by {0}", org.shortName);
      try
      {
        //First see if synch folder exists at machine level
        string syncFolder = string.Format("{0}/{1}", path, tccSynchFolder);
        if (FolderExists(org.filespaceId, syncFolder))
        {
          files = GetFiles(org, syncFolder);
        }
        else
        {
          Log.DebugFormat("Folders {0} does not exists for org {1}, executing recursive search", syncFolder, org.shortName);
          //Get next level of folders and repeat to see if it's a work group
          List<string> folders = GetFolders(org, path);
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
              Log.InfoFormat("Got files in folder {0} for org {1}. Total file number: {2}", syncFolder, org.shortName,files.Count);
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.ErrorFormat("Failed to get list of TCC tag files for path {0}: {1}", path,ex.Message);
        return null;
      }
      //Order files by date and return a maximum of maxNumberFiles

      if (files == null)
      {
        Log.DebugFormat("No files returned for org {0} path {1}", org.shortName, path);
        return new List<TagFile>();
      }
      return files;
    }

    public Stream GetFile(Organization org, string fullName)
    {
      Log.DebugFormat("GetFile: org={0} {1}, fullName={2}", org.shortName, org.filespaceId, fullName);
      try
      {
        GetFileParams getFileParams = new GetFileParams
                                      {
                                          filespaceid = org.filespaceId,
                                          path = fullName
                                      };
        var result = ExecuteRequest(Ticket, Method.GET, "/tcc/GetFile", getFileParams, typeof (ApiResult), true);
        if (result != null)
        {
          //If it has failed we get an error id and message
          if (result is ApiResult)
          {
            CheckForInvalidTicket(result as ApiResult, "GetFile");
          }
          else
          {
            //File contents as bytes
            return new MemoryStream(result);
          }
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

    public bool CancelCheckout(Organization org, string fullName)
    {
      Log.DebugFormat("CancelCheckout: org={0} {1}, fullName={2}", org.shortName, org.filespaceId, fullName);

      try
      {
        CancelCheckoutParams ccParams = new CancelCheckoutParams
        {
          filespaceid = org.filespaceId,
          path = fullName
        };
        var result = ExecuteRequest(Ticket, Method.GET, "/tcc/CancelCheckout", ccParams, typeof(CancelCheckoutResult));
        CancelCheckoutResult ccResult = result as CancelCheckoutResult;
        Log.Debug($"Result: {JsonConvert.SerializeObject(ccResult)}");
        if (ccResult != null && !ccResult.success &&
            ccResult.errorid.Contains("WORKING_COPY_OF_CHECKED_OUT_FILE_IS_NOT_FOUND"))
        {
          var editCopyName = $"{fullName.Substring(0, fullName.Length - 4)}-EditCopy.tag";
          var editCopyCreated = CopyFile(org, fullName, editCopyName);
          if (editCopyCreated)
          {
            result = ExecuteRequest(Ticket, Method.GET, "/tcc/CancelCheckout", ccParams, typeof(CancelCheckoutResult));
            ccResult = result as CancelCheckoutResult;
          }
        }
        if (ccResult != null)
        {
          if (ccResult.success || ccResult.errorid.Contains("NOT_CHECKED_OUT"))
          {
            return true;
          }
          CheckForInvalidTicket(ccResult, "CancelCheckout");
        }
        else
        {
          Log.ErrorFormat("Null result from CancelCheckout for org {0} file {1}", org.shortName, fullName);
        }
      }
      catch (Exception ex)
      {
        Log.ErrorFormat("Failed to cancel checkout TCC file for org {0} file {1}: {2}", org.shortName, fullName,
          ex.Message);
      }
      return false;
    }

    public bool CopyFile(Organization org, string srcFullName, string dstFullName)
    {
      Log.DebugFormat("CopyFile: org={0} {1}, srcFullName={2}, dstFullName={3}", org.shortName, org.filespaceId,
        srcFullName, dstFullName);
      try
      {
        if (!FolderExists(org.filespaceId, dstFullName.Substring(0, dstFullName.LastIndexOf("/")) + "/"))
        {
          MkDir mkdirParams = new MkDir()
          {
            filespaceid = org.filespaceId,
            force = true,
            path = dstFullName.Substring(0, dstFullName.LastIndexOf("/")) + "/"
          };
          var resultCreate = ExecuteRequest(Ticket, Method.GET, "/tcc/MkDir", mkdirParams, typeof(ApiResult));
          if (resultCreate == null)
          {
            Log.ErrorFormat("Can not create folder for org {0} folder {1}", org.shortName, dstFullName.Substring(0, dstFullName.LastIndexOf("/")) + "/");
            return false;
          }
        }

        CopyParams copyParams = new CopyParams()
        {
          filespaceid = org.filespaceId,
          path = srcFullName,
          newfilespaceid = org.filespaceId,
          newPath = dstFullName,
          merge = false,
          replace = true
        };
        var result = ExecuteRequest(Ticket, Method.GET, "/tcc/Copy", copyParams, typeof(ApiResult));
        if (result != null)
        {
          if (result.success)
          {
            return true;
          }
          CheckForInvalidTicket(result, "CopyFile");
        }
        else
        {
          Log.ErrorFormat("Null result from CopyFile for org {0} file {1}", org.shortName, srcFullName);
        }
      }
      catch (Exception ex)
      {
        Log.ErrorFormat("Failed to copy TCC file for org {0} file {1}: {2}", org.shortName, srcFullName, ex.Message);
      }
      return false;
    }

    private List<string> GetFolders(Organization org, string path)
    {
      Log.DebugFormat("GetFolders: org={0} {1}, path={2}", org.shortName, org.filespaceId, path);
      List<string> folders = null;
      try
      {
        //Get list of folders one level down from path
        DirParams dirParams = new DirParams
        {
          filespaceid = org.filespaceId,
          path = path,
          recursive = false,
          filterfolders = true,
          filemasklist = "*.tag"
        };
        var result = ExecuteRequest(Ticket, Method.GET, "/tcc/Dir", dirParams, typeof (DirResult));
        DirResult dirResult = result as DirResult;
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

                folders.Add($"/{folderEntry.entryName}");
              }

              if (!folders.Any())
              {
                Log.WarnFormat("No folders found for org {0}", org.shortName);
              }
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

    private List<TagFile> GetFiles(Organization org, string syncFolder)
    {
      Log.DebugFormat("Found synch folder for {0}", syncFolder);

      DirParams dirParams = new DirParams
      {
        filespaceid = org.filespaceId,
        path = syncFolder,
        recursive = true,
        filterfolders = true,
        filemasklist = "*.tag"
      };
      var result = ExecuteRequest(Ticket, Method.GET, "/tcc/Dir", dirParams, typeof (DirResult));
      DirResult dirResult = result as DirResult;
      if (dirResult != null)
      {
        if (dirResult.success)
        {
          List<TagFile> files = new List<TagFile>();
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

    private bool FolderExists(string filespaceId, string folder)
    {
      Log.DebugFormat("Searching for folder {0}", folder);
      try
      {
        GetFileAttributesParams getFileAttrParams = new GetFileAttributesParams
                                                    {
                                                        filespaceid = filespaceId,
                                                        path = folder
                                                    };
        var result = ExecuteRequest(Ticket, Method.GET, "/tcc/GetFileAttributes", getFileAttrParams,
            typeof (GetFileAttributesResult));
        GetFileAttributesResult getFileAttrResult = result as GetFileAttributesResult;
        if (getFileAttrResult != null)
        {
          if (getFileAttrResult.success)
          {
            return true;
          }
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
        {
          foreach (var e in entry.entries)
          {
            GetTagFiles(e, files, path);
          }
        }
      }
      else
      {
        if (entry.leaf)
        {
          files.Add(new TagFile
          {
            fullName = $"{path}/{entry.entryName}",
            createdUTC = entry.createTime
          });
        }
      }
    }

    public void Login()
    {
      Log.InfoFormat("Logging in to TCC: user={0}, org={1}", tccUserName, tccOrganization);
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
        var result = ExecuteRequest(ticket, Method.POST, "/tcc/Login", loginParams, typeof (LoginResult));
        LoginResult loginResult = result as LoginResult;
        if (loginResult != null)
        {
          if (loginResult.success)
          {
            ticket = loginResult.ticket;
          }
          else
          {
            Log.ErrorFormat("Failed to login to TCC: errorId={0}, reason={1}", loginResult.errorid, loginResult.reason);
          }
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
        {
            Ticket = null;
        }
        else 
        {
          Log.WarnFormat("{0} failed: errorid={1}, message={2}", what, result.errorid, result.message);
        }
      }
    }

    private dynamic ExecuteRequest(string token, Method method, string contractPath, object requestData,
        Type expectedResultType, bool returnRaw = false)
    {
      if (String.IsNullOrEmpty(tccBaseUrl))
        throw new Exception("Configuration Error - no TCC url specified");

      RestClient client = new RestClient(tccBaseUrl);
      client.Timeout = (int) tccRequestTimeout.TotalMilliseconds;
      client.ReadWriteTimeout = (int) tccRequestTimeout.TotalMilliseconds;
      RestRequest request = new RestRequest(contractPath, method);
      request.RequestFormat = DataFormat.Json;
      request.AddHeader("Accept", returnRaw ? "application/octet-stream" : "application/json");
      if (!string.IsNullOrEmpty(ticket))
        request.AddQueryParameter("Ticket", token);

      var properties = from p in requestData.GetType().GetFields()
                       where p.GetValue(requestData) != null
                       select new {p.Name, Value = p.GetValue(requestData)};
      foreach (var p in properties)
      {
        if (method == Method.GET)
          request.AddQueryParameter(p.Name, p.Value.ToString());
        else
          request.AddParameter(p.Name, p.Value, ParameterType.GetOrPost);
      }

       IRestResponse response = null;
      
        var timestamp1 = DateTime.UtcNow;
        var reqTask = client.ExecuteTaskAsync(request);
        if (!reqTask.Wait(tccRequestTimeout))
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
            response.Content.Length/1024.0/(timestamp2 - timestamp1).TotalMilliseconds*1000.0);

        if (returnRaw && response.ContentType == "application/octet-stream")
          return response.RawBytes;
        dynamic result = JsonConvert.DeserializeObject(response.Content, expectedResultType);
        if (result == null)
          Log.WarnFormat("Can not execute request TCC response. Details: {0}", response.ErrorMessage);
        return result;
      }
      else
      {
        Log.ErrorFormat("Failed to get response from TCC: ResponseStatus={0}, StatusCode={1}, StatusDescription={2}, ErrorMessage={3}", 
          response.ResponseStatus, response.StatusCode, response.StatusDescription, response.ErrorMessage);
        return null;
      }
    }

    public class TagFile
    {
      public string fullName;
      public DateTime createdUTC;
    }
 
  }
}
