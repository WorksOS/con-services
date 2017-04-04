using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using TagFileHarvester.Models;
using VSS.GenericConfiguration;

namespace TCCFileAccess.Implementation
{

    /// <summary>
    /// File access class to talk to TCC. This class is thread safe 
    /// </summary>
    /// <seealso cref="TagFileHarvester.Interfaces.IFileRepository" />
    /// TODO add resiliency with Polly. Include CheckForInvalidTicket into resiliency logic.
    public class FileRepository : IFileRepository
    {
        public string tccBaseUrl { get; }
        public string tccUserName { get; }
        public string tccPassword { get; }
        public string tccOrganization { get; }

        private const string INVALID_TICKET_ERRORID = "NOT_AUTHENTICATED";
        private const string FILE_NOT_FOUND = "WRONG_PATH";
        private const string INVALID_TICKET_MESSAGE = "You have not authenticated, use login action";

        private readonly ILogger<FileRepository> Log;

        private string ticket = String.Empty;
        private string Ticket
        {
            get
            {
                if (string.IsNullOrEmpty(ticket))
                {
                    ticket = Login();
                }
                return ticket;
            }
            set { ticket = value; }
        }

        public FileRepository(IConfigurationStore configuration, ILoggerFactory logger)
        {
            tccBaseUrl = configuration.GetValueString("VSPDB");
            tccUserName = configuration.GetValueString("VSPDB");
            tccPassword = configuration.GetValueString("VSPDB");
            tccOrganization = configuration.GetValueString("VSPDB");
            Log = logger.CreateLogger<FileRepository>();
        }

        public List<Organization> ListOrganizations()
        {
            Log.LogDebug("ListOrganizations");
            List<Organization> orgs = null;
            try
            {
                GetFileSpacesParams fileSpaceParams = new GetFileSpacesParams {filter = "otherorgs"};
                var result = ExecuteRequest(Ticket, Method.GET, "/tcc/GetFileSpaces", fileSpaceParams,
                    typeof(GetFileSpacesResult));
                GetFileSpacesResult filespacesResult = result as GetFileSpacesResult;
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

        public List<string> ListFolders(Organization org, DateTime lastModifiedUTC)
        {
            Log.LogDebug("ListFolders: org={0} {1}, lastModfiedUTC={2}", org.shortName, org.filespaceId,
                lastModifiedUTC);
            return GetFolders(org, lastModifiedUTC, "/");
        }

        public List<TCCFile> ListFiles(Organization org, string path, DateTime createdAfterUTC)
        {
            Log.LogDebug("ListFiles: org={0} {1}, path={2}, createdAfterUTC={3}",
                org.shortName, org.filespaceId, path, createdAfterUTC);

            return GetFiles(org.filespaceId, path, createdAfterUTC);
        }


        /// <summary>
        /// Gets the file. The resulting stream should be disposed after read completed
        /// </summary>
        /// <param name="org">The org.</param>
        /// <param name="fullName">The full name.</param>
        /// <returns></returns>
        public Stream GetFile(Organization org, string fullName)
        {
            Log.LogDebug("GetFile: org={0} {1}, fullName={2}", org.shortName, org.filespaceId, fullName);
            try
            {
                GetFileParams getFileParams = new GetFileParams
                {
                    filespaceid = org.filespaceId,
                    path = fullName
                };
                var result = ExecuteRequest(Ticket, Method.GET, "/tcc/GetFile", getFileParams, typeof(ApiResult), true);
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
                    Log.LogError("Null result from GetFile for org {0} file {1}", org.shortName, fullName);
                }
            }
            catch (Exception ex)
            {
                Log.LogError("Failed to get TCC file", ex);
            }
            return null;
        }

        public bool MoveFile(Organization org, string srcFullName, string dstFullName)
        {
            Log.LogDebug("MoveFile: org={0} {1}, srcFullName={2}, dstFullName={3}", org.shortName, org.filespaceId,
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
                    var resultCreate = ExecuteRequest(Ticket, Method.GET, "/tcc/MkDir", mkdirParams, typeof(RenResult));
                    if (resultCreate == null)
                    {
                        Log.LogError("Can not create folder for org {0} folder {1}", org.shortName,
                            dstFullName.Substring(0, dstFullName.LastIndexOf("/")) + "/");
                        return false;
                    }
                }

                RenParams renParams = new RenParams
                {
                    filespaceid = org.filespaceId,
                    path = srcFullName,
                    newfilespaceid = org.filespaceId,
                    newPath = dstFullName,
                    merge = false,
                    replace = true
                };
                var result = ExecuteRequest(Ticket, Method.GET, "/tcc/Ren", renParams, typeof(RenResult));
                RenResult renResult = result as RenResult;
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

        public List<string> GetFolders(Organization org, DateTime lastModifiedUTC, string path)
        {
            Log.LogDebug("GetFolders: org={0} {1}, lastModfiedUTC={2}, path={3}", org.shortName, org.filespaceId,
                lastModifiedUTC, path);
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
                    filemasklist = "*.*"
                };
                var result = ExecuteRequest(Ticket, Method.GET, "/tcc/Dir", dirParams, typeof(DirResult));
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

                                DateTime lastChanged = GetLastChangedTime(org.filespaceId, lPath);


                                if (lastModifiedUTC == DateTime.MinValue)
                                {
                                    folders.Add(string.Format("/{0}", folderEntry.entryName));
                                }
                                else if (lastChanged > lastModifiedUTC)
                                {
                                    folders.Add(string.Format("/{0}", folderEntry.entryName));
                                }

                            }

                            if (!folders.Any())
                            {
                                Log.LogWarning("No folders modified after {0} found for org {1}", lastModifiedUTC,
                                    org.shortName);
                            }
                        }
                        else
                        {
                            Log.LogWarning("No folders returned from GetFolders for org {0}", org.shortName);
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
                    Log.LogError("Null result from GetFolders for org {0}", org.shortName);
                }
            }
            catch (Exception ex)
            {
                Log.LogError("Failed to get list of TCC folders: {0}", ex.Message);
            }
            return folders;
        }

        public DateTime GetLastChangedTime(string filespaceId, string path)
        {
            Log.LogDebug("GetLastChangedTime: filespaceId={0}, path={1}", filespaceId, path);

            try
            {
                LastDirChangeParams lastDirChangeParams = new LastDirChangeParams
                {
                    filespaceid = filespaceId,
                    path = path,
                    recursive = true
                };
                var result = ExecuteRequest(Ticket, Method.GET, "/tcc/LastDirChange", lastDirChangeParams,
                    typeof(LastDirChangeResult));
                LastDirChangeResult lastDirChangeResult = result as LastDirChangeResult;
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

        public List<TCCFile> GetFiles(string filespaceId, string syncFolder, DateTime createdAfterUTC)
        {
            Log.LogDebug("Found synch folder for {0}", syncFolder);
            DirParams dirParams = new DirParams
            {
                filespaceid = filespaceId,
                path = syncFolder,
                recursive = true,
                filterfolders = true,
                filemasklist = "*.tag"
            };
            var result = ExecuteRequest(Ticket, Method.GET, "/tcc/Dir", dirParams, typeof(DirResult));
            DirResult dirResult = result as DirResult;
            if (dirResult != null)
            {
                if (dirResult.success)
                {
                    List<TCCFile> files = new List<TCCFile>();
                    ListFiles(dirResult, files, createdAfterUTC, syncFolder);
                    return files;
                }
                CheckForInvalidTicket(dirResult, "GetFiles");
            }
            else
            {
                Log.LogError("Null result from GetFiles for {0}", syncFolder);
            }
            return null;
        }

        public bool FolderExists(string filespaceId, string folder)
        {
            Log.LogDebug("Searching for folder {0}", folder);
            try
            {
                GetFileAttributesParams getFileAttrParams = new GetFileAttributesParams
                {
                    filespaceid = filespaceId,
                    path = folder
                };
                var result = ExecuteRequest(Ticket, Method.GET, "/tcc/GetFileAttributes", getFileAttrParams,
                    typeof(GetFileAttributesResult));
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
                Log.LogError("Failed to get TCC file attributes: {0}", ex.Message);
            }
            return false;
        }

        public void ListFiles(DirResult entry, List<TCCFile> files, DateTime createdAfterUTC, string path)
        {
            if (entry.isFolder)
            {
                if (!entry.leaf && entry.entries != null)
                {
                    foreach (var e in entry.entries)
                    {
                        ListFiles(e, files, createdAfterUTC, path);
                    }
                }
            }
            else
            {
                {
                    if (entry.leaf && entry.createTime > createdAfterUTC)
                    {
                        files.Add(new TCCFile
                        {
                            fullName = string.Format("{0}/{1}", path, entry.entryName),
                            createdUTC = entry.createTime
                        });
                    }
                }
            }
        }

        private string Login()
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
                var result = ExecuteRequest(ticket, Method.POST, "/tcc/Login", loginParams, typeof(LoginResult));
                LoginResult loginResult = result as LoginResult;
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
                return string.Empty;
            }
            catch (Exception ex)
            {
                Log.LogError("Failed to login to TCC: {0}", ex.Message);
                return string.Empty;
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
                    Log.LogWarning("{0} failed: errorid={1}, message={2}", what, result.errorid, result.message);
                }
            }
        }

        private dynamic ExecuteRequest(string token, Method method, string contractPath, object requestData,
            Type expectedResultType, bool returnRaw = false)
        {
            if (String.IsNullOrEmpty(tccBaseUrl))
                throw new Exception("Configuration Error - no TCC url specified");

            RestClient client = new RestClient(tccBaseUrl);
            RestRequest request = new RestRequest(contractPath, method);
            request.RequestFormat = DataFormat.Json;
            request.AddHeader("Accept", returnRaw ? "application/octet-stream" : "application/json");
            if (!string.IsNullOrEmpty(ticket))
                request.AddQueryParameter("Ticket", token);

            var properties = from p in requestData.GetType().GetRuntimeFields()
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
            var reqTask = client.Execute(request);
            response = reqTask;
            var timestamp2 = DateTime.UtcNow;

            if (response.ResponseStatus == ResponseStatus.Completed)
            {
                Log.LogDebug(
                    "Response Status: StatusCode={0}, StatusDescription={1}, data size={2} at {3:0.00} KB/sec",
                    response.StatusCode, response.StatusDescription, response.Content.Length,
                    response.Content.Length / 1024.0 / (timestamp2 - timestamp1).TotalMilliseconds * 1000.0);

                if (returnRaw && response.ContentType == "application/octet-stream")
                    return response.RawBytes;
                dynamic result = JsonConvert.DeserializeObject(response.Content, expectedResultType);
                if (result == null)
                    Log.LogWarning("Can not execute request TCC response. Details: {0}", response.ErrorMessage);
                return result;
            }
            else
            {
                Log.LogError(
                    "Failed to get response from TCC: ResponseStatus={0}, StatusCode={1}, StatusDescription={2}, ErrorMessage={3}",
                    response.ResponseStatus, response.StatusCode, response.StatusDescription, response.ErrorMessage);
                return null;
            }
        }
    }
}
