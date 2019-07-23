using Apache.Ignite.Core.Binary;
using VSS.Productivity3D.Models.Models.Reports;
using VSS.TRex.Common;


namespace VSS.TRex.QuantizedMesh.GridFabric.Responses
{
  public class ElevationGridResponse : SubGridsPipelinedResponseBase
  {
    private static byte VERSION_NUMBER = 1;

    public ReportReturnCode ReturnCode; // == TRaptorReportReturnCode
    public ReportType ReportType;       // == TRaptorReportType
 //   public List<GriddedElevDataRow> GriddedElevDataRowList;

    public ElevationGridResponse()
    {
      Clear();
    }

    public void Clear()
    {
      ReturnCode = ReportReturnCode.NoError;
      ReportType = ReportType.Gridded;
   //   GriddedElevDataRowList = new List<GriddedElevDataRow>();
    }

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteInt((int)ReturnCode);
      writer.WriteInt((int)ReportType);
      /*
      writer.WriteInt(GriddedElevDataRowList.Count);
      for (int i = 0; i < GriddedElevDataRowList.Count; i++)
      {
        GriddedElevDataRowList[i].ToBinary(writer);
      }
      */
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      ReturnCode = (ReportReturnCode)reader.ReadInt();
      ReportType = (ReportType)reader.ReadInt();
      /*
      var griddedRowsCount = reader.ReadInt();
      GriddedElevDataRowList = new List<GriddedElevDataRow>();
      for (int i = 0; i < griddedRowsCount; i++)
      {
        var row = new GriddedElevDataRow();
        row.FromBinary(reader);
        GriddedElevDataRowList.Add(row);
      }
      */
    }
  }
}
