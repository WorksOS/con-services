using VSS.Productivity3D.Filter.Abstractions.Models;

namespace MockProjectWebApi.Services
{
  public interface IFiltersService
  {
    FilterData GetFilter(string projectUid, string filterUid);
    FilterListData GetFilters(string projectUid);
  }
}
