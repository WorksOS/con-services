using System;
using System.Collections.Generic;
using System.Text;
using Apache.Ignite.Core.DataStructures;
using VSS.TRex.TAGFiles.Classes.Validator;
using VSS.TRex.TAGFiles.Models;

namespace VSS.TRex.TAGFiles.Classes
{
    public interface ITFAProxy
    {

       /* Now redundant
        RequestResult GetProjectId(long assetId, long lat, long lon, long height, DateTime timeOfPosition, string tccOrgId, out long projectId);
        RequestResult GetAssetId(long projectId, TAGValidator.DeviceType deviceType, string radioSerial, out Guid assetId, out TAGValidator.MachineLevel machineLevel);
        RequestResult GetProjectBoundariesAtDate(long assetId, DateTime tagFileUtc, out ProjectBoundaryPackage projectBoundaries);
        RequestResult GetProjectBoundaryAtDate(long projectId, DateTime tagFileUtc, out TWGS84FenceContainer boundary);
        */

        ValidationResult ValidateTagfile(string tccOrgId, string radioSerial, string radioType, double lat,
                double lon, DateTime timeOfPosition, out Guid projectId, out Guid assetId);
    }
}
