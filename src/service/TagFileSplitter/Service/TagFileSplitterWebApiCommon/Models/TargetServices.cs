using System.Collections.Generic;
using System.Linq;
using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace CCSS.TagFileSplitter.WebAPI.Common.Models
{
  public class TargetServices
  {
    public List<TargetService> Services { get; protected set; }

    public TargetServices()
    {
      Services = new List<TargetService>();
    }

    public int SetServices(string configString)
    {
      Services = new List<TargetService>();
      AppendServices((configString));

      return Services.Count;
    }

    public int AppendServices(string configString)
    {
      if (!string.IsNullOrEmpty(configString) && configString.Trim().Length > 7)
      {
        var query = from services in configString.Split(';')
          let service = services.Split(',')
          select new TargetService(service[0].Trim(), service[1].Trim(), service[2].Trim(), service[3].Trim());

        try
        {
          Services.AddRange(query.ToList());
        }
        catch
        {
          // ignored
        }
      }

      return Services.Count;
    }

    public void Validate()
    {
      if (Services == null || !Services.Any())
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Unable to identify Target services"));

      foreach (var service in Services)
        service.Validate();
    }
  }
}
