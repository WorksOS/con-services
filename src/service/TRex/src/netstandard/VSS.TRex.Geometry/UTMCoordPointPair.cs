namespace VSS.TRex.Geometry
{
  /// <summary>
  /// Describes a pait of 3D point positions (X, Y and Z dimensions in NEE space) with an attached UTM zone that
  /// are used describe the measured UTM positions of a machine implement (blade or drum etc)
  /// </summary>
  public struct UTMCoordPointPair
  {
    /// <summary>
    /// Left machine implement position
    /// </summary>
    public XYZ Left;

    /// <summary>
    /// Right machine implement position
    /// </summary>
    public XYZ Right;

    /// <summary>
    /// Universal Transverse Mercartor xone projection that left and right positions are measured in.
    /// </summary>
    public byte UTMZone;

    /// <summary>
    /// Constructor taking the left, right positions and zone information
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <param name="uTMZone"></param>
    public UTMCoordPointPair(XYZ left, XYZ right, byte uTMZone)
    {
      Left = left;
      Right = right;
      UTMZone = uTMZone;
    }

    /// <summary>
    /// Returns a new instance with all fields set to 'null'
    /// </summary>
    public static UTMCoordPointPair Null => new UTMCoordPointPair(XYZ.Null, XYZ.Null, byte.MaxValue);
  }
}
