using System;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.WebApi.Models.Report.Models;
using VSS.Productivity3D.WebApiModels.Report.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Helpers
{
  public interface IExportRequestHandler
  {
    ExportRequestHelper SetRaptorClient(IASNodeClient raptorClient);

    ExportReport CreateExportRequest(
      DateTime? startUtc,
      DateTime? endUtc,
      CoordType coordType,
      ExportTypes exportType,
      string fileName,
      bool restrictSize,
      bool rawData,
      OutputTypes outputType,
      string machineNames,
      double tolerance = 0.0);
  }
}
