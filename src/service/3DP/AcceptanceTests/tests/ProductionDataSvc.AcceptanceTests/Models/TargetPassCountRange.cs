namespace ProductionDataSvc.AcceptanceTests.Models
{
    /// <summary>
    /// Contains a range of Target Pass Count values.
    /// </summary>
    /// 
    public class TargetPassCountRange
    {
        #region Members
        /// <summary>
        /// The minimum range value. Must be between 1 and 65535.
        /// </summary>
        public ushort min { get; set; }

        /// <summary>
        /// The maximum range value. Must be between 1 and 65535.
        /// </summary>
        public ushort max { get; set; } 
        #endregion

        #region Equality test
        public static bool operator ==(TargetPassCountRange a, TargetPassCountRange b)
        {
            return a.min == b.min && a.min == b.min;
        }

        public static bool operator !=(TargetPassCountRange a, TargetPassCountRange b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is TargetPassCountRange && this == (TargetPassCountRange)obj;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }
}
