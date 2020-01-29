using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
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
      if (!string.IsNullOrEmpty(configString))
      {
        var query = from services in configString.Split(';')
                    let service = services.Split(',')
                    select new TargetService(service[0].Trim(), service[1].Trim(), service[2].Trim());
        Services = query.ToList();
      }

      return Services.Count;
    }

    public int AppendServices(string configString)
    {
      if (!string.IsNullOrEmpty(configString))
      {
        var query = from services in configString.Split(';')
          let service = services.Split(',')
          select new TargetService(service[0].Trim(), service[1].Trim(), service[2].Trim());
        Services.AddRange(query.ToList());
      }

      return Services.Count;
    }

    public void Validate()
    {
      if (Services == null || !Services.Any())
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "No target services are configured"));

      foreach (var service in Services)
        service.Validate();
    }
  }

  public class TargetService
  {
    public ApiService ApiService { get; }

    public string AutoRoute { get; }
    public string DirectRoute { get; }
    // probably other stuff
    //   e.g. whether to use service discovery
    //        perhaps specific endpoints e.g. api/v2/tagfiles/auto ; api/v2/tagfiles/manual; api/v2/tagfiles/directEC520; api/v2/tagfiles/directTMC

    public TargetService(string apiService, string autoRoute, string directRoute)
    {
      // for now, but be a service known to our service discovery pattern.
      // could be expanded to any target url todoJeannie
      var isOk = Enum.TryParse<ApiService>(apiService, out var parsedApiService);
      ApiService = isOk ? parsedApiService : ApiService.None;

      AutoRoute = autoRoute;
      DirectRoute = directRoute;
    }

    public void Validate()
    {
      if (ApiService != ApiService.Productivity3D && ApiService != ApiService.Productivity3DVSS )
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            $"Target service: {ApiService} is not a supported type"));

      if (string.IsNullOrEmpty(AutoRoute))
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Auto route missing"));

      if (string.IsNullOrEmpty(DirectRoute))
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Direct route missing"));
    }
  }
}
