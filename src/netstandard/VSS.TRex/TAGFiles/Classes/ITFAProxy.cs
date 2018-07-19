using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Apache.Ignite.Core.DataStructures;
using Microsoft.Extensions.Logging;
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
/// <summary>
/// 
/// </summary>
/// <param name="submittedProjectId"></param>
/// <param name="tccOrgId"></param>
/// <param name="radioSerial"></param>
/// <param name="radioType"></param>
/// <param name="lat"></param>
/// <param name="lon"></param>
/// <param name="timeOfPosition"></param>
/// <param name="projectId"></param>
/// <param name="assetId"></param>
/// <returns></returns>
        ValidationResult ValidateTagfile(Guid? submittedProjectId, Guid tccOrgId, string radioSerial, int radioType,
                double lat, double lon, DateTime timeOfPosition, ref Guid? projectId, out Guid? assetId, out string message, ref int code);

    }
}
