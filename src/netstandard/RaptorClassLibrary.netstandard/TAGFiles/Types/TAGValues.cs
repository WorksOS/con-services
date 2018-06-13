using System.Collections.Generic;
using System.Linq;

namespace VSS.TRex.TAGFiles
{
    /// <summary>
    /// The array of all names TAGs that are supported by the TAG file parser
    /// </summary>
    public static class TAGValueNames
    {
        public const string kTagFileTimeTag  = "TIME";
        public const string kTagFileWeekTag = "WEEK";

        public const string kTagFileCorrectionAgeTag = "AGE";
        public const string kTagFileValidPositionTag = "VALID_POSITION";

        public const string kTagFileLeftTag = "LEFT";
        public const string kTagFileRightTag = "RIGHT";
        public const string kTagFileEastingTag = "EASTING";
        public const string kTagFileNorthingTag = "NORTHING";
        public const string kTagFileElevationTag = "ELEVATION";

        public const string kTagFileLeftTrackTag = "LEFTTRACK";
        public const string kTagFileRightTrackTag = "RIGHTTRACK";
        public const string kTagFileEastingTrackTag = "EASTINGTRACK";
        public const string kTagFileNorthingTrackTag = "NORTHINGTRACK";
        public const string kTagFileElevationTrackTag = "ELEVATIONTRACK";

        public const string kTagFileLeftWheelTag = "LEFTWHEEL";
        public const string kTagFileRightWheelTag = "RIGHTWHEEL";
        public const string kTagFileEastingWheelTag = "EASTINGWHEEL";
        public const string kTagFileNorthingWheelTag = "NORTHINGWHEEL";
        public const string kTagFileElevationWheelTag = "ELEVATIONWHEEL";

        public const string kTagFileLeftRearTag = "LEFTREAR";
        public const string kTagFileRightRearTag = "RIGHTREAR";
        public const string kTagFileEastingRearTag = "EASTINGREAR";
        public const string kTagFileNorthingRearTag = "NORTHINGREAR";
        public const string kTagFileElevationRearTag = "ELEVATIONREAR";

        public const string kTagFileGPSModeTag = "GPS_MODE";
        public const string kTagFileDesignTag = "DESIGN";
        public const string kTagFileSerialTag = "SERIAL";
        public const string kTagFileMachineIDTag = "MACHINEID";
        public const string kTagFileSequenceTag = "SEQ";
        public const string kTagFileStartProofingTag = "START_PROOFING";
        public const string kTagFileStartProofingTimeTag = "END_PROOFING_START_TIME";
        public const string kTagFileStartProofingWeekTag = "END_PROOFING_START_WEEK";
        public const string kTagFileEndProofingNameTag = "END_PROOFING_NAME";
        public const string kTagFileICCCVTag = "CCV";
        public const string kTagFileICCCVTargetTag = "TARGET_CCV";
        public const string kTagFileICPassTargetTag = "TARGET_PASSES";
        public const string kTagFileICRMVTag = "RMV";
        public const string kTagFileICFrequencyTag = "FREQ";
        public const string kTagFileICAmplitudeTag = "AMP";
        public const string kTagFileICGearTag = "GEAR";
        public const string kTagFileICModeTag = "FLAGS";
        public const string kTagFileStartupTag = "STARTUP";
        public const string kTagFileShutdownTag = "SHUTDOWN";
        public const string kTagFileMapReset = "MAP_RESET";
        public const string kTagFileTargetLiftThickness = "TARGET_THICKNESS";
        public const string kTagFileOnGroundTag = "ON_GROUND";
        public const string kTagFileMinElevMappingFlag = "MIN_ELEV_MAP";
        public const string kTagFileBladeOnGroundTag = "BLADE_ON_GROUND";
        public const string kTagFileICMDPTag = "MDP";
        public const string kTagFileICMDPTargetTag = "TARGET_MDP";
        public const string kTagFileWheelWidthTag = "WHEEL_WIDTH";
        public const string kTagFileLayerIDTag = "LAYER";

        // New tags added in GCS900 v10.8
        public const string kTagFileApplicationVersion = "APPLICATION_VERSION";
        public const string kTagFileCompactorSensorType = "COMPACT_SENSOR_TYPE";
        public const string kTagFileRMVJumpThreshold = "RMV_JUMP_THRESHOLD";
        public const string kTagFileVolkelMeasRange = "MEASRANGE";
        public const string kTabFileVolkelMeasRangeUtil = "MEASRANGEUTIL";

        public const string kTagFileControlStateLeftLift = "CONTROL_STATE_LEFT_LIFT";
        public const string kTagFileControlStateRightLift = "CONTROL_STATE_RIGHT_LIFT";
        public const string kTagFileControlStateLift = "CONTROL_STATE_LIFT";
        public const string kTagFileControlStateSideShift = "CONTROL_STATE_SIDE_SHIFT";
        public const string kTagFileControlStateTilt = "CONTROL_STATE_TILT";

        public const string kTagFileDirectionTag = "DIRECTION";

        // New tags added in GCS900 v11.20
        public const string kTagGPSAccuracy = "GPS_ACCURACY";
        public const string kTagInAvoidZone = "IN_AVOID_ZONE";
        public const string kTagMachineType = "MACHINETYPE";
        // Unused by any matcher        public const string kTagReportingTime = "REPORTING_TIME";
        // Unused by any matcher       public const string kTagReportingWeek = "REPORTING_WEEK";
        public const string kTagUTSMode = "UTS_MODE";
// Unused by any matcher        public const string kTagMapRecStatus = "MAP_REC_STATUS";

        // New tags added in GCS900 v11.30
        public const string kTagPositionLatitude = "LATITUDE";
        public const string kTagPositionLongitude = "LONGITUDE";
        public const string kTagPositionHeight = "HEIGHT";
        public const string kTagRadioSerial = "RADIO_SERIAL";
        public const string kTagRadioType = "RADIO_TYPE";

        // New tags added in GCS900 v11.30/12.00
        public const string kTemperatureTag = "TEMP";

        // New tags added in GCS900 v12.00
        public const string kTempLevelMinTag = "TEMP_MIN";
        public const string kTempLevelMaxTag = "TEMP_MAX";
// Unused by any matcher        public const string kTagLLHPosition = "LLH_POSITION";
        public const string kTagUTMZone = "UTM";
        public const string kTagCoordSysType = "COORD_SYS_TYPE";
        public const string kTagGPSBasePosition = "GPS_BASE_POSITION";

        public const string kTag3DSonic = "3D_SONIC";  // GCS All

        public const string kTagResearchData = "RESEARCH_DATA";

        // New TAG fields added to support Quattro
        public const string kTagUsingCCA = "USING_CCA";

        // 34665 add machine speed
        public const string kTagMachineSpeed = "MACHINE_SPEED";

        // CCA
        public const string kTagFileICCCATag = "CCA_PASS_COUNT";
        public const string kTagFileICCCATargetTag = "CCA_MINIMUM_PASSES";

        public const string kTagFileICCCALeftFrontTag = "CCA_PASS_COUNT_LEFT_FRONT";
        public const string kTagFileICCCARightFrontTag = "CCA_PASS_COUNT_RIGHT_FRONT";
        public const string kTagFileICCCALeftRearTag = "CCA_PASS_COUNT_LEFT_REAR";
        public const string kTagFileICCCARightRearTag = "CCA_PASS_COUNT_RIGHT_REAR";

      /// <summary>
      /// Returns all TAG name strings as a list
      /// </summary>
      /// <returns></returns>
      public static List<string> Names() => typeof(TAGValueNames).GetFields().Select(x => x.GetValue(typeof(TAGValueNames)).ToString()).ToList();
  }

  /*
  /// <summary>
  /// Utility class for TAG names
  /// </summary>
  public static class TagValueNamesArray
    {
        /// <summary>
        /// Returns all TAG name strings as a list
        /// </summary>
        /// <returns></returns>
        public static List<string> Names()
        {
            Dictionary<string, string> constProperties = new Dictionary<string, string>();
       
            var fields = typeof(TAGValueNames).GetFields();
            foreach (var field in fields)
            {
                constProperties.Add(field.Name, field.GetValue(typeof(TAGValueNames)).ToString());
            }

            return constProperties.Values.ToList();
        }
    }
    */
}
