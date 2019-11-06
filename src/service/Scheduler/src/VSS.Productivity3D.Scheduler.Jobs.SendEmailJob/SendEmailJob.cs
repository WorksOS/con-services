using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Scheduler.Abstractions;
using VSS.Productivity3D.Scheduler.Models;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.Scheduler.Jobs.AssetWorksManagerJob
{
  public class SendEmailJob : IJob
  {
    public static Guid VSSJOB_UID = Guid.Parse("7c2fc23d-ca84-490d-9240-8e2e622c2470");
    public Guid VSSJobUid => VSSJOB_UID;

    private readonly ILogger log;

    private readonly IJobRunner jobRunner;
    private readonly ITPaaSApplicationAuthentication authn;
    private readonly ITpaasEmailProxy tpaasEmail;

    private IDictionary<string, string> headers;


    public SendEmailJob( IJobRunner jobRunner, ITPaaSApplicationAuthentication authn, ITpaasEmailProxy emailProxy, ILoggerFactory logger)
    {
      log = logger.CreateLogger<SendEmailJob>();
      this.jobRunner = jobRunner;
      this.authn = authn;
      this.tpaasEmail = emailProxy;
    }

    public async Task Setup(object o, object context)
    {
      headers = authn.CustomHeaders();
    }

    public Task Run(object o, object context)
    {
      EmailModel emailModel = o.GetConvertedObject<EmailModel>();
      return tpaasEmail.SendEmail(emailModel, headers);
    }

    public Task TearDown(object o, object context) => Task.FromResult(true);
    
  }
}
