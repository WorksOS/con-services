using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VSS.Raptor.Service.ConfigurationStore
{
  public interface IConfigurationStore
  {
    string GetValueString(string v);
    bool? GetValueBool(string v);
    int GetValueInt(string v);
    string GetConnectionString(string connectionType);
  }
}
