using System.ServiceModel;
using System.ServiceModel.Web;
using System.Xml.Linq;
using Microsoft.ServiceModel.Web;
using VSS.Hosted.VLCommon.Bss.Schema.V2;
using VSS.Hosted.VLCommon.ServiceContracts;
using System.Xml;

namespace VSS.Nighthawk.NHBssSvc.BSSEndPoints
{
  /// <summary>
  /// The Business Support Systems (BSS) service endpoint interface. 
  /// </summary>
  [ServiceContract(Namespace = ContractConstants.NHBssSvc)]
  public interface INHBSSEndpointSvc
  {
    #region V2 Endpoints

    /// <summary>
    /// This endpoint is the target for customer account hierarchies. Customer records are created in NH_OP
    /// from this data. Also, the relationships between Customers (NH_OP.Account) are formulated from this data.
    /// </summary>
    [OperationContract]
    [WebHelp(Comment = "Provides an endpoint to deliver the BSS Account Hierarchy to Nighthawk")]
    [WebInvoke(Method = "POST", UriTemplate = "v2/AccountHierarchy",
      ResponseFormat = WebMessageFormat.Xml,
      BodyStyle = WebMessageBodyStyle.Bare,
      RequestFormat = WebMessageFormat.Xml)]
    void AccountHierarchiesV2(AccountHierarchy ah);

    [OperationContract]
    [WebHelp(Comment = "Provides an endpoint to deliver the BSS InstallBase Version 2 to Nighthawk")]
    [WebInvoke(Method = "POST", UriTemplate = "v2/InstallBase",
      ResponseFormat = WebMessageFormat.Xml,
      BodyStyle = WebMessageBodyStyle.Bare,
      RequestFormat = WebMessageFormat.Xml)]
    void InstallBasesV2(InstallBase ib);

    [OperationContract]
    [WebHelp(Comment = "Provides an endpoint to deliver the BSS Service Plans to Nighthawk")]
    [WebInvoke(Method = "POST", UriTemplate = "v2/ServicePlan",
      ResponseFormat = WebMessageFormat.Xml,
      BodyStyle = WebMessageBodyStyle.Bare,
      RequestFormat = WebMessageFormat.Xml)]
    void ServicePlansV2(ServicePlan servicePlan);


    /// <summary>
    /// BSS supports the replacement of a device, that they have on record, from one equipment to another. This endpoint provides
    /// BSS with a target to communicate these transfers. Nighthawk is able to use information from INHBSSEndpointSvc. InstallBases/> 
    /// to determine device transfers, so has little use for this data.
    /// </summary>
    [OperationContract]
    [WebHelp(Comment = "Provides an endpoint to deliver the BSS Device Transfers to Nighthawk")]
    [WebInvoke(Method = "POST", UriTemplate = "v2/DeviceReplacement",
      ResponseFormat = WebMessageFormat.Xml,
      BodyStyle = WebMessageBodyStyle.Bare,
      RequestFormat = WebMessageFormat.Xml)]
    void DeviceReplacementV2(DeviceReplacement deviceTransfer);

    /// <summary>
    /// BSS supports registration/deregisration of a device, which are registered earlier as per the InstallBase record. This endpoint provides
    /// BSS with a target to communicate these registration/deregistration action. Nighthawk is able to use information from INHBSSEndpointSvc.InstallBases/> 
    /// to determine device registration/deregistartion, so has little use for this data.
    /// </summary>
    [OperationContract]
    [WebHelp(Comment = "Provides an endpoint to deliver the BSS Device Registration/De-Registration to Nighthawk")]
    [WebInvoke(Method = "POST", UriTemplate = "v2/DeviceRegistration",
      ResponseFormat = WebMessageFormat.Xml,
      BodyStyle = WebMessageBodyStyle.Bare,
      RequestFormat = WebMessageFormat.Xml)]
    void DeviceRegistrationV2(DeviceRegistration deviceRegistration);

    /// <summary>
    /// This is an endpoint used to make sure that the service is up and running
    /// </summary>
    [OperationContract]
    [WebHelp(Comment = "Provides an endpoint to Verify that the service is running and accepting connections")]
    [WebInvoke(Method = "POST", UriTemplate = "v2/ServiceStatus",
      ResponseFormat = WebMessageFormat.Xml,
      BodyStyle = WebMessageBodyStyle.Bare,
      RequestFormat = WebMessageFormat.Xml)]
    void WebServiceAvailability(XElement serviceAvailability);

    /// <summary>
    /// This is an endpoint used to make sure that the service is up and running
    /// </summary>
    [OperationContract]
    [WebHelp(Comment = "Provides an endpoint to deliver the AssetID change information to BSS subsystems")]
    [WebInvoke(Method = "GET", UriTemplate = "v2/AssetIDChanges/{bookMarkUTC}",
      ResponseFormat = WebMessageFormat.Xml,
      BodyStyle = WebMessageBodyStyle.Bare,
      RequestFormat = WebMessageFormat.Xml)]
    AssetIDChanges AssetIDChanges(string bookMarkUTC);

    #endregion
  }
}
