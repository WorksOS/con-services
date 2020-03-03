using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSP.MasterData.Asset.Data.Helpers
{
  public static class GuidExtensions
  {
    public static string ToStringWithoutHyphens(this Guid meGuid)
    {
      return meGuid.ToString("N");
    }
  }
}
