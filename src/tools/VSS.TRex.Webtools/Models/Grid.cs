using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Apache.Ignite.Core;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Grids;

namespace VSS.TRex.Webtools.Models
{
  public class Grid
  {
    
    public string Name { get; set; }
    public bool IsActive
    {
      get
      {
        IIgnite ignite = DIContext.Obtain<ITRexGridFactory>().Grid(Name);
        return ignite != null ? ignite.GetCluster().IsActive() : false;
      }
      set
      {
        IIgnite ignite = DIContext.Obtain<ITRexGridFactory>().Grid(Name);
        ignite?.GetCluster().SetActive(value);
      }
    }

    public Grid(string name)
    {
      Name = name;
    }

  }
}
