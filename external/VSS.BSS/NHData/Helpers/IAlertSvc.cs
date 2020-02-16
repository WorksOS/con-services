using System.Collections.Generic;
using VSS.Hosted.VLCommon;

namespace VSS.Nighthawk.NHDataSvc.Helpers
{
  public interface IAlertSvc
  {
    void Send(List<INHDataObject> alertItems, List<DataHoursLocation> locationItems);
  }
}
