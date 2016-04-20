using System;
using System.Collections.Generic;

namespace LandfillService.AcceptanceTests.Models.Landfill
{
    public class ProjectData
    {
        public IEnumerable<DayEntry> entries { get; set; }
        public bool retrievingVolumes { get; set; } 
        public Project project { get; set; }
    }
}