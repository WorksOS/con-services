using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace VSS.MasterData.Customer.AcceptanceTests.Utils.Features.Classes.CustomerService
{

    public class CreateCustomerModel
    {
        public CreateCustomerEvent CreateCustomerEvent;
    }

    public class CreateCustomerEvent
    {
        public string CustomerName { get; set; }
        public string CustomerType { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string BSSID { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string DealerNetwork { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string NetworkDealerCode { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string NetworkCustomerCode { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string DealerAccountCode { get; set; }
        public Guid CustomerUID { get; set; }
        public DateTime ActionUTC { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? ReceivedUTC { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string PrimaryContactEmail { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string FirstName { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string LastName { get; set; }
    }

    public class UpdateCustomerModel
    {
        public UpdateCustomerEvent UpdateCustomerEvent;
    }

    public class UpdateCustomerEvent
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string CustomerName { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string BSSID { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string DealerNetwork { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string NetworkDealerCode { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string NetworkCustomerCode { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string DealerAccountCode { get; set; }
        public Guid CustomerUID { get; set; }
        public DateTime ActionUTC { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? ReceivedUTC { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string PrimaryContactEmail { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string FirstName { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string LastName { get; set; }
    }

    public class DeleteCustomerModel
    {
        public DeleteCustomerEvent DeleteCustomerEvent;
    }

    public class DeleteCustomerEvent
    {

        public Guid CustomerUID { get; set; }

        public DateTime ActionUTC { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? ReceivedUTC { get; set; }
    }

    public class InvalidCreateCustomerEvent
    {
        public string CustomerName { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string CustomerType { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string BSSID { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string DealerNetwork { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string NetworkDealerCode { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string NetworkCustomerCode { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string DealerAccountCode { get; set; }
        public string CustomerUID { get; set; }
        public string ActionUTC { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ReceivedUTC { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string PrimaryContactEmail { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string FirstName { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string LastName { get; set; }
    }

    public class InvalidUpdateCustomerEvent
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string CustomerName { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string BSSID { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string DealerNetwork { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string NetworkDealerCode { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string NetworkCustomerCode { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string DealerAccountCode { get; set; }
        public string CustomerUID { get; set; }
        public string ActionUTC { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ReceivedUTC { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string PrimaryContactEmail { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string FirstName { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string LastName { get; set; }
    }

    public class InvalidDeleteCustomerEvent
    {

        public string CustomerUID { get; set; }

        public string ActionUTC { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ReceivedUTC { get; set; }
    }

    public enum CustomerType
    {
        Customer = 0,
        Dealer = 1,
        Operations = 2,
        Corporate = 3
    }



}

