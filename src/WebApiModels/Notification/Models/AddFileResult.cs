using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.Common.ResultsHandling;

namespace VSS.Productivity3D.WebApi.Models.Notification.Models
{
  public class AddFileResult : ContractExecutionResult
  {
    public AddFileResult(int code, string message) : base(code, message)
    { }

    /// <summary>
    /// The minimum zoom level that DXF tiles have been generated for.
    /// </summary>
    public int minZoomLevel;
    /// <summary>
    /// The maximum zoom level that DXF tiles have been generated for.
    /// </summary>
    public int maxZoomLevel;
  }
}
