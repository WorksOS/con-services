using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.MasterData.Customer.AcceptanceTests.Utils.Features.Classes.CustomerRelationshipService
{
  public class Modelstate
  {
    public List<string> createCustomerRelationshipEvent { get; set; }
  }

  public class CustomerRelationshipErrorResponseModel
  {
    public string Message { get; set; }
    public Modelstate ModelState { get; set; }
  }
}
