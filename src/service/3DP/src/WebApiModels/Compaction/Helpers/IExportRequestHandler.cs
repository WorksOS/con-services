using System;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.WebApi.Models.Report.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Helpers
{
  public interface IExportRequestHandler
  {
#if RAPTOR
    ExportRequestHelper SetRaptorClient(IASNodeClient raptorClient);
#endif
    ExportReport CreateExportRequest(
      DateTime? startUtc,
      DateTime? endUtc,
      CoordType coordType,
      ExportTypes exportType,
      string fileName,
      bool restrictSize,
      bool rawData,
      OutputTypes outputType,
      string machineNameString,
      double tolerance = 0.0);
  }
}
