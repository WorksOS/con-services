namespace VSS.Productivity3D.TagFileAuth.WebAPI.RadioSerialMap
{
  /// <summary>
  /// Interface providing access to radio serial/type asset/project mapping
  /// </summary>
  public interface ICustomRadioSerialProjectMap
  {
    /// <summary>
    /// Locate a defined mapping for a radio serial/type and asset/project combination
    /// </summary>
    bool LocateAsset(string radioSerial, string radioType, out RadioSerialMapAssetIdentifier radioSerialMapAssetIdentifier);
  }
}
