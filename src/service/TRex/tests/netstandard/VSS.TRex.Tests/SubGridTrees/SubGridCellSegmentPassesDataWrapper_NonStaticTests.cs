using System;
using System.Text;
using System.IO;
using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.IO;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.Types;
using Xunit;
using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.TestFixtures;

namespace VSS.TRex.Tests.SubGridTrees
{
    public class SubGridCellSegmentPassesDataWrapper_NonStaticTests : IClassFixture<DILoggingFixture>
    {
        /// <summary>
        /// A handy test cell pass for the unit tests below to use
        /// </summary>
        /// <returns></returns>
        private CellPass TestCellPass()
        {
            return new CellPass()
            {
                Amplitude = 100,
                CCA = 101,
                CCV = 102,
                Frequency = 103,
                gpsMode = GPSMode.Fixed,
                HalfPass = false,
                Height = 104,
                InternalSiteModelMachineIndex = 105,
                GPSModeStore = 106,
                MachineSpeed = 106,
                MaterialTemperature = 107,
                MDP = 108,
                PassType = PassType.Track,
                RadioLatency = 109,
                RMV = 110,
                Time = DateTime.SpecifyKind(new DateTime(2000, 1, 2, 3, 4, 5), DateTimeKind.Utc)
            };
        }

        [Fact()]
        public void SubGridCellSegmentPassesDataWrapper_NonStatic_Test()
        {
          using (var item = new SubGridCellSegmentPassesDataWrapper_NonStatic())
          {
            Assert.NotNull(item);

            Assert.NotNull(item as ISubGridCellSegmentPassesDataWrapper);
          }
        }

        [Fact()]
        public void SubGridCellSegmentPassesDataWrapper_NonStatic_PassCount_Test()
        {
          using (var item = new SubGridCellSegmentPassesDataWrapper_NonStatic())
          {
            Assert.Equal(0, item.PassCount(1, 1));

            item.AddPass(1, 1, new CellPass() {Time = DateTime.UtcNow});
            Assert.Equal(1, item.PassCount(1, 1));
          }
        }

        [Fact()]
        public void SubGridCellSegmentPassesDataWrapper_NonStatic_AllocatePasses_Test()
        {
          using (var item = new SubGridCellSegmentPassesDataWrapper_NonStatic())
          {
            item.AllocatePasses(1, 1, 10);

            Assert.Equal(0, item.PassCount(1, 1));
          }
        }

        [Fact()]
        public void SubGridCellSegmentPassesDataWrapper_NonStatic_AllocatePassesExact_Test()
        {
          using (var item = new SubGridCellSegmentPassesDataWrapper_NonStatic())
          {
            item.AllocatePassesExact(1, 1, 10);

            Assert.Equal(0, item.PassCount(1, 1));

            item.AddPass(1, 1, new CellPass() {Time = DateTime.UtcNow});
            item.AddPass(1, 1, new CellPass() {Time = DateTime.UtcNow});

            Assert.Equal(2, item.PassCount(1, 1));

            item.AllocatePassesExact(1, 1, 1);
            Assert.Equal(1, item.PassCount(1, 1));
          }
        }

        [Fact()]
        public void SubGridCellSegmentPassesDataWrapper_NonStatic_AddPass_Test()
        {
          using (var item = new SubGridCellSegmentPassesDataWrapper_NonStatic())
          {
            item.AddPass(1, 1, TestCellPass());
            Assert.Equal(1, item.PassCount(1, 1));

            Assert.True(item.ExtractCellPass(1, 1, 0).Equals(TestCellPass()),
              "Cell added is not as expected when retrieved");
          }
        }

        [Fact()]
        public void SubGridCellSegmentPassesDataWrapper_NonStatic_ReplacePass_Test()
        {
          using (var item = new SubGridCellSegmentPassesDataWrapper_NonStatic())
          {
            CellPass pass = TestCellPass();

            item.AddPass(1, 1, pass);
            Assert.Equal(1, item.PassCount(1, 1));
            Assert.True(item.ExtractCellPass(1, 1, 0).Equals(pass), "Cell added is not as expected when retrieved");

            pass.CCV = 1000; // Change the cell pass a little

            item.ReplacePass(1, 1, 0, pass);

            Assert.True(item.ExtractCellPass(1, 1, 0).Equals(pass), "Cell added is not as expected when retrieved");
          }
        }

        [Fact()]
        public void SubGridCellSegmentPassesDataWrapper_NonStatic_ExtractCellPass_Test()
        {
          using (var item = new SubGridCellSegmentPassesDataWrapper_NonStatic())
          {
            CellPass pass = TestCellPass();

            item.AddPass(1, 1, pass);
            Assert.True(item.ExtractCellPass(1, 1, 0).Equals(pass), "Cell added is not as expected when retrieved");
          }
        }

        [Fact()]
        public void SubGridCellSegmentPassesDataWrapper_NonStatic_LocateTime_Test()
        {
          CellPass pass1 = TestCellPass();
          pass1.Time = DateTime.SpecifyKind(new DateTime(2000, 1, 1, 0, 0, 0), DateTimeKind.Utc);

          CellPass pass2 = TestCellPass();
          pass2.Time = DateTime.SpecifyKind(new DateTime(2000, 1, 2, 0, 0, 0), DateTimeKind.Utc);

          CellPass pass3 = TestCellPass();
          pass3.Time = DateTime.SpecifyKind(new DateTime(2000, 1, 3, 0, 0, 0), DateTimeKind.Utc);

          using (var item = new SubGridCellSegmentPassesDataWrapper_NonStatic())
          {
            item.AddPass(1, 1, pass1);
            item.AddPass(1, 1, pass2);
            item.AddPass(1, 1, pass3);

            Assert.Equal(3, item.PassCount(1, 1));

            bool exactMatch = item.LocateTime(1, 1,
              DateTime.SpecifyKind(new DateTime(1999, 12, 31, 0, 0, 0), DateTimeKind.Utc), out int index);
            Assert.False(exactMatch, "Exact match found!!!");
            Assert.Equal(0, index);

            exactMatch = item.LocateTime(1, 1,
              DateTime.SpecifyKind(new DateTime(2000, 1, 1, 0, 0, 0), DateTimeKind.Utc), out index);
            Assert.True(exactMatch && index > -1 && item.Pass(1, 1, (int) index).Equals(pass1),
              $"Failed to locate pass at DateTime(2000, 1, 1, 0, 0, 0), located pass is {item.Pass(1, 1, (int) index)}");

            exactMatch = item.LocateTime(1, 1,
              DateTime.SpecifyKind(new DateTime(2000, 1, 1, 0, 0, 1), DateTimeKind.Utc), out index);
            Assert.True(exactMatch == false && item.Pass(1, 1, (int) index - 1).Equals(pass1),
              $"Failed to locate pass at DateTime(2000, 1, 1, 0, 0, 1), index = {index}");

            exactMatch = item.LocateTime(1, 1,
              DateTime.SpecifyKind(new DateTime(2000, 1, 2, 10, 0, 0), DateTimeKind.Utc), out index);
            Assert.True(!exactMatch && index > -1 && item.Pass(1, 1, (int) index - 1).Equals(pass2),
              $"Failed to locate pass at DateTime(2001, 1, 2, 10, 0, 0), index = {index}");

            exactMatch = item.LocateTime(1, 1,
              DateTime.SpecifyKind(new DateTime(2001, 1, 1, 0, 0, 0), DateTimeKind.Utc), out index);
            Assert.True(!exactMatch && index > -1 && item.Pass(1, 1, (int) index - 1).Equals(pass3),
              $"Failed to locate pass at DateTime(2001, 1, 1, 0, 0, 0), index = {index}");
          }
        }

        [Fact()]
        public void SubGridCellSegmentPassesDataWrapper_NonStatic_WriteRead_Test()
        {
            // Create the main 2D array of cell pass arrays
            var cellPasses = new Cell_NonStatic[SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension];

            // Create each sub array and add a test cell pass to it
            SubGridUtilities.SubGridDimensionalIterator((x, y) =>
            {
              cellPasses[x, y].Passes = new TRexSpan<CellPass>(new CellPass[1], TRexSpan<CellPass>.NO_SLAB_INDEX, 0, 1, false);
              cellPasses[x, y].Passes.Add(TestCellPass());
            });

            using (var item1 = new SubGridCellSegmentPassesDataWrapper_NonStatic())
            {
              MemoryStream ms = new MemoryStream(Consts.TREX_DEFAULT_MEMORY_STREAM_CAPACITY_ON_CREATION);

              // Write to the stream...
              BinaryWriter writer = new BinaryWriter(ms, Encoding.UTF8, true);
              item1.SetState(cellPasses);
              item1.Write(writer);

              // Create a new segment and read it back again
              using (var item2 = new SubGridCellSegmentPassesDataWrapper_NonStatic())
              {
                ms.Position = 0;
                BinaryReader reader = new BinaryReader(ms, Encoding.UTF8, true);
                item2.Read(reader);

                SubGridUtilities.SubGridDimensionalIterator((col, row) =>
                {
                  var cellPasses1 = item1.ExtractCellPasses(col, row);
                  var cellPasses2 = item2.ExtractCellPasses(col, row);

                  Assert.True(cellPasses1.PassCount == cellPasses2.PassCount,
                    "Read segment does not contain the same list of cell passes written into it - counts do not match");

                  for (int i = 0; i < cellPasses1.PassCount; i++)
                    Assert.True(cellPasses1.Passes.GetElement(i).Equals(cellPasses2.Passes.GetElement(i)),
                      "Read segment does not contain the same list of cell passes written into it");
                });
              }
            }
        }

        [Fact()]
        public void SubGridCellSegmentPassesDataWrapper_NonStatic_PassHeight_Test()
        {
          using (var item = new SubGridCellSegmentPassesDataWrapper_NonStatic())
          {
            CellPass pass = TestCellPass();
            item.AddPass(1, 1, pass);

            Assert.True(item.PassHeight(1, 1, 0).Equals(pass.Height), "Cell pass height not same as value added");
          }
        }

        [Fact()]
        public void SubGridCellSegmentPassesDataWrapper_NonStatic_PassTime_Test()
        {
          using (var item = new SubGridCellSegmentPassesDataWrapper_NonStatic())
          {
            CellPass pass = TestCellPass();
            item.AddPass(1, 1, pass);

            Assert.True(item.PassTime(1, 1, 0).Equals(pass.Time), "Cell pass time not same as value added");
          }
        }

        [Fact()]
        public void SubGridCellSegmentPassesDataWrapper_NonStatic_Integrate_Test()
        {
          using (var item = new SubGridCellSegmentPassesDataWrapper_NonStatic())
          {
            CellPass pass = TestCellPass();
            item.AddPass(1, 1, pass);

            CellPass pass2 = TestCellPass();
            pass2.Time = pass2.Time.AddSeconds(60);

            Cell_NonStatic integrateFrom = new Cell_NonStatic();
            integrateFrom.AddPass(pass2);

            item.Integrate(1, 1, integrateFrom, 0, 0, out int addedCount, out int modifiedCount);

            Assert.Equal(2, item.PassCount(1, 1));
            Assert.Equal(1, addedCount);
            Assert.Equal(0, modifiedCount);
          }
        }

        [Fact()]
        public void SubGridCellSegmentPassesDataWrapper_NonStatic_Cell_Test()
        {
            CellPass pass1 = TestCellPass();
            pass1.Time = DateTime.SpecifyKind(new DateTime(2000, 1, 1, 0, 0, 0), DateTimeKind.Utc);

            CellPass pass2 = TestCellPass();
            pass2.Time = DateTime.SpecifyKind(new DateTime(2000, 1, 2, 0, 0, 0), DateTimeKind.Utc);

            CellPass pass3 = TestCellPass();
            pass3.Time = DateTime.SpecifyKind(new DateTime(2000, 1, 3, 0, 0, 0), DateTimeKind.Utc);

            CellPass[] passes = new CellPass[] { pass1, pass2, pass3 };

            using (var item = new SubGridCellSegmentPassesDataWrapper_NonStatic())
            {
              item.AddPass(1, 1, pass1);
              item.AddPass(1, 1, pass2);
              item.AddPass(1, 1, pass3);

              Assert.Equal(3, item.PassCount(1, 1));

              Cell_NonStatic cell = item.ExtractCellPasses(1, 1);

              for (int i = 0; i < cell.PassCount; i++)
                Assert.True(cell.Passes.GetElement(i).Equals(passes[i]),
                  "Extracted cell does not contain the same cell passes added to it");
            }
        }

        [Fact()]
        public void SubGridCellSegmentPassesDataWrapper_NonStatic_Pass_Test()
        {
          using (var item = new SubGridCellSegmentPassesDataWrapper_NonStatic())
          {
            CellPass pass = TestCellPass();
            item.AddPass(1, 1, pass);

            Assert.True(item.Pass(1, 1, 0).Equals(pass), "Cell pass not same as value added");
          }
        }

        /// <summary>
        /// Tests that the method to take a set of cell passes for the segment can set all those call passes into the 
        /// internal representation
        /// </summary>
        [Fact()]
        public void SubGridCellSegmentPassesDataWrapper_NonStatic_SetState_Test()
        {
            // Create the main 2D array of cell pass arrays
            var cellPasses = new Cell_NonStatic[SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension];

            // Create each sub array and add a test cell pass to it
            SubGridUtilities.SubGridDimensionalIterator((x, y) =>
            {
              cellPasses[x, y].Passes = new TRexSpan<CellPass>(new CellPass[1], TRexSpan<CellPass>.NO_SLAB_INDEX, 0, 1, false);
              cellPasses[x, y].Passes.Add(TestCellPass());
            });

            using (var item = new SubGridCellSegmentPassesDataWrapper_NonStatic())
            {
              // Feed the cell passes to the segment
              item.SetState(cellPasses);

              // Check the passes all match
              SubGridUtilities.SubGridDimensionalIterator((x, y) =>
              {
                Assert.True(cellPasses[x, y].Passes.First().Equals(item.Pass(x, y, 0)),
                  $"Pass in cell {x}:{y} does not match");
              });
            }
        }

        /// <summary>
        /// Tests that the machine ID set for an uncompressed non static cell pass wrapper is null (as expected)
        /// </summary>
        [Fact()]
        public void SubGridCellSegmentPassesDataWrapper_NonStatic_NoMachineIDSet_Test()
        {
          using (var item = new SubGridCellSegmentPassesDataWrapper_NonStatic())
          {
            // Uncompressed static cell pass wrappers do not provide machine ID sets
            Assert.Null(item.GetMachineIDSet());
          }
        }
    }
}
