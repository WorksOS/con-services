using VSS.MasterData.Models.Models;

namespace MockProjectWebApi.Services
{
  public interface IFiltersService
  {
    FilterData GetFilter(string projectUid, string filterUid);
    FilterListData GetFilters(string projectUid);
  }
}
