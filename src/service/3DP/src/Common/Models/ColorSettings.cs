using System.Collections.Generic;

namespace VSS.Productivity3D.Common.Models
{
  public class ColorSettings
    {
        public ColorValue elevationMinimum;
        public ColorValue elevationMaximum;
        public uint elevationAboveColor;
        public uint elevationBelowColor;
        public bool setToDataExtents;
        public ColorValue cmvMinimum;
        public ColorValue cmvTarget;
        public ColorValue cmvMaximum;
        public ColorValue cmvPercentMinimum;
        public ColorValue cmvPercentTarget;
        public ColorValue cmvPercentMaximum;

        public uint ccvSummaryCompleteLayerColor;
        public uint ccvSummaryWorkInProgressLayerColor;
        public uint ccvSummaryUndercompactedLayerColor;
        public uint ccvSummaryOvercompactedLayerColor;
        public uint ccvSummaryTooThickLayerColor;
        public uint ccvSummaryApprovedLayerColor;

        public List<ColorValue> passCountDetailColors; //There are 9 of these
        public ColorValue passCountMinimum;
        public ColorValue passCountTarget;
        public ColorValue passCountMaximum;
        public double cutGradeTolerance;
        public double fillGradeTolerance;
        public List<ColorValue> cutFillColors;//There are 7 of these

        public uint passProfileFirstColor;
        public uint passProfileLastColor;
        public uint passProfileHighestColor;
        public uint passProfileLowestColor;
        public uint passProfileDesignsColor;
        public uint passProfileSurveysColor;
        public uint passProfileCompositeLastColor;
        public uint passProfileReferenceColor;
        public uint volumeSummaryCoverageColor;
        public uint volumeSummaryVolumeColor;
        public uint volumeSummaryNoChangeColor;

        //Used for both compaction coverage and volumes coverage
        public uint coverageColor;
        public uint surveyedSurfaceColor;

        // temperature used in compaction
        public uint temperatureMinimumColor;
        public uint temperatureTargetColor;
        public uint temperatureMaximumColor;

        // MDP settings
        public ColorValue mdpMinimum;
        public ColorValue mdpTarget;
        public ColorValue mdpMaximum;
        public ColorValue mdpPercentMinimum;
        public ColorValue mdpPercentTarget;
        public ColorValue mdpPercentMaximum;

        public uint mdpSummaryCompleteLayerColor;
        public uint mdpSummaryWorkInProgressLayerColor;
        public uint mdpSummaryUndercompactedLayerColor;
        public uint mdpSummaryOvercompactedLayerColor;
        public uint mdpSummaryTooThickLayerColor;
        public uint mdpSummaryApprovedLayerColor;

        // Machine Speed settings...
        public List<ColorValue> machineSpeedColors; //There are 5 of these

        // Machine speed summary settings...
        public uint machineSpeedMinimumColor;
        public uint machineSpeedTargetColor;
        public uint machineSpeedMaximumColor;

      
      public ColorSettingsFlags colorSettingsFlags;

        public ColorSettings() { }

        public static ColorSettings Default
        {
            get
            {
                if (_default == null)
                {
                    _default = new ColorSettings
                    {
                        elevationMinimum = new ColorValue { color = Colors.Navy, value = 0.0 },
                        elevationMaximum = new ColorValue { color = Colors.Red, value = 2000.0 },
                        elevationAboveColor = Colors.Purple,
                        elevationBelowColor = Colors.Fuchsia,
                        setToDataExtents = false,
                        cmvMinimum = new ColorValue { color = Colors.Blue, value = 0.0 },
                        cmvTarget = new ColorValue { color = Colors.Lime, value = 40.0 },
                        cmvMaximum = new ColorValue { color = Colors.Red, value = 80.0 },
                        cmvPercentMinimum = new ColorValue { color = Colors.Red, value = 0.0 },
                        cmvPercentTarget = new ColorValue { color = Colors.Lime, value = 85.0 },
                        cmvPercentMaximum = new ColorValue { color = Colors.Blue, value = 130.0 },
                        passCountDetailColors = new List<ColorValue>
                        {
                  new ColorValue { color = 0x333300, value = 9 },
                  new ColorValue { color = 0x663300, value = 8 },
                  new ColorValue { color = 0x660099, value = 7 },
                  new ColorValue { color = 0xFF00FF, value = 6 },
                  new ColorValue { color = 0xCCFF00, value = 5 },
                  new ColorValue { color = 0x0099FF, value = 4 },
                  new ColorValue { color = 0x00FFFF, value = 3 },
                  new ColorValue { color = 0xFFFF00, value = 2 },
                  new ColorValue { color = 0xFFCC33, value = 1 }
                },
                        passCountMinimum = new ColorValue { color = Colors.Blue, value = 1 },
                        passCountTarget = new ColorValue { color = Colors.Lime, value = 6.0 },
                        passCountMaximum = new ColorValue { color = Colors.Red, value = 1000000 },
                        cutGradeTolerance = 0.05,
                        fillGradeTolerance = 0.05,
                        cutFillColors = new List<ColorValue>
                        {
                  // match the color patterns that SiteVision Office uses
                  new ColorValue { color = Colors.Maroon, value = 0.05*4 },
                  new ColorValue { color = Colors.Red, value = 0.05*2 },
                  new ColorValue { color = Colors.RedGreenTransition, value = 0.05 },
                  new ColorValue { color = Colors.Lime, value = 0 },
                  new ColorValue { color = Colors.BlueGreenTransition, value = -0.05 },
                  new ColorValue { color = Colors.Blue, value = -0.05*2 },
                  new ColorValue { color = Colors.Navy, value = -0.05*4 }  
                },
                        passProfileFirstColor = Colors.Green,
                        passProfileLastColor = Colors.Fuchsia,
                        passProfileHighestColor = Colors.Blue,
                        passProfileLowestColor = Colors.Orange,
                        passProfileDesignsColor = Colors.Yellow,
                        passProfileSurveysColor = Colors.Cyan,
                        passProfileCompositeLastColor = Colors.Brown,
                        passProfileReferenceColor = Colors.Olive,
                        ccvSummaryCompleteLayerColor = Colors.Lime,
                        ccvSummaryWorkInProgressLayerColor = Colors.Yellow,
                        ccvSummaryUndercompactedLayerColor = Colors.Blue,
                        ccvSummaryOvercompactedLayerColor = Colors.Red,
                        ccvSummaryTooThickLayerColor = Colors.Purple,
                        ccvSummaryApprovedLayerColor = Colors.White,
                        volumeSummaryCoverageColor = Colors.Green,
                        volumeSummaryVolumeColor = Colors.Purple,
                        volumeSummaryNoChangeColor = Colors.Gray,
                        coverageColor = Colors.Green,
                        surveyedSurfaceColor = Colors.Cyan,
                        temperatureMinimumColor = Colors.Blue,
                        temperatureTargetColor = Colors.Lime,
                        temperatureMaximumColor = Colors.Red,
                        // MDP US15167
                        mdpMinimum = new ColorValue { color = Colors.Blue, value = 0.0 },
                        mdpTarget = new ColorValue { color = Colors.Lime, value = 40.0 },
                        mdpMaximum = new ColorValue { color = Colors.Red, value = 80.0 },
                        mdpPercentMinimum = new ColorValue { color = Colors.Red, value = 0.0 },
                        mdpPercentTarget = new ColorValue { color = Colors.Lime, value = 85.0 },
                        mdpPercentMaximum = new ColorValue { color = Colors.Blue, value = 130.0 },
                        mdpSummaryCompleteLayerColor = Colors.Lime,
                        mdpSummaryWorkInProgressLayerColor = Colors.Yellow,
                        mdpSummaryUndercompactedLayerColor = Colors.Blue,
                        mdpSummaryOvercompactedLayerColor = Colors.Red,
                        mdpSummaryTooThickLayerColor = Colors.Purple,
                        mdpSummaryApprovedLayerColor = Colors.White,

                        machineSpeedColors = new List<ColorValue>
                        {
                          new ColorValue { color = 0xCCFF00, value = 2000 },
                          new ColorValue { color = 0x0099FF, value = 1500 },
                          new ColorValue { color = 0x00FFFF, value = 1000 },
                          new ColorValue { color = 0xFFFF00, value = 500 },
                          new ColorValue { color = 0xFFCC33, value = 0 }
                        },

                        machineSpeedMinimumColor = Colors.Aqua,
                        machineSpeedTargetColor  = Colors.Lime,
                        machineSpeedMaximumColor = Colors.Purple,

                        colorSettingsFlags = new ColorSettingsFlags
                        {
                            ccvSummaryWorkInProgressLayerVisible = true,
                            ccvSummaryTooThickLayerVisible = true,
                            mdpSummaryWorkInProgressLayerVisible = true,
                            mdpSummaryTooThickLayerVisible = true
                        },

                    };
                }

                return _default;
            }

        }

        private static ColorSettings _default = null;
    }
}
