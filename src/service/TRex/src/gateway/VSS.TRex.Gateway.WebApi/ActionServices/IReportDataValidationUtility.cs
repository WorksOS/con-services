using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VSS.TRex.Gateway.WebApi.ActionServices
{
  public interface IReportDataValidationUtility
  {
    /// <summary>
    /// Uploads a file to the Raptor host.
    /// </summary>
    bool ValidateData(object request);

  }
}
