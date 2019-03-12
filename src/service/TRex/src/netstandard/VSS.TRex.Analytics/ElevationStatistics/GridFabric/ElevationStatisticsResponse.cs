using Apache.Ignite.Core.Binary;
using VSS.TRex.Analytics.Foundation.GridFabric.Responses;
using VSS.TRex.Analytics.Foundation.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.ExtensionMethods;
using VSS.TRex.GridFabric.Interfaces;

namespace VSS.TRex.Analytics.ElevationStatistics.GridFabric
{
  /// <summary>
  /// The response state returned from a Elevation statistics request.
  /// </summary>
  public class ElevationStatisticsResponse : BaseAnalyticsResponse, IAggregateWith<ElevationStatisticsResponse>, IAnalyticsOperationResponseResultConversion<ElevationStatisticsResult>
  {
    /// <summary>
    /// The cell size of the site model the aggregation is being performed over.
    /// </summary>
    public double CellSize { get; set; }

    /// <summary>
    /// The minimum elevation value of the site model. 
    /// </summary>
    public double MinElevation { get; set; }

    /// <summary>
    /// The maximum elevation value of the site model.
    /// </summary>
    public double MaxElevation { get; set; }

    /// <summary>
    /// Records how many cells were used in the calculation.
    /// </summary>
    public int CellsUsed { get; set; }

    /// <summary>
    /// Records the total number of cells that were considered by
    /// the engine. This includes cells outside of reference design fence boundaries
    /// and cells where both base and top values may have been null.
    /// </summary>
    public int CellsScanned { get; set; }

    /// <summary>
    /// The area of cells that we have considered and successfully computed information from.
    /// </summary>
    public double CoverageArea => CellsUsed * (CellSize * CellSize);

    /// <summary>
    /// The bounding extents of the computed area.
    /// </summary>
    public BoundingWorldExtent3D BoundingExtents = new BoundingWorldExtent3D();

    /// <summary>
    /// The total area of the data cells.
    /// </summary>
    public double TotalArea => CellsScanned * (CellSize * CellSize);


    public ElevationStatisticsResponse AggregateWith(ElevationStatisticsResponse other)
    {
      CellSize = other.CellSize;

      if (other.MinElevation < MinElevation)
        MinElevation = other.MinElevation;

      if (other.MaxElevation > MaxElevation)
        MaxElevation = other.MaxElevation;

      CellsUsed += other.CellsUsed;
      CellsScanned += other.CellsScanned;

      BoundingExtents.Include(other.BoundingExtents);

      return this;
    }

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      writer.WriteDouble(CellSize);
      writer.WriteDouble(MinElevation);
      writer.WriteDouble(MaxElevation);
      writer.WriteInt(CellsUsed);
      writer.WriteInt(CellsScanned);

      BoundingExtents.ToBinary(writer);
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      CellSize = reader.ReadDouble();
      MinElevation = reader.ReadDouble();
      MaxElevation = reader.ReadDouble();
      CellsUsed = reader.ReadInt();
      CellsScanned = reader.ReadInt();

      BoundingExtents.FromBinary(reader);
    }

    public ElevationStatisticsResult ConstructResult()
    {
      return new ElevationStatisticsResult
      {
        MinElevation = MinElevation,
        MaxElevation = MaxElevation,
        CoverageArea = CoverageArea,
        BoundingExtents = BoundingExtents,

        ResultStatus = ResultStatus
      };
    }
  }
}
