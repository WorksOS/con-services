using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies;
using VSS.Productivity3D.Filter.Abstractions.Interfaces;
using VSS.Productivity3D.Filter.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Scheduler.Abstractions;
using VSS.Productivity3D.Scheduler.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.Scheduler.Jobs.MachinePassesExportJob
{
  public class MachinePassesExport : IJob
  {
    public static Guid VSSJOB_UID = Guid.Parse("39d6c48a-cc74-42d3-a839-1a6b77e8e076");

    private const string FILTER_JSON = "{\"filterType\":1,\"filterJson\":\"{\\\"dateRangeType\\\":1,\\\"elevationType\\\":null,\\\"vibeStateOn\\\":true}\"}";

    public Guid VSSJobUid => VSSJOB_UID;

    private readonly ILogger log;
    private readonly IProjectProxy projects;
    private readonly IFilterServiceProxy filters;
    private readonly IJobRunner jobRunner;
    private readonly ITPaaSApplicationAuthentication authn;
    private readonly IServiceResolution serviceResolution;
    private readonly IExportEmailGenerator exportEmailGenerator;
    private IDictionary<string, string> headers;
    private string[] recipients;

    private List<ProjectData> customerProjects;

    public MachinePassesExport(IProjectProxy projectyProxy, IFilterServiceProxy filterProxy,
      IJobRunner jobRunner, ITPaaSApplicationAuthentication authn,
      ILoggerFactory logger, IServiceResolution serviceResolution, IExportEmailGenerator emailGenerator)
    {
      log = logger.CreateLogger<MachinePassesExport>();
      projects = projectyProxy;
      filters = filterProxy;
      this.jobRunner = jobRunner;
      this.authn = authn;
      this.serviceResolution = serviceResolution;
      exportEmailGenerator = emailGenerator;
    }

    public async Task Setup(object o, object context)
    {
      var customerUid = o.GetConvertedObject<string>();

      if (string.IsNullOrEmpty(customerUid))
      {
        throw new NullReferenceException("customerUid cannot be null");
      }

      headers = authn.CustomHeadersJWTAndBearer();
      headers.AppendOrOverwriteCustomerHeader(customerUid);
      log.LogDebug($"Requesting projects for customer {customerUid}");
      customerProjects = (await projects.GetProjectsV4(customerUid, headers)).Where(p => !p.IsArchived).ToList();
      log.LogDebug($"Recieved {customerProjects.Count} projects for customer {customerUid}");
    }

    public async Task Run(object o, object context)
    {
      recipients = o.GetConvertedObject<string[]>();

      log.LogDebug($"Starting to process {customerProjects?.Count} projects");

      foreach (var project in customerProjects)
      {
        JobRequest jobRequest;

        try
        {
          log.LogInformation($"Processing project {project.Name}");
          // Create a relevant filter
          var filter = await filters.CreateFilter(project.ProjectUid, new FilterRequest() { FilterType = FilterType.Transient, FilterJson = FILTER_JSON }, headers);
          log.LogDebug($"Created filter {filter.FilterDescriptor.FilterUid}");
          //generate filename
          var generatedFilename = $"{project.Name + " " + DateTime.UtcNow.ToString("yyyy-MM-ddTHH-mm-ss")}";
          log.LogDebug($"Generated filename {generatedFilename}");
          //generate uri
          var baseUri = await serviceResolution.ResolveService("productivity3dinternal_service_public_v2");
          var requestUri = $"{baseUri.Endpoint}/api/v2/export/machinepasses?projectUid={project.ProjectUid}&filename={generatedFilename}&filterUid={filter.FilterDescriptor.FilterUid}&coordType=0&outputType=1&restrictOutput=False&rawDataOutput=False";
          log.LogDebug($"Export request url {requestUri}");
          var jobExportRequest = new ScheduleJobRequest() { Url = requestUri, Timeout = 9000000, Filename = generatedFilename };
          jobRequest = new JobRequest() { JobUid = Guid.Parse("c3cbb048-05c1-4961-a799-70434cb2f162"), SetupParameters = jobExportRequest, RunParameters = headers, AttributeFilters = SpecialFilters.ExportFilter };
        }
        catch (Exception e)
        {
          log.LogError(e, $"Failed to prepare for exports with exception");
          throw;
        }

        try
        {
          log.LogDebug($"Firing export job for project {project.Name}");
          var hangfireJobId = jobRunner.QueueHangfireJob(jobRequest, exportEmailGenerator);
          JobStorage.Current.GetConnection().SetJobParameter(hangfireJobId, Tags.PROJECTNAME_TAG, JsonConvert.SerializeObject(project.Name));
          JobStorage.Current.GetConnection().SetJobParameter(hangfireJobId, Tags.RECIPIENTS_TAG, JsonConvert.SerializeObject(recipients));
        }
        catch (Exception e)
        {
          log.LogError(e, $"Queue VSS job failed with exception {e.Message}");
          throw;
        }
      }
    }

    public Task TearDown(object o, object context) => Task.FromResult(true);
  }
}
