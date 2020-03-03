using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.MasterData.Customer.AcceptanceTests.Utils.Features.Classes.CustomerRelationshipService
{

  #region CreateCustomerRelationship
  public class CreateCustomerRelationshipModel
    {
      public CreateCustomerRelationshipEvent CreateCustomerRelationshipEvent;
    }

    public class CreateCustomerRelationshipEvent
    {

      public Guid ParentCustomerUID { get; set; }


      public Guid ChildCustomerUID { get; set; }

    public Guid? AccountCustomerUID { get; set; }
    public DateTime ActionUTC { get; set; }

      [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
      public DateTime? ReceivedUTC { get; set; }
    }
    #endregion

    #region DeleteCustomerRelationship
    public class DeleteCustomerRelationshipModel
    {
      public DeleteCustomerRelationshipEvent DeleteCustomerRelationshipEvent;
    }

    public class DeleteCustomerRelationshipEvent
    {

      public Guid ParentCustomerUID { get; set; }


      public Guid ChildCustomerUID { get; set; }


      public DateTime ActionUTC { get; set; }

      [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
      public DateTime? ReceivedUTC { get; set; }
    }
    #endregion

    #region InvalidCreateCustomerRelationship


    public class InvalidCreateCustomerRelationshipEvent
    {

      public string ParentCustomerUID { get; set; }


      public string ChildCustomerUID { get; set; }


      public string ActionUTC { get; set; }

      [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
      public DateTime? ReceivedUTC { get; set; }
    }
    #endregion

    #region InvalidDeleteCustomerRelationship

    public class InvalidDeleteCustomerRelationshipEvent
    {

      public string ParentCustomerUID { get; set; }


      public string ChildCustomerUID { get; set; }


      public string ActionUTC { get; set; }

      [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
      public DateTime? ReceivedUTC { get; set; }
    }
    #endregion
  
}
