using System;
using System.Collections.Generic;

namespace LandfillService.AcceptanceTests.Models.Landfill
{
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
}