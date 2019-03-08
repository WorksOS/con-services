using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Filters;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Filters.Models;
using VSS.TRex.Geometry;
using VSS.TRex.Tests.BinarizableSerialization;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Filters
{
  public class FilterSetTests
  {
    [Fact]
    public void Test_FilterSet_Creation_SingleNull1()
    {
      var f = new FilterSet();

      Assert.True(f.Filters != null, "Filters in filter set null after creation");
      Assert.True(f.Filters.Length == 0, "Filterset count not zero after creation");
    }

    [Fact]
    public void Test_FilterSet_Creation_SingleNull2()
    {
      var f = new FilterSet((ICombinedFilter)null);

      Assert.True(f.Filters != null, "Filters in filter set null after creation");
      Assert.True(f.Filters.Length == 0, "Filterset count not zero after creation");
    }

    [Fact]
    public void Test_FilterSet_Creation_SingleNull3a()
    {
      var f = new FilterSet(null, new CombinedFilter());

      Assert.True(f.Filters != null, "Filters in filter set null after creation");
      Assert.True(f.Filters.Length == 1, "Filterset count not one after creation");
    }

    [Fact]
    public void Test_FilterSet_Creation_SingleNull3b()
    {
      var f = new FilterSet(new CombinedFilter(), null);

      Assert.True(f.Filters != null, "Filters in filter set null after creation");
      Assert.True(f.Filters.Length == 1, "Filterset count not one after creation");
    }

    [Fact]
    public void Test_FilterSet_Creation_SingleNull4()
    {
      var f = new FilterSet(new [] {null, new CombinedFilter(), new CombinedFilter()});

      Assert.True(f.Filters != null, "Filters in filter set null after creation");
      Assert.True(f.Filters.Length == 2, "Filterset count not two after creation");
    }

    [Fact]
    public void Test_FilterSet_Creation_DoubleNull1()
    {
      var f = new FilterSet(new ICombinedFilter[] { null, null });

      Assert.True(f.Filters != null, "Filters in filter set null after creation");
      Assert.True(f.Filters.Length == 0, "Filterset count not zero after creation");
    }

    [Fact]
    public void Test_FilterSet_Creation_DoubleNull2()
    {
      var f = new FilterSet(new[] { null, null, new CombinedFilter() });

      Assert.True(f.Filters != null, "Filters in filter set null after creation");
      Assert.True(f.Filters.Length == 1, "Filterset count not one after creation");
    }

    [Fact]
    public void Test_FilterSet_Creation_AllNull1()
    {
      var f = new FilterSet(null, null);

      Assert.True(f.Filters != null, "Filters in filter set null after creation");
      Assert.True(f.Filters.Length == 0, "Filterset count not zero after creation");
    }

    [Fact]
    public void Test_FilterSet_Creation_AllNull2()
    {
      var f = new FilterSet(new ICombinedFilter[] { null, null, null });

      Assert.True(f.Filters != null, "Filters in filter set null after creation");
      Assert.True(f.Filters.Length == 0, "Filterset count not zero after creation");
    }

    [Fact]
    public void Test_FilterSet_Creation_EmptyArray()
    {
      var f = new FilterSet(new ICombinedFilter[0]);

      Assert.True(f.Filters != null, "Filters in filter set null after creation");
      Assert.True(f.Filters.Length == 0, "Filterset count not zero after creation");
    }

    [Fact]
    public void Test_FilterSet_FromToBinary()
    {
      var data = new FilterSet(new ICombinedFilter[]
      {
        new CombinedFilter
        {
          AttributeFilter =
          {
            GPSAccuracy = GPSAccuracy.Fine,
            HasDesignFilter = true,
            MachinesList = new [] {Guid.NewGuid(), Guid.NewGuid() },
            SurveyedSurfaceExclusionList = new [] {Guid.NewGuid(), Guid.NewGuid() }
          },
          SpatialFilter = new CellSpatialFilter
          {
            AlignmentFence = new Fence(0, 0, 100, 100)
          }
        }, 
      }); 

      var writer = new TestBinaryWriter();
      data.ToBinary(writer);

      var data2 = new FilterSet();
      data2.FromBinary(new TestBinaryReader(writer._stream.BaseStream as MemoryStream));

      data2.Should().BeEquivalentTo(data2);
    }

    [Fact]
    public void Test_FilterSet_FromToBinary_FailWithTooManyFilters()
    {
      var data = new FilterSet(Enumerable.Range(0, FilterSet.MAX_REASONABLE_NUMBER_OF_FILTERS + 1)
        .Select(x => 
        new CombinedFilter
        {
          AttributeFilter =
          {
            GPSAccuracy = GPSAccuracy.Fine,
            HasDesignFilter = true
          },
          SpatialFilter = new CellSpatialFilter
          {
            AlignmentFence = new Fence(0, 0, 100 + x, 100 + x)
          }
      }).ToArray());

      var writer = new TestBinaryWriter();
      data.ToBinary(writer);

      var data2 = new FilterSet();

      Action act = () => data2.FromBinary(new TestBinaryReader(writer._stream.BaseStream as MemoryStream));

      act.Should().Throw<TRexException>().WithMessage("Invalid number of filters * in deserialisation");
    }

    [Fact]
    public void Test_FilterSet_FromToBinary_ApplyFilterAndSubsetBoundariesToExtents()
    {
      var data = new FilterSet(
          new CombinedFilter
          {
            AttributeFilter =
            {
              GPSAccuracy = GPSAccuracy.Fine,
              HasDesignFilter = false
            },
            SpatialFilter = new CellSpatialFilter
            {
              IsSpatial = true,
              Fence = new Fence(1, 1, 100, 100)
            }
          });

      BoundingWorldExtent3D expectedExtent = new BoundingWorldExtent3D(1, 1, 100, 100);
      BoundingWorldExtent3D startingExtent = new BoundingWorldExtent3D(0, 0, 101, 101);
      data.ApplyFilterAndSubsetBoundariesToExtents(startingExtent);
      startingExtent.Should().BeEquivalentTo(expectedExtent);
    }    
  }
}
