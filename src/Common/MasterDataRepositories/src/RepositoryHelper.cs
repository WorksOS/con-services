using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using VSS.Common.Abstractions.Clients.CWS.Models;

namespace VSS.MasterData.Repositories
{
  public static class RepositoryHelper
  {
    public static string WKTToSpatial(string geometryWKT)
    {
      return string.IsNullOrEmpty(geometryWKT) ? "null" : $"ST_GeomFromText('{geometryWKT}')";
    }

  }
}
