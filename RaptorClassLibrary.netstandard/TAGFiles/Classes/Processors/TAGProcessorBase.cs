using System;
using System.Collections.Generic;
using System.Linq;
using VSS.VisionLink.Raptor.Cells;
using VSS.VisionLink.Raptor.Geometry;
using VSS.VisionLink.Raptor.TAGFiles.Types;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes
{
    /// <summary>
    /// TAGProcessorBase extends TAGProcessorStateBase
    /// to provide a processing structure for reading and converting the epoch based
    /// information held in the file into grid (or other) forms of data for use by the
    /// application. Descendant classes must override the following functions:
    ///   DoProcessEpochContext - does all epoch interval based processing
    ///   DoPostProcessFileAction - is called when the entirety of the file has been
    ///                             processed
    ///   DoEpochPreProcessAction - is called for every epoch (regardless of whether
    ///                             the epoch based information is eventually processed
    ///                             or not. It allows client classes to retrieve other
    ///                             information present for the epoch and process it in
    ///                             special ways if necessary.
    /// </summary>
    public abstract class TAGProcessorBase : TAGProcessorStateBase
    {
    /// <summary>
    /// Allow maximum of 10 meters separation between processed epochs
    /// </summary>
    private const double kMaxEpochInterval = 10;

        /// <summary>
        /// Any gap of over this many seconds indicates the loggin was paused
        /// Epochs crossing such gaps, even if they fall within the Epoch distance
        /// interval, are not processed.
        /// </summary>
        private const int kPausedLoggingIntervalSeconds = 5; 

//        private const int kPausedLoggingIntervalInDays = kPausedLoggingInterval* OneSecond;

        /// <summary>
        ///  Destroy any state that the .Net garbage collector wont look after for us.
        /// </summary>
        private void DestroyInterpolationState()
        {
        }

        private void UpdateInterpolationStateForNextEpoch()
        {
            FrontHeightInterpolator1.SetVertices(FrontHeights[0], FrontHeights[1], FrontHeights[2]);
            FrontHeightInterpolator2.SetVertices(FrontHeights[1], FrontHeights[2], FrontHeights[3]);
            FrontTimeInterpolator1.SetVertices(FrontTimes[0], FrontTimes[1], FrontTimes[2]);
            FrontTimeInterpolator2.SetVertices(FrontTimes[1], FrontTimes[2], FrontTimes[3]);

            RearHeightInterpolator1.SetVertices(RearHeights[0], RearHeights[1], RearHeights[2]);
            RearHeightInterpolator2.SetVertices(RearHeights[1], RearHeights[2], RearHeights[3]);
            RearTimeInterpolator1.SetVertices(RearTimes[0], RearTimes[1], RearTimes[2]);
            RearTimeInterpolator2.SetVertices(RearTimes[1], RearTimes[2], RearTimes[3]);

            TrackHeightInterpolator1.SetVertices(TrackHeights[0], TrackHeights[1], TrackHeights[2]);
            TrackHeightInterpolator2.SetVertices(TrackHeights[1], TrackHeights[2], TrackHeights[3]);
            TrackTimeInterpolator1.SetVertices(TrackTimes[0], TrackTimes[1], TrackTimes[2]);
            TrackTimeInterpolator2.SetVertices(TrackTimes[1], TrackTimes[2], TrackTimes[3]);

            WheelHeightInterpolator1.SetVertices(WheelHeights[0], WheelHeights[1], WheelHeights[2]);
            WheelHeightInterpolator2.SetVertices(WheelHeights[1], WheelHeights[2], WheelHeights[3]);
            WheelTimeInterpolator1.SetVertices(WheelTimes[0], WheelTimes[1], WheelTimes[2]);
            WheelTimeInterpolator2.SetVertices(WheelTimes[1], WheelTimes[2], WheelTimes[3]);
        }

        /// <summary>
        /// Constructs all the state necessary to describe an interpolation epoch
        /// </summary>
        private void ConstructInterpolationState()
        {
            // Construct the processing state used to calculate interpolated values

            //------------ FRONT AXLE ----------------
            FrontLeftInterpolationFence = new Fence();
            FrontLeftInterpolationFence.SetRectangleFence(0, 0, 1, 1);

            FrontRightInterpolationFence = new Fence();
            FrontRightInterpolationFence.SetRectangleFence(0, 0, 1, 1);

            for (int I = 0; I < 4; I++)
            {
                FrontHeights[I] = new XYZ(0, 0, 0);
                FrontTimes[I] = new XYZ(0, 0, 0);
            }

            FrontHeightInterpolator1 = new SimpleTriangle(FrontHeights[0], FrontHeights[1], FrontHeights[2]);
            FrontHeightInterpolator2 = new SimpleTriangle(FrontHeights[1], FrontHeights[2], FrontHeights[3]);

            FrontTimeInterpolator1 = new SimpleTriangle(FrontTimes[0], FrontTimes[1], FrontTimes[2]);
            FrontTimeInterpolator2 = new SimpleTriangle(FrontTimes[1], FrontTimes[2], FrontTimes[3]);


            //------------ REAR AXLE ----------------
            RearLeftInterpolationFence = new Fence();
            RearLeftInterpolationFence.SetRectangleFence(0, 0, 1, 1);

            RearRightInterpolationFence = new Fence();
            RearRightInterpolationFence.SetRectangleFence(0, 0, 1, 1);

            for (int I = 0; I < 4; I++)
            {
                RearHeights[I] = new XYZ(0, 0, 0);
                RearTimes[I] = new XYZ(0, 0, 0);
            }

            RearHeightInterpolator1 = new SimpleTriangle(RearHeights[0], RearHeights[1], RearHeights[2]);
            RearHeightInterpolator2 = new SimpleTriangle(RearHeights[1], RearHeights[2], RearHeights[3]);

            RearTimeInterpolator1 = new SimpleTriangle(RearTimes[0], RearTimes[1], RearTimes[2]);
            RearTimeInterpolator2 = new SimpleTriangle(RearTimes[1], RearTimes[2], RearTimes[3]);


            //------------ Track ----------------
            TrackLeftInterpolationFence = new Fence();
            TrackLeftInterpolationFence.SetRectangleFence(0, 0, 1, 1);

            TrackRightInterpolationFence = new Fence();
            TrackRightInterpolationFence.SetRectangleFence(0, 0, 1, 1);

            for (int I = 0; I < 4; I++)
            {
                TrackHeights[I] = new XYZ(0, 0, 0);
                TrackTimes[I] = new XYZ(0, 0, 0);
            }

            TrackHeightInterpolator1 = new SimpleTriangle(TrackHeights[0], TrackHeights[1], TrackHeights[2]);
            TrackHeightInterpolator2 = new SimpleTriangle(TrackHeights[1], TrackHeights[2], TrackHeights[3]);

            TrackTimeInterpolator1 = new SimpleTriangle(TrackTimes[0], TrackTimes[1], TrackTimes[2]);
            TrackTimeInterpolator2 = new SimpleTriangle(TrackTimes[1], TrackTimes[2], TrackTimes[3]);


            //------------ Wheel ----------------
            WheelLeftInterpolationFence = new Fence();
            WheelLeftInterpolationFence.SetRectangleFence(0, 0, 1, 1);

            WheelRightInterpolationFence = new Fence();
            WheelRightInterpolationFence.SetRectangleFence(0, 0, 1, 1);

            for (int I = 0; I < 4; I++)
            {
                WheelHeights[I] = new XYZ(0, 0, 0);
                WheelTimes[I] = new XYZ(0, 0, 0);
            }

            WheelHeightInterpolator1 = new SimpleTriangle(WheelHeights[0], WheelHeights[1], WheelHeights[2]);
            WheelHeightInterpolator2 = new SimpleTriangle(WheelHeights[1], WheelHeights[2], WheelHeights[3]);

            WheelTimeInterpolator1 = new SimpleTriangle(WheelTimes[0], WheelTimes[1], WheelTimes[2]);
            WheelTimeInterpolator2 = new SimpleTriangle(WheelTimes[1], WheelTimes[2], WheelTimes[3]);

            int NumFences = Enum.GetValues(typeof(MachineSide)).Length;
            InterpolationFences = Enumerable.Range(1, NumFences).Select(x => new List<Fence>()).ToArray();
           // InterpolationFences[(int)MachineSide.None] = null;
        }

        /// <summary>
        /// Prepares the processor for the next interplation interval between measurement epochs
        /// </summary>
        private void ClearInterpolationState()
        {
            DestroyInterpolationState();
            ConstructInterpolationState();
        }

        /// <summary>
        /// Set up the interpolation state for processing of an epoch interval between two measurement epochs
        /// </summary>
        /// <param name="ClearInterpolators"></param>
        /// <param name="LeftInterpolationFence"></param>
        /// <param name="RightInterpolationFence"></param>
        /// <param name="LeftFence1"></param>
        /// <param name="LeftFence2"></param>
        /// <param name="RightFence1"></param>
        /// <param name="RightFence2"></param>
        /// <param name="Height1"></param>
        /// <param name="Height2"></param>
        /// <param name="Time1"></param>
        /// <param name="Time2"></param>
        /// <param name="ADataTime"></param>
        /// <param name="ADataLeft"></param>
        /// <param name="ADataRight"></param>
        private void SetupInterpolators(bool ClearInterpolators,
                                   Fence LeftInterpolationFence, Fence RightInterpolationFence,
                                   FencePoint LeftFence1, FencePoint LeftFence2,
                                   FencePoint RightFence1, FencePoint RightFence2,
                                   ref XYZ Height1, ref XYZ Height2,
                                   ref XYZ Time1, ref XYZ Time2,
                                   double ADataTime,
                                   XYZ ADataLeft, XYZ ADataRight)
        {
            if (ClearInterpolators)
            {
                foreach (var t in InterpolationFences)
                {
                    t.Clear();
                }
            }

            if (ADataLeft.IsNull || ADataRight.IsNull) 
            {
                // There is no data available to construct an interpolation context...
                return;
            }

            if (MachineWheelWidth > 0)
            {
                double DeltaX = ADataRight.X - ADataLeft.X;
                double DeltaY = ADataRight.Y - ADataLeft.Y;
                double WheelBaseWidth = Math.Sqrt(DeltaX * DeltaX + DeltaY * DeltaY);
                double Ratio = MachineWheelWidth / WheelBaseWidth;

                if (Ratio >= 0.5)
                {
                    LeftFence1.SetXY(ADataLeft.X, ADataLeft.Y);
                    LeftFence2.SetXY(ADataRight.X, ADataRight.Y);

//                    LeftInterpolationFence.UpdateExtents();
                    InterpolationFences[MachineSideConst.None].Add(LeftInterpolationFence); // machineside none
        }
                else
                {
                    LeftFence1.SetXY(ADataLeft.X, ADataLeft.Y);
                    LeftFence2.SetXY(ADataLeft.X + Ratio * DeltaX, ADataLeft.Y + Ratio * DeltaY);

//                    LeftInterpolationFence.UpdateExtents();
                    InterpolationFences[MachineSideConst.Left].Add(LeftInterpolationFence); // machineside left

          RightFence1.SetXY(ADataRight.X - Ratio * DeltaX, ADataRight.Y - Ratio * DeltaY);
                    RightFence2.SetXY(ADataRight.X, ADataRight.Y);

//                    RightInterpolationFence.UpdateExtents();
                    InterpolationFences[MachineSideConst.Right].Add(RightInterpolationFence); // machineside right
        }
            }
            else
            {
                LeftFence1.SetXY(ADataLeft.X, ADataLeft.Y);
                LeftFence2.SetXY(ADataRight.X, ADataRight.Y);

//                LeftInterpolationFence.UpdateExtents();
                InterpolationFences[MachineSideConst.None].Add(LeftInterpolationFence); // machineside none
            }

            Height1 = ADataLeft;
            Height2 = ADataRight;

            Time1 = new XYZ(ADataLeft.X, ADataLeft.Y, ADataTime);
            Time2 = new XYZ(ADataRight.X, ADataRight.Y, ADataTime);
        }

        /// <summary>
        /// At every epoch we process a set of state information that has been set into
        /// this processor by the active tag file value sink. Most of this information
        /// persists between epochs. However, some of this information is cleared at
        /// the end of each epoch. ClearEpochSpecificData performs this operation
        /// and is called at the end of ProcessEpochContext()
        /// </summary>
        protected virtual void ClearEpochSpecificData()
        {
            // Clear the proofing run related information
            StartProofing = "";
            StartProofingTime = 0;
            StartProofingWeek = 0;
            EndProofingName = "";

            StartProofingDataTime = DateTime.MinValue;
            HaveSeenAProofingStart = false;
        }

        /// <summary>
        /// Defines the maximum distance between two epochs that is supported for processing.
        /// Epochs with larger intervals are not processed.
        /// </summary>
        /// <returns></returns>
        protected virtual double MaxEpochInterval()
        {
            return kMaxEpochInterval;
        }

        /// <summary>
        /// Ignore invalid positions encountered in the TAG value data.
        /// </summary>
        /// <returns></returns>
        protected virtual bool IgnoreInvalidPositions()
        {
            return false;
        }

        public Fence FrontLeftInterpolationFence { get; set; }
        public Fence FrontRightInterpolationFence { get; set; }

        public Fence RearLeftInterpolationFence { get; set; }
        public Fence RearRightInterpolationFence { get; set; }

        public Fence TrackLeftInterpolationFence { get; set; }
        public Fence TrackRightInterpolationFence { get; set; }

        public Fence WheelLeftInterpolationFence { get; set; }
        public Fence WheelRightInterpolationFence { get; set; }

        public List<Fence>[] InterpolationFences { get; set; }

        public SimpleTriangle FrontHeightInterpolator1 { get; set; }
        public SimpleTriangle FrontHeightInterpolator2 { get; set; }
        public SimpleTriangle FrontTimeInterpolator1 { get; set; }
        public SimpleTriangle FrontTimeInterpolator2 { get; set; }

        public bool HasRearAxleInThisEpoch { get; set; }
        public bool HasTrackInThisEpoch { get; set; }
        public bool HasWheelInThisEpoch { get; set; }

        public SimpleTriangle RearHeightInterpolator1 { get; set; }
        public SimpleTriangle RearHeightInterpolator2 { get; set; }
        public SimpleTriangle RearTimeInterpolator1 { get; set; }
        public SimpleTriangle RearTimeInterpolator2 { get; set; }

        public SimpleTriangle TrackHeightInterpolator1 { get; set; }
        public SimpleTriangle TrackHeightInterpolator2 { get; set; }
        public SimpleTriangle TrackTimeInterpolator1 { get; set; }
        public SimpleTriangle TrackTimeInterpolator2 { get; set; }

        public SimpleTriangle WheelHeightInterpolator1 { get; set; }
        public SimpleTriangle WheelHeightInterpolator2 { get; set; }
        public SimpleTriangle WheelTimeInterpolator1 { get; set; }
        public SimpleTriangle WheelTimeInterpolator2 { get; set; }

        private DateTime PrevEpochTime { get; set; } = DateTime.MinValue;

        XYZ[] FrontHeights = new XYZ[4];  // First and second Epoch points for front axle
        XYZ[] FrontTimes = new XYZ[4];  // First and second Epoch points for front axle

        XYZ[] RearHeights = new XYZ[4];  // First and second Epoch points for rear axle
        XYZ[] RearTimes = new XYZ[4];  // First and second Epoch points for rear axle

        XYZ[] TrackHeights = new XYZ[4];  // First and second Epoch points for track
        XYZ[] TrackTimes = new XYZ[4];  // First and second Epoch points for track

        XYZ[] WheelHeights = new XYZ[4];  // First and second Epoch points for wheel
        XYZ[] WheelTimes = new XYZ[4];  // First and second Epoch points for wheel

        /// <summary>
        /// No argument constructor
        /// </summary>
        public TAGProcessorBase()
        {
            ConstructInterpolationState();
        }

        /// <summary>
        /// Processes the TAG values in given file 
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public virtual bool ProcessFile(string filename)
        {
            return false; // Not implemented
        }

        /// <summary>
        /// Performs any required processing of the state acquired for the currnet time epoch in teh TAG values
        /// </summary>
        /// <returns></returns>
        public override bool ProcessEpochContext()
        {
            HasRearAxleInThisEpoch = HaveReceivedValidRearPositions;
            HasTrackInThisEpoch = HaveReceivedValidTrackPositions;
            HasWheelInThisEpoch = HaveReceivedValidWheelPositions;

            // Substitute incoming blade positions with converted ones...
            if (ConvertedBladePositions.Count > 0 && ConvertedBladePositions[VisitedEpochCount].UTMZone > 0)
            {
                DataLeft = ConvertedBladePositions[VisitedEpochCount].Left;
                DataRight = ConvertedBladePositions[VisitedEpochCount].Right;
            }

            // Substitute incoming rear axle positions with converted ones...
            if (HasRearAxleInThisEpoch)
            {
                if (ConvertedRearAxlePositions.Count > 0 && ConvertedRearAxlePositions[VisitedEpochCount].UTMZone > 0)
                {
                    DataRearLeft = ConvertedRearAxlePositions[VisitedEpochCount].Left;
                    DataRearRight = ConvertedRearAxlePositions[VisitedEpochCount].Right;
                }
            }

            // Substitute incoming track positions with converted ones...
            if (HasTrackInThisEpoch)
            {
                if (ConvertedTrackPositions.Count > 0 && ConvertedTrackPositions[VisitedEpochCount].UTMZone > 0)
                {
                    DataTrackLeft = ConvertedTrackPositions[VisitedEpochCount].Left;
                    DataTrackRight = ConvertedTrackPositions[VisitedEpochCount].Right;
                }
            }

            // Substitute incoming wheel positions with converted ones...
            if (HasWheelInThisEpoch)
            {
                if (ConvertedWheelPositions.Count > 0 && ConvertedWheelPositions[VisitedEpochCount].UTMZone > 0)
                {
                    DataWheelLeft = ConvertedWheelPositions[VisitedEpochCount].Left;
                    DataWheelRight = ConvertedWheelPositions[VisitedEpochCount].Right;
                }
            }

            VisitedEpochCount++;

            try
            {
                if (!base.ProcessEpochContext())
                {
                    return false;
                }

                if (!DoEpochPreProcessAction())
                {
                    return false;

                }

                // Check to see if we have a position that is of good enough quality to use.
                if (ValidPosition == 0)
                {
                    // If we're ignoring invalid positions, then don't use any of the positional
                    // information in the invalid epoch, but DO use the other information recorded
                    // within it (eg: CCV, GPS Mode etc).

                    // We do this by allowing subsequent valid position epoch to form a processing
                    // interval with the last valid position epoch. This processing interval uses
                    // the cached CCV values etc read for the intervening invalid position epochs.

                    // If we are not ignoring invalid positions (ie: we are processing them),
                    // then we continue and process the epoch as if the position information
                    // within it is 'valid'.

                    if (IgnoreInvalidPositions())  // Don't process this interval...
                    {
                        return false;
                    }
                }

                if (!HaveFirstEpoch)
                {
                    // We do not have a viable processing context yet. Set up the first
                    // epoch and leave...
                    HaveFirstEpoch = true;
                    PrevEpochTime = DataTime;

                    //------------ FRONT AXLE ----------------
                    SetupInterpolators(true,
                                       FrontLeftInterpolationFence, FrontRightInterpolationFence,
                                       FrontLeftInterpolationFence[1], FrontLeftInterpolationFence[0],
                                       FrontRightInterpolationFence[1], FrontRightInterpolationFence[0],
                                       ref FrontHeights[0], ref FrontHeights[1],
                                       ref FrontTimes[0], ref FrontTimes[1],
                                       DataTime.ToOADate(), DataLeft, DataRight);
                }

                //------------ REAR AXLE ----------------
                if (HasRearAxleInThisEpoch && !HaveFirstRearEpoch)
                {
                    HaveFirstRearEpoch = true;

                    SetupInterpolators(false,
                                       RearLeftInterpolationFence, RearRightInterpolationFence,
                                       RearLeftInterpolationFence[1], RearLeftInterpolationFence[0],
                                       RearRightInterpolationFence[1], RearRightInterpolationFence[0],
                                       ref RearHeights[0], ref RearHeights[1],
                                       ref RearTimes[0], ref RearTimes[1],
                                       DataTime.ToOADate(), DataRearLeft, DataRearRight);
                }


                //------------ Track ----------------
                if (HasTrackInThisEpoch && !HaveFirstTrackEpoch)
                {
                    HaveFirstTrackEpoch = true;

                    SetupInterpolators(false,
                                       TrackLeftInterpolationFence, TrackRightInterpolationFence,
                                       TrackLeftInterpolationFence[1], TrackLeftInterpolationFence[0],
                                       TrackRightInterpolationFence[1], TrackRightInterpolationFence[0],
                                       ref TrackHeights[0], ref TrackHeights[1],
                                       ref TrackTimes[0], ref TrackTimes[1],
                                       DataTime.ToOADate(), DataTrackLeft, DataTrackRight);
                }

                //------------ Wheel ----------------
                if (HasWheelInThisEpoch && !HaveFirstWheelEpoch)
                {
                    HaveFirstWheelEpoch = true;

                    SetupInterpolators(false,
                                       WheelLeftInterpolationFence, WheelRightInterpolationFence,
                                       WheelLeftInterpolationFence[1], WheelLeftInterpolationFence[0],
                                       WheelRightInterpolationFence[1], WheelRightInterpolationFence[0],
                                       ref WheelHeights[0], ref WheelHeights[1],
                                       ref WheelTimes[0], ref WheelTimes[1],
                                       DataTime.ToOADate(), DataWheelLeft, DataWheelRight);
                }

                if (!HaveFirstEpoch)
                {
                    return false;
                }

                // Get details for second epoch
                //------------ FRONT AXLE ----------------
                SetupInterpolators(true,
                                   FrontLeftInterpolationFence, FrontRightInterpolationFence,
                                   FrontLeftInterpolationFence[2], FrontLeftInterpolationFence[3],
                                   FrontRightInterpolationFence[2], FrontRightInterpolationFence[3],
                                   ref FrontHeights[2], ref FrontHeights[3],
                                   ref FrontTimes[2], ref FrontTimes[3],
                                   DataTime.ToOADate(), DataLeft, DataRight);

                //------------ REAR AXLE ----------------
                if (HasRearAxleInThisEpoch && HaveFirstRearEpoch)
                {
                    SetupInterpolators(false,
                             RearLeftInterpolationFence, RearRightInterpolationFence,
                             RearLeftInterpolationFence[2], RearLeftInterpolationFence[3],
                             RearRightInterpolationFence[2], RearRightInterpolationFence[3],
                             ref RearHeights[2], ref RearHeights[3],
                             ref RearTimes[2], ref RearTimes[3],
                             DataTime.ToOADate(), DataRearLeft, DataRearRight);
                }

                //------------ Track ----------------
                if (HasTrackInThisEpoch && HaveFirstTrackEpoch)
                {
                    SetupInterpolators(false,
                             TrackLeftInterpolationFence, TrackRightInterpolationFence,
                             TrackLeftInterpolationFence[2], TrackLeftInterpolationFence[3],
                             TrackRightInterpolationFence[2], TrackRightInterpolationFence[3],
                             ref TrackHeights[2], ref TrackHeights[3],
                             ref TrackTimes[2], ref TrackTimes[3],
                             DataTime.ToOADate(), DataTrackLeft, DataTrackRight);
                }


                //------------ Wheel ----------------
                if (HasWheelInThisEpoch && HaveFirstWheelEpoch)
                {
                    SetupInterpolators(false,
                             WheelLeftInterpolationFence, WheelRightInterpolationFence,
                             WheelLeftInterpolationFence[2], WheelLeftInterpolationFence[3],
                             WheelRightInterpolationFence[2], WheelRightInterpolationFence[3],
                             ref WheelHeights[2], ref WheelHeights[3],
                             ref WheelTimes[2], ref WheelTimes[3],
                             DataTime.ToOADate(), DataWheelLeft, DataWheelRight);
                }

                UpdateInterpolationStateForNextEpoch();

                ProcessedEpochCount++;
                double MaxEpochIntervalSquared = MaxEpochInterval() * MaxEpochInterval();

        // If the time intervalue between the two epochs is > kPausedLoggingInterval then
        // don't process this epoch pair as this indicates logging has been paused.
        // If the distance between the two epochs is > kMaxEpochInterval, then don't process this epoch pair
        // Test both sides of the quadrilateral to see if either is longer than the largest inter-epoch gap


        // todo check this code works      
        //foreach (MachineSide machSide in Enum.GetValues(typeof(MachineSide)))
          for (int J = 0; J < InterpolationFences.Length; J++)
              {

                for (int I = 0; I < InterpolationFences[J].Count; I++)
                {
                  if (DataTime < PrevEpochTime.AddSeconds(kPausedLoggingIntervalSeconds) && !InterpolationFences[J][I].IsNull())
                  {
                    if (((Math.Pow(InterpolationFences[J][I][0].X - InterpolationFences[J][I][3].X, 2) +
                          Math.Pow(InterpolationFences[J][I][0].Y - InterpolationFences[J][I][3].Y, 2)) <= MaxEpochIntervalSquared) &&
                        ((Math.Pow(InterpolationFences[J][I][1].X - InterpolationFences[J][I][2].X, 2) +
                          Math.Pow(InterpolationFences[J][I][1].Y - InterpolationFences[J][I][2].Y, 2)) <= MaxEpochIntervalSquared))
                    {
                      //                            InterpolationFences[I].UpdateExtents();

                      // Process the quadrilateral formed by the two epochs
                      if (!DoProcessEpochContext(InterpolationFences[J][I],(MachineSide)J))
                      {
                        return false;
                      }
                    }
                  }
                }
              }


        // Set first epoch to second for next loop

        //------------ FRONT AXLE ----------------
        FrontLeftInterpolationFence[0].Assign(FrontLeftInterpolationFence[3]);
                FrontLeftInterpolationFence[1].Assign(FrontLeftInterpolationFence[2]);

                FrontRightInterpolationFence[0].Assign(FrontRightInterpolationFence[3]);
                FrontRightInterpolationFence[1].Assign(FrontRightInterpolationFence[2]);

                FrontHeights[0] = FrontHeights[2];
                FrontHeights[1] = FrontHeights[3];

                FrontTimes[0] = FrontTimes[2];
                FrontTimes[1] = FrontTimes[3];

                //------------ REAR AXLE ----------------
                if (HasRearAxleInThisEpoch && HaveFirstRearEpoch)
                {
                    RearLeftInterpolationFence[0].Assign(RearLeftInterpolationFence[3]);
                    RearLeftInterpolationFence[1].Assign(RearLeftInterpolationFence[2]);

                    RearRightInterpolationFence[0].Assign(RearRightInterpolationFence[3]);
                    RearRightInterpolationFence[1].Assign(RearRightInterpolationFence[2]);

                    RearHeights[0] = RearHeights[2];
                    RearHeights[1] = RearHeights[3];

                    RearTimes[0] = RearTimes[2];
                    RearTimes[1] = RearTimes[3];
                }

                //------------ Track ----------------
                if (HasTrackInThisEpoch && HaveFirstTrackEpoch)
                {
                    TrackLeftInterpolationFence[0].Assign(TrackLeftInterpolationFence[3]);
                    TrackLeftInterpolationFence[1].Assign(TrackLeftInterpolationFence[2]);

                    TrackRightInterpolationFence[0].Assign(TrackRightInterpolationFence[3]);
                    TrackRightInterpolationFence[1].Assign(TrackRightInterpolationFence[2]);

                    TrackHeights[0] = TrackHeights[2];
                    TrackHeights[1] = TrackHeights[3];

                    TrackTimes[0] = TrackTimes[2];
                    TrackTimes[1] = TrackTimes[3];
                }

                //------------ Wheel ----------------
                if (HasWheelInThisEpoch && HaveFirstWheelEpoch)
                {
                    WheelLeftInterpolationFence[0].Assign(WheelLeftInterpolationFence[3]);
                    WheelLeftInterpolationFence[1].Assign(WheelLeftInterpolationFence[2]);

                    WheelRightInterpolationFence[0].Assign(WheelRightInterpolationFence[3]);
                    WheelRightInterpolationFence[1].Assign(WheelRightInterpolationFence[2]);

                    WheelHeights[0] = WheelHeights[2];
                    WheelHeights[1] = WheelHeights[3];

                    WheelTimes[0] = WheelTimes[2];
                    WheelTimes[1] = WheelTimes[3];
                }

                PrevEpochTime = DataTime;

                DiscardAllButLatestAttributeAccumulatorValues();
            }
            finally
            {
                ClearEpochSpecificData();
            }

            return true;
        }

    /// <summary>
    /// DoProcessEpochContext is the method that does the actual processing
    /// of the epoch intervals into the appropriate data structures. Descendant
    /// classes must override this function.
    /// </summary>
    /// <param name="InterpolationFence"></param>
    /// <param name="machineSide"></param>
    /// <returns></returns>
    public abstract bool DoProcessEpochContext(Fence InterpolationFence, MachineSide machineSide);

        /// <summary>
        /// DoPostProcessFileAction is called immediately after the file has been
        /// processed. It allows a descendent class to implement appropriate actions
        /// such as saving data when the reading process is complete.
        /// SuccessState reflects the success or failure of the file processing.
        /// </summary>
        /// <param name="successState"></param>
        public abstract void DoPostProcessFileAction(bool successState);

        /// <summary>
        /// DoEpochPreProcessAction is called in ProcessEpochContext immediately
        /// before any processing of the epoch information is done. It allows a
        /// descendent class to implement appropriate actions such as inspecting
        /// or processing other information in the epoch not direclty related
        /// to the epoch interval itself (such as proofing run information in
        /// intelligent compaction tag files.
        /// </summary>
        /// <returns></returns>
        public abstract bool DoEpochPreProcessAction();

      public byte SelectCCAValue(DateTime dateTime, PassType passType, MachineSide machineSide)
      {
        byte myResult = CellPass.NullCCA;

        switch (passType)
        {
          case PassType.Front:
            switch (machineSide)
            {
              case MachineSide.Left:
                myResult = ICCCALeftFrontValues.GetCCAValueAtDateTime(dateTime);
                break;
              case MachineSide.Right:
                myResult = ICCCARightFrontValues.GetCCAValueAtDateTime(dateTime);
                break;
            }
            break;

          case PassType.Rear:
            switch (machineSide)
            {
              case MachineSide.Left:
                myResult = ICCCALeftRearValues.GetCCAValueAtDateTime(dateTime);
                break;
              case MachineSide.Right:
                myResult = ICCCARightRearValues.GetCCAValueAtDateTime(dateTime);
                break;
            }
            break;
        }

        if (myResult == CellPass.NullCCA)
             myResult = ICCCAValues.GetCCAValueAtDateTime(dateTime);

        return myResult;
      }
    }
}

