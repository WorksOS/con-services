﻿using System;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.VisionLink.Interfaces.Events.MasterData.Models
{
  public class UpdateProjectEvent : IProjectEvent
  {
    public DateTime ProjectEndDate { get; set; }
    public string ProjectTimezone { get; set; }
    public string ProjectName { get; set; }
    public string Description { get; set; }
    public ProjectType ProjectType { get; set; }

    public Guid ProjectUID { get; set; }
    public string ProjectBoundary { get; set; } // this is an addition later in the game, so optional

    public string CoordinateSystemFileName { get; set; }
    public byte[] CoordinateSystemFileContent { get; set; }

    public DateTime ActionUTC { get; set; }
    public DateTime ReceivedUTC { get; set; }
  }
}
