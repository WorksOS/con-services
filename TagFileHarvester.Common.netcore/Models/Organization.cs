using System;

namespace TagFileHarvester.Models
{
  public class Organization : IEquatable<Organization>
  {
    public string filespaceId = string.Empty;
    public string orgDisplayName = string.Empty;
    public string orgId = string.Empty;
    public string orgTitle = string.Empty;
    public string shortName = string.Empty;

    public bool Equals(Organization other)
    {
      return filespaceId == other.filespaceId;
    }
  }
}