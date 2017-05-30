using System.Collections.Generic;
using MockProjectWebApi.Utils;

namespace MockProjectWebApi.Models
{
  public class CustomerDataResult : ContractExecutionResult
  {
    public CustomerDataResult(int code, string message = "success") : base(code, message)
    {
    }

    /// <summary>
    /// Gets or sets the customer descriptors.
    /// </summary>
    /// <value>
    /// The customer descriptors.
    /// </value>
    public List<CustomerData> CustomerDescriptors { get; set; }
  }
}
