using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
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

    private readonly ILogger _log;
    private readonly IJobRunner _jobRunner;
    private readonly ITPaaSApplicationAuthentication _authn;
    private readonly ITpaasEmailProxy _tpaasEmail;

    private IHeaderDictionary _headers;

    public SendEmailJob(IJobRunner jobRunner, ITPaaSApplicationAuthentication authn, ITpaasEmailProxy emailProxy, ILoggerFactory logger)
    {
      _log = logger.CreateLogger<SendEmailJob>();
      _jobRunner = jobRunner;
      _authn = authn;
      _tpaasEmail = emailProxy;
    }

    public async Task Setup(object o, object context)
    {
      _headers = _authn.CustomHeaders();
    }

    public Task Run(object o, object context)
    {
      EmailModel emailModel = o.GetConvertedObject<EmailModel>();
      return _tpaasEmail.SendEmail(emailModel, _headers);
    }

    public Task TearDown(object o, object context) => Task.FromResult(true);
  }
}
