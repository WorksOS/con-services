using System.Drawing;
using Apache.Ignite.Core.Binary;

namespace VSS.TRex.Rendering.Palettes
{
  /// <summary>
  /// Transition represents a point on a value line being visualised where a new color starts being used to render the interval of values above the transition value
  /// </summary>
  public struct Transition
  {
    public double Value;
    public Color Color;

    public Transition(double value, Color color)
    {
      Value = value;
      Color = color;
    }

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public void ToBinary(IBinaryRawWriter writer)
    {
      writer.WriteDouble(Value);
      writer.WriteInt(Color.ToArgb());
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public void FromBinary(IBinaryRawReader reader)
    {
      Value = reader.ReadDouble();
      Color = Color.FromArgb(reader.ReadInt());
    }

  }
}
