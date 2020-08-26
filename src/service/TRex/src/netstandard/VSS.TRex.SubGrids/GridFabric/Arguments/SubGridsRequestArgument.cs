using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Common.Models;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.SubGrids.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.SubGrids.GridFabric.Arguments
{
  /// <summary>
  /// Contains all the parameters necessary to be sent for a generic sub grids request made to the compute cluster
  /// </summary>
  public class SubGridsRequestArgument : BaseApplicationServiceRequestArgument, ISubGridsRequestArgument
  {
    private const byte VERSION_NUMBER = 1;

    /// <summary>
    /// The request ID for the sub grid request
    /// </summary>
    public Guid RequestID { get; set; } = Guid.Empty;

    /// <summary>
    /// The grid data type to extract from the processed sub grids
    /// </summary>
    public GridDataType GridDataType { get; set; } = GridDataType.All;

    /// <summary>
    /// The serialized contents of the SubGridTreeSubGridExistenceBitMask that notes the address of all sub grids that need to be requested for production data
    /// </summary>
    public byte[] ProdDataMaskBytes { get; set; }

    /// <summary>
    /// The serialized contents of the SubGridTreeSubGridExistenceBitMask that notes the address of all sub grids that need to be requested for surveyed surface data ONLY
    /// </summary>
    public byte[] SurveyedSurfaceOnlyMaskBytes { get; set; }

    /// <summary>
    /// Denotes whether results of these requests should include any surveyed surfaces in the site model
    /// </summary>
    public bool IncludeSurveyedSurfaceInformation { get; set; }

    public AreaControlSet AreaControlSet { get; set; } = AreaControlSet.CreateAreaControlSet();

    public SubGridsRequestComputeStyle SubGridsRequestComputeStyle { get; set; } = SubGridsRequestComputeStyle.Normal;

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public SubGridsRequestArgument()
    {
    }

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      base.InternalToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteGuid(RequestID);
      writer.WriteInt((int)GridDataType);

      writer.WriteByteArray(ProdDataMaskBytes);
      writer.WriteByteArray(SurveyedSurfaceOnlyMaskBytes);

      writer.WriteBoolean(IncludeSurveyedSurfaceInformation);

      AreaControlSet.ToBinary(writer);

      writer.WriteInt((int)SubGridsRequestComputeStyle);
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      base.InternalFromBinary(reader);

      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (version == 1)
      {
        RequestID = reader.ReadGuid() ?? Guid.Empty;
        GridDataType = (GridDataType) reader.ReadInt();

        ProdDataMaskBytes = reader.ReadByteArray();
        SurveyedSurfaceOnlyMaskBytes = reader.ReadByteArray();

        IncludeSurveyedSurfaceInformation = reader.ReadBoolean();

        AreaControlSet = new AreaControlSet();
        AreaControlSet.FromBinary(reader);

        SubGridsRequestComputeStyle = (SubGridsRequestComputeStyle) reader.ReadInt();
      }
    }
  }
}
