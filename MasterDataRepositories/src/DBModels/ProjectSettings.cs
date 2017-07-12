using System;

namespace VSS.Productivity3D.Repo.DBModels
{
    public class ProjectSettings
    {
        public string ProjectUid { get; set; }
        public string Settings { get; set; }
        public DateTime LastActionedUtc { get; set; }
    }
}