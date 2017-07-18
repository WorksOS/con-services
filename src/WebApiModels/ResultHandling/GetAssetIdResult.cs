namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling
{
  /// <summary>
  /// The result of a request to get the legacyAssetID for a radioSerial
  /// </summary>
  public class GetAssetIdResult : ContractExecutionResult
  {
    /// <summary>
    /// The id of the asset. -1 if unknown. 
    /// </summary>
    public long assetId { get; set; }

    /// <summary>
    /// The subscription level of the asset. 
    /// Valid values are 0=Unknown, 15=2D Project Monitoring, 16=3D Project Monitoring, 18=Manual 3D Project Monitoring
    /// </summary>
    public int machineLevel { get; set; }

    /// <summary>
    /// Create instance of GetAssetIdResult
    /// </summary>
    public static GetAssetIdResult CreateGetAssetIdResult(bool result, long assetId, int machineLevel,
      ContractExecutionStatesEnum code = ContractExecutionStatesEnum.ExecutedSuccessfully,
      string message = "success")
    {
      return new GetAssetIdResult
      {
        Result = result,
        assetId = assetId,
        machineLevel = machineLevel,
        Code = code,
        Message = message
      };
    }
  }
}