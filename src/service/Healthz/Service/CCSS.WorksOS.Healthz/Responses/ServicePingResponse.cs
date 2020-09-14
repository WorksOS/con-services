using System;
using CCSS.WorksOS.Healthz.Types;

namespace CCSS.WorksOS.Healthz.Responses
{
  public class ServicePingResponse
  {
    public string Id { get; private set; }
    public TimeSpan ResponseTime { get; private set; }
    public ServiceState State { get; private set; }

    public static ServicePingResponse Create(string id, TimeSpan responseTime, bool isFalted) =>
      new ServicePingResponse
      {
        Id = id,
        ResponseTime = responseTime,
        State = isFalted ? ServiceState.Unavailable : ServiceState.Available
      };
  }
}
