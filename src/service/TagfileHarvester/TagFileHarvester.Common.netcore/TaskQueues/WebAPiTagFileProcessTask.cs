using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using CCSS.TagFileSplitter.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using TagFileHarvester.Interfaces;
using TagFileHarvester.Models;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;

namespace TagFileHarvester.Common.netcore.TaskQueues
{
  public class WebApiTagFileProcessTask
  {
    private readonly ILogger log;
    private readonly CancellationToken token;
    private readonly IUnityContainer unityContainer;

    public WebApiTagFileProcessTask(IUnityContainer unityContainer, CancellationToken token)
    {
      this.unityContainer = unityContainer;
      log = unityContainer.Resolve<ILogger>();
      this.token = token;
    }

    public TagFileSplitterAutoResponse ProcessTagfile(string tagFilename, Organization org)
    {
      if (token.IsCancellationRequested)
        return null;
      try
      {
        log.LogDebug("Processing file {0} for org {1}", tagFilename, org.shortName);
        var file = unityContainer.Resolve<IFileRepository>().GetFile(org, tagFilename);
        if (file == null) return null;
        if (token.IsCancellationRequested)
          return null;
        log.LogDebug("Submitting file {0} for org {1}", tagFilename, org.shortName);
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

        IRestResponse<TagFileSplitterAutoResponse> response = null;
        TagFileSplitterAutoResponse result;

        // In both the following cases we don't want to reprocess the tag file later.
        //   Setting these codes will result in the tag file being left in Production-Data so that it can be reprocessed next time.
        // 1) TFSS should return ok, else the service may be down
        HttpStatusCode? unexpectedTFSSStatusCode = null;
        // 2) any target service returned a status code other than: HttpStatusCode.BadRequest or HttpStatusCode.OK
        //   this could occur because a target service, or one of it's dependents was down. 
        TargetServiceResponse failedTargetServiceResponse = null;
        TargetServiceResponse vssTargetServiceResponse = null;

        try
        {
          response = client.Execute<TagFileSplitterAutoResponse>(request);
        }
        finally
        {
          result = JsonConvert.DeserializeObject<TagFileSplitterAutoResponse>(response?.Content);

          // tfss call failed?
          unexpectedTFSSStatusCode = response?.StatusCode == HttpStatusCode.OK ? null : response?.StatusCode;

          // did one of tfss's target 3dpm services failed or none were configured?
          failedTargetServiceResponse = result.TargetServiceResponses.FirstOrDefault(r => r.StatusCode != HttpStatusCode.OK && r.StatusCode != HttpStatusCode.BadRequest);
          vssTargetServiceResponse = result.TargetServiceResponses.First(r => r.ApiService == ApiService.Productivity3DVSS.ToString());
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
            { "Code", unexpectedTFSSStatusCode ?? failedTargetServiceResponse?.StatusCode ?? vssTargetServiceResponse.StatusCode },
            { "ProcessingCode", result.Code.ToString() },
            { "ProcessingMessage", result.Message }
          };

          NewRelic.Api.Agent.NewRelic.RecordCustomEvent("TagFileHarvester_Process", eventAttributes);
        }
        log.LogInformation($"File {tagFilename} for org {org.shortName} processed with overall (TFSS) status code {unexpectedTFSSStatusCode?.ToString()} message {result?.Message} processingCode {result?.Code.ToString()} targetResults: {JsonConvert.SerializeObject(result?.TargetServiceResponses)}");

        if (unexpectedTFSSStatusCode != null)
        {
          log.LogError($"File {tagFilename} for org {org.shortName} processed with status code {response?.StatusCode.ToString()} message {result?.Message}.  TFSS error result code. Probably issue with the tagFile Splitter service not available. Not moving tag file anywhere.");
          return null;
        }
        
        if (!result.TargetServiceResponses.Any())
        {
          log.LogError($"File {tagFilename} for org {org.shortName} processed with no vss targetService found. Probably issue with TFSS configuration. Not moving tag file anywhere.");
          return null;
        }
        
        if (failedTargetServiceResponse != null)
        {
          log.LogError($"File {tagFilename} for org {org.shortName} processed with one of the targetServices failing with status code {failedTargetServiceResponse.StatusCode.ToString()} message {failedTargetServiceResponse.Message}. Not moving tag file anywhere.");
          return null;
        }

        // archiving depends on the VSS result. CCSS 3dpm service does its own archiving (S3).
        switch (vssTargetServiceResponse.StatusCode)
        {
          case HttpStatusCode.OK:
            {
              log.LogDebug("Archiving file {0} for org {1} to {2} folder", tagFilename, org.shortName,
                OrgsHandler.TCCSynchProductionDataArchivedFolder);

              if (!MoveFileTo(tagFilename, org, fileRepository, OrgsHandler.TCCSynchProductionDataArchivedFolder))
                return null;

              break;
            }
          case HttpStatusCode.BadRequest:
            {
              switch (vssTargetServiceResponse.Code)
              {
                case 2010:
                case 2011:
                case 2014:
                case 2013:
                  {
                    log.LogDebug("Moving file {0} for org {1} to {2} folder", tagFilename, org.shortName,
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
                    log.LogDebug("Moving file {0} for org {1} to {2} folder", tagFilename, org.shortName,
                      OrgsHandler.TCCSynchSubscriptionIssueFolder);

                    if (!MoveFileTo(tagFilename, org, fileRepository, OrgsHandler.TCCSynchSubscriptionIssueFolder))
                      return null;

                    break;
                  }
                case 2021:
                  {
                    log.LogError("TFA is likely down for {0} org {1}", tagFilename, org.shortName);
                    break;
                  }
                default:
                  {
                    log.LogDebug("Moving file {0} for org {1} to {2} folder", tagFilename, org.shortName,
                      OrgsHandler.TCCSynchProjectBoundaryIssueFolder);

                    if (!MoveFileTo(tagFilename, org, fileRepository, OrgsHandler.TCCSynchProjectBoundaryIssueFolder))
                      return null;
                    break;
                  }
              }

              break;
            }
          default:
            {
              log.LogDebug("Other issue, but file {0} for org {1} will NOT now be moved to {2} folder", tagFilename, org.shortName,
                OrgsHandler.TCCSynchOtherIssueFolder);

              //If any other error occured do NOT move this file anywhere so it can be picked up during the next epoch
              //Potential risk here is with locked\corrupted files but this should be handled in 3dpm service
              /*if (!MoveFileTo(tagFilename, org, fileRepository, OrgsHandler.TCCSynchOtherIssueFolder))
                return null;*/

              break;
            }
        }

        return result;
      }
      catch (Exception ex)
      {
        log.LogError("Exception while processing file {0} occured {1} {2}", tagFilename, ex.Message, ex.StackTrace);
      }

      return null;
    }

    private bool MoveFileTo(string tagFilename, Organization org, IFileRepository fileRepository, string destFolder)
    {
      return fileRepository.MoveFile(org, tagFilename,
        tagFilename.Remove(tagFilename.IndexOf(OrgsHandler.tccSynchMachineFolder, StringComparison.Ordinal),
          OrgsHandler.tccSynchMachineFolder.Length + 1).Replace(OrgsHandler.TCCSynchProductionDataFolder,
          destFolder));
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
}
