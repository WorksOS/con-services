using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.MasterData.Data.Tests.Helpers
{
  public static class GuidExtensions
  {
    public static string ToStringWithoutHyphens(this Guid meGuid)
    {
      return meGuid.ToString("N");
    }
  }
}
