using CommonModel.Enum;
using System;

namespace DbModel.AssetSettings
{
	public class AssetWeeklyTargetsDto
    {
        public Guid AssetTargetUID { get; set; }
        public Guid AssetUID { get; set; }
        public DateTime? EndDate { get; set; }
        public double FridayTargetValue { get; set; }
        public double MondayTargetValue { get; set; }
        public double SaturdayTargetValue { get; set; }
        public DateTime StartDate { get; set; }
        public double SundayTargetValue { get; set; }
        public AssetTargetType TargetType { get; set; }
        public double ThursdayTargetValue { get; set; }
        public double TuesdayTargetValue { get; set; }
        public double WednesdayTargetValue { get; set; }
        public Guid? UserUID { get; set; }

        public bool Status { get; set; }
    }
}
