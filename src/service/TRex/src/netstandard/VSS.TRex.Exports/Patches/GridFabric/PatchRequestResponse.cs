using System.Collections.Generic;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.DI;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Exports.Patches.GridFabric
{
  /// <summary>
  /// The response returned from the Patches request executor that contains the response code and the set of
  /// subgrids extracted for the patch in question
  /// </summary>
  public class PatchRequestResponse : SubGridsPipelinedResponseBase
  {
    /// <summary>
    /// The total number of pages of subgrids required to contain the maximum number of subgrids'
    /// that may be returned for the query
    /// </summary>
    public int TotalNumberOfPagesToCoverFilteredData { get; set; }
  
    /// <summary>
    /// The set of subgrids matching the filters and patch page requested
    /// </summary>
    public List<IClientLeafSubGrid> SubGrids { get; set; }

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      writer.WriteInt(TotalNumberOfPagesToCoverFilteredData);

      writer.WriteBoolean(SubGrids != null);
      if (SubGrids != null)
      {
        writer.WriteInt(SubGrids.Count);

        foreach (var subGrid in SubGrids)
        {
          writer.WriteInt((int) subGrid.GridDataType);
          writer.WriteByteArray(subGrid.ToBytes());
        }
      }
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      TotalNumberOfPagesToCoverFilteredData = reader.ReadInt();

      if (reader.ReadBoolean())
      {
        var numberOfSubGrids = reader.ReadInt();

        if (numberOfSubGrids > 0)
        {
          SubGrids = new List<IClientLeafSubGrid>();

          var clientLeafSubgridFactory = DIContext.Obtain<IClientLeafSubGridFactory>();

          for (var i = 1; i <= numberOfSubGrids; i++)
          {
            var subgrid = clientLeafSubgridFactory.GetSubGrid((GridDataType) reader.ReadInt());

            subgrid?.FromBytes(reader.ReadByteArray());

            SubGrids.Add(subgrid);
          }
        }
      }
    }
  }
}
