using System;
using System.Collections.Generic;

namespace LandfillService.AcceptanceTests.Models
{
    /// <summary>
    /// Project representation
    /// </summary>
    public class Project
    {
        public uint id { get; set; }
        public string name { get; set; }
        public string timeZoneName { get; set; }      // project time zone name
        public int? daysToSubscriptionExpiry { get; set; }
    }

    /// <summary>
    /// Weight entry submitted by the user
    /// </summary>
    public class WeightEntry
    {
        public DateTime date { get; set; }          // date of the entry; always in the project time zone
        public double weight { get; set; }

        /// <summary>
        /// ToString override
        /// </summary>
        /// <returns>A string representation of volume filter params</returns>
        public override string ToString()
        {
            return String.Format("date:{0}, weight:{1}", date, weight);
        }
    }

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

    /// <summary>
    /// Encapsulates project data sent to the client 
    /// </summary>
    public class ProjectData
    {
        public IEnumerable<DayEntry> entries { get; set; }
        public bool retrievingVolumes { get; set; }          // is the service currently retrieving volumes for this project?
        public Project project { get; set; }
    }
}