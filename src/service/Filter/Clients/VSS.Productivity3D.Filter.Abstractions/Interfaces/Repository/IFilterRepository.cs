using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Visionlink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.Productivity3D.Filter.Abstractions.Interfaces.Repository
{
  public interface IFilterRepository
  {
    Task<IEnumerable<MasterData.Repositories.DBModels.Filter>> GetFiltersForProjectUser(string customerUid, string projectUid, string userUid, bool includeAll = false);
    Task<IEnumerable<MasterData.Repositories.DBModels.Filter>> GetFiltersForProject(string projectUid);
    Task<MasterData.Repositories.DBModels.Filter> GetFilter(string filterUid);
    Task<int> DeleteTransientFilters(string deleteOlderThanUtc);
    Task<int> StoreEvent(IFilterEvent evt);
  }
}
