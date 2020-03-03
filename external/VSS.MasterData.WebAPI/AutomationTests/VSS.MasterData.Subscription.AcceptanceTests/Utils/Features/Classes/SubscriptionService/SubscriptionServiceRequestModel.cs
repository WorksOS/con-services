using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.MasterData.Subscription.AcceptanceTests.Utils.Features.Classes.SubscriptionService
{
    #region Valid CreateAssetSubscriptionRequest
    public class CreateAssetSubscriptionModel
    {
        public CreateAssetSubscriptionEvent CreateAssetSubscriptionEvent;
    }

    public class CreateAssetSubscriptionEvent
    {
        public Guid SubscriptionUID { get; set; }
        public Guid CustomerUID { get; set; }
        public Guid AssetUID { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Guid? DeviceUID { get; set; }
        public string SubscriptionType { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Source { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime ActionUTC { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? ReceivedUTC { get; set; }
    }
    #endregion

    #region Valid UpdateAssetSubscriptionRequest
    public class UpdateAssetSubscriptionModel
    {
        public UpdateAssetSubscriptionEvent UpdateAssetSubscriptionEvent;
    }

    public class UpdateAssetSubscriptionEvent
    {
        public Guid SubscriptionUID { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Guid? CustomerUID { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Guid? AssetUID { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Guid? DeviceUID { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string SubscriptionType { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Source { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? StartDate { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? EndDate { get; set; }
        public DateTime ActionUTC { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? ReceivedUTC { get; set; }
    }
    #endregion

    #region Invalid CreateAssetSubscriptionRequest
    public class InvalidCreateAssetSubscriptionEvent
    {
        public string SubscriptionUID { get; set; }
        public string CustomerUID { get; set; }
        public string AssetUID { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string DeviceUID { get; set; }
        public string SubscriptionType { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string ActionUTC { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ReceivedUTC { get; set; }
    }
    #endregion

    #region Invalid UpdateAssetSubscriptionRequest
    public class InvalidUpdateAssetSubscriptionEvent
    {
        public string SubscriptionUID { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string CustomerUID { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string AssetUID { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string DeviceUID { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string SubscriptionType { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string StartDate { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string EndDate { get; set; }
        public string ActionUTC { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ReceivedUTC { get; set; }
    }
    #endregion

    #region Valid CreateCustomerSubscriptionRequest
    public class CreateCustomerSubscriptionModel
    {
        public CreateCustomerSubscriptionEvent CreateCustomerSubscriptionEvent;
    }

    public class CreateCustomerSubscriptionEvent
    {
        public Guid SubscriptionUID { get; set; }


        public Guid CustomerUID { get; set; }


        public string SubscriptionType { get; set; }


        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }


        public DateTime ActionUTC { get; set; }

        public DateTime ReceivedUTC { get; set; }
    }
    #endregion

    #region Valid UpdateCustomerSubscriptionRequest
    public class UpdateCustomerSubscriptionModel
    {
        public UpdateCustomerSubscriptionEvent UpdateCustomerSubscriptionEvent;
    }
    public class UpdateCustomerSubscriptionEvent
    {

        public Guid SubscriptionUID { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? StartDate { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? EndDate { get; set; }


        public DateTime ActionUTC { get; set; }

        public DateTime ReceivedUTC { get; set; }
    }
    #endregion

    #region Valid CreateProjectSubscriptionRequest
    public class CreateProjectSubscriptionModel
    {
        public CreateProjectSubscriptionEvent CreateProjectSubscriptionEvent;
    }

    public class CreateProjectSubscriptionEvent
    {
        public Guid SubscriptionUID { get; set; }

        public Guid CustomerUID { get; set; }

        public string SubscriptionType { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public DateTime ActionUTC { get; set; }

        public DateTime ReceivedUTC { get; set; }
    }
    #endregion

    #region Valid UpdateProjectSubscriptionRequest
    public class UpdateProjectSubscriptionModel
    {
        public UpdateProjectSubscriptionEvent UpdateProjectSubscriptionEvent;
    }

    public class UpdateProjectSubscriptionEvent
    {
        public Guid SubscriptionUID { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Guid? CustomerUID { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string SubscriptionType { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? StartDate { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? EndDate { get; set; }

        public DateTime ActionUTC { get; set; }

        public DateTime ReceivedUTC { get; set; }
    }
    #endregion

    #region Valid AssociateProjectSubscriptionRequest
    public class AssociateProjectSubscriptionModel
    {
        public AssociateProjectSubscriptionEvent AssociateProjectSubscriptionEvent;
    }

    public class AssociateProjectSubscriptionEvent
    {
        public Guid SubscriptionUID { get; set; }

        public Guid ProjectUID { get; set; }

        public DateTime EffectiveDate { get; set; }

        public DateTime ActionUTC { get; set; }

        public DateTime ReceivedUTC { get; set; }
    }
    #endregion

    #region Valid DissociateProjectSubscriptionRequest
    public class DissociateProjectSubscriptionModel
    {
        public DissociateProjectSubscriptionEvent DissociateProjectSubscriptionEvent;
    }
    public class DissociateProjectSubscriptionEvent
    {
        public Guid SubscriptionUID { get; set; }

        public Guid ProjectUID { get; set; }

        public DateTime EffectiveDate { get; set; }

        public DateTime ActionUTC { get; set; }

        public DateTime ReceivedUTC { get; set; }
    }
    #endregion

    #region Invalid CreateCustomerSubscriptionRequest
    public class InvalidCreateCustomerSubscriptionEvent
    {

        public string SubscriptionUID { get; set; }

        public string CustomerUID { get; set; }

        public string SubscriptionType { get; set; }

        public string StartDate { get; set; }

        public string EndDate { get; set; }

        public string ActionUTC { get; set; }

        public string ReceivedUTC { get; set; }
    }
    #endregion

    #region Invalid UpdateCustomerSubscriptionRequest
    public class InvalidUpdateCustomerSubscriptionEvent
    {
        public string SubscriptionUID { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string StartDate { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string EndDate { get; set; }

        public string ActionUTC { get; set; }

        public string ReceivedUTC { get; set; }
    }
    #endregion

    #region  Invalid CreateProjectSubscriptionRequest
    public class InvalidCreateProjectSubscriptionEvent
    {
        public string SubscriptionUID { get; set; }

        public string CustomerUID { get; set; }

        public string SubscriptionType { get; set; }

        public string StartDate { get; set; }

        public string EndDate { get; set; }

        public string ActionUTC { get; set; }

        public string ReceivedUTC { get; set; }
    }
    #endregion

    #region  Invalid UpdateProjectSubscriptionRequest
    public class InvalidUpdateProjectSubscriptionEvent
    {
        public string SubscriptionUID { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string CustomerUID { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string SubscriptionType { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string StartDate { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string EndDate { get; set; }

        public string ActionUTC { get; set; }

        public string ReceivedUTC { get; set; }
    }
    #endregion

    #region  Invalid AssociateProjectSubscriptionRequest
    public class InvalidAssociateProjectSubscriptionEvent
    {
        public string SubscriptionUID { get; set; }

        public string ProjectUID { get; set; }

        public string EffectiveDate { get; set; }

        public string ActionUTC { get; set; }

        public string ReceivedUTC { get; set; }
    }
    #endregion

    #region Invalid DissociateProjectSubscriptionRequest
    public class InvalidDissociateProjectSubscriptionEvent
    {
        public string SubscriptionUID { get; set; }

        public string ProjectUID { get; set; }

        public string EffectiveDate { get; set; }

        public string ActionUTC { get; set; }

        public string ReceivedUTC { get; set; }
    }
    #endregion

}
