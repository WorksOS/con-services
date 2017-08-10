using System;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Helpers
{
  public interface IProfileDesignRequestHandler
  {
    ProfileResult CreateDesignProfileResponse(Guid projectUid, double latRadians1, double lngRadians1, double latRadians2, double lngRadians2, long designId, string designFilename, int importedFileTypeid, bool alignmentProfile, long alignmentId, double startStation, double endStation, string callId);
  }
}