using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TCCFileAccess.Models;
using VSS.GenericConfiguration;
using VSS.Raptor.Service.Common.Proxies;

namespace TCCFileAccess
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

        public async Task<PutFileResponse> PutFile(Organization org, string path, string filename, Stream contents, long sizeOfContents)
        {
            Log.LogDebug("PutFile: org={0} {1}, fullName={2} {3}", org.shortName, org.filespaceId, path, filename);
            PutFileRequest sendFileParams = new PutFileRequest()
            {
                filespaceid = org.filespaceId,
                path = path,
                replace = true,
                commitUpload = true,
                upfile = filename
            };
            if (String.IsNullOrEmpty(tccBaseUrl))
                throw new Exception("Configuration Error - no TCC url specified");

            var gracefulClient = new GracefulWebRequest(logFactory);
            var (requestString, headers) = FormRequest(sendFileParams, "PutFile");

            headers.Add("X-File-Name", filename);
            headers.Add("X-File-Size", sizeOfContents.ToString());
            headers.Add("X-FileType","");

            PutFileResponse result = default(PutFileResponse);
            try
            {
                result = await gracefulClient.ExecuteRequest<PutFileResponse>(requestString, contents, headers);
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
            GetFileParams getFileParams = new GetFileParams
            {
                filespaceid = org.filespaceId,
                path = fullName
            };

            if (String.IsNullOrEmpty(tccBaseUrl))
                throw new Exception("Configuration Error - no TCC url specified");

            var gracefulClient = new GracefulWebRequest(logFactory);
            var (requestString, headers) = FormRequest(getFileParams,"GetFile");

            try
            {
                return await gracefulClient.ExecuteRequest(requestString, "GET", headers);
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
            return null;
        }

        private (string,Dictionary<string,string>) FormRequest(object request,string endpoint, string token = null)
        {
            var requestString = $"{tccBaseUrl}/tcc/{endpoint}?ticket={token??Ticket}";
            var headers = new Dictionary<string, string>();
            var properties = from p in request.GetType().GetRuntimeFields()
                where p.GetValue(request) != null
                select new {p.Name, Value = p.GetValue(request) };
            foreach (var p in properties)
            {
                requestString += $"&{p.Name}={p.Value.ToString()}";
            }
            return (requestString, headers);
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
                    var resultCreate = await ExecuteRequest<RenResult>(Ticket, "MkDir", mkdirParams);
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

        public async Task<DirResult> GetFolders(Organization org, DateTime lastModifiedUTC, string path)
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
                var dirResult = await ExecuteRequest<DirResult>(Ticket, "Dir", dirParams);
                if (dirResult != null)
                {
                    return dirResult;
                }
                CheckForInvalidTicket(dirResult,"GetFolders");
                Log.LogError("Null result from GetFolders for org {0}", org.shortName);
            }
            catch (Exception ex)
            {
                Log.LogError("Failed to get list of TCC folders: {0}", ex.Message);
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
                    path = path,
                    recursive = true
                };
                var lastDirChangeResult = await ExecuteRequest<LastDirChangeResult>(Ticket, "LastDirChange", lastDirChangeParams);
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
            Log.LogDebug("Searching for folder {0}", folder);
            try
            {
                GetFileAttributesParams getFileAttrParams = new GetFileAttributesParams
                {
                    filespaceid = filespaceId,
                    path = folder
                };
                var getFileAttrResult = await ExecuteRequest<GetFileAttributesResult>(Ticket, "GetFileAttributes", getFileAttrParams);
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
                var loginResult = await ExecuteRequest<LoginResult>(ticket,"Login", loginParams);
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

        private async Task<T> ExecuteRequest<T>(string token, string contractPath, object requestData)
        {
            if (String.IsNullOrEmpty(tccBaseUrl))
                throw new Exception("Configuration Error - no TCC url specified");

            var gracefulClient = new GracefulWebRequest(logFactory);
            var (requestString, headers) = FormRequest(requestData, contractPath,token);

            headers.Add("Content-Type", "application/json");
            var result = default(T);
            try
            {
                    result = await gracefulClient.ExecuteRequest<T>(requestString, "GET", headers);
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
