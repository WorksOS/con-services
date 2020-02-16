
namespace VSS.Nighthawk.NHDataSvc.Interfaces.Events
{
  public interface INewNhDataTokenEvent
  {
    string NHDataObjectUrl { get; set; }
    long Id { get; set; }
  }
}
