using VSS.Nighthawk.NHDataSvc.Interfaces.Events;

namespace VSS.Nighthawk.MTSGateway.Common.Commands.NHDataSvc
{
  public class NewNHDataTokenEvent : INewNhDataTokenEvent
  {
    public string NHDataObjectUrl { get; set; }
    public long Id { get; set; }
  }
}
