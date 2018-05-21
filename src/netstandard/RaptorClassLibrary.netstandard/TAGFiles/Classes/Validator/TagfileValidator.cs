using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;
using VSS.TRex;
using VSS.TRex.Events;
using VSS.TRex.Machines;
using VSS.TRex.SiteModels;
using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.TAGFiles.Classes;
using VSS.TRex.TAGFiles.Classes.Sinks;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.Validator
{
    public static class TagfileValidator
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

        /// <summary>
        /// Calls the TFA service to lookup assetId and projectId and validates licensing etc
        /// </summary>
        /// <param name="tagDetail"></param>
        /// <param name="processor"></param>
        /// <returns></returns>
        private static ValidationResult CheckFileIsProcessible(ref TagfileDetail tagDetail, TAGProcessor processor)
        {

            /*
            Three different types of tagfile submission
            Type A: Automatic submission.
            This is where a tagfile comes in from a known org and the system works out what Asset and Project it belongs to. Licensing is checked

            Type B:  Manual submission.
            This is where the project is known and supplied as an override projectid value. The Asset is worked out(TFA) or assigned a JohnDoe id if not known. (I don’t believe there would be a situation where an AssetId is overridden but ProjectId is to be worked out)
            Licensing is checked for manual subscription

            Type C: Override submission.
            This is where the ProjectId and AssetId is both supplied. It bypasses TFA service and providing the tagfile is valid, is processed straight into the project.
            This is not a typical submission but is handy for testing and in a situation where a known third party source other than NG could determine the AssetId and Project. Typical NG users could not submit via this method thus avoiding our license check. 
             
             */

            // Type C. Do we have what we need already (Most likley test tool submission)
            if (tagDetail.assetId != Guid.Empty && tagDetail.projectId != Guid.Empty) 
                return ValidationResult.Valid;

            // Business rule for device type conversion
            // DeviceType radioType = processor.RadioType == "torch" ? DeviceType.SNM940 : DeviceType.ManualDevice;
            int radioType = processor.RadioType == "torch" ? 6 : 0;


            TFAProxy tfa = new TFAProxy(); // Todo This can be refactored at a later stage
            Log.LogInformation($"#Info# Calling TFA servce to validate tagfile {tagDetail.tagFileName} ");
            // use decimal degrees
            // return ValidationResult.Valid;
            var apiResult = tfa.ValidateTagfile(tagDetail.projectId, Guid.Parse(tagDetail.tccOrgId), processor.RadioSerial, radioType, processor.LLHLat * (180 / Math.PI), processor.LLHLon * (180 / Math.PI), processor.DataTime, out tagDetail.projectId, out tagDetail.assetId);
            Log.LogInformation($"#Info# TFA GetId returned for {tagDetail.tagFileName} StatusCode: {apiResult}, ProjectId:{tagDetail.projectId}, AssetId:{tagDetail.assetId}");
            if (apiResult == ValidationResult.Valid)
            {
               // Check For JohnDoe machines
                if (tagDetail.assetId == Guid.Empty)
                {
                    tagDetail.IsJohnDoe = true; // JohnDoe Machine and OK to process
                }

            }
            return apiResult;
        }

        /// <summary>
        /// Inputs a tagfile for validation and asset licensing checks
        /// </summary>
        /// <param name="tagDetail"></param>
        /// <returns></returns>
        public static ValidationResult ValidSubmission(ref TagfileDetail tagDetail)
        {

            ValidationResult result = ValidationResult.Unknown;

            // Perform some Validation Checks

            if (tagDetail.tagFileContent.Length <= TRexConfig.MinTAGFileLength)
                return ValidationResult.Invalid;

            // Now open tagfile and validate contents
            var siteModel = new SiteModel(Guid.Empty);
            var machine = new Machine()
            {
                TargetValueChanges = new ProductionEventLists(siteModel, 0)
            };

            var siteModelGridAggregator = new ServerSubGridTree(siteModel);
            var machineTargetValueChangesAggregator = new ProductionEventLists(siteModel, Machine.kNullInternalSiteModelMachineIndex);
            TAGProcessor processor = new TAGProcessor(siteModel, machine, siteModelGridAggregator,machineTargetValueChangesAggregator);
            TAGValueSink sink = new TAGVisionLinkPrerequisitesValueSink(processor);
            TAGReader reader = new TAGReader(new MemoryStream(tagDetail.tagFileContent));
            TAGFile tagFile = new TAGFile();
            TAGReadResult readResult = tagFile.Read(reader, sink);
            if (readResult != TAGReadResult.NoError)
                return ValidationResult.Invalid;

            // Tagfile contents are OK so proceed

            if (!TRexConfig.EnableTFAService) // allows us to bypass a TFA service
            {
                Log.LogWarning($"SubmitTAGFileResponse.ValidSubmission. EnableTFAService disabled. Bypassing TFS validation checks");
                if (tagDetail.assetId != Guid.Empty && tagDetail.projectId != Guid.Empty) // do we have what we need
                    {
                    // they may want to    tagDetail.IsJohnDoe = false;
                        return ValidationResult.Valid;
                    }
                else
                    return ValidationResult.Invalid; // cannot process with asset and project id
            }

            return CheckFileIsProcessible(ref tagDetail, processor);

        }
    }
}
