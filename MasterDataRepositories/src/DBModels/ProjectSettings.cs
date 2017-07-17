using System;

namespace VSS.MasterData.Repositories.DBModels
{
    public class ProjectSettings
    {
        public string ProjectUid { get; set; }
        public string Settings { get; set; }
        public DateTime LastActionedUtc { get; set; }
    }
}