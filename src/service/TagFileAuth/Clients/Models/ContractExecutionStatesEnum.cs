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
      DynamicAddwithOffset("A problem occurred accessing a service. Service: {0} Exception: {1}", 17);
      DynamicAddwithOffset("Manual Import: Unable to determine lat/long from northing/easting position", 18);
      DynamicAddwithOffset("Latitude should be between -90 degrees and 90 degrees", 21);
      DynamicAddwithOffset("Longitude should be between -180 degrees and 180 degrees", 22);
      DynamicAddwithOffset("ProjectUid is present, but invalid", 36);
      DynamicAddwithOffset("Platform serial number must be provided", 37);
      DynamicAddwithOffset("Manual Import: Unable to find the Project requested", 38);
      DynamicAddwithOffset("Manual Import: project does not intersect the location provided", 41);
      DynamicAddwithOffset("Manual Import: cannot import to an archived project", 43);
      DynamicAddwithOffset("No projects found at the location provided", 44);
      DynamicAddwithOffset("Unable to identify the device by this serialNumber", 47);
      DynamicAddwithOffset("No projects found for this device", 48);
      DynamicAddwithOffset("More than 1 project meets the location requirements", 49);
      DynamicAddwithOffset("Must contain a EC520 serial number", 51); // earthworks only
      DynamicAddwithOffset("Manual Import: cannot import to a project which doesn't accept tag files", 53);

      // these error numbers come from calls to projectService
      DynamicAddwithOffset("Unable to locate device by serialNumber in cws", 100);
      DynamicAddwithOffset("Unable to locate any account for the device in cws", 102);
      DynamicAddwithOffset("There is >1 active account for the device in cws", 103);
      DynamicAddwithOffset("Unable to locate projects for device in cws", 105);
      DynamicAddwithOffset("A problem occurred at the {0} endpoint. Exception: {1}", 124);
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
