using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using log4net;
using VSS.TRex.TAGFiles.Classes.Validator;
using VSS.TRex.TAGFiles.Models;
using VSS.Velociraptor.DesignProfiling.Servers.Client;

namespace VSS.TRex.TAGFiles.Classes
{
    public class TFAProxy : ITFAProxy
    {

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


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
            // dont waste the services time if you dont have any details
            if (tccOrgId == string.Empty && radioType == string.Empty)
                return ValidationResult.BadRequest;
            Log.Info($"#Info# Details passed to TFA servce. ProjectID:{projectId}, AssetId:{assetId}, TCCOrgId:{tccOrgId}, radioSerial:{radioSerial}, radioType:{radioType}, lat:{lat}, lon:{lon}, DateTime:{timeOfPosition}");            
            return ValidationResult.Valid;
        }

    }
}
