using System;

// todo this is temp until db repo completed
namespace Repositories.DBModels
{
  public class ProjectSettings
    {
      public string ProjectUid { get; set; }

    public string Settings { get; set; }

    public DateTime LastActionedUTC { get; set; }

      public override bool Equals(object obj)
      {
        var otherProject = obj as ProjectSettings;
        if (otherProject == null) return false;
        return otherProject.ProjectUid == this.ProjectUid
               && otherProject.Settings == this.Settings
          ;
      }

    public override int GetHashCode()
      {
        return 0;
      }
    }
  }
