using CCSS.WorksOS.Reports.Common.Models;

namespace CCSS.WorksOS.Reports.Common.DataGrabbers
{
  public interface IDataGrabber
  {
    DataGrabberResponse GetReportsData();
  }
}
