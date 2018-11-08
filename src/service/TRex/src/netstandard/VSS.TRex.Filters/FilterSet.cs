using System;
using System.Diagnostics;
using System.Linq;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Geometry;
using VSS.TRex.Filters.Interfaces;

namespace VSS.TRex.Filters
{
  /// <summary>
  /// FilterSet represents a set of filters to be applied to each subgrid in a query within a single operation
  /// </summary>
  public class FilterSet : IFilterSet, IEquatable<IFilterSet>
  {
    /// <summary>
    /// The list of combined attribute and spatial filters to be used
    /// </summary>
    public ICombinedFilter[] Filters { get; set; }

    /// <summary>
    /// Default no-arg constructor that creates a zero-sized array of combined filters
    /// </summary>
    public FilterSet()
    {
      Filters = new ICombinedFilter[0];
    }

    /// <summary>
    /// Constructor accepting a single filters to be set into the filter set
    /// Null filters are not incorporated into the resulting filter set
    /// </summary>
    /// <param name="filter"></param>
    public FilterSet(ICombinedFilter filter)
    {      
      Filters = filter != null ? new [] { filter } : new ICombinedFilter[0];
    }

    /// <summary>
    /// Constructor accepting a pair of filter to be set into the filter set
    /// Null filters are not incorporated into the resulting filter set
    /// </summary>
    /// <param name="filter1"></param>
    /// <param name="filter2"></param>
    public FilterSet(ICombinedFilter filter1, ICombinedFilter filter2)
    {
      Filters = filter1 == null && filter2 == null 
        ? new ICombinedFilter[0] 
        : filter2 == null 
          ? new[] { filter1 } 
          : filter1 == null 
            ? new [] {filter2} 
            : new [] { filter1, filter2 };
    }

    /// <summary>
    /// Constructor accepting a pre-initialized array of filters to be included in the filter set
    /// </summary>
    /// <param name="filters"></param>
    public FilterSet(ICombinedFilter[] filters)
    {
      if (filters == null || filters.Length == 0)
        Filters = new ICombinedFilter[0];
      else
        Filters = filters.Where(x => x != null).ToArray();
    }

    /// <summary>
    /// Applies spatial filter restrictions to the extents required to request data for.
    /// </summary>
    /// <param name="extents"></param>
    public void ApplyFilterAndSubsetBoundariesToExtents(BoundingWorldExtent3D extents)
    {
      foreach (var filter in Filters)
      {
        filter?.SpatialFilter?.CalculateIntersectionWithExtents(extents);
      }
    }

    public void ToBinary(IBinaryRawWriter writer)
    {
      const byte versionNumber = 1;

      writer.WriteByte(versionNumber);

      writer.WriteInt(Filters.Length);
      foreach (var filter in Filters)
      { 
        // Handle cases where filter entry is null
        writer.WriteBoolean(filter != null);
        filter?.ToBinary(writer);
      }
    }

    public void FromBinary(IBinaryRawReader reader)
    {
      const byte versionNumber = 1;

      byte readVersionNumber = reader.ReadByte();

      Debug.Assert(readVersionNumber == versionNumber, $"Invalid version number: {readVersionNumber}, expecting {versionNumber}");

      Filters = new ICombinedFilter[reader.ReadInt()];
      for(int i = 0; i < Filters.Length; i++)
        Filters[i] = reader.ReadBoolean() ? new CombinedFilter(reader) : null;
    }

    protected bool Equals(FilterSet other)
    {
      return Equals(Filters, other.Filters);
    }

    public bool Equals(IFilterSet other)
    {
      if (other == null || Filters.Length != other.Filters.Length)
        return false;

      return !Filters.Where((t, i) => !t.Equals(other.Filters[i])).Any();
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != GetType()) return false;
      return Equals((IFilterSet) obj);
    }

    public override int GetHashCode()
    {
      return (Filters != null ? Filters.GetHashCode() : 0);
    }
  }
}
