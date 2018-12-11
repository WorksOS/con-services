using Apache.Ignite.Core.Binary;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.Profiling.Interfaces;

namespace VSS.TRex.Profiling
{
  public class ProfileCellBase : IProfileCellBase, IFromToBinary
  {
    /// <summary>
    /// The real-world distance from the 'start' of the profile line drawn by the user;
    /// this is used to ensure that the client GUI correctly aligns the profile
    /// information drawn in the Long Section view with the profile line on the Plan View.
    /// </summary>
    public double Station { get; set; }

    /// <summary>
    /// The real-world length of that part of the profile line which crosses the underlying cell;
    /// used to determine the width of the profile column as displayed in the client GUI
    /// </summary>
    public double InterceptLength { get; set; }

    /// <summary>
    /// OTGCellX, OTGCellY is the on the ground index of the this particular grid cell
    /// </summary>
    public uint OTGCellX { get; set; }

    /// <summary>
    /// OTGCellX, OTGCellY is the on the ground index of the this particular grid cell
    /// </summary>
    public uint OTGCellY { get; set; }

    public float DesignElev { get; set; }

    public virtual bool IsNull() => false;

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public void ToBinary(IBinaryRawWriter writer)
    {
      writer.WriteDouble(Station);
      writer.WriteDouble(InterceptLength);

      writer.WriteInt((int)OTGCellX);
      writer.WriteInt((int)OTGCellY);

      writer.WriteFloat(DesignElev);
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public void FromBinary(IBinaryRawReader reader)
    {
      Station = reader.ReadDouble();
      InterceptLength = reader.ReadDouble();

      OTGCellX = (uint)reader.ReadInt();
      OTGCellY = (uint)reader.ReadInt();

      DesignElev = reader.ReadFloat();
    }
  }
}
