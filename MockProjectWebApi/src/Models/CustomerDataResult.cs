using System.Collections.Generic;
using MockProjectWebApi.Utils;

namespace MockProjectWebApi.Models
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
