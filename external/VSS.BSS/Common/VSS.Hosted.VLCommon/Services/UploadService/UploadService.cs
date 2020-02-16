using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.ServiceModel.Activation;
using log4net;
using System.Reflection;
using System.IO;
using System.Net;
using VSS.Hosted.VLCommon;
using System.Configuration;
using System.Web;

namespace VSS.Hosted.VLCommon
{
  [ServiceContract]
  interface IUploadService
  {
    [OperationContract]
    [WebInvoke(UriTemplate = "UploadFile?SESSIONID={sessionID}&FILENAME={fileName}",
      Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Xml)]
    void UploadFile(string sessionID, string fileName, global::System.IO.Stream fileData);
    [OperationContract]
    [WebInvoke(UriTemplate = "UploadFileChunk?SESSIONID={sessionID}&FILENAME={fileName}&CHUNK={chunk}",
      Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Xml)]
    void UploadFileChunk(string sessionID, string fileName, int chunk, global::System.IO.Stream fileData);
    [OperationContract]
    [WebGet(UriTemplate = "")]
    string Wild();
  }

  [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
  [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.PerCall)]
  public class UploadService : IUploadService
  {
    private static readonly ILog log = LogManager.GetLogger(MethodInfo.GetCurrentMethod().DeclaringType);

    const long MAX_UPLOAD_SIZE_LIMIT = 20971520; //1024 * 1024 * 20

    public static ServiceRoute UploadServiceRoute
    {
      get 
      {
        return new ServiceRoute("UploadService", new WebServiceHostFactory(), typeof(VSS.Hosted.VLCommon.UploadService));
      }
    }

    public static IPathMapper PathMapper
    {
      get { return pathMapper; }
      set { pathMapper = value; }
    }

    public string Wild()
    {
      throw new HttpException(401, "No data available");
    }

    public void UploadFile(string sessionID, string fileName, Stream fileData)
    {
      string HTMLDecodedFileName = System.Net.WebUtility.HtmlDecode(fileName);

      log.IfInfoFormat("S UploadService.UploadFile: sessionID={0}, fileName={1}, Mapped FileName={2}",
                       sessionID, HTMLDecodedFileName, MappedFileName(sessionID, HTMLDecodedFileName));

      var session = API.Session.Validate(sessionID);

      using (var ms = new MemoryStream())
      {
        fileData.CopyTo(ms);
        var fileContent = ms.ToArray();

        if (fileContent.LongLength > MAX_UPLOAD_SIZE_LIMIT)
          throw new WebException("File too large", WebExceptionStatus.MessageLengthLimitExceeded);

        using (BinaryWriter file = new BinaryWriter(File.OpenWrite(MappedFileName(sessionID, HTMLDecodedFileName))))
        {
          file.Write(fileContent);
          file.Close();
        }
      }
      log.IfInfoFormat("E UploadService.UploadFile: sessionID={0}, fileName={1}", sessionID, HTMLDecodedFileName);
    }

    public void UploadFileChunk(string sessionID, string fileName, int chunk, Stream fileData)
    {
      string HTMLDecodedFileName = System.Net.WebUtility.HtmlDecode(fileName);

      log.IfInfoFormat("S UploadService.UploadChunk: sessionID={0}, fileName={1}, chunk={2}, Mapped FileName={3}",
                       sessionID, HTMLDecodedFileName, chunk, MappedFileName(sessionID, HTMLDecodedFileName));

      //chunk is not really needed but used to stop the URL being cached by the browser

      var session = API.Session.Validate(sessionID);

      using (var ms = new MemoryStream())
      {
        fileData.CopyTo(ms);
        var fileContent = ms.ToArray();

        using (BinaryWriter file = new BinaryWriter(File.Open(MappedFileName(sessionID, HTMLDecodedFileName), chunk == 1 ? FileMode.Create : FileMode.Append, FileAccess.Write)))
        {
          file.Write(fileContent);
          file.Close();
        }
        
        log.IfInfoFormat("E UploadService.UploadChunk: sessionID={0}, fileName={1}, chunk={2}, size={3}", sessionID, HTMLDecodedFileName, chunk, ms.Length);
      }      
    }

    public static string MappedFileName(string sessionID, string fileName)
    {
      string mappedPath = MappedPath(sessionID);
      DirectoryInfo dirInfo = new DirectoryInfo(mappedPath);
      if (!dirInfo.Exists)
      {
        try
        {
          dirInfo.Create();
        }
        catch (Exception ex)
        {
          log.IfWarnFormat("Failed to create temporary upload folder {0}: {1}", mappedPath, ex.Message);
          throw ex;
        }
      }
      return Path.Combine(mappedPath, fileName);
    }

    public static string MappedPath(string sessionID)
    {
      if (pathMapper == null)
        log.IfWarn("Missing Path Mapper in ProjectSvc!");

      return pathMapper != null ? pathMapper.MapPath(sessionID, TempUploadFolder) : TempUploadFolder;
    }

    private static string TempUploadFolder
    {
      get
      {
        string uploadDir = ConfigurationManager.AppSettings["UploadFolder"];
        if (string.IsNullOrEmpty(uploadDir))
        {
          throw new ArgumentNullException("Missing upload folder setting in config file");
        }
        return uploadDir;
      }
    }

    private static IPathMapper pathMapper = null;

  }
}
