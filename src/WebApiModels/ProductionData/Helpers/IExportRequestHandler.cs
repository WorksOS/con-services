using System;
using System.Threading.Tasks;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.WebApiModels.Report.Models;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Helpers
{
  public interface IExportRequestHandler
  {
    ExportRequestHelper SetRaptorClient(IASNodeClient raptorClient);

    Task<ExportReport> CreateExportRequest(
      DateTime? startUtc,
      DateTime? endUtc,
      CoordTypes coordType,
      ExportTypes exportType,
      string fileName,
      bool restrictSize,
      bool rawData,
      OutputTypes outputType,
      string machineNames,
      double tolerance = 0.0);
  }
}