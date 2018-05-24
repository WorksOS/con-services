using System;
using VSS.TRex.Common;
using VSS.TRex.Tests.netcore.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Filters.Tests
{
        public class CellPassAttributeFilterTests : IClassFixture<DILoggingFixture>
  {
        [Fact()]
        public void Test_CellPassAttributeFilter_CellPassAttributeFilter()
        {
            CellPassAttributeFilter filter = new CellPassAttributeFilter(/*null*/);

            Assert.False(filter.AnyFilterSelections || filter.AnyMachineEventFilterSelections || filter.AnyNonMachineEventFilterSelections,
                "Filter flags set for default filter");
        }

        [Fact()]
        public void Test_CellPassAttributeFilter_Prepare()
        {
            CellPassAttributeFilter filter = new CellPassAttributeFilter(/*null*/);

            Assert.False(filter.ElevationRangeIsInitialised, "Elevation range is initialised");

            filter.Prepare();

            Assert.False(filter.ElevationRangeIsInitialised, "Elevation range is not initialised");

            filter.ClearFilter();

            Assert.False(filter.AnyFilterSelections, "AnyFilterSelections not false");
            Assert.False(filter.AnyMachineEventFilterSelections, "AnyMachineEventFilterSelections not false");
            Assert.False(filter.AnyNonMachineEventFilterSelections, "AnyNonMachineEventFilterSelections not false");

            filter.HasTimeFilter = true;
            filter.Prepare();

            Assert.True(filter.AnyFilterSelections, "AnyFilterSelections not true after adding time filter");
            Assert.False(filter.AnyMachineEventFilterSelections, "AnyMachineEventFilterSelections not false");
            Assert.True(filter.AnyNonMachineEventFilterSelections, "AnyNonMachineEventFilterSelections not true after adding time filter");

            filter.HasPositioningTechFilter = true;

            filter.Prepare();

            Assert.True(filter.AnyFilterSelections, "AnyFilterSelections not true");
            Assert.True(filter.AnyMachineEventFilterSelections, "AnyMachineEventFilterSelections not true");
            Assert.True(filter.AnyNonMachineEventFilterSelections, "AnyNonMachineEventFilterSelections not true");
        }

        [Fact(Skip = "Not Implemented")]
        public void Test_CellPassAttributeFilter_ClearFilter()
        {
            Assert.True(false);
        }

        private void Test_CellPassAttributeFilter_ClearFilter_Aspect
            (string name, Action<CellPassAttributeFilter> setState, Func<CellPassAttributeFilter, bool> checkSetState,
                         Action<CellPassAttributeFilter> clearState, Func<CellPassAttributeFilter, bool> checkClearState)
        {
            CellPassAttributeFilter filter = new CellPassAttributeFilter(/*null*/);

            Assert.True(checkClearState(filter), $"[{name}] State set when expected to be not set (1)");

            setState(filter);
            Assert.True(checkSetState(filter), $"[{name}] State not set when expected to be set");

            clearState(filter);
            Assert.True(checkClearState(filter), $"[{name}] State set when expected to be not set (2)");
        }

        [Fact()]
        public void Test_CellPassAttributeFilter_ClearVibeState()
        {
            Test_CellPassAttributeFilter_ClearFilter_Aspect("VibeState", 
                                                            x => { x.HasVibeStateFilter = true; x.VibeState = VibrationState.Off; },
                                                            x => x.HasVibeStateFilter && x.VibeState == VibrationState.Off,
                                                            x => { x.ClearVibeState(); }, 
                                                            x => !x.HasVibeStateFilter && x.VibeState == VibrationState.Invalid);
        }

        private void Test_CellPassAttributeFilter_CompareTo_Aspect(string name, Action<CellPassAttributeFilter> SetState)
        {
            CellPassAttributeFilter filter1 = new CellPassAttributeFilter(/*null*/);
            CellPassAttributeFilter filter2 = new CellPassAttributeFilter(/*null*/);

            SetState(filter1);
            Assert.Equal(-1, filter1.CompareTo(filter2));

            SetState(filter2);
            Assert.Equal(0, filter1.CompareTo(filter2));
        }

        [Fact()]
        public void Test_CellPassAttributeFilter_CompareTo_Time()
        {
            Test_CellPassAttributeFilter_CompareTo_Aspect("Time", x => { x.HasTimeFilter = true; x.StartTime = new DateTime(2000, 1, 1); });
        }

        [Fact()]
        public void Test_CellPassAttributeFilter_CompareTo_CompactionMachines()
        {
            Test_CellPassAttributeFilter_CompareTo_Aspect("CompactionMachinesOnly", x => { x.HasCompactionMachinesOnlyFilter = true; });
        }

        [Fact()]
        public void Test_CellPassAttributeFilter_CompareTo_DesignNameID()
        {
            Test_CellPassAttributeFilter_CompareTo_Aspect("Time", x => { x.HasDesignFilter = true; x.DesignNameID = 10; });
        }

        [Fact()]
        public void Test_CellPassAttributeFilter_CompareTo_ElevationRange()
        {
            Test_CellPassAttributeFilter_CompareTo_Aspect("ElevationRange_Design",
                x =>
                {
                    x.HasElevationRangeFilter = true;
                    x.ElevationRangeDesignID = Guid.Empty;
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

        [Fact()]
        public void Test_CellPassAttributeFilter_CompareTo_ElevationType()
        {
            Test_CellPassAttributeFilter_CompareTo_Aspect("ElevationType", x => { x.HasElevationTypeFilter = true; x.ElevationType = ElevationType.Highest; });
        }

        [Fact()]
        public void Test_CellPassAttributeFilter_CompareTo_GCSGuidanceMode()
        {
            Test_CellPassAttributeFilter_CompareTo_Aspect("GCSGuidanceMode", x => { x.HasGCSGuidanceModeFilter = true; x.GCSGuidanceMode = MachineAutomaticsMode.Automatics; });
        }

        [Fact()]
        public void Test_CellPassAttributeFilter_CompareTo_GPSAccuracy()
        {
            Test_CellPassAttributeFilter_CompareTo_Aspect("GPSAccuracy", x => { x.HasGPSAccuracyFilter = true; x.GPSAccuracy = GPSAccuracy.Medium; });
        }

        [Fact()]
        public void Test_CellPassAttributeFilter_CompareTo_GPSTolerance()
        {
            Test_CellPassAttributeFilter_CompareTo_Aspect("GPSTolerance", x => { x.HasGPSToleranceFilter = true; x.GPSTolerance = 1000; });
        }

        [Fact()]
        public void Test_CellPassAttributeFilter_CompareTo_LayerID()
        {
            Test_CellPassAttributeFilter_CompareTo_Aspect("LayerID", x => { x.HasLayerIDFilter = true; x.LayerID = 10; });
        }

        [Fact()]
        public void Test_CellPassAttributeFilter_CompareTo_LayerState()
        {
            Test_CellPassAttributeFilter_CompareTo_Aspect("LayerState On", x => { x.HasLayerStateFilter = true; x.LayerState = LayerState.On; });
            Test_CellPassAttributeFilter_CompareTo_Aspect("LayerState Off", x => { x.HasLayerStateFilter = true; x.LayerState = LayerState.Off; });
        }

        [Fact()]
        public void Test_CellPassAttributeFilter_CompareTo_MachineDirection()
        {
            Test_CellPassAttributeFilter_CompareTo_Aspect("MachineDirection forward", x => { x.HasMachineDirectionFilter = true; x.MachineDirection = MachineDirection.Forward; });
            Test_CellPassAttributeFilter_CompareTo_Aspect("MachineDirection reverse", x => { x.HasMachineDirectionFilter = true; x.MachineDirection = MachineDirection.Reverse; });
        }

        [Fact()]
        public void Test_CellPassAttributeFilter_CompareTo_Machine()
        {
            // TODO readd when machines are available
            //Test_CellPassAttributeFilter_CompareTo_Aspect("Machine", x => { x.HasMachineFilter = true; x.Machine =  });
        }

        [Fact()]
        public void Test_CellPassAttributeFilter_CompareTo_MinElevMapping()
        {
            Test_CellPassAttributeFilter_CompareTo_Aspect("MinElevationMapping true", x => { x.HasMinElevMappingFilter = true; x.MinElevationMapping = true; });
            Test_CellPassAttributeFilter_CompareTo_Aspect("MinElevationMapping false", x => { x.HasMinElevMappingFilter = true; x.MinElevationMapping = true; });
        }

        [Fact()]
        public void Test_CellPassAttributeFilter_CompareTo_PassType()
        {
            Test_CellPassAttributeFilter_CompareTo_Aspect("PassType", x => { x.HasPassTypeFilter = true; x.PassTypeSet = PassTypeSet.Front; });
            Test_CellPassAttributeFilter_CompareTo_Aspect("PassType", x => { x.HasPassTypeFilter = true; x.PassTypeSet = PassTypeSet.Rear; });
            Test_CellPassAttributeFilter_CompareTo_Aspect("PassType", x => { x.HasPassTypeFilter = true; x.PassTypeSet = PassTypeSet.Track; });
            Test_CellPassAttributeFilter_CompareTo_Aspect("PassType", x => { x.HasPassTypeFilter = true; x.PassTypeSet = PassTypeSet.Wheel; });
            Test_CellPassAttributeFilter_CompareTo_Aspect("PassType", x => { x.HasPassTypeFilter = true; x.PassTypeSet = PassTypeSet.Front | PassTypeSet.Rear | PassTypeSet.Wheel | PassTypeSet.Track; });
        }

        [Fact()]
        public void Test_CellPassAttributeFilter_CompareTo_PositioningTech()
        {
            Test_CellPassAttributeFilter_CompareTo_Aspect("PositioningTech GPS", x => { x.HasPositioningTechFilter = true; x.PositioningTech = PositioningTech.GPS; });
            Test_CellPassAttributeFilter_CompareTo_Aspect("PositioningTech UTS", x => { x.HasPositioningTechFilter = true; x.PositioningTech = PositioningTech.UTS; });
        }

        [Fact()]
        public void Test_CellPassAttributeFilter_CompareTo_VibeState()
        {
            Test_CellPassAttributeFilter_CompareTo_Aspect("VibeState Off", x => { x.HasVibeStateFilter = true; x.VibeState = VibrationState.Off; });
            Test_CellPassAttributeFilter_CompareTo_Aspect("VibeState On", x => { x.HasVibeStateFilter = true; x.VibeState = VibrationState.On; });
        }

        [Fact()]
        public void Test_CellPassAttributeFilter_ClearDesigns()
        {
            Test_CellPassAttributeFilter_ClearFilter_Aspect("Designs",
                                                            x => { x.HasDesignFilter = true; x.DesignNameID = 42; },
                                                            x => x.HasDesignFilter && x.DesignNameID == 42,
                                                            x => { x.ClearDesigns(); },
                                                            x => !x.HasDesignFilter && x.DesignNameID == Consts.kNoDesignNameID);
        }

        [Fact()]
        public void Test_CellPassAttributeFilter_ClearElevationRange()
        {
            Test_CellPassAttributeFilter_ClearFilter_Aspect("ElevationRange Design",
                                                            x => { x.HasElevationRangeFilter = true;
                                                                   x.ElevationRangeDesignID = Guid.NewGuid();
                                                                   x.ElevationRangeOffset = 10;
                                                                   x.ElevationRangeThickness = 1;
                                                            },
                                                            x => x.HasElevationRangeFilter && x.ElevationRangeDesignID != Guid.Empty && x.ElevationRangeOffset == 10 && x.ElevationRangeThickness == 1,
                                                            x => { x.ClearElevationRange(); },
                                                            x => !x.HasElevationRangeFilter && x.ElevationRangeDesignID == Guid.Empty && x.ElevationRangeOffset == Consts.NullDouble && x.ElevationRangeThickness == Consts.NullDouble);

            Test_CellPassAttributeFilter_ClearFilter_Aspect("ElevationRange Level",
                                                            x => {
                                                                x.HasElevationRangeFilter = true;
                                                                x.ElevationRangeLevel = 100;
                                                                x.ElevationRangeOffset = 10;
                                                                x.ElevationRangeThickness = 1;
                                                            },
                                                            x => x.HasElevationRangeFilter && x.ElevationRangeLevel == 100 && x.ElevationRangeOffset == 10 && x.ElevationRangeThickness == 1,
                                                            x => { x.ClearElevationRange(); },
                                                            x => !x.HasElevationRangeFilter && x.ElevationRangeLevel == Consts.NullDouble && x.ElevationRangeLevel == Consts.NullDouble && x.ElevationRangeThickness == Consts.NullDouble);
        }

        [Fact()]
        public void Test_CellPassAttributeFilter_ClearElevationRangeFilterInitialisation()
        {
            CellPassAttributeFilter filter = new CellPassAttributeFilter(/*null*/)
            {
                ElevationRangeIsInitialised = true,
                ElevationRangeDesignElevations = new SubGridTrees.Client.ClientHeightLeafSubGrid(null, null, 6, 1, 0)
            };
            filter.ClearElevationRangeFilterInitialisation();

            Assert.True(filter.ElevationRangeIsInitialised == false && filter.ElevationRangeDesignElevations == null);
        }

        [Fact(Skip = "Not Implemented")]
        public void Test_CellPassAttributeFilter_ClearElevationType()
        {
            Assert.True(false);
        }

        [Fact(Skip = "Not Implemented")]
        public void Test_CellPassAttributeFilter_ClearGPSAccuracy()
        {
            Assert.True(false);
        }

        [Fact(Skip = "Not Implemented")]
        public void Test_CellPassAttributeFilter_ClearGPSTolerance()
        {
            Assert.True(false);
        }

        [Fact()]
        public void Test_CellPassAttributeFilter_ClearGuidanceMode()
        {
            Test_CellPassAttributeFilter_ClearFilter_Aspect("GuidanceMode",
                                                            x => { x.HasGCSGuidanceModeFilter = true; x.GCSGuidanceMode = MachineAutomaticsMode.Automatics; },
                                                            x => x.HasGCSGuidanceModeFilter && x.GCSGuidanceMode == MachineAutomaticsMode.Automatics,
                                                            x => { x.ClearGuidanceMode(); },
                                                            x => !x.HasGCSGuidanceModeFilter && x.GCSGuidanceMode == MachineAutomaticsMode.Unknown);
        }

        [Fact()]
        public void Test_CellPassAttributeFilter_ClearLayerID()
        {
            Test_CellPassAttributeFilter_ClearFilter_Aspect("LayerID",
                                                            x => { x.HasLayerIDFilter = true; x.LayerID = 42; },
                                                            x => x.HasLayerIDFilter && x.LayerID == 42,
                                                            x => { x.ClearLayerID(); },
                                                            x => !x.HasLayerIDFilter && x.LayerID == 0);
        }

        [Fact()]
        public void Test_CellPassAttributeFilter_ClearLayerState()
        {
            Test_CellPassAttributeFilter_ClearFilter_Aspect("Layer State",
                                                            x => { x.HasLayerStateFilter = true; },
                                                            x => x.HasLayerStateFilter,
                                                            x => { x.ClearLayerState(); },
                                                            x => !x.HasLayerStateFilter);
        }

        [Fact(Skip = "Not Implemented")]
        public void Test_CellPassAttributeFilter_Assign()
        {
            Assert.True(false);
        }

        [Fact()]
        public void Test_CellPassAttributeFilter_ClearCompactionMachineOnlyRestriction()
        {
            Test_CellPassAttributeFilter_ClearFilter_Aspect("Compaction only",
                                                            x => { x.HasCompactionMachinesOnlyFilter = true; },
                                                            x => x.HasCompactionMachinesOnlyFilter,
                                                            x => { x.ClearCompactionMachineOnlyRestriction(); },
                                                            x => !x.HasCompactionMachinesOnlyFilter);
        }

        [Fact()]
        public void Test_CellPassAttributeFilter_ClearMachineDirection()
        {
            Test_CellPassAttributeFilter_ClearFilter_Aspect("MachineDirection",
                                                            x => { x.HasMachineDirectionFilter = true; x.MachineDirection = MachineDirection.Reverse; },
                                                            x => x.HasMachineDirectionFilter,
                                                            x => { x.ClearMachineDirection(); },
                                                            x => !x.HasMachineDirectionFilter && x.MachineDirection == MachineDirection.Unknown);
        }

        [Fact(Skip = "Not Implemented")]
        public void Test_CellPassAttributeFilter_ClearMachines()
        {
            //TODO: Readd when machines are available
            Assert.True(false);

        }

        [Fact()]
        public void Test_CellPassAttributeFilter_ClearMinElevationMapping()
        {
            Test_CellPassAttributeFilter_ClearFilter_Aspect("MinElevMapping",
                                                            x => { x.HasMinElevMappingFilter = true; x.MinElevationMapping = true; },
                                                            x => x.HasMinElevMappingFilter,
                                                            x => { x.ClearMinElevationMapping(); },
                                                            x => !x.HasMinElevMappingFilter && x.MinElevationMapping == false);
        }

        [Fact()]
        public void Test_CellPassAttributeFilter_ClearPassType()
        {
            Test_CellPassAttributeFilter_ClearFilter_Aspect("PassType",
                                                            x => { x.HasPassTypeFilter = true; x.PassTypeSet = PassTypeSet.Front | PassTypeSet.Rear; },
                                                            x => x.HasPassTypeFilter,
                                                            x => { x.ClearPassType(); },
                                                            x => !x.HasPassTypeFilter && x.PassTypeSet == PassTypeSet.None);
        }

        [Fact()]
        public void Test_CellPassAttributeFilter_ClearPositioningTech()
        {
            Test_CellPassAttributeFilter_ClearFilter_Aspect("PositioningTech",
                                                            x => { x.HasPositioningTechFilter = true; x.PositioningTech = PositioningTech.GPS; },
                                                            x => x.HasPositioningTechFilter,
                                                            x => { x.ClearPositioningTech(); },
                                                            x => !x.HasPositioningTechFilter);
        }

        [Fact(Skip = "Not Implemented")]
        public void Test_CellPassAttributeFilter_ClearSurveyedSurfaceExclusionList()
        {
            Assert.True(false);

        }

        [Fact()]
        public void Test_CellPassAttributeFilter_ClearTime()
        {
            Test_CellPassAttributeFilter_ClearFilter_Aspect("Time",
                                                            x => { x.HasTimeFilter = true; x.StartTime = new DateTime(2000, 1, 1); x.EndTime = new DateTime(2000, 1, 2); },
                                                            x => x.HasTimeFilter,
                                                            x => { x.ClearTime(); },
                                                            x => !x.HasTimeFilter && x.StartTime == DateTime.MinValue && x.EndTime == DateTime.MaxValue);
        }

        [Fact(Skip = "Not Implemented")]
        public void Test_CellPassAttributeFilter_FilterPass()
        {
            Assert.True(false);

        }

        [Fact(Skip = "Not Implemented")]
        public void Test_CellPassAttributeFilter_FilterPassTest1()
        {
            Assert.True(false);

        }

        [Fact(Skip = "Not Implemented")]
        public void Test_CellPassAttributeFilter_FilterPassUsingElevationRange()
        {
            Assert.True(false);

        }

        [Fact(Skip = "Not Implemented")]
        public void Test_CellPassAttributeFilter_FilterPassUsingTimeOnly()
        {
            Assert.True(false);

        }

        [Fact(Skip = "Not Implemented")]
        public void Test_CellPassAttributeFilter_FilterPass_MachineEvents()
        {
            Assert.True(false);

        }

        [Fact(Skip = "Not Implemented")]
        public void Test_CellPassAttributeFilter_FilterPass_NoMachineEvents()
        {
            Assert.True(false);

        }

        [Fact(Skip = "Not Implemented")]
        public void Test_CellPassAttributeFilter_FiltersElevation()
        {
            Assert.True(false);

        }

        [Fact(Skip = "Not Implemented")]
        public void Test_CellPassAttributeFilter_FiltersElevationTest1()
        {
            Assert.True(false);

        }

        [Fact(Skip = "Not Implemented")]
        public void Test_CellPassAttributeFilter_FilterSinglePass()
        {
            Assert.True(false);

        }

        [Fact(Skip = "Not Implemented")]
        public void Test_CellPassAttributeFilter_InitaliaseFilteringForCell()
        {
            Assert.True(false);

        }

        [Fact(Skip = "Not Implemented")]
        public void Test_CellPassAttributeFilter_InitialiseElevationRangeFilter()
        {
            Assert.True(false);

        }

        [Fact(Skip = "Not Implemented")]
        public void Test_CellPassAttributeFilter_InitialiseMachineIDsSet()
        {
            // TODO: Add when machine are available
            Assert.True(false);

        }

        [Fact(Skip = "Not Implemented")]
        public void Test_CellPassAttributeFilter_IsTimeRangeFilter()
        {
            CellPassAttributeFilter filter = new CellPassAttributeFilter(/*null*/);

            Assert.False(filter.IsTimeRangeFilter(), "Time range set");

            filter.HasTimeFilter = true;
            Assert.False(filter.IsTimeRangeFilter(), "Time range set");

            filter.StartTime = new DateTime(2000, 1, 1);
            Assert.True(filter.IsTimeRangeFilter(), "Time range not set");
        }

        [Fact(Skip = "Not Implemented")]
        public void Test_CellPassAttributeFilter_FilterMultiplePasses()
        {
            Assert.True(false);

        }
    }
}
