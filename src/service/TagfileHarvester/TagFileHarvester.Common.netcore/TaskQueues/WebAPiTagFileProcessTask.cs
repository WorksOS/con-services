using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using log4net;
using Newtonsoft.Json;
using RestSharp;
using TagFileHarvester.Interfaces;
using TagFileHarvester.Models;

namespace TagFileHarvester.TaskQueues
{
  public class WebApiTagFileProcessTask
  {
    private readonly ILog log;
    private readonly CancellationToken token;
    private readonly IUnityContainer unityContainer;

    public WebApiTagFileProcessTask(IUnityContainer unityContainer, CancellationToken token)
    {
      this.unityContainer = unityContainer;
      log = unityContainer.Resolve<ILog>();
      this.token = token;
    }

    public BaseDataResult ProcessTagfile(string tagFilename, Organization org)
    {
      if (token.IsCancellationRequested)
        return null;
      try
      {
        tagFilename = tagFilename.Replace("//", "/");
        log.DebugFormat("Processing file {0} for org {1}", tagFilename, org.shortName);
        var file = unityContainer.Resolve<IFileRepository>().GetFile(org, tagFilename);
        if (file == null) return null;
        if (token.IsCancellationRequested)
          return null;
        log.DebugFormat("Submittting file {0} for org {1}", tagFilename, org.shortName);
        var data = new byte[file.Length];
        using (var reader = new BinaryReader(file))
        {
          data = reader.ReadBytes((int) file.Length);
        }

        var requestData =
          CompactionTagFileRequest.CreateCompactionTagFileRequest(Path.GetFileName(tagFilename), data, org.orgId);
        var client = new RestClient(OrgsHandler.TagFileEndpoint);
        //  client.Proxy=new WebProxy("127.0.0.1:8888",false);
        var request = new RestRequest(Method.POST);

        request.AddParameter("application/json; charset=utf-8", JsonConvert.SerializeObject(requestData),
          ParameterType.RequestBody);
        request.RequestFormat = DataFormat.Json;

        IRestResponse<BaseDataResult> response = null;
        BaseDataResult result = null;
        HttpStatusCode? code = null;

        try
        {
          response = client.Execute<BaseDataResult>(request);
        }
        finally
        {
          result = JsonConvert.DeserializeObject<BaseDataResult>(response?.Content);
          code = response?.StatusCode;
        }

        if (token.IsCancellationRequested)
          return null;

        var fileRepository = unityContainer.Resolve<IFileRepository>();

        if (fileRepository == null)
          return null;

        if (OrgsHandler.newrelic == "true")
        {
          var eventAttributes = new Dictionary<string, object>
          {
            { "Org", org.shortName },
            { "Filename", tagFilename },
            { "Code", code.ToString() },
            { "ProcessingCode", result.Code.ToString() },
            { "ProcessingMessage", result.Message }
          };

          NewRelic.Api.Agent.NewRelic.RecordCustomEvent("TagFileHarvester_Process", eventAttributes);
        }

        switch (code)
        {
          case HttpStatusCode.OK:
          {

           /* log.DebugFormat("Archiving file {0} for org {1} to {2} folder", tagFilename, org.shortName,
              OrgsHandler.TCCSynchProductionDataArchivedFolder);

              if (!MoveFileTo(tagFilename, org, fileRepository, OrgsHandler.TCCSynchProductionDataArchivedFolder))
              return null;*/

            break;
          }
          case HttpStatusCode.BadRequest:
          {
            switch (result.Code)
            {
              case 2010:
              case 2011:
              case 2014:
              case 2013:
              {
                log.DebugFormat("Moving file {0} for org {1} to {2} folder", tagFilename, org.shortName,
                  OrgsHandler.TCCSynchProjectBoundaryIssueFolder);

                if (!MoveFileTo(tagFilename, org, fileRepository, OrgsHandler.TCCSynchProjectBoundaryIssueFolder))
                  return null;
                break;
              }
              case 2008:
              case 2009:
              case 2006:
                //SecondCondition
              {
                log.DebugFormat("Moving file {0} for org {1} to {2} folder", tagFilename, org.shortName,
                  OrgsHandler.TCCSynchSubscriptionIssueFolder);

                if (!MoveFileTo(tagFilename, org, fileRepository, OrgsHandler.TCCSynchSubscriptionIssueFolder))
                  return null;

                break;
              }
              case 2021:
              {
                log.ErrorFormat("TFA is likely down for {0} org {1}", tagFilename, org.shortName);
                break;
              }
              default:
              {
                log.DebugFormat("Moving file {0} for org {1} to {2} folder", tagFilename, org.shortName,
                  OrgsHandler.TCCSynchProjectBoundaryIssueFolder);

               if (!MoveFileTo(tagFilename, org, fileRepository, OrgsHandler.TCCSynchOtherIssueFolder))
                  return null;
                break;
              }
            }

            break;
          }
          default:
          {
            log.ErrorFormat("TFA or 3Dpm is likely down for {0} org {1}", tagFilename, org.shortName);
            break;
              /*   log.DebugFormat("Moving file {0} for org {1} to {2} folder", tagFilename, org.shortName,
                   OrgsHandler.TCCSynchOtherIssueFolder);

                 if (!MoveFileTo(tagFilename, org, fileRepository, OrgsHandler.TCCSynchOtherIssueFolder))
                   return null;*/

              break;
          }
        }

        return result;
      }
      catch (Exception ex)
      {
        log.ErrorFormat("Exception while processing file {0} occured {1} {2}", tagFilename, ex.Message, ex.StackTrace);
      }

      return null;
    }

    private bool MoveFileTo(string tagFilename, Organization org, IFileRepository fileRepository, string destFolder)
    {
      return fileRepository.MoveFile(org, tagFilename,
        tagFilename.Replace(OrgsHandler.tccSynchMachineFolder, destFolder));
    }
  }

  /// <summary>
  ///   TAG file domain object. Model represents TAG file submitted to Raptor.
  /// </summary>
  internal class CompactionTagFileRequest
  {
    /// <summary>
    ///   Private constructor
    /// </summary>
    private CompactionTagFileRequest()
    {
    }

    /// <summary>
    ///   A project unique identifier.
    /// </summary>
    [JsonProperty(PropertyName = "projectUid", Required = Required.Default)]
    public Guid? projectUid { get; private set; }

    /// <summary>
    ///   The name of the TAG file.
    /// </summary>
    /// <value>Required. Shall contain only ASCII characters. Maximum length is 256 characters.</value>
    [JsonProperty(PropertyName = "fileName", Required = Required.Always)]
    public string fileName { get; private set; }

    /// <summary>
    ///   The content of the TAG file as an array of bytes.
    /// </summary>
    [JsonProperty(PropertyName = "data", Required = Required.Always)]
    public byte[] data { get; private set; }


    /// <summary>
    ///   Defines Org ID (either from TCC or Connect) to support project-based subs
    /// </summary>
    [JsonProperty(PropertyName = "OrgID", Required = Required.Default)]
    public string OrgID { get; private set; }

    /// <summary>
    ///   Create instance of CompactionTagFileRequest
    /// </summary>
    /// <param name="fileName">file name</param>
    /// <param name="data">metadata</param>
    /// <param name="projectUid">project UID</param>
    /// <returns></returns>
    public static CompactionTagFileRequest CreateCompactionTagFileRequest(
      string fileName,
      byte[] data,
      string orgId = null,
      Guid? projectUid = null)
    {
      return new CompactionTagFileRequest
      {
        fileName = fileName,
        data = data,
        OrgID = orgId,
        projectUid = projectUid
      };
    }
  }

  public class BaseDataResult
  {
    public int Code { get; set; }
    public string Message { get; set; }
  }
}