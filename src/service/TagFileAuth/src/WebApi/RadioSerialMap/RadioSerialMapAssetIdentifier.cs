using System;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.RadioSerialMap
{
  /// <summary>
  /// Defines the known short and long asset and project IDs for a radio serial/type key combination
  /// </summary>
  public struct RadioSerialMapAssetIdentifier
  {
    public long assetId;
    public Guid assetUid;
    public long projectId;
    public long projectUid;
  }
}
