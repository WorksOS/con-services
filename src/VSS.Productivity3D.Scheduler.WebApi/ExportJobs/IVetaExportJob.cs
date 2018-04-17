using Hangfire.Server;
using System;
using System.Collections.Generic;

namespace VSS.Productivity3D.Scheduler.WebAPI.ExportJobs
{
  /// <summary>
  /// Veta export job handler interface.
  /// </summary>
  public interface IVetaExportJob
  {
    /*
    /// <summary>
    /// Exports to Veta.
    /// </summary>
    void ExportDataToVeta(Guid projectUid, string fileName, string machineNames, Guid? filterUid,
      IDictionary<string, string> customHeaders,
      PerformContext context);
      */
  }
}