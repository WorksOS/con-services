using System;
using System.Collections.Generic;

namespace LandfillService.AcceptanceTests.Models.Landfill
{
    /// <summary>
    /// Data entry for a given date - part of project data sent to the client 
    /// </summary>
    public class DayEntry
    {
        public DateTime date { get; set; }
        public bool entryPresent { get; set; }    // true if the entry has at least the weight value
        public double weight { get; set; }
        public double volume { get; set; }
    }
}