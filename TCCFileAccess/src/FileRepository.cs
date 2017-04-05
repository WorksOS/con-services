using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Extensions;
using TagFileHarvester.Models;
using VSS.GenericConfiguration;
using VSS.Raptor.Service.Common.Proxies;

namespace TCCFileAccess.Implementation
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
        private const string FILE_NOT_FOUND = "WRONG_PATH";
        private const string INVALID_TICKET_MESSAGE = "You have not authenticated, use login action";

        private readonly ILogger<FileRepository> Log;
        private readonly ILoggerFactory logFactory;

        private string ticket = String.Empty;

        private string Ticket
        {
            get
            {
                if (string.IsNullOrEmpty(ticket))
                {
                    ticket = Login().Result;
                }
                return ticket;
            }
        }

        public FileRepository(IConfigurationStore configuration, ILoggerFactory logger)
        {
            tccBaseUrl = configuration.GetValueString("TCCBASEURL");
            tccUserName = configuration.GetValueString("TCCUSERNAME");
            tccPassword = configuration.GetValueString("TCCPWD");
            tccOrganization = configuration.GetValueString("TCCORG");
            logFactory = logger;
            Log = logger.CreateLogger<FileRepository>();
        }

        public async Task<List<Organization>> ListOrganizations()
        {
            Log.LogDebug("ListOrganizations");
            List<Organization> orgs = null;
            try
            {
                GetFileSpacesParams fileSpaceParams = new GetFileSpacesParams {};
                var filespacesResult = await ExecuteRequest<GetFileSpacesResult>(Ticket, Method.GET,
                    "/tcc/GetFileSpaces", fileSpaceParams);
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

        public async Task<List<string>> ListFolders(Organization org, DateTime lastModifiedUTC)
        {
            Log.LogDebug("ListFolders: org={0} {1}, lastModfiedUTC={2}", org.shortName, org.filespaceId,
                lastModifiedUTC);
            return await GetFolders(org, lastModifiedUTC, "/");
        }

        public async Task<List<TCCFile>> ListFiles(Organization org, string path, DateTime createdAfterUTC)
        {
            Log.LogDebug("ListFiles: org={0} {1}, path={2}, createdAfterUTC={3}",
                org.shortName, org.filespaceId, path, createdAfterUTC);

            return await GetFiles(org.filespaceId, path, createdAfterUTC);
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
                //TODO fixup returning of byte array
/*                var result = ExecuteRequest(Ticket, Method.GET, "/tcc/GetFile", getFileParams, typeof(ApiResult), true);
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
                }*/
            }
            catch (Exception ex)
            {
                Log.LogError("Failed to get TCC file", ex);
            }
            return null;
        }

        public async Task<bool> MoveFile(Organization org, string srcFullName, string dstFullName)
        {
            Log.LogDebug("MoveFile: org={0} {1}, srcFullName={2}, dstFullName={3}", org.shortName, org.filespaceId,
                srcFullName, dstFullName);
            try
            {
                if (!await FolderExists(org.filespaceId, dstFullName.Substring(0, dstFullName.LastIndexOf("/")) + "/"))
                {
                    MkDir mkdirParams = new MkDir()
                    {
                        filespaceid = org.filespaceId,
                        force = true,
                        path = dstFullName.Substring(0, dstFullName.LastIndexOf("/")) + "/"
                    };
                    var resultCreate = await ExecuteRequest<RenResult>(Ticket, Method.GET, "/tcc/MkDir", mkdirParams);
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
                var renResult = await ExecuteRequest<RenResult>(Ticket, Method.GET, "/tcc/Ren", renParams);
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

        public async Task<List<string>> GetFolders(Organization org, DateTime lastModifiedUTC, string path)
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
                 //   filemasklist = "*.*"
                };
                var dirResult = await ExecuteRequest<DirResult>(Ticket, Method.GET, "/tcc/Dir", dirParams);
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

                                DateTime lastChanged = await GetLastChangedTime(org.filespaceId, lPath);


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

        public async Task<DateTime> GetLastChangedTime(string filespaceId, string path)
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
                var lastDirChangeResult = await ExecuteRequest<LastDirChangeResult>(Ticket, Method.GET,
                    "/tcc/LastDirChange", lastDirChangeParams);
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

        public async Task<List<TCCFile>> GetFiles(string filespaceId, string syncFolder, DateTime createdAfterUTC)
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
            var dirResult = await ExecuteRequest<DirResult>(Ticket, Method.GET, "/tcc/Dir", dirParams);
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

        public async Task<bool> FolderExists(string filespaceId, string folder)
        {
            Log.LogDebug("Searching for folder {0}", folder);
            try
            {
                GetFileAttributesParams getFileAttrParams = new GetFileAttributesParams
                {
                    filespaceid = filespaceId,
                    path = folder
                };
                var getFileAttrResult = await ExecuteRequest<GetFileAttributesResult>(Ticket, Method.GET,
                    "/tcc/GetFileAttributes", getFileAttrParams);
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
                var loginResult = await ExecuteRequest<LoginResult>(ticket, Method.GET, "/tcc/Login", loginParams);
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
                    ticket = null;
                }
                else
                {
                    Log.LogWarning("{0} failed: errorid={1}, message={2}", what, result.errorid, result.message);
                }
            }
        }

        private async Task<T> ExecuteRequest<T>(string token, Method method, string contractPath, object requestData,
            bool returnRaw = false)
        {
            if (String.IsNullOrEmpty(tccBaseUrl))
                throw new Exception("Configuration Error - no TCC url specified");

            var gracefulClient = new GracefulWebRequest(logFactory);
            string requestString = string.Empty;
            requestString = $"{tccBaseUrl}{contractPath}?ticket={token}";
            var headers = new Dictionary<string, string>();
            string body = String.Empty;

            var properties = from p in requestData.GetType().GetRuntimeFields()
                where p.GetValue(requestData) != null
                select new {p.Name, Value = p.GetValue(requestData)};
            if (method == Method.GET)
                foreach (var p in properties)
                {
                    requestString += $"&{p.Name}={p.Value.ToString()}";
                }
            else
                body = JsonConvert.SerializeObject(requestData);

            headers.Add("Accept", returnRaw ? "application/octet-stream" : "application/json");
            headers.Add("Content-Type", "application/json");
            var result = default(T);
            try
            {
                if (method == Method.GET)
                    result = await gracefulClient.ExecuteRequest<T>(requestString, method.ToString(), headers);
                else
                    result = await gracefulClient.ExecuteRequest<T>(requestString, method.ToString(), headers, body);

            }
            catch (WebException webException)
            {
                using (var response = webException.Response)
                {
                    Log.LogWarning(
                        $"Can not execute request TCC response. {GetStringFromResponseStream(webException.Response)}");
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
    }
}
