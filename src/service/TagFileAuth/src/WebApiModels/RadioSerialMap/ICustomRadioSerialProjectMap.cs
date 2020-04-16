namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.RadioSerialMap
{
  /// <summary>
  /// Interface providing access to radio serial/type asset/project mapping
  /// </summary>
  public interface ICustomRadioSerialProjectMap
  {
    /// <summary>
    /// Locate a defined mapping for a radio serial/type and asset/project combination
    /// </summary>
    bool LocateAsset(string radioSerial, int deviceType, out RadioSerialMapAssetIdentifier radioSerialMapAssetIdentifier);
  }
}
