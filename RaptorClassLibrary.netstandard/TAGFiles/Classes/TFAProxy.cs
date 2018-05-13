using System;
using System.Collections.Generic;
using System.Text;
using VSS.TRex.TAGFiles.Classes.Validator;
using VSS.TRex.TAGFiles.Models;

namespace VSS.TRex.TAGFiles.Classes
{
    public class TFAProxy : ITFAProxy
    {

        /* Legacy functions that wont be used by TRex anymore
        public RequestResult GetProjectId(long assetId, long lat, long lon, long height, DateTime timeOfPosition, string tccOrgId, out long projectId)
        {
            projectId = 0;
            return RequestResult.None;
        }

        public RequestResult GetAssetId(long projectId, TAGValidator.DeviceType deviceType, string radioSerial, out Guid assetId, out TAGValidator.MachineLevel machineLevel)
        {
            assetId = Guid.Empty;
            machineLevel = TAGValidator.MachineLevel.ProductionAndCompaction;
            return RequestResult.None;
        }
        public RequestResult GetProjectBoundariesAtDate(long assetId, DateTime tagFileUtc, out ProjectBoundaryPackage projectBoundaries)
        {
            projectBoundaries = new ProjectBoundaryPackage();
            return RequestResult.None;

        }
        public RequestResult GetProjectBoundaryAtDate(long projectId, DateTime tagFileUtc, out TWGS84FenceContainer boundary)
        { 
            boundary = new TWGS84FenceContainer();
            return RequestResult.None;

        }
        */

        /// <summary>
        /// Calls Tagfile Auth Service to lookup project details and check assett is licensed
        /// </summary>
        /// <param name="tccOrgId"></param>
        /// <param name="radioSerial"></param>
        /// <param name="radioType"></param>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        /// <param name="timeOfPosition"></param>
        /// <param name="projectId"></param>
        /// <param name="assetId"></param>
        /// <returns></returns>
        public ValidationResult ValidateTagfile(string tccOrgId, string radioSerial, string radioType, double lat, double lon, DateTime timeOfPosition, out Guid projectId, out Guid assetId)
        {
            //projectId = ;
            return ValidationResult.Valid;
        }

    }
}
