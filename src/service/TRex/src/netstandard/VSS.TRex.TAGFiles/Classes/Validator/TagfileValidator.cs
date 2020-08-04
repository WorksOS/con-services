using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.TagFileAuth.Abstractions.Interfaces;
using VSS.Productivity3D.TagFileAuth.Models;
using VSS.Productivity3D.TagFileAuth.Models.ResultsHandling;
using VSS.TRex.Common;
using VSS.TRex.Common.Utilities;
using VSS.TRex.DI;
using VSS.TRex.TAGFiles.Executors;
using VSS.TRex.TAGFiles.Models;

namespace VSS.TRex.TAGFiles.Classes.Validator
{
  public static class TagfileValidator
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

    private static bool WarnOnTFAServiceDisabled = true;

    private static readonly int minTagFileLength = DIContext.Obtain<IConfigurationStore>().GetValueInt("MIN_TAGFILE_LENGTH", Consts.kMinTagFileLengthDefault);
    private static readonly bool tfaServiceEnabled = DIContext.Obtain<IConfigurationStore>().GetValueBool("ENABLE_TFA_SERVICE", Consts.ENABLE_TFA_SERVICE);

    /// <summary>
    /// Calls the TFA service via the nuget Proxy,
    ///    to validates licensing etc
    ///      and lookup assetId and projectId 
    /// </summary>
    private static async Task<GetProjectUidsResult> ValidateWithTfa(GetProjectUidsRequest getProjectUidsRequest)
    {
      Log.LogInformation($"#Progress# ValidateWithTFA. Calling TFA service to identify potential project: {JsonConvert.SerializeObject(getProjectUidsRequest)}");
      var tfa = DIContext.Obtain<ITagFileAuthProjectV5Proxy>();

      GetProjectUidsResult tfaResult;
      try
      {
        var customHeaders = new HeaderDictionary();
        tfaResult = await tfa.GetProjectUids(getProjectUidsRequest, customHeaders).ConfigureAwait(false);
      }
      catch (Exception e)
      {
        return new GetProjectUidsResult(string.Empty, string.Empty, string.Empty, (int)TRexTagFileResultCode.TRexTfaException, e.Message);
      }

      return tfaResult;
    }

    /// <summary>
    /// this needs to be public, only for unit tests
    /// </summary>
    public static async Task<GetProjectUidsResult> CheckFileIsProcessable(TagFileDetail tagDetail, TAGFilePreScan preScanState)
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

      var platformSerialNumber = string.Empty;
      if (preScanState.PlatformType >= CWSDeviceTypeEnum.EC520 && preScanState.PlatformType <= CWSDeviceTypeEnum.TMC)
        platformSerialNumber = preScanState.HardwareID;


      // Type C. Do we have what we need already (Most likely test tool submission)
      if (tagDetail.assetId != null && tagDetail.projectId != null)
        if (tagDetail.assetId != Guid.Empty && tagDetail.projectId != Guid.Empty)
          return new GetProjectUidsResult(tagDetail.projectId.ToString(), tagDetail.assetId.ToString(), string.Empty, 0, "success");
      
      if ((tagDetail.projectId == null || tagDetail.projectId == Guid.Empty) && string.IsNullOrEmpty(platformSerialNumber))
      {
        // this is a TFA code. This check is also done as a pre-check as the scenario is very frequent, to avoid the API call overhead.
        var message = "#Progress# CheckFileIsProcessable. Must have either a valid platformSerialNumber or ProjectUID";
        Log.LogWarning(message);
        return new GetProjectUidsResult(tagDetail.projectId.ToString(), tagDetail.assetId.ToString(), string.Empty, (int) TRexTagFileResultCode.TRexMissingProjectUidAndPlatformSerial, message);
      }

      var seedLatitude = MathUtilities.RadiansToDegrees(preScanState.SeedLatitude ?? 0.0);
      var seedLongitude = MathUtilities.RadiansToDegrees(preScanState.SeedLongitude ?? 0.0);
      var seedNorthing = preScanState.SeedNorthing;
      var seedEasting = preScanState.SeedEasting;
      if (Math.Abs(seedLatitude) < Consts.TOLERANCE_DECIMAL_DEGREE && Math.Abs(seedLongitude) < Consts.TOLERANCE_DECIMAL_DEGREE && (seedNorthing == null || seedEasting == null))
      {
        // This check is also done as a pre-check as the scenario is very frequent, to avoid the TFA API call overhead.
        var message = $"#Progress# CheckFileIsProcessable. Unable to determine a tag file seed position. projectID {tagDetail.projectId} serialNumber {platformSerialNumber} filename {tagDetail.tagFileName} Lat {preScanState.SeedLatitude} Long {preScanState.SeedLongitude} northing {preScanState.SeedNorthing} easting {preScanState.SeedNorthing}";
        Log.LogWarning(message);
        return new GetProjectUidsResult(tagDetail.projectId.ToString(), tagDetail.assetId.ToString(), string.Empty, (int) TRexTagFileResultCode.TRexInvalidLatLong, message);
      }

      var tfaRequest = new GetProjectUidsRequest(
        tagDetail.projectId == null ? string.Empty : tagDetail.projectId.ToString(),
        platformSerialNumber, 
        seedLatitude, seedLongitude,
        preScanState.SeedNorthing, preScanState.SeedEasting);
      Log.LogInformation($"#Progress# CheckFileIsProcessable. tfaRequest {JsonConvert.SerializeObject(tfaRequest)}");

      var tfaResult = await ValidateWithTfa(tfaRequest).ConfigureAwait(false);

      Log.LogInformation($"#Progress# CheckFileIsProcessable. TFA validate returned for {tagDetail.tagFileName} tfaResult: {JsonConvert.SerializeObject(tfaResult)}");
      if (tfaResult?.Code == (int)TRexTagFileResultCode.Valid)
      {
        // if not overriding take TFA projectid
        if ((tagDetail.projectId == null || tagDetail.projectId == Guid.Empty) && (Guid.Parse(tfaResult.ProjectUid) != Guid.Empty))
        {
          tagDetail.projectId = Guid.Parse(tfaResult.ProjectUid);
        }

        // take what TFA gives us including an empty guid which is a JohnDoe
        tagDetail.assetId = string.IsNullOrEmpty(tfaResult.DeviceUid) ? Guid.Empty : (Guid.Parse(tfaResult.DeviceUid));

        // Check For JohnDoe machines. if you get a valid pass and no assetid it means it had a manual3dlicense
        if (tagDetail.assetId == Guid.Empty)
          tagDetail.IsJohnDoe = true; // JohnDoe Machine and OK to process
      }

      return tfaResult;
    }

    /// <summary> 
    /// reads and validates tagFile, returning model for multi-use  
    /// </summary>
    public static ContractExecutionResult PreScanTagFile(TagFileDetail tagDetail, out TAGFilePreScan tagFilePreScan)
    {
      tagFilePreScan = new TAGFilePreScan(); 
      
      if (tagDetail.tagFileContent.Length <= minTagFileLength)
        return new ContractExecutionResult((int)TRexTagFileResultCode.TRexInvalidTagfile, TRexTagFileResultCode.TRexInvalidTagfile.ToString());
      
      using (var stream = new MemoryStream(tagDetail.tagFileContent))
        tagFilePreScan.Execute(stream);

      if (tagFilePreScan.ReadResult != TAGReadResult.NoError)
        return new ContractExecutionResult((int)TRexTagFileResultCode.TRexTagFileReaderError, tagFilePreScan.ReadResult.ToString());
      return new ContractExecutionResult((int)TRexTagFileResultCode.Valid);
    }

    /// <summary>
    /// Inputs a tagfile for validation and asset licensing checks
    ///      includes already scanned tagfile
    /// </summary>
    public static async Task<ContractExecutionResult> ValidSubmission(TagFileDetail tagDetail, TAGFilePreScan tagFilePreScan)
    {

      // TAG file contents are OK so proceed
      if (!tfaServiceEnabled) // allows us to bypass a TFA service
      {
        if (WarnOnTFAServiceDisabled)
          Log.LogWarning("SubmitTAGFileResponse.ValidSubmission. EnableTFAService disabled. Bypassing TFS validation checks");

        if (tagDetail.projectId != null && tagDetail.projectId != Guid.Empty) // do we have what we need
        {
          if (tagDetail.assetId == null || tagDetail.assetId == Guid.Empty)
            tagDetail.IsJohnDoe = true;
          return new ContractExecutionResult((int)TRexTagFileResultCode.Valid);
        }

        // cannot process without asset and project id
        return new ContractExecutionResult((int)TRexTagFileResultCode.TRexBadRequestMissingProjectUid, "TRexTagFileResultCode.TRexBadRequestMissingProjectUid");
      }

      // If the TFA service is enabled, but the TAG file has attributes that allow project and asset (or JohnDoe status) to be determined, then
      // allow the TAG file to be processed without additional TFA involvement

      if (tagDetail.projectId != null && tagDetail.projectId != Guid.Empty)
      {
        if ((tagDetail.assetId != null && tagDetail.assetId == Guid.Empty) || tagDetail.IsJohnDoe)
          return new ContractExecutionResult((int)TRexTagFileResultCode.Valid);
      }

      // Contact TFA service to validate tag file details
      var tfaResult = await CheckFileIsProcessable(tagDetail, tagFilePreScan).ConfigureAwait(false);
      return new ContractExecutionResult((int)tfaResult.Code, tfaResult.Message);
    }
  }
}
