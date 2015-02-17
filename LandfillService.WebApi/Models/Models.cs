using System;

namespace LandfillService.WebApi.Models
{
    public class Credentials
    {
        public string userName { get; set; }
        public string password { get; set; }
    }

    public class Project
    {
        public int id { get; set; }
        public string name { get; set; }
    };

    public class DayEntry
    {
        public DateTime date { get; set; }
        public bool entryPresent { get; set; }
        public int density { get; set; }
        public int weight { get; set; }
    };


}