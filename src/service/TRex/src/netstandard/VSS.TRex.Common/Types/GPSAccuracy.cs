namespace VSS.TRex.Types
{
    /// <summary>
    /// Global Positioning Accuracy metric emitted from the GCS900 machine control system at the time cell passes
    /// are being measured
    /// </summary>
    public enum GPSAccuracy : byte
  {
        Fine,
        Medium,
        Coarse,
        Unknown
    }
}
