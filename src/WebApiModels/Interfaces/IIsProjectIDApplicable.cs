using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VSS.Raptor.Service.WebApiModels.Interfaces
{    
  /// <summary>
  /// Defines whether the project ID is applicable...
  /// </summary>

  public interface IIsProjectIDApplicable
  {
    bool HasProjectID();
  }
}
