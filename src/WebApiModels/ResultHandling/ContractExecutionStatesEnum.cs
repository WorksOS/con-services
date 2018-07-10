namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling
{
  /// <summary>
  ///   Defines standard return codes for a contract.
  /// </summary>
  public class ContractExecutionStatesEnum : GenericEnum<ContractExecutionStatesEnum, int>
  {
    public ContractExecutionStatesEnum()
    {
      DynamicAddwithOffset("success", 0);
      DynamicAddwithOffset("TccOrgId, if present, must be a valid Guid.", 1);
      DynamicAddwithOffset("AssetId, if present, must be >= -1", 2);
      DynamicAddwithOffset("ProjectId, if present, must be -1, -2, -3 or > 0", 3);
      DynamicAddwithOffset("Must have valid error number", 4);
      DynamicAddwithOffset("Must have tag file name", 5);
      DynamicAddwithOffset("TagFileName invalid as no DisplaySerialNumber", 6);
      DynamicAddwithOffset("TagFileName invalid as no MachineName", 7);
      DynamicAddwithOffset("TagFileName invalid as no valid CreatedUtc", 8);
      DynamicAddwithOffset("Must have assetId", 9);
      DynamicAddwithOffset("Radio Serial is invalid", 10);
      DynamicAddwithOffset("TagFileProcessingErrorV1Executor: Invalid request structure", 11);
      DynamicAddwithOffset("TagFileProcessingErrorV1Executor: Failed to create an alert for tag file processing error",   12);
      DynamicAddwithOffset("TagFileProcessingErrorV2Executor: Invalid request structure", 13);
      DynamicAddwithOffset("TagFileProcessingErrorV2Executor: Failed to create an alert for tag file processing error", 14);
      DynamicAddwithOffset("Failed to get legacy asset id", 15);
      DynamicAddwithOffset("Failed to get project boundaries", 16);
      DynamicAddwithOffset("TagFileUTC must have occurred within last 50 years", 17);
      DynamicAddwithOffset("Must have projectId", 18);
      DynamicAddwithOffset("Failed to get project id", 19);
      DynamicAddwithOffset("Must contain one or more of assetId or tccOrgId", 20);
      DynamicAddwithOffset("Latitude should be between -90 degrees and 90 degrees", 21);
      DynamicAddwithOffset("Longitude should be between -180 degrees and 180 degrees", 22);
      DynamicAddwithOffset("TimeOfPosition must have occurred within last 50 years", 23);
      DynamicAddwithOffset("Must have assetId and/or projectID", 24);
      DynamicAddwithOffset("AssetId must have valid deviceType", 25);
      DynamicAddwithOffset("A manual/unknown deviceType must have a projectID", 26);
      DynamicAddwithOffset("Failed to get project boundary", 27);
      DynamicAddwithOffset("A problem occurred accessing database. Exception: {0}", 28);
      DynamicAddwithOffset("Unable to identify any projects", 29);
      DynamicAddwithOffset("DeviceType is invalid", 30);
      DynamicAddwithOffset("Unable to create Kafka event. Reason: {0}.", 31);
      DynamicAddwithOffset("Multiple projects found", 32);
      DynamicAddwithOffset("Unable to identify RadioSerial in the 3dPM system", 33);
      DynamicAddwithOffset("Failed to get project uid", 34);
      DynamicAddwithOffset("GetProjectUid internal error", 35);
      DynamicAddwithOffset("ProjectUid is present, but invalid", 36);
      DynamicAddwithOffset("Either Radio Serial or TCCOrgId must be provided", 37);
      DynamicAddwithOffset("Unable to find the Project requested", 38);
      DynamicAddwithOffset("Manual Import unable to locate any valid subscriptions", 39);
      DynamicAddwithOffset("Manual Import unable to locate any valid projectSubscriptions or locate asset", 40);
      DynamicAddwithOffset("Manual Import matches incorrect number of projects: {0}", 41);
      DynamicAddwithOffset("Manual Import project does not intersect location and time provided", 42);
    }

    /// <summary>
    /// The execution result offset to create dynamically add custom errors
    /// </summary>
    private const int executionResultOffset = 3000;

    /// <summary>
    ///   Service request executed successfully
    /// </summary>
    public static readonly int ExecutedSuccessfully = 0;


    /// <summary>
    ///   Supplied data didn't pass validation
    /// </summary>
    public static readonly int ValidationError = -1;

    /// <summary>
    ///   Serializing request erors
    /// </summary>
    public static readonly int SerializationError = -2;

    /// <summary>
    ///   Internal processing error
    /// </summary>
    public static readonly int InternalProcessingError = -3;


    /// <summary>
    /// Dynamically adds new error messages addwith offset.
    /// </summary>
    /// <param name="name">The name of error.</param>
    /// <param name="value">The value of code.</param>
    public void DynamicAddwithOffset(string name, int value)
    {
      DynamicAdd(name, value + executionResultOffset);
    }

    /// <summary>
    /// Gets the error numberwith offset.
    /// </summary>
    /// <param name="errorNum">The error number.</param>
    /// <returns></returns>
    public int GetErrorNumberwithOffset(int errorNum)
    {
      return errorNum + executionResultOffset;
    }

    /// <summary>
    /// Gets the frist available name of a error code taking into account 
    /// </summary>
    /// <param name="value">The code vale to get the name against.</param>
    /// <returns></returns>
    public string FirstNameWithOffset(int value)
    {
      return FirstNameWith(value + executionResultOffset);
    }
  }
}
