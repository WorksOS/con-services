using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagFileHarvester.Models
{
  public class Organization : IEquatable<Organization>
  {
    public string filespaceId = String.Empty;
    public string shortName = String.Empty;
    public string orgId = String.Empty;
    public string orgDisplayName = String.Empty;
    public string orgTitle = String.Empty;

    public bool Equals(Organization other)
    {
      return filespaceId == other.filespaceId;
    }
  }
}
