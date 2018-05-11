using System;
using System.Collections.Generic;
using System.Text;
using Apache.Ignite.Core.DataStructures;
using VSS.TRex.TAGFiles.Models;

namespace VSS.TRex.TAGFiles.Classes
{
    public interface ITFAProxy
    {

        /*
      function GetProjectID(const AssetID: Int64; const Lat, Lon, Height: Double; const TimeOfPosition: TDateTime; const TCCOrgID : String; out ProjectID: Int64) : Boolean;
      function GetAssetID(const ProjectID : Int64; const DeviceType: DeviceTypeEnum; const RadioSerial: String; out AssetID: Int64; out MachineLevel : MachineLevelEnum) : Boolean;
      function GetProjectBoundariesAtDate(const AssetID: Int64; const tagFileUTC: TDateTime; out ProjectBoundaries: ProjectsBoundaryPackage) : Boolean;
      function GetProjectBoundaryAtDate(const ProjectID : Int64; const tagFileUTC: TDateTime; out Boundary: TWGS84FenceContainer) : Boolean;
         
         */
        RequestResult GetProjectId(long assetId, long lat, long lon, long height, DateTime timeOfPosition, string tccOrgId, out long projectId);
        RequestResult GetAssetId(long projectId, TAGValidator.DeviceType deviceType, string radioSerial, out Guid assetId, out TAGValidator.MachineLevel machineLevel);
        RequestResult GetProjectBoundariesAtDate(long assetId, DateTime tagFileUtc, out ProjectBoundaryPackage projectBoundaries);
        RequestResult GetProjectBoundaryAtDate(long projectId, DateTime tagFileUtc, out TWGS84FenceContainer boundary);

    }
}
