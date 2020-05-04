using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.TagFileAuth.Models
{
  /// <summary>
  ///   Defines standard return codes for a contract.
  /// </summary>
  public class ContractExecutionStatesEnum : GenericEnum<ContractExecutionStatesEnum, int>
  {
    public ContractExecutionStatesEnum()
    {
      DynamicAddwithOffset("success", 0);
      DynamicAddwithOffset("Manual Import: The Devices account cannot have not have a free device entitlement.", 1);
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
      DynamicAddwithOffset("A problem occurred accessing a service. Service: {0} Exception: {1}", 17);
      DynamicAddwithOffset("Must have projectId", 18);
      DynamicAddwithOffset("Failed to get project id", 19);
      DynamicAddwithOffset("Must contain one or more of assetId or tccOrgId", 20);
      DynamicAddwithOffset("Latitude should be between -90 degrees and 90 degrees", 21);
      DynamicAddwithOffset("Longitude should be between -180 degrees and 180 degrees", 22);
      DynamicAddwithOffset("TimeOfPosition must have occurred between 50 years ago and the next 2 days", 23);
      DynamicAddwithOffset("Must have serialNumber and/or projectID", 24);
      DynamicAddwithOffset("AssetId must have valid deviceType", 25);
      DynamicAddwithOffset("A manual/unknown deviceType must have a projectID", 26);
      DynamicAddwithOffset("Failed to get project boundary", 27);
      DynamicAddwithOffset("A problem occurred accessing database. Exception: {0}", 28);
      DynamicAddwithOffset("Unable to identify any projects", 29);
      DynamicAddwithOffset("DeviceType is invalid", 30);
      DynamicAddwithOffset("Manual Import: The Projects account cannot have not have a free device entitlement.", 31);
      DynamicAddwithOffset("Multiple projects found", 32);
      DynamicAddwithOffset("Unable to locate device by the EC or RadioSerial", 33);
      DynamicAddwithOffset("Failed to get project uid", 34);
      DynamicAddwithOffset("GetProjectUid internal error", 35);
      DynamicAddwithOffset("ProjectUid is present, but invalid", 36);
      DynamicAddwithOffset("Auto Import: Either Radio Serial or ec520 Serial must be provided", 37);
      DynamicAddwithOffset("Unable to find the Project requested", 38);
      DynamicAddwithOffset("Manual Import: got asset. Unable to locate any valid project, or asset subscriptions", 39);
      DynamicAddwithOffset("Manual Import: no asset provided or identifiable. Unable to locate any valid project subscriptions", 40);
      DynamicAddwithOffset("Manual Import: no intersecting projects found", 41);
      DynamicAddwithOffset("Manual Import: project does not intersect location and time provided", 42);
      DynamicAddwithOffset("Manual Import: cannot import to an archived project", 43);
      DynamicAddwithOffset("No projects found at the location provided", 44);
      DynamicAddwithOffset("Projects found at the location provided, however the device does not have access to it/those", 45);
      DynamicAddwithOffset("Manual Import: internal unhandled path", 46);
      DynamicAddwithOffset("Auto Import: unable to identify the device by this serialNumber", 47);
      DynamicAddwithOffset("unused", 48);
      DynamicAddwithOffset("More than 1 project meets the location requirements", 49);
      DynamicAddwithOffset("Manual Import: intersecting projects found, but not the one requested", 50);
      DynamicAddwithOffset("Must contain a EC520 serial number", 51);
      DynamicAddwithOffset("Asset found, but has no valid subscriptions", 52);

      // these error numbers come from calls to projectService
      DynamicAddwithOffset("Unable to locate device by serialNumber in cws", 100);
      DynamicAddwithOffset("Unable to locate device in localDB", 101);
      DynamicAddwithOffset("Unable to locate any account for the device in cws", 102);
      DynamicAddwithOffset("There is >1 active account for the device in cws", 103);
      DynamicAddwithOffset("A problem occurred at the {0} endpoint. Exception: {1}", 104);
      DynamicAddwithOffset("Unable to locate projects for device in cws", 105);
      DynamicAddwithOffset("Project accountId differs between WorksOS and WorksManager", 106);
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
    /// Gets the error number with offset.
    /// </summary>
    /// <param name="errorNum">The error number.</param>
    /// <returns></returns>
    public int GetErrorNumberwithOffset(int errorNum)
    {
      return errorNum + executionResultOffset;
    }

    /// <summary>
    /// Gets the first available name of a error code taking into account 
    /// </summary>
    /// <param name="value">The code vale to get the name against.</param>
    /// <returns></returns>
    public string FirstNameWithOffset(int value)
    {
      return FirstNameWith(value + executionResultOffset);
    }
  }
}
