using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling
{
  /// <summary>
  /// The data for a compaction profile. 
  /// </summary>
  public class CompactionProfileDataResult
  {
    /// <summary>
    /// The type indicates what type of production data e.g. lastPass, cutFill, passCount etc.
    /// </summary>
    public string type;
    /// <summary>
    /// A list of data points for the profile.
    /// </summary>
    public List<CompactionDataPoint> data;
  }
}
