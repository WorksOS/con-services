using System.Collections.Generic;
using System.Linq;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.MasterData.Models.Models;

namespace VSS.MasterData.Models.ResultHandling
{
  public class CustomerDataResult : IMasterDataModel
  {
    public int status = 200;
    public Metadata metadata;
    public List<CustomerData> customer { get; set; }

    public List<string> GetIdentifiers() => customer?
                                              .SelectMany(c => c.GetIdentifiers())
                                              .Distinct()
                                              .ToList() ?? new List<string>();
  }

  public class Metadata
  {
    public string msg = "Customers retrieved successfully";
  }
}
