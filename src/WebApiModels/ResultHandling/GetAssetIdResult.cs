namespace WebApiModels.ResultHandling
{
  /// <summary>
  /// The result of a request to get the legacyAssetID for a radioSerial
  /// </summary>
  public class GetAssetIdResult : ContractExecutionResult
  {
    private long _assetId;
    private int _machineLevel;

    /// <summary>
    /// The id of the asset. -1 if unknown. 
    /// </summary>
    public long assetId { get { return _assetId; } set { _assetId = value; } } 

    /// <summary>
    /// The subscription level of the asset. 
    /// Valid values are 0=Unknown, 15=2D Project Monitoring, 16=3D Project Monitoring, 18=Manual 3D Project Monitoring
    /// </summary>
    public int machineLevel { get { return _machineLevel; }  set { _machineLevel = value; } } // Removed private as this won't deserialize

    // acceptance tests cannot serialize with a private const.
    //private GetAssetIdResult()
    //{ }

    /// <summary>
    /// Create instance of GetAssetIdResult
    /// </summary>
    public static GetAssetIdResult CreateGetAssetIdResult(bool result, long assetId, int machineLevel)
    {
      return new GetAssetIdResult
      {
        result = result,
        assetId = assetId,
        machineLevel = machineLevel
      };
    }
    
  }
}