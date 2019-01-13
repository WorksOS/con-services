using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TCCToDataOcean
{
  interface IMigrator
  {
    Task<bool> MigrateFilesForAllActiveProjects();
  }
}
