namespace VSS.TRex.QuantizedMesh.Models
{
  public struct GriddedElevDataRow
  {
    public double Northing { get; set; }
    public double Easting { get; set; }
    public float Elevation { get; set; }

    public GriddedElevDataRow(double northing, double easting, float elevation)
    {
      Northing = northing;
      Easting = easting;
      Elevation = elevation;
    }
/*
    public void Write(BinaryWriter writer)
    {
      writer.Write(Northing);
      writer.Write(Easting);
      writer.Write(Elevation);
    }

    public void Read(BinaryReader reader)
    {
      Northing = reader.ReadDouble();
      Easting = reader.ReadDouble();
      Elevation = reader.ReadSingle();
    }

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public void ToBinary(IBinaryRawWriter writer)
    {
      writer.WriteDouble(Northing);
      writer.WriteDouble(Easting);
      writer.WriteDouble(Elevation);
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public void FromBinary(IBinaryRawReader reader)
    {
      Northing = reader.ReadDouble();
      Easting = reader.ReadDouble();
      Elevation = reader.ReadDouble();
    }
    */
  }
}
