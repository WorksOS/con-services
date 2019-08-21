using System.Threading.Tasks;

namespace TCCToDataOcean.Interfaces
{
  interface IMigrator
  {
    Task MigrateFilesForAllActiveProjects();
  }
}
