using System;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Scheduler.Abstractions;
using VSS.Productivity3D.Scheduler.Models;
using VSS.Productivity3D.Scheduler.WebAPI.ExportJobs;

namespace VSS.Productivity3D.Scheduler.Jobs.SendEmailJob
{
  public class ExportEmailGenerator : IExportEmailGenerator
  {
    private readonly ILogger log;

    public ExportEmailGenerator(ILoggerFactory logger)
    {
      log = logger.CreateLogger<ExportEmailGenerator>();
    }

    public JobRequest ExecuteCallback(object parameter, object context)
    {
      var parentJob = (string)parameter;
      var currentContext = (PerformContext)context;

      var jobData = JobStorage.Current.GetConnection()?.GetJobData(parentJob);
      var status = jobData?.State;

      if (status == null)
      {
        throw new NullReferenceException("JobData.State cannot be null");
      }

      var details = new FailureDetails { Code = System.Net.HttpStatusCode.OK, Result = new ContractExecutionResult() };

      if (status.Equals(Hangfire.States.DeletedState.StateName, StringComparison.OrdinalIgnoreCase))
      {
        var detailsJson = JobStorage.Current.GetConnection().GetJobParameter(parentJob, ExportFailedState.EXPORT_DETAILS_KEY);

        log.LogDebug($"GetExportJobStatus: detailsJson={detailsJson}");

        if (!string.IsNullOrEmpty(detailsJson))
        {
          details = JsonConvert.DeserializeObject<FailureDetails>(detailsJson);
        }
      }

      try
      {
        var key = "";
        var downloadLink = "";

        if (details.Result.Code == ContractExecutionStatesEnum.ExecutedSuccessfully)
        {
          key = JobStorage.Current.GetConnection().GetJobParameter(parentJob, ExportJob.ExportJob.S3KeyStateKey);
          downloadLink = JobStorage.Current.GetConnection().GetJobParameter(parentJob, ExportJob.ExportJob.DownloadLinkStateKey);
        }

        var projectName = currentContext.GetJobParameter<string>(Tags.PROJECTNAME_TAG);
        var recipients = currentContext.GetJobParameter<string[]>(Tags.RECIPIENTS_TAG);

        var result = new JobStatusResult { Key = key, Status = status, DownloadLink = downloadLink, FailureDetails = details };
        var emailParameters = new EmailModel { FromName = "no-reply-3d@trimble.com", To = recipients, Subject = $"Roller Report {projectName} {DateTime.UtcNow:yyyy-MM-ddTHH-mm}" };

        if (string.IsNullOrEmpty(downloadLink))
          result.Status = Hangfire.States.FailedState.StateName;

        emailParameters.SetContent(JsonConvert.SerializeObject(result));

        log.LogDebug($"Getting ready to send email { JsonConvert.SerializeObject(emailParameters) }");

        return new JobRequest { JobUid = Guid.Parse("7c2fc23d-ca84-490d-9240-8e2e622c2470"), SetupParameters = null, RunParameters = emailParameters };
      }
      catch (Exception ex)
      {
        log.LogError(ex, "Failed to prepare email message");
        throw;
      }
    }
  }
}
