using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.Rendering.Palettes.Interfaces;
using Draw = System.Drawing;

namespace VSS.TRex.Rendering.Palettes
{
  // A basic palette class that defines a set of transitions covering a value range being rendered
  public class PaletteBase : IPlanViewPalette, IFromToBinary
  {
    private const byte VERSION_NUMBER = 1;

    public PaletteBase(Transition[] transitions)
    {
      PaletteTransitions = transitions;
    }

    // The set of transition value/colour pairs defining a render-able value range
    public Transition[] PaletteTransitions { get; set; }

    /// <summary>
    /// Logic to choose a colour from the set of transitions depending on the value. Slow but simple for the POC...
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public Draw.Color ChooseColour(float value)
    {
      var color = Draw.Color.Empty;

      for (var i = PaletteTransitions.Length - 1; i >= 0; i--)
      {
        if (value >= PaletteTransitions[i].Value)
        {
          color = PaletteTransitions[i].Color;
          break;
        }
      }

      return color;
    }

    /// <summary>
    /// Cutfill palette logic
    /// </summary>
    public Draw.Color ChooseCutFillColour(float value)
    {
      // Cutfill pallete is in descending order and typically will be 7 colours with a zero centre point value
      // value either side of centre point is the torrence for a match in volume
      var color = Draw.Color.Empty;

      int ptr;
      if (value < 0)
        ptr = PaletteTransitions.Length - 1; // move up palette
      else
        ptr = 0; // move down palette

      for (var i = PaletteTransitions.Length - 1; i >= 0; i--)
      {

        if (value == 0)
        {
          if (value == PaletteTransitions[ptr].Value)
          {
            color = PaletteTransitions[ptr].Color;
            break;
          }
        }
        else if (value > 0)
        {
          if (value >= PaletteTransitions[ptr].Value && PaletteTransitions[ptr].Value >= 0)
          {
            color = PaletteTransitions[ptr].Color;
            break;
          }
        }
        else
        if (value <= PaletteTransitions[ptr].Value)
        {
          color = PaletteTransitions[ptr].Color;
          break;
        }

        if (value < 0)
          ptr--;
        else
          ptr++;

      }

      return color;
    }


    /// <summary>
    /// Logic to choose a colour from the set of transitions depending on the value. Slow but simple for the POC...
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public Draw.Color ChooseColour(double value)
    {
      var color = Draw.Color.Empty;

      for (var i = PaletteTransitions.Length - 1; i >= 0; i--)
      {
        if (value >= PaletteTransitions[i].Value)
        {
          color = PaletteTransitions[i].Color;
          break;
        }
      }

      return color;
    }

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public virtual void ToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteBoolean(PaletteTransitions != null);

      if (PaletteTransitions != null)
      {
        writer.WriteInt(PaletteTransitions.Length);

        foreach (var transition in PaletteTransitions)
          transition.ToBinary(writer);
      }
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public virtual void FromBinary(IBinaryRawReader reader)
    {
      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (reader.ReadBoolean())
      {
        var numberOfTransitions = reader.ReadInt();

        PaletteTransitions = new Transition[numberOfTransitions];

        for (var i = 0; i < PaletteTransitions.Length; i++)
          PaletteTransitions[i].FromBinary(reader);
      }
    }
  }
}
