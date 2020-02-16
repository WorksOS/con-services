using System;

namespace VSS.Nighthawk.DataOut.Interfaces.Models.BlockDefinitions.SwitchConfiguration
{
  /// <summary>
  /// Digital Switch Configuration
  /// This notifies the target about VisionLink’s digital switch configuration.
  /// </summary>
  [Serializable]
  public class DigitalSwitchConfiguration : Block
  {
    public int Number { get; set; } // The switch number.

    /// <remarks>A string with max length of 64 alphanumeric chars</remarks>
    public string SwitchOnDescription { get; set; } //The description to display when this switch in on.
    public double SensitivitySeconds { get; set; }

    #region State - One of the four 'State' tags must be specified.

    public SwitchState State { get; set; }

    //public StateNotInstalled StateNotInstalled { get; set; }
    //public StateNotConfigured StateNotConfigured { get; set; }
    //public StateNormallyOpen StateNormallyOpen { get; set; }
    //public StateNormallyClosed StateNormallyClosed { get; set; }
    
    #endregion

    #region Monitored - One of the four 'Monitored' tags must be specified.

    public Monitored Monitored { get; set; }
    
    //public MonitoredAlways MonitoredAlways { get; set; }
    //public MonitoredWhenKeyOffEngineOff MonitoredWhenKeyOffEngineOff { get; set; }
    //public MonitoredWhenKeyOnEngineOff MonitoredWhenKeyOnEngineOff { get; set; }
    //public MonitoredWhenKeyOnEngineOn MonitoredWhenKeyOnEngineOn { get; set; }
    
    #endregion
  }
}
