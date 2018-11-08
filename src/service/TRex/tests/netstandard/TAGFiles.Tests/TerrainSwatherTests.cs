using System;
using VSS.TRex.Events;
using VSS.TRex.Geometry;
using VSS.TRex.Machines;
using VSS.TRex.Machines.Interfaces;
using VSS.TRex.SiteModels;
using VSS.TRex.Storage;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.TAGFiles.Classes.Processors;
using VSS.TRex.TAGFiles.Classes.Swather;
using VSS.TRex.TAGFiles.Types;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace TAGFiles.Tests
{
    public class TerrainSwatherTests : IClassFixture<DITagFileFixture>
    {
        [Fact]
        public void Test_TerrainSwather_Creation()
        {
            var siteModel = new SiteModel();
            var machine = new Machine();
            var grid = new ServerSubGridTree(siteModel.ID);
            var fence = new Fence();
            var SiteModelGridAggregator = new ServerSubGridTree(siteModel.ID);
            var MachineTargetValueChangesAggregator = new ProductionEventLists(siteModel, MachineConsts.kNullInternalSiteModelMachineIndex);
            var processor = new TAGProcessor(siteModel, machine, SiteModelGridAggregator, MachineTargetValueChangesAggregator);

            TerrainSwather swather = new TerrainSwather(processor, MachineTargetValueChangesAggregator, siteModel, grid, fence);

            Assert.True(swather != null && swather.InternalSiteModelMachineIndex == machine.InternalSiteModelMachineIndex, 
                "TerrainSwather not created as expected");
        }

        [Fact]
        public void Test_TerrainSwather_PerformSwathing()
        {
            var siteModel = new SiteModel();
            var machine = new VSS.TRex.Machines.Machine();
            var grid = new ServerSubGridTree(siteModel.ID);
            var SiteModelGridAggregator = new ServerSubGridTree(siteModel.ID);
            var MachineTargetValueChangesAggregator = new ProductionEventLists(siteModel, MachineConsts.kNullInternalSiteModelMachineIndex);
            var processor = new TAGProcessor(siteModel, machine, SiteModelGridAggregator, MachineTargetValueChangesAggregator);

            var fence = new Fence();
            fence.SetRectangleFence(0, 0, 10, 2);

            TerrainSwather swather = new TerrainSwather(processor, MachineTargetValueChangesAggregator, siteModel, grid, fence);

            // Create four corner vertices for location of the processing context
            var V00 = new XYZ(0, 0, 0);
            var V01 = new XYZ(0, 2, 0);
            var V10 = new XYZ(10, 0, 0);
            var V11 = new XYZ(10, 2, 0);

            // Create four corner vertices for time of the processing context (with two epochs three seconds apart
            var T00 = new XYZ(0, 0, new DateTime(2000, 1, 1, 1, 1, 0).ToOADate());
            var T01 = new XYZ(0, 2, new DateTime(2000, 1, 1, 1, 1, 0).ToOADate());
            var T10 = new XYZ(10, 0, new DateTime(2000, 1, 1, 1, 1, 3).ToOADate());
            var T11 = new XYZ(10, 2, new DateTime(2000, 1, 1, 1, 1, 3).ToOADate());

            // Create the height and time interpolation triangles
            var HeightInterpolator1 = new SimpleTriangle(V00, V01, V10);
            var HeightInterpolator2 = new SimpleTriangle(V01, V11, V10);
            var TimeInterpolator1 = new SimpleTriangle(T00, T01, T10);
            var TimeInterpolator2 = new SimpleTriangle(T01, T11, T10);

            // Compute swath with full cell pass on the front (blade) mesurement location
            bool swathResult = swather.PerformSwathing(HeightInterpolator1, HeightInterpolator2, TimeInterpolator1, TimeInterpolator2, false, PassType.Front, MachineSide.None);

            // Did the swathing operation succeed?
            Assert.True(swathResult, "Performaswathing failed");

            // Did it produce the expected set of swathed cells?
            Assert.Equal(1, grid.Root.CountChildren());

            // Computation of the latest pass information which aids locating cells with non-null values
            try
            {
                IStorageProxy storageProxy = StorageProxy.Instance(StorageMutability.Mutable);
                grid.Root.ScanSubGrids(grid.FullCellExtent(), x =>
                {
                    ((IServerLeafSubGrid)x).ComputeLatestPassInformation(true, storageProxy);
                    return true;
                });
            }
            catch (Exception E)
            {
                Assert.False(true, $"Exception {E} occured computing latest cell information");
            }

            grid.CalculateIndexOfCellContainingPosition(grid.CellSize / 2, grid.CellSize / 2, out uint _, out uint _);

            int nonNullCellCount = 0;
            try
            {
                grid.Root.ScanSubGrids(grid.FullCellExtent(), x =>
                {
                    nonNullCellCount += ((IServerLeafSubGrid)x).CountNonNullCells();
                    return true;
                });
            }
            catch (Exception e)
            {
                Assert.False(true, $"Exception {e} occured counting non-null cells");
            }

            Assert.Equal(174, nonNullCellCount);

            // Iterate over the cells and confirm their content is as expected

            
        }
    }
}
