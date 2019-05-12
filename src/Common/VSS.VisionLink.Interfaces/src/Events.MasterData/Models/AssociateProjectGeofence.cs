﻿using System;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.VisionLink.Interfaces.Events.MasterData.Models
{
  public class AssociateProjectGeofence : IProjectEvent
  {
    public Guid ProjectUID { get; set; }
    public Guid GeofenceUID { get; set; }
    public DateTime ActionUTC { get; set; }
    public DateTime ReceivedUTC { get; set; }
  }
}