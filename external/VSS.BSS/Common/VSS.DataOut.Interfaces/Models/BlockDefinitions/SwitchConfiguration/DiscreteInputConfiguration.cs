using System;


namespace VSS.Nighthawk.DataOut.Interfaces.Models.BlockDefinitions.SwitchConfiguration
{
  /// <summary>
  /// Discrete Input Configuration
  /// This notifies the target about VisionLink’s discrete input configuration.
  /// </summary>
  [Serializable]
  public class DiscreteInputConfiguration : Block
  {
    /// <remarks>The switch number from 1 - 16.</remarks>
    public int Number { get; set; } // The switch number.
    public string Name { get; set; } // The switch name.

    public string OpenDescription { get; set; } // The description to display when this switch in open.
    public string ClosedDescription { get; set; } // The description to display when this switch in closed.
    public double SensitivitySeconds { get; set; }

    #region Optional, either enabled or disabled will be sent.

    public bool Enabled { get; set; }

    #endregion

    #region Monitored - Optional, if specified one of the four 'Monitored' tags must be specified.

    public Monitored? Monitored { get; set; }
    //public MonitoredAlways MonitoredAlways { get; set; }
    //public MonitoredWhenKeyOffEngineOff MonitoredWhenKeyOffEngineOff { get; set; }
    //public MonitoredWhenKeyOnEngineOff MonitoredWhenKeyOnEngineOff { get; set; }
    //public MonitoredWhenKeyOnEngineOn MonitoredWhenKeyOnEngineOn { get; set; }

    #endregion

    public WakeUpWhenClosed WakeUp { get; set; } //Optional, wakes up the radio when switch is closed.
  }
}
