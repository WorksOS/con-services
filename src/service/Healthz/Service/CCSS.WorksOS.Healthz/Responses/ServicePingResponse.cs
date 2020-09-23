using System;
using CCSS.WorksOS.Healthz.Types;

namespace CCSS.WorksOS.Healthz.Responses
{
  public class ServicePingResponse
  {
    public string Id { get; private set; }
    public TimeSpan ResponseTime { get; private set; }
    public long ResponseTimeTicks { get; private set; }
    public ServiceState State { get; private set; }

    public static ServicePingResponse Create(string id, long responseTimeTicks, bool isSuccessStatusCode) =>
      new ServicePingResponse
      {
        Id = id,
        ResponseTimeTicks = responseTimeTicks,
        ResponseTime = TimeSpan.FromTicks(responseTimeTicks),
        State = isSuccessStatusCode ? ServiceState.Available : ServiceState.Unavailable
      };
  }
}
