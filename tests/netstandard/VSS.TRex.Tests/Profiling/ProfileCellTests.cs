using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VSS.TRex.Filters;
using VSS.TRex.Filters.Models;
using VSS.TRex.Profiling;
using VSS.TRex.Profiling.Interfaces;
using VSS.TRex.Profiling.Models;
using Xunit;

namespace VSS.TRex.Tests.Profiling
{
    public class ProfileCellTests
    {
      [Fact]
      public void Test_ProfileCell_Creation()
      {
        ProfileCell cell = new ProfileCell();

        Assert.NotNull(cell);
        Assert.True(cell.Layers != null, "Cell profile layer not created");
        Assert.True(cell.AttributeExistenceFlags == ProfileCellAttributeExistenceFlags.None, "Cell attribute flags not correctly initialsed");
      }

      [Fact]
      public void Test_ProfileCell_Creation2_NoPasses()
      {
        ProfileCell cell = new ProfileCell(new FilteredMultiplePassInfo(), 1, 2, 3.0, 4.0, true);

        Assert.Equal((uint)1, cell.OTGCellX);
        Assert.Equal((uint)2, cell.OTGCellY);
        Assert.Equal(3.0, cell.Station);
        Assert.Equal(4.0, cell.InterceptLength);

        // ReSharper disable once UseMethodAny.2
        Assert.True(cell.Layers.Count() == 0, "Layer count not zero after constructor creation from cell passes");
    }

      [Fact]
      public void Test_ProfileCell_Creation2_WithPasses()
      {
        ProfileCell cell = new ProfileCell(new FilteredMultiplePassInfo
          {
            PassCount = 1,
            FilteredPassData = new FilteredPassData[] {new FilteredPassData() }
          },
          1, 2, 3.0, 4.0, true);

        Assert.Equal((uint)1, cell.OTGCellX);
        Assert.Equal((uint)2, cell.OTGCellY);
        Assert.Equal(3.0, cell.Station);
        Assert.Equal(4.0, cell.InterceptLength);

        Assert.True(cell.Layers.Count() == 1, "Layer count not one after constructor creation from cell passes");
      }

      [Fact]
      public void Test_ProfileCell_AddLayer()
      {
        ProfileCell cell = new ProfileCell();

        cell.AddLayer(new FilteredMultiplePassInfo
          {
            PassCount = 1,
            FilteredPassData = new FilteredPassData[] { new FilteredPassData() }
          });

        Assert.True(1 == cell.Layers.Count(), "Layer count not one after adding layer");
    }

      [Fact]
      public void Test_ProfileCell_ClearLayers()
      {
        ProfileCell cell = new ProfileCell();

        cell.AddLayer(new FilteredMultiplePassInfo
        {
          PassCount = 1,
          FilteredPassData = new FilteredPassData[] { new FilteredPassData() }
        });

        cell.ClearLayers();

        Assert.True(cell.IsEmpty(), "Cell layers not empty after clear layers");
    }

    [Fact]
      public void Test_ProfileCell_IsEmpty()
      {
        ProfileCell cell = new ProfileCell();

        Assert.True(cell.IsEmpty(), "Cell not empty after construction");

        cell.AddLayer(new FilteredMultiplePassInfo
          {
            PassCount = 1,
            FilteredPassData = new FilteredPassData[] { new FilteredPassData() }
          });

        Assert.False(cell.IsEmpty(), "Cell empty after addition of a layer");

        cell.ClearLayers();

        Assert.True(cell.IsEmpty(), "Cell layers not empty after clear layers");
      }

      [Fact]
      public void Test_ProfileCell_RequestNewLayer()
      {
        ProfileCell cell = new ProfileCell();

        IProfileLayer layer = cell.RequestNewLayer(out int RecycledIndex);
        cell.Layers.Add(layer, RecycledIndex);

        Assert.True(layer != null, "RequestNewLayer did not return a new layer");
        Assert.True(-1 == RecycledIndex, "Recycled index not -1 for new layer with no recyclbles available");

        cell.ClearLayers();

        layer = cell.RequestNewLayer(out RecycledIndex);

        Assert.True(layer != null, "RequestNewLayer did not return a new layer after recycling previous layer");
        Assert.True(0 == RecycledIndex, "Recycled index not 0 for new layer with one recyclable available");
    }

      [Fact]
      public void Test_ProfileCell_AnalyseSpeedTargets()
      {
        ProfileCell cell = new ProfileCell();

        cell.AnalyzeSpeedTargets(25);
        cell.AnalyzeSpeedTargets(50);

        Assert.True(25 == cell.CellMinSpeed, "Miniumum speed is not 25 as expected");
        Assert.True(50 == cell.CellMaxSpeed, "Maxiumum speed is not 50 as expected");
    }
  }
  }
