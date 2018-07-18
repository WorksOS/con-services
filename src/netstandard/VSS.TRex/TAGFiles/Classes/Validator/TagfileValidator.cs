using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VSS.TRex.DI;
using VSS.TRex.Events;
using VSS.TRex.Machines;
using VSS.TRex.SiteModels;
using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.TAGFiles.Classes.Processors;
using VSS.TRex.TAGFiles.Classes.Sinks;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.Validator
{
  public static class TagfileValidator
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

    /// <summary>
    /// Calls the TFA service to lookup assetId and projectId and validates licensing etc
    /// </summary>
    /// <param name="tagDetail"></param>
    /// <param name="processor"></param>
    /// <returns></returns>
    private static ValidationResult CheckFileIsProcessible(TagFileDetail tagDetail, TAGProcessor processor)
    {

      /*
      Three different types of tagfile submission
      Type A: Automatic submission.
      This is where a tagfile comes in from a known org and the system works out what Asset and Project it belongs to. Licensing is checked

      Type B:  Manual submission.
      This is where the project is known and supplied as an override projectid value. The Asset is worked out via TFA service or assigned a JohnDoe id if not known. 
      Licensing is checked for manual subscription

      Type C: Override submission.
      This is where the ProjectId and AssetId is both supplied. It bypasses TFA service and providing the tagfile is valid, is processed straight into the project.
      This is not a typical submission but is handy for testing and in a situation where a known third party source other than NG could determine the AssetId and Project. Typical NG users could not submit via this method thus avoiding our license check. 

       */

      // Type C. Do we have what we need already (Most likley test tool submission)
      if (tagDetail.assetId != null && tagDetail.projectId != null)
        if (tagDetail.assetId != Guid.Empty && tagDetail.projectId != Guid.Empty)
          return ValidationResult.Valid;

      // Business rule for device type conversion
      int radioType = processor.RadioType == "torch" ? (int)DeviceType.SNM940 : (int)DeviceType.ManualDevice; // torch device set to type 6

      ITFAProxy tfa = DIContext.Obtain<ITFAProxy>();
      Log.LogInformation($"Calling TFA servce to validate tagfile:{tagDetail.tagFileName}");

      // use decimal degrees when calling API
      var apiResult = tfa.ValidateTagfile(tagDetail.projectId, Guid.Parse(tagDetail.tccOrgId), processor.RadioSerial, radioType, processor.LLHLat * (180 / Math.PI), processor.LLHLon * (180 / Math.PI), processor.DataTime, ref tagDetail.projectId, out tagDetail.assetId);
      Log.LogInformation($"TFA GetId returned for {tagDetail.tagFileName} StatusCode: {apiResult}, ProjectId:{tagDetail.projectId}, AssetId:{tagDetail.assetId}");
      if (apiResult == ValidationResult.Valid)
      {
        // Check For JohnDoe machines. if you get a valid pass and no assetid it means it had a manual3dlicense
        if (tagDetail.assetId == Guid.Empty)
          tagDetail.IsJohnDoe = true; // JohnDoe Machine and OK to process
      }
      return apiResult;
    }

    /// <summary>
    /// Inputs a tagfile for validation and asset licensing checks
    /// </summary>
    /// <param name="tagDetail"></param>
    /// <returns></returns>
    public static ValidationResult ValidSubmission(TagFileDetail tagDetail)
    {
      // Perform some Validation Checks 
      ValidationResult result = ValidationResult.Unknown;

      // get our settings
      IConfiguration config = DIContext.Obtain<IConfiguration>();
      int MinTAGFileLength = config.GetValue<int>("MinTAGFileLength", 100);
      bool TFAServiceEnabled = config.GetValue<bool>("EnableTFAService", true);

      if (tagDetail.tagFileContent.Length <= MinTAGFileLength)
        return ValidationResult.InvalidTagfile;

      // Now open tagfile and validate contents
      var siteModel = new SiteModel(Guid.Empty);
      var machine = new Machine()
      {
        TargetValueChanges = new ProductionEventLists(siteModel, Machine.kNullInternalSiteModelMachineIndex)
      };
      var siteModelGridAggregator = new ServerSubGridTree(siteModel);
      var machineTargetValueChangesAggregator = new ProductionEventLists(siteModel, Machine.kNullInternalSiteModelMachineIndex);
      TAGProcessor processor = new TAGProcessor(siteModel, machine, siteModelGridAggregator, machineTargetValueChangesAggregator);
      TAGValueSink sink = new TAGVisionLinkPrerequisitesValueSink(processor);
      TAGReader reader = new TAGReader(new MemoryStream(tagDetail.tagFileContent));
      TAGFile tagFile = new TAGFile();

      TAGReadResult readResult = tagFile.Read(reader, sink);
      if (readResult != TAGReadResult.NoError)
        return ValidationResult.InvalidTagfile; //Exit if failed content validation

      // Tagfile contents are OK so proceed

      if (!TFAServiceEnabled) // allows us to bypass a TFA service
      {
        Log.LogWarning($"SubmitTAGFileResponse.ValidSubmission. EnableTFAService disabled. Bypassing TFS validation checks");
        if (tagDetail.projectId != Guid.Empty) // do we have what we need
        {
          if (tagDetail.assetId == Guid.Empty)
            tagDetail.IsJohnDoe = true;
          return ValidationResult.Valid;
        }
        else
          return ValidationResult.BadRequest; // cannot process without asset and project id
      }

      return CheckFileIsProcessible(tagDetail, processor);

    }
  }
}
