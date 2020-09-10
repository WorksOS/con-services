﻿namespace VSS.Productivity3D.Models.Enums
{
  /// <summary>
  /// Provides description of GPS accuracy for the current cell, current machine and current datetime
  /// </summary>
  public enum GPSAccuracy
  {
    /// <summary>
    /// Fine accuracy
    /// </summary>
    Fine = 0,
    /// <summary>
    /// Medium accuracy
    /// </summary>
    Medium = 1,
    /// <summary>
    /// Coarse accuracy
    /// </summary>
    Coarse = 2,
    Unknown = 3
  }
}