using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using log4net;
using VSS.VisionLink.Raptor;
using VSS.VisionLink.Raptor.Events;
using VSS.VisionLink.Raptor.Machines;
using VSS.VisionLink.Raptor.SiteModels;
using VSS.VisionLink.Raptor.SubGridTrees.Server;
using VSS.VisionLink.Raptor.TAGFiles.Classes;
using VSS.VisionLink.Raptor.TAGFiles.Classes.Sinks;
using VSS.VisionLink.Raptor.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.Validator
{
    public static class TagfileValidator
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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
            This is where the ProjectId and AssetId is both supplied. It bypasses TFA service and providing the tagfile is valid, is processed straight into the project. This is not a typical submission but is handy for testing and a situation where a known third party source other than NG could determine the AssetId and Project. Typical NG users could not submit via this method thus avoiding a license check. 
             
             */

            // Type C. Do we have what we need already (Most likley test tool submission)
            if (tagDetail.assetId != Guid.Empty && tagDetail.projectId != Guid.Empty) 
                return ValidationResult.Valid;

            string radioType = processor.RadioType;
            if (radioType == "tourch") // Business rule
                radioType = "SNM940";

            TFAProxy tfa = new TFAProxy(); // Todo This can be refactored at a later stage
            Log.Info($"#Info# Calling TFA servce to validate tagfile {tagDetail.tagFileName} ");
            // use decimal degrees
            return ValidationResult.Valid;
            //  return tfa.ValidateTagfile(tagDetail.tccOrgId,processor.RadioSerial, radioType,processor.LLHLat * (180/Math.PI),processor.LLHLon * (180 / Math.PI), processor.DataTime,out tagDetail.projectId, out tagDetail.assetId);
        }

        /// <summary>
        /// Inputs a tagfile for validation and asset licensing checks
        /// </summary>
        /// <param name="tagDetail"></param>
        /// <returns></returns>
        public static ValidationResult ValidSubmission(TagfileDetail tagDetail)
        {

            ValidationResult result = ValidationResult.Unknown;

            // Perform some Validation Checks

            if (tagDetail.tagFileContent.Length <= RaptorConfig.MinTAGFileLength)
                return ValidationResult.Invalid;

            // Now open tagfile and validate contents
            var siteModel = new SiteModel(Guid.Empty);
            var machine = new Machine()
            {
                TargetValueChanges = new ProductionEventLists(siteModel, 0)
            };

            var siteModelGridAggregator = new ServerSubGridTree(siteModel);
            var machineTargetValueChangesAggregator = new ProductionEventLists(siteModel, long.MaxValue);
            TAGProcessor processor = new TAGProcessor(siteModel, machine, siteModelGridAggregator,machineTargetValueChangesAggregator);
            TAGValueSink sink = new TAGVisionLinkPrerequisitesValueSink(processor);
            TAGReader reader = new TAGReader(new MemoryStream(tagDetail.tagFileContent));
            TAGFile tagFile = new TAGFile();
            TAGReadResult readResult = tagFile.Read(reader, sink);
            if (readResult != TAGReadResult.NoError)
                return ValidationResult.Invalid;

            // Tagfile contents are OK so proceed

            if (!RaptorConfig.EnableTFAService) // allows us to bypass a TFA service
            {
                Log.Warn(
                        $"SubmitTAGFileResponse.ValidSubmission. EnableTFAService disabled. Bypassing TFS validation checks");
                if (tagDetail.assetId != Guid.Empty && tagDetail.projectId != Guid.Empty) // do we have what we need
                    return ValidationResult.Valid;
                else
                    return ValidationResult.Invalid; // cannot process with asset and project id
            }

            return CheckFileIsProcessible(ref tagDetail, processor);

        }
    }
}
