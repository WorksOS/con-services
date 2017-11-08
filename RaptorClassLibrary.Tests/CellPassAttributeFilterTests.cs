using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.VisionLink.Raptor.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Common;

namespace VSS.VisionLink.Raptor.Filters.Tests
{
    [TestClass()]
    public class CellPassAttributeFilterTests
    {
        [TestMethod()]
        public void Test_CellPassAttributeFilter_CellPassAttributeFilter()
        {
            CellPassAttributeFilter filter = new CellPassAttributeFilter(null);

            Assert.IsFalse(filter.AnyFilterSelections || filter.AnyMachineEventFilterSelections || filter.AnyNonMachineEventFilterSelections,
                "Filter flags set for default filter");
        }

        [TestMethod()]
        public void Test_CellPassAttributeFilter_Prepare()
        {
            CellPassAttributeFilter filter = new CellPassAttributeFilter(null);

            Assert.IsFalse(filter.ElevationRangeIsInitialised, "Elevation range is initialised");

            filter.Prepare();

            Assert.IsFalse(filter.ElevationRangeIsInitialised, "Elevation range is not initialised");

            filter.ClearFilter();

            Assert.IsFalse(filter.AnyFilterSelections, "AnyFilterSelections not false");
            Assert.IsFalse(filter.AnyMachineEventFilterSelections, "AnyMachineEventFilterSelections not false");
            Assert.IsFalse(filter.AnyNonMachineEventFilterSelections, "AnyNonMachineEventFilterSelections not false");

            filter.HasTimeFilter = true;
            filter.Prepare();

            Assert.IsTrue(filter.AnyFilterSelections, "AnyFilterSelections not true after adding time filter");
            Assert.IsFalse(filter.AnyMachineEventFilterSelections, "AnyMachineEventFilterSelections not false");
            Assert.IsTrue(filter.AnyNonMachineEventFilterSelections, "AnyNonMachineEventFilterSelections not true after adding time filter");

            filter.HasPositioningTechFilter = true;

            filter.Prepare();

            Assert.IsTrue(filter.AnyFilterSelections, "AnyFilterSelections not true");
            Assert.IsTrue(filter.AnyMachineEventFilterSelections, "AnyMachineEventFilterSelections not true");
            Assert.IsTrue(filter.AnyNonMachineEventFilterSelections, "AnyNonMachineEventFilterSelections not true");
        }

        [TestMethod()]
        public void Test_CellPassAttributeFilter_ClearFilter()
        {
            Assert.Fail();
        }

        private void Test_CellPassAttributeFilter_ClearFilter_Aspect
            (string name, Action<CellPassAttributeFilter> setState, Func<CellPassAttributeFilter, bool> checkSetState,
                         Action<CellPassAttributeFilter> clearState, Func<CellPassAttributeFilter, bool> checkClearState)
        {
            CellPassAttributeFilter filter = new CellPassAttributeFilter(null);

            Assert.IsTrue(checkClearState(filter), "[{0}] State set when expected to be not set (1)", name);

            setState(filter);
            Assert.IsTrue(checkSetState(filter), "[{0}] State not set when expected to be set", name);

            clearState(filter);
            Assert.IsTrue(checkClearState(filter), "[{0}] State set when expected to be not set (2)", name);
        }

        [TestMethod()]
        public void Test_CellPassAttributeFilter_ClearVibeState()
        {
            Test_CellPassAttributeFilter_ClearFilter_Aspect("VibeState", 
                                                            x => { x.HasVibeStateFilter = true; x.VibeState = Types.VibrationState.Off; },
                                                            x => { return x.HasVibeStateFilter && x.VibeState == Types.VibrationState.Off; },
                                                            x => { x.ClearVibeState(); }, 
                                                            x => { return !x.HasVibeStateFilter && x.VibeState == Types.VibrationState.Invalid; });
        }

        private void Test_CellPassAttributeFilter_CompareTo_Aspect(string name, Action<CellPassAttributeFilter> SetState)
        {
            CellPassAttributeFilter filter1 = new CellPassAttributeFilter(null);
            CellPassAttributeFilter filter2 = new CellPassAttributeFilter(null);

            SetState(filter1);
            Assert.IsTrue(filter1.CompareTo(filter2) == -1, "[{0}] filter1.CompareTo(filter2) != -1, = {1}", name, filter1.CompareTo(filter2));

            SetState(filter2);
            Assert.IsTrue(filter1.CompareTo(filter2) == 0, "[{0}] filter1.CompareTo(filter2) != 0, = {1}", name, filter1.CompareTo(filter2));
        }

        [TestMethod()]
        public void Test_CellPassAttributeFilter_CompareTo_Time()
        {
            Test_CellPassAttributeFilter_CompareTo_Aspect("Time", x => { x.HasTimeFilter = true; x.StartTime = new DateTime(2000, 1, 1); });
        }

        [TestMethod()]
        public void Test_CellPassAttributeFilter_CompareTo_CompactionMachines()
        {
            Test_CellPassAttributeFilter_CompareTo_Aspect("CompactionMachinesOnly", x => { x.HasCompactionMachinesOnlyFilter = true; });
        }

        [TestMethod()]
        public void Test_CellPassAttributeFilter_CompareTo_DesignNameID()
        {
            Test_CellPassAttributeFilter_CompareTo_Aspect("Time", x => { x.HasDesignFilter = true; x.DesignNameID = 10; });
        }

        [TestMethod()]
        public void Test_CellPassAttributeFilter_CompareTo_ElevationRange()
        {
            Test_CellPassAttributeFilter_CompareTo_Aspect("ElevationRange_Design",
                x =>
                {
                    x.HasElevationRangeFilter = true;
                    x.ElevationRangeDesign = DesignDescriptor.Null();
                    x.ElevationRangeOffset = 10;
                    x.ElevationRangeThickness = 1;
                });

            Test_CellPassAttributeFilter_CompareTo_Aspect("ElevationRange_Level",
                x =>
                {
                    x.HasElevationRangeFilter = true;
                    x.ElevationRangeLevel = 100;
                    x.ElevationRangeOffset = 10;
                    x.ElevationRangeThickness = 1;
                });
        }

        [TestMethod()]
        public void Test_CellPassAttributeFilter_CompareTo_ElevationType()
        {
            Test_CellPassAttributeFilter_CompareTo_Aspect("ElevationType", x => { x.HasElevationTypeFilter = true; x.ElevationType = Types.ElevationType.Highest; });
        }

        [TestMethod()]
        public void Test_CellPassAttributeFilter_CompareTo_GCSGuidanceMode()
        {
            Test_CellPassAttributeFilter_CompareTo_Aspect("GCSGuidanceMode", x => { x.HasGCSGuidanceModeFilter = true; x.GCSGuidanceMode = Types.MachineAutomaticsMode.Automatics; });
        }

        [TestMethod()]
        public void Test_CellPassAttributeFilter_CompareTo_GPSAccuracy()
        {
            Test_CellPassAttributeFilter_CompareTo_Aspect("GPSAccuracy", x => { x.HasGPSAccuracyFilter = true; x.GPSAccuracy = Types.GPSAccuracy.Medium; });
        }

        [TestMethod()]
        public void Test_CellPassAttributeFilter_CompareTo_GPSTolerance()
        {
            Test_CellPassAttributeFilter_CompareTo_Aspect("GPSTolerance", x => { x.HasGPSToleranceFilter = true; x.GPSTolerance = 1000; });
        }

        [TestMethod()]
        public void Test_CellPassAttributeFilter_CompareTo_LayerID()
        {
            Test_CellPassAttributeFilter_CompareTo_Aspect("LayerID", x => { x.HasLayerIDFilter = true; x.LayerID = 10; });
        }

        [TestMethod()]
        public void Test_CellPassAttributeFilter_CompareTo_LayerState()
        {
            Test_CellPassAttributeFilter_CompareTo_Aspect("LayerState On", x => { x.HasLayerStateFilter = true; x.LayerState = Types.LayerState.On; });
            Test_CellPassAttributeFilter_CompareTo_Aspect("LayerState Off", x => { x.HasLayerStateFilter = true; x.LayerState = Types.LayerState.Off; });
        }

        [TestMethod()]
        public void Test_CellPassAttributeFilter_CompareTo_MachineDirection()
        {
            Test_CellPassAttributeFilter_CompareTo_Aspect("MachineDirection forward", x => { x.HasMachineDirectionFilter = true; x.MachineDirection = Types.MachineDirection.Forward; });
            Test_CellPassAttributeFilter_CompareTo_Aspect("MachineDirection reverse", x => { x.HasMachineDirectionFilter = true; x.MachineDirection = Types.MachineDirection.Reverse; });
        }

        [TestMethod()]
        public void Test_CellPassAttributeFilter_CompareTo_Machine()
        {
            // TODO readd when machines are available
            //Test_CellPassAttributeFilter_CompareTo_Aspect("Machine", x => { x.HasMachineFilter = true; x.Machine =  });
        }

        [TestMethod()]
        public void Test_CellPassAttributeFilter_CompareTo_MinElevMapping()
        {
            Test_CellPassAttributeFilter_CompareTo_Aspect("MinElevationMapping true", x => { x.HasMinElevMappingFilter = true; x.MinElevationMapping = true; });
            Test_CellPassAttributeFilter_CompareTo_Aspect("MinElevationMapping false", x => { x.HasMinElevMappingFilter = true; x.MinElevationMapping = true; });
        }

        [TestMethod()]
        public void Test_CellPassAttributeFilter_CompareTo_PassType()
        {
            // TODO readd when pass type available
            //Test_CellPassAttributeFilter_CompareTo_Aspect("PassType", x => { x.HasPassTypeFilter = true; x.PassType; });
        }

        [TestMethod()]
        public void Test_CellPassAttributeFilter_CompareTo_PositioningTech()
        {
            Test_CellPassAttributeFilter_CompareTo_Aspect("PositioningTech GPS", x => { x.HasPositioningTechFilter = true; x.PositioningTech = Types.PositioningTech.GPS; });
            Test_CellPassAttributeFilter_CompareTo_Aspect("PositioningTech UTS", x => { x.HasPositioningTechFilter = true; x.PositioningTech = Types.PositioningTech.UTS; });
        }

        [TestMethod()]
        public void Test_CellPassAttributeFilter_CompareTo_VibeState()
        {
            Test_CellPassAttributeFilter_CompareTo_Aspect("VibeState Off", x => { x.HasVibeStateFilter = true; x.VibeState = Types.VibrationState.Off; });
            Test_CellPassAttributeFilter_CompareTo_Aspect("VibeState On", x => { x.HasVibeStateFilter = true; x.VibeState = Types.VibrationState.On; });
        }

        [TestMethod()]
        public void Test_CellPassAttributeFilter_ClearDesigns()
        {
            Test_CellPassAttributeFilter_ClearFilter_Aspect("Designs",
                                                            x => { x.HasDesignFilter = true; x.DesignNameID = 42; },
                                                            x => { return x.HasDesignFilter && x.DesignNameID == 42; },
                                                            x => { x.ClearDesigns(); },
                                                            x => { return !x.HasDesignFilter && x.DesignNameID == Consts.kNoDesignNameID; });
        }

        [TestMethod()]
        public void Test_CellPassAttributeFilter_ClearElevationRange()
        {
            Test_CellPassAttributeFilter_ClearFilter_Aspect("ElevationRange Design",
                                                            x => { x.HasElevationRangeFilter = true;
                                                                   x.ElevationRangeDesign = new DesignDescriptor(1, "fileSpace", "FileSpaceID", "Folder", "FileName", 1.0);
                                                                   x.ElevationRangeOffset = 10;
                                                                   x.ElevationRangeThickness = 1;
                                                            },
                                                            x => { return x.HasElevationRangeFilter && !x.ElevationRangeDesign.IsNull && x.ElevationRangeOffset == 10 && x.ElevationRangeThickness == 1; },
                                                            x => { x.ClearElevationRange(); },
                                                            x => { return !x.HasElevationRangeFilter && x.ElevationRangeDesign.IsNull && x.ElevationRangeOffset == Consts.NullDouble && x.ElevationRangeThickness == Consts.NullDouble; });

            Test_CellPassAttributeFilter_ClearFilter_Aspect("ElevationRange Level",
                                                            x => {
                                                                x.HasElevationRangeFilter = true;
                                                                x.ElevationRangeLevel = 100;
                                                                x.ElevationRangeOffset = 10;
                                                                x.ElevationRangeThickness = 1;
                                                            },
                                                            x => { return x.HasElevationRangeFilter && x.ElevationRangeLevel == 100 && x.ElevationRangeOffset == 10 && x.ElevationRangeThickness == 1; ; },
                                                            x => { x.ClearElevationRange(); },
                                                            x => { return !x.HasElevationRangeFilter && x.ElevationRangeLevel == Consts.NullDouble && x.ElevationRangeLevel == Consts.NullDouble && x.ElevationRangeThickness == Consts.NullDouble; });
        }

        [TestMethod()]
        public void Test_CellPassAttributeFilter_ClearElevationRangeFilterInitialisation()
        {
            CellPassAttributeFilter filter = new CellPassAttributeFilter(null)
            {
                ElevationRangeIsInitialised = true,
                ElevationRangeDesignElevations = new SubGridTrees.Client.ClientHeightLeafSubGrid(null, null, 6, 1, 0)
            };
            filter.ClearElevationRangeFilterInitialisation();

            Assert.IsTrue(filter.ElevationRangeIsInitialised == false && filter.ElevationRangeDesignElevations == null);
        }

        [TestMethod()]
        public void Test_CellPassAttributeFilter_ClearElevationType()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void Test_CellPassAttributeFilter_ClearGPSAccuracy()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void Test_CellPassAttributeFilter_ClearGPSTolerance()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void Test_CellPassAttributeFilter_ClearGuidanceMode()
        {
            Test_CellPassAttributeFilter_ClearFilter_Aspect("GuidanceMode",
                                                            x => { x.HasGCSGuidanceModeFilter = true; x.GCSGuidanceMode = Types.MachineAutomaticsMode.Automatics; },
                                                            x => { return x.HasGCSGuidanceModeFilter && x.GCSGuidanceMode == Types.MachineAutomaticsMode.Automatics; },
                                                            x => { x.ClearGuidanceMode(); },
                                                            x => { return !x.HasGCSGuidanceModeFilter && x.GCSGuidanceMode == Types.MachineAutomaticsMode.Unknown; });
        }

        [TestMethod()]
        public void Test_CellPassAttributeFilter_ClearLayerID()
        {
            Test_CellPassAttributeFilter_ClearFilter_Aspect("LayerID",
                                                            x => { x.HasLayerIDFilter = true; x.LayerID = 42; },
                                                            x => { return x.HasLayerIDFilter && x.LayerID == 42; },
                                                            x => { x.ClearLayerID(); },
                                                            x => { return !x.HasLayerIDFilter && x.LayerID == 0; });
        }

        [TestMethod()]
        public void Test_CellPassAttributeFilter_ClearLayerState()
        {
            Test_CellPassAttributeFilter_ClearFilter_Aspect("Layer State",
                                                            x => { x.HasLayerStateFilter = true; },
                                                            x => { return x.HasLayerStateFilter; },
                                                            x => { x.ClearLayerState(); },
                                                            x => { return !x.HasLayerStateFilter; });
        }

        [TestMethod()]
        public void Test_CellPassAttributeFilter_Assign()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void Test_CellPassAttributeFilter_ClearCompactionMachineOnlyRestriction()
        {
            Test_CellPassAttributeFilter_ClearFilter_Aspect("Compaction only",
                                                            x => { x.HasCompactionMachinesOnlyFilter = true; },
                                                            x => { return x.HasCompactionMachinesOnlyFilter; },
                                                            x => { x.ClearCompactionMachineOnlyRestriction(); },
                                                            x => { return !x.HasCompactionMachinesOnlyFilter; });
        }

        [TestMethod()]
        public void Test_CellPassAttributeFilter_ClearMachineDirection()
        {
            Test_CellPassAttributeFilter_ClearFilter_Aspect("MachineDirection",
                                                            x => { x.HasMachineDirectionFilter = true; x.MachineDirection = Types.MachineDirection.Reverse; },
                                                            x => { return x.HasMachineDirectionFilter; },
                                                            x => { x.ClearMachineDirection(); },
                                                            x => { return !x.HasMachineDirectionFilter && x.MachineDirection == Types.MachineDirection.Unknown; });
        }

        [TestMethod()]
        public void Test_CellPassAttributeFilter_ClearMachines()
        {
            //TODO: Readd when machines are available
            Assert.Inconclusive();
        }

        [TestMethod()]
        public void Test_CellPassAttributeFilter_ClearMinElevationMapping()
        {
            Test_CellPassAttributeFilter_ClearFilter_Aspect("MinElevMapping",
                                                            x => { x.HasMinElevMappingFilter = true; x.MinElevationMapping = true; },
                                                            x => { return x.HasMinElevMappingFilter; },
                                                            x => { x.ClearMinElevationMapping(); },
                                                            x => { return !x.HasMinElevMappingFilter && x.MinElevationMapping == false; });
        }

        [TestMethod()]
        public void Test_CellPassAttributeFilter_ClearPassType()
        {
            // TODO: Readd when pass type is available
            /*Test_CellPassAttributeFilter_ClearFilter_Aspect("PassType",
                                                            x => { x.HasPassTypeFilter = true; x.??? = ??; },
                                                            x => { return x.HasPassTypeFilter; },
                                                            x => { x.ClearPassType(); },
                                                            x => { return !x.HasPassTypeFilter; x.??? = ??; });*/
            Assert.Inconclusive();
        }

        [TestMethod()]
        public void Test_CellPassAttributeFilter_ClearPositioningTech()
        {
            Test_CellPassAttributeFilter_ClearFilter_Aspect("PositioningTech",
                                                            x => { x.HasPositioningTechFilter = true; x.PositioningTech = Types.PositioningTech.GPS; },
                                                            x => { return x.HasPositioningTechFilter; },
                                                            x => { x.ClearPositioningTech(); },
                                                            x => { return !x.HasPositioningTechFilter; });
        }

        [TestMethod()]
        public void Test_CellPassAttributeFilter_ClearSurveyedSurfaceExclusionList()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void Test_CellPassAttributeFilter_ClearTime()
        {
            Test_CellPassAttributeFilter_ClearFilter_Aspect("Time",
                                                            x => { x.HasTimeFilter = true; x.StartTime = new DateTime(2000, 1, 1); x.EndTime = new DateTime(2000, 1, 2); },
                                                            x => { return x.HasTimeFilter; },
                                                            x => { x.ClearTime(); },
                                                            x => { return !x.HasTimeFilter && x.StartTime == DateTime.MinValue && x.EndTime == DateTime.MaxValue; });
        }

        [TestMethod()]
        public void Test_CellPassAttributeFilter_FilterPass()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void Test_CellPassAttributeFilter_FilterPassTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void Test_CellPassAttributeFilter_FilterPassUsingElevationRange()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void Test_CellPassAttributeFilter_FilterPassUsingTimeOnly()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void Test_CellPassAttributeFilter_FilterPass_MachineEvents()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void Test_CellPassAttributeFilter_FilterPass_NoMachineEvents()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void Test_CellPassAttributeFilter_FiltersElevation()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void Test_CellPassAttributeFilter_FiltersElevationTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void Test_CellPassAttributeFilter_FilterSinglePass()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void Test_CellPassAttributeFilter_InitaliaseFilteringForCell()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void Test_CellPassAttributeFilter_InitialiseElevationRangeFilter()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void Test_CellPassAttributeFilter_InitialiseMachineIDsSet()
        {
            // TODO: Add when machine are available
            Assert.Inconclusive();
        }

        [TestMethod()]
        public void Test_CellPassAttributeFilter_IsTimeRangeFilter()
        {
            CellPassAttributeFilter filter = new CellPassAttributeFilter(null);

            Assert.IsFalse(filter.IsTimeRangeFilter(), "Time range set");

            filter.HasTimeFilter = true;
            Assert.IsFalse(filter.IsTimeRangeFilter(), "Time range set");

            filter.StartTime = new DateTime(2000, 1, 1);
            Assert.IsTrue(filter.IsTimeRangeFilter(), "Time range not set");
        }

        [TestMethod()]
        public void Test_CellPassAttributeFilter_FilterMultiplePasses()
        {
            Assert.Fail();
        }
    }
}
