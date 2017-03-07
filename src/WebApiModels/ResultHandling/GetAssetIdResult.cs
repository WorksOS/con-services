namespace VSS.TagFileAuth.Service.WebApiModels.ResultHandling
{
  /// <summary>
  /// The result of a request to get the legacyAssetID for a radioSerial
  /// </summary>
  public class GetAssetIdResult : ContractExecutionResult
  {
    /// <summary>
    /// The result of the request. True for success and false for failure.
    /// </summary>
    public bool result { get; private set; }

    private long _assetId;
    private int _machineLevel;
    /// <summary>
    /// The id of the asset. -1 if unknown.
    /// </summary>
    public long assetId { get { return _assetId; } private set { _assetId = value; } }

    /// <summary>
    /// The subscription level of the asset. 
    /// Valid values are 0=Unknown, 15=2D Project Monitoring, 16=3D Project Monitoring, 18=Manual 3D Project Monitoring
    /// </summary>
    public int machineLevel { get { return _machineLevel; } private set { _machineLevel = value; } }

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

    /// <summary>
    /// Example for Help
    /// </summary>
    public static GetAssetIdResult HelpSample
    {
      get { return CreateGetAssetIdResult(true, 1892337661625085, 16); }
    }
  }
}