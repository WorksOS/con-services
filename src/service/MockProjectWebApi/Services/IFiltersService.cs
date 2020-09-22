using VSS.Productivity3D.Filter.Abstractions.Models;
using VSS.Productivity3D.Filter.Abstractions.Models.ResultHandling;

namespace MockProjectWebApi.Services
{
  public interface IFiltersService
  {
    FilterDescriptorSingleResult GetFilter(string projectUid, string filterUid);
    FilterDescriptorListResult GetFilters(string projectUid);
  }
}
