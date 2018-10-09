using System;
using System.Collections.Generic;

namespace VSS.VersionTool
{
  public class VersionNumber : IEquatable<VersionNumber>
  {
    public VersionNumber(string version)
    {
      Version = version;
      var split = version.Split(".");
      if (split.Length >= 1)
        Major = split[0];
      if(split.Length >= 2)
        Minor = split[1];
      if(split.Length >= 3)
        Patch = split[2];
    }

    public string Version { get; }

    public string Major { get; }

    public string Minor { get; }

    public string Patch { get; }

    public override string ToString()
    {
      return Version;
    }

    public bool Equals(VersionNumber other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;

      var versionValues = new List<Tuple<string, string>>
      {
        new Tuple<string, string>(Major, other.Major),
        new Tuple<string, string>(Minor, other.Minor),
        new Tuple<string, string>(Patch, other.Patch)
      };

      foreach (var value in versionValues)
      {
        if (int.TryParse(value.Item1, out var ourValue) && int.TryParse(value.Item2, out var theirValue))
        {
          if (ourValue != theirValue)
            return false;
        }
        else if(string.Compare(value.Item1, value.Item2, StringComparison.CurrentCultureIgnoreCase) != 0)
        {
          return value.Item1 == "*" || value.Item2 == "*";
        }
      }

      // If we got this far, all the values are an exact match
      return true;
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((VersionNumber) obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = (Version != null ? Version.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ (Major != null ? Major.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ (Minor != null ? Minor.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ (Patch != null ? Patch.GetHashCode() : 0);
        return hashCode;
      }
    }
  }
}