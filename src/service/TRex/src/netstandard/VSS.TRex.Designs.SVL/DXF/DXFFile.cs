using System.Collections.Generic;
using System.IO;
using VSS.TRex.Geometry;

namespace VSS.TRex.Designs.SVL.DXF
{
  /// <summary>
  /// Represents a simple DXF file writer
  /// </summary>
  public class DXFFile
  {
    public List<DXFEntity> Entities = new List<DXFEntity>();
    public List<string> Layers = new List<string>();

    public DistanceUnitsType OutputUnits = DistanceUnitsType.Meters; // Default to meters

    public DXFFile()
    {
    }

    public void WriteHeaderSection(StreamWriter writer)
    {
      DXFUtils.WriteDXFRecord(writer, 0, "SECTION");
      DXFUtils.WriteDXFRecord(writer, 2, "HEADER");

      /*
      DXFUtils.WriteDXFRecord(writer, 9, "$LIMMIN");
      DXFUtils.WriteDXFRecord(writer, 10, DXFUtils.NoLocaleFloatToStrF(DXFUtils.DXFDistance(FLimMinX, FOutputUnits), 6));
      DXFUtils.WriteDXFRecord(writer, 20, DXFUtils.NoLocaleFloatToStrF(DXFUtils.DXFDistance(FLimMinY, FOutputUnits), 6));

      DXFUtils.WriteDXFRecord(writer, 9, "$LIMMAX");
      DXFUtils.WriteDXFRecord(writer, 10, DXFUtils.NoLocaleFloatToStrF(DXFUtils.DXFDistance(FLimMaxX, FOutputUnits), 6));
      DXFUtils.WriteDXFRecord(writer, 20, DXFUtils.NoLocaleFloatToStrF(DXFUtils.DXFDistance(FLimMaxY, FOutputUnits), 6));
      */

      DXFUtils.WriteDXFRecord(writer, 0, "ENDSEC");
    }

    public void WriteTablesSection(StreamWriter writer)
    {
      DXFUtils.WriteDXFRecord(writer, 0, "SECTION");
      DXFUtils.WriteDXFRecord(writer, 2, "TABLES");
      WriteLayers(writer);
      //WriteStyles(writer);
      DXFUtils.WriteDXFRecord(writer, 0, "ENDSEC");
    }

    public void WriteLayers(StreamWriter writer)
    {
      if (Layers.Count == 0)
        return;

      DXFUtils.WriteDXFRecord(writer, 0, "TABLE");
      DXFUtils.WriteDXFRecord(writer, 2, "LAYER");
      for (int I = 0; I < Layers.Count; I++)
      {
        DXFUtils.WriteDXFRecord(writer, 0, "LAYER");
        DXFUtils.WriteDXFRecord(writer, 2, DXFUtils.DXFiseLayerName(Layers[I]));
        DXFUtils.WriteDXFRecord(writer, 70, "0");
        DXFUtils.WriteDXFRecord(writer, 62, "7");
        DXFUtils.WriteDXFRecord(writer, 6, "CONTINUOUS");
      }
      DXFUtils.WriteDXFRecord(writer, 0, "ENDTAB");
    }

    public void WriteEntitiesSection(StreamWriter writer)
    {
      DXFUtils.WriteDXFRecord(writer, 0, "SECTION");
      DXFUtils.WriteDXFRecord(writer, 2, "ENTITIES");

      foreach (var entity in Entities)
        entity.SaveToFile(writer, OutputUnits);

      DXFUtils.WriteDXFRecord(writer, 0, "ENDSEC");
    }

    public void SaveToFile(StreamWriter writer)
    {
      // Construct the styles list from the text entities we are to write out
      // ProcessStyles;

      WriteHeaderSection(writer);
      WriteTablesSection(writer);
      WriteEntitiesSection(writer);
        
      DXFUtils.WriteDXFRecord(writer, 0, "EOF");
    }
  }
}
