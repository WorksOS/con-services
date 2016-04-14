using System;
using System.Collections.Generic;

namespace LandfillService.AcceptanceTests.Models.Landfill
{
    public class DayEntry
    {
        public DateTime date { get; set; }
        public bool entryPresent { get; set; }
        public double weight { get; set; }
        public double volume { get; set; }
    }
}