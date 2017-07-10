using System.Collections.Generic;
using VSS.Productivity3D.MasterDataProxies.Models;

namespace VSS.Productivity3D.MasterDataProxies.ResultHandling
{
  public class CustomerDataResult
  {
    public int status = 200;
    public Metadata metadata;
    public List<CustomerData> customer { get; set; }
  }

  public class Metadata
  {
    public string msg = "Customers retrieved successfully";
  }
}
