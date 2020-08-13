namespace VSS.Productivity3D.Models.Models
{
  public enum TAGFileOriginSource
  {    /// <summary>
       /// Represents any machine control system capable of producing a TAG file as defined and used by the CTCT GCS900 & Earthworks product families
       /// </summary>
    LegacyTAGFileSource = 0,

    /// <summary>
    /// A file produced by the Volvo compaction oriented machine assist system
    /// </summary>
    VolvoMachineAssistCompactionCSV = 1,

    /// <summary>
    /// A file produced by the Volvo earthworks oriented machine assist system
    /// </summary>
    VolvoMachineAssistEarthworksCSV = 2
  }
}
