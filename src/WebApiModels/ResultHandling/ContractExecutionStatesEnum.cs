using System;
using System.Collections.Generic;
using System.Text;

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
      DynamicAddwithOffset("Not enough information to identify Customer", 10);
      DynamicAddwithOffset("TagFileProcessingErrorV1Executor: Invalid request structure", 11);
      DynamicAddwithOffset("TagFileProcessingErrorV1Executor: Failed to create an alert for tag file processing error", 12);
      DynamicAddwithOffset("TagFileProcessingErrorV2Executor: Invalid request structure", 13);
      DynamicAddwithOffset("TagFileProcessingErrorV2Executor: Failed to create an alert for tag file processing error", 14);
      //DynamicAddwithOffset(, 0);
      //DynamicAddwithOffset(, 0);
      //DynamicAddwithOffset(, 0);
      //DynamicAddwithOffset(, 0);
      //DynamicAddwithOffset(, 0);
    }

    /// <summary>
    /// The execution result offset to create dynamically add custom errors
    /// </summary>
    private const int executionResultOffset = 0;

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
