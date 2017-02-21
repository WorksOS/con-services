using System.Collections.Generic;

namespace VSS.Raptor.Service.Common.Interfaces
{
    public interface IProjectProxy
    {
      long GetProjectId(string projectUid, IDictionary<string, string> customHeaders = null);
    }
}
