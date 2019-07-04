using System.Collections.Generic;
using System.IO;
using System.Text;
using Apache.Ignite.Core.Binary;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Common;
using VSS.TRex.Common.Utilities.ExtensionMethods;
using VSS.TRex.IO.Helpers;
using VSS.TRex.SubGridTrees.Client.Types;

namespace VSS.TRex.CellDatum.GridFabric.Responses
{
  public class CellPassesResponse : BaseRequestResponse
  {
    private const byte VERSION_NUMBER = 1;

    /// <summary>
    /// The cell Passes
    /// </summary>
    public List<ClientCellProfileLeafSubgridRecord> CellPasses { get; set; }

    /// <summary>
    /// The internal result code resulting from the request.
    /// </summary>
    public CellPassesReturnCode ReturnCode { get; set; } 

    /// <summary>
    /// Northing ordinate of the cell datum requested
    /// </summary>
    public double Northing { get; set; }

    /// <summary>
    /// Easting ordinate of the cells requested
    /// </summary>
    public double Easting { get; set; }

    public override void ToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);
      
      writer.WriteInt((int)ReturnCode);
      writer.WriteDouble(Northing);
      writer.WriteDouble(Easting);
      writer.WriteInt(CellPasses.Count);

      using (var ms = RecyclableMemoryStreamManagerHelper.Manager.GetStream())
      {
        using (var bw = new BinaryWriter(ms, Encoding.UTF8, true))
        {
          CellPasses.ForEach(result =>
          {
            result.Write(bw);
          });
        }

        var bytes = ms.ToArray();
        writer.WriteByteArray(bytes);
      }
    }

    public override void FromBinary(IBinaryRawReader reader)
    {
      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      ReturnCode = (CellPassesReturnCode) reader.ReadInt();
      Northing = reader.ReadDouble();
      Easting = reader.ReadDouble();

      var numResults = reader.ReadInt();
      CellPasses = new List<ClientCellProfileLeafSubgridRecord>(numResults);
      var bytes = reader.ReadByteArray();
      using (var ms = new MemoryStream(bytes))
      {
        using (var br = new BinaryReader(ms, Encoding.UTF8, true))
        {
          for (var i = 0; i < numResults; i++)
          {
            var record = new ClientCellProfileLeafSubgridRecord();
            record.Read(br);
            CellPasses.Add(record);
          }
        }
      }
    }

    public CellPassesResponse()
    {
      CellPasses = new List<ClientCellProfileLeafSubgridRecord>();
    }
  }
}
