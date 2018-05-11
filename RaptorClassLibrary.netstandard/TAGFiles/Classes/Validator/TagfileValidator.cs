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


        private static bool CheckFileIsProcessible(TagfileDetail tagDetail, TAGProcessor processor)
        {

            return true; //for now

            long ProjectIDOverride;
            Guid AssetID = Guid.Empty;
            DeviceType RadioType;
            MachineLevel MachineLevel;
            bool AssetIsAJohnDoe;
            string TCCOrgID = tagDetail.tccOrgId;


            //todo Call new TFA endpoint to validate
            TFAProxy tfa = new TFAProxy();

       /*     
            var res = tfa.ValidateTagfile(etc etc projectUID, assetUID);
            if (res == OK)
                return true;
            else
            {
                todo
            }
            */
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

            if (!(tagDetail.projectId > 0 || tagDetail.projectId == -1)) // todo will be guid soon so probably remove check. Guid.Empty will be like -1
                return ValidationResult.BadRequest;

            // Now open tagfile and validate
            var siteModel = new SiteModel(-1);
            var machine = new Machine()
            {
                TargetValueChanges = new ProductionEventLists(siteModel, 0)
            };

            var siteModelGridAggregator = new ServerSubGridTree(siteModel);
            var machineTargetValueChangesAggregator = new ProductionEventLists(siteModel, long.MaxValue);
            TAGProcessor processor = new TAGProcessor(siteModel, machine, siteModelGridAggregator,
                    machineTargetValueChangesAggregator);
            TAGValueSink sink = new TAGVisionLinkPrerequisitesValueSink(processor);
            TAGReader reader = new TAGReader(new MemoryStream(tagDetail.tagFileContent));
            TAGFile tagFile = new TAGFile();
            TAGReadResult readResult = tagFile.Read(reader, sink);
            if (readResult != TAGReadResult.NoError)
                return ValidationResult.Invalid;


            if (!RaptorConfig.EnableTFAService)
            {
                Log.Warn($"SubmitTAGFileResponse.ValidSubmission. EnableTFAService disabled. Bypassing TFS validation checks");
                return ValidationResult.Valid;
            }

            // todo return proper error code. like TFA down etc
            return CheckFileIsProcessible(tagDetail, processor)
                    ? ValidationResult.Valid
                    : ValidationResult.NotProcessible;

        }
    }
}
