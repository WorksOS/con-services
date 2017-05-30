using System.Collections.Generic;
using MasterDataProxies.ResultHandling;

namespace MasterDataProxies.Models
{
  public class CustomerDataResult : ContractExecutionResult
  {
    /// <summary>
    /// Gets or sets the customer descriptors.
    /// </summary>
    /// <value>
    /// The customer descriptors.
    /// </value>
    public List<CustomerData> CustomerDescriptors { get; set; }
  }
}
