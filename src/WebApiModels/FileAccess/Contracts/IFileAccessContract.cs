using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VSS.Raptor.Service.Common.Filters;
using VSS.Raptor.Service.Common.Models;
using VSS.Raptor.Service.WebApiModels.FileAccess.Models;
using VSS.Raptor.Service.WebApiModels.FileAccess.ResultHandling;

namespace VSS.Raptor.Service.WebApiModels.FileAccess.Contracts
{
  /// <summary>
  /// Data contract representing a file access...
  /// </summary>
  public interface IFileAccessContract
  {
    /// <summary>
    /// Gets requested file for Raptor from TCC and stores in the specified location.
    /// </summary>
    /// <param name="request">Details of the requested file.</param>
    /// <returns>Returns JSON structure wtih operation result. {"Code":0,"Message":"User-friendly"}</returns>
    FileAccessResult Post([FromBody]FileAccessRequest request);

    /// <summary>
    /// Gets requested file for Raptor from TCC and returns it as a raw array of bytes.
    /// </summary>
    /// <param name="request">Details of the requested file.</param>
    /// <returns>File content as a bytes array.</returns>
    RawFileContainer PostRaw([FromBody]FileDescriptor request);
  }  
}
