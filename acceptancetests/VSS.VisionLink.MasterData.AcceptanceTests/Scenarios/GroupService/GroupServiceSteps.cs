using System;
using TechTalk.SpecFlow;
using AutomationCore.Shared.Library;
using VSS.VisionLink.MasterData.AcceptanceTests.Utils.Features.Classes;
using System.Collections.Generic;
using VSS.VisionLink.MasterData.AcceptanceTests.Helpers;
using VSS.KafkaWrapper;
using System.Threading;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Threading.Tasks;


namespace VSS.VisionLink.MasterData.AcceptanceTests.Scenarios.GroupService
{
  [Binding]
  public class GroupServiceSteps
  {

    public string TestName;
    private static Log4Net Log = new Log4Net(typeof(GroupServiceSteps));
    private static GroupServiceSupport GroupServiceSupport = new GroupServiceSupport(Log);
    private ConsumerWrapper _consumerWrapper;


    [Given(@"GroupService Is Ready To Verify '(.*)'")]

    public void GivenGroupServiceIsReadyToVerify(string TestDescription)
    {
      //log the scenario info
      TestName = (ScenarioContext.Current.ScenarioInfo).Title + "_" + TestDescription;
      //TestName = TestDescription;
      LogResult.Report(Log, "log_ForInfo", "Execution started for Test Scenario" + TestName);
    }

    [Given(@"GroupServiceCreate Request Is Setup With Default Values")]
    public void GivenGroupServiceCreateRequestIsSetupWithDefaultValues()
    {
      GroupServiceSupport.CreateGroupModel = GetDefaultValidGroupServiceCreateRequest();
    }

    [When(@"I Post Valid GroupServiceCreate Request")]
    public void WhenIPostValidGroupServiceCreateRequest()
    {
      GroupServiceSupport.SetupCreateGroupKafkaConsumer(GroupServiceSupport.CreateGroupModel.GroupUID, GroupServiceSupport.CreateGroupModel.UserUID, GroupServiceSupport.CreateGroupModel.ActionUTC);

      GroupServiceSupport.PostValidCreateRequestToService();
    }

    [Then(@"The Processed GroupServiceCreate Message must be available in Kafka topic")]
    public void ThenTheProcessedGroupServiceCreateMessageMustBeAvailableInKafkaTopic()
    {
      Task.Factory.StartNew(() => GroupServiceSupport._consumerWrapper.Consume());
      Thread.Sleep(new TimeSpan(0, 0, 20));
      CreateGroupServiceModel kafkaresponse = GroupServiceSupport._checkForGroupCreateHandler.groupEvent;
      Assert.IsTrue(GroupServiceSupport._checkForGroupCreateHandler.HasFound()); //Asserts that the CreateAssetEvent has published into the AssetKafkaTopic by validating the presence of the particular AssetUid and actionutc
      GroupServiceSupport.VerifyGroupServiceCreateResponse(kafkaresponse);
    }
    [Given(@"GroupServiceUpdate Request Is Setup With Default Values")]
    public void GivenGroupServiceUpdateRequestIsSetupWithDefaultValues()
    {
      GroupServiceSupport.UpdateGroupModel = GetDefaultValidGroupServiceUpdateRequest();
    }

    [When(@"I Post Valid GroupServiceUpdate Request")]
    public void WhenIPostValidGroupServiceUpdateRequest()
    {
      GroupServiceSupport.SetupUpdateGroupKafkaConsumer(GroupServiceSupport.UpdateGroupModel.GroupUID, GroupServiceSupport.UpdateGroupModel.UserUID, GroupServiceSupport.UpdateGroupModel.ActionUTC);

      GroupServiceSupport.PostValidUpdateRequestToService();
    }

    [Then(@"The Processed GroupServiceUpdate Message must be available in Kafka topic")]
    public void ThenTheProcessedGroupServiceUpdateMessageMustBeAvailableInKafkaTopic()
    {
      Task.Factory.StartNew(() => GroupServiceSupport._consumerWrapper.Consume());
      Thread.Sleep(new TimeSpan(0, 0, 20));
      UpdateGroupServiceModel kafkaresponse = GroupServiceSupport._checkForGroupUpdateHandler.groupEvent;
      Assert.IsTrue(GroupServiceSupport._checkForGroupUpdateHandler.HasFound()); //Asserts that the UpdateGroupEvent has published into the AssetKafkaTopic by validating the presence of the particular AssetUid and actionutc
      GroupServiceSupport.VerifyGroupServiceUpdateResponse(kafkaresponse);
    }

    [Given(@"GroupServiceDelete Request Is Setup With Default Values")]
    public void GivenGroupServiceDeleteRequestIsSetupWithDefaultValues()
    {
      GroupServiceSupport.DeleteGroupModel = GetDefaultValidGroupServiceDeleteRequest();
    }

    [When(@"I Post Valid GroupServiceDelete Request")]
    public void WhenIPostValidGroupServiceDeleteRequest()
    {
      GroupServiceSupport.SetupDeleteGroupKafkaConsumer(GroupServiceSupport.DeleteGroupModel.GroupUID,
          GroupServiceSupport.DeleteGroupModel.UserUID, GroupServiceSupport.DeleteGroupModel.ActionUTC);

      GroupServiceSupport.PostValidDeleteRequestToService(GroupServiceSupport.DeleteGroupModel.GroupUID,
          GroupServiceSupport.DeleteGroupModel.UserUID, GroupServiceSupport.DeleteGroupModel.ActionUTC);
    }

    [Then(@"The Processed GroupServiceDelete Message must be available in Kafka topic")]
    public void ThenTheProcessedGroupServiceDeleteMessageMustBeAvailableInKafkaTopic()
    {
      Task.Factory.StartNew(() => GroupServiceSupport._consumerWrapper.Consume());
      Thread.Sleep(new TimeSpan(0, 0, 20));
      DeleteGroupServiceModel kafkaresponse = GroupServiceSupport._checkForGroupDeleteHandler.groupEvent;
      Assert.IsTrue(GroupServiceSupport._checkForGroupDeleteHandler.HasFound()); //Asserts that the UpdateGroupEvent has published into the AssetKafkaTopic by validating the presence of the particular AssetUid and actionutc
      GroupServiceSupport.VerifyGroupServiceDeleteResponse(kafkaresponse);
    }

    [Given(@"GroupServiceCreate Request  Is Setup With Invalid Default Values")]
    public void GivenGroupServiceCreateRequestIsSetupWithInvalidDefaultValues()
    {
      GroupServiceSupport.InValidCreateGroupModel = GetDefaultInValidGroupServiceCreateRequest();
    }

    [When(@"I Set Invalid GroupServiceCreate GroupName To '(.*)'")]
    public void WhenISetInvalidGroupServiceCreateGroupNameTo(string groupName)
    {
      GroupServiceSupport.InValidCreateGroupModel.GroupName = InputGenerator.GetValue(groupName);
    }

    [When(@"I Post Invalid GroupServiceCreate Request")]
    public void WhenIPostInvalidGroupServiceCreateRequest()
    {
      string contentType = "application/json";
      GroupServiceSupport.PostInValidCreateRequestToService(contentType, HttpStatusCode.BadRequest);
    }

    [Then(@"GroupServiceCreate Response With '(.*)' Should Be Returned")]
    public void ThenGroupServiceCreateResponseWithShouldBeReturned(string errorMessage)
    {
      GroupServiceSupport.VerifyErrorResponse(errorMessage);
    }

    [When(@"I Set Invalid GroupServiceCreate CustomerUID To '(.*)'")]
    public void WhenISetInvalidGroupServiceCreateCustomerUIDTo(string customerUid)
    {
      GroupServiceSupport.InValidCreateGroupModel.CustomerUID = InputGenerator.GetValue(customerUid);
    }

    [When(@"I Set Invalid GroupServiceCreate UserUID To '(.*)'")]
    public void WhenISetInvalidGroupServiceCreateUserUIDTo(string userUid)
    {
      GroupServiceSupport.InValidCreateGroupModel.UserUID = InputGenerator.GetValue(userUid);
    }

    [When(@"I Set Invalid GroupServiceCreate AssetUID To '(.*)'")]
    public void WhenISetInvalidGroupServiceCreateAssetUIDTo(string assetUid)
    {
      GroupServiceSupport.InValidCreateGroupModel.AssetUID = InputGenerator.GetValue(assetUid);
    }

    [When(@"I Set Invalid GroupServiceCreate GroupUID To '(.*)'")]
    public void WhenISetInvalidGroupServiceCreateGroupUIDTo(string groupUid)
    {
      GroupServiceSupport.InValidCreateGroupModel.GroupUID = InputGenerator.GetValue(groupUid);
    }

    [When(@"I Set Invalid GroupServiceCreate ActionUTC To '(.*)'")]
    public void WhenISetInvalidGroupServiceCreateActionUTCTo(string actionUtc)
    {
      GroupServiceSupport.InValidCreateGroupModel.ActionUTC = InputGenerator.GetValue(actionUtc.ToString());
    }

    [Given(@"GroupServiceUpdate Request Is Setup With Invalid Default Values")]
    public void GivenGroupServiceUpdateRequestIsSetupWithInvalidDefaultValues()
    {
      GroupServiceSupport.InValidUpdateGroupModel = GetDefaultInValidGroupServiceUpdateRequest();
    }

    [When(@"I Set Invalid GroupServiceUpdate UserUID To '(.*)'")]
    public void WhenISetInvalidGroupServiceUpdateUserUIDTo(string userUid)
    {
      GroupServiceSupport.InValidUpdateGroupModel.UserUID = InputGenerator.GetValue(userUid);
    }

    [When(@"I Post Invalid GroupServiceUpdate Request")]
    public void WhenIPostInvalidGroupServiceUpdateRequest()
    {
      string contentType = "application/json";
      GroupServiceSupport.PostInValidUpdateRequestToService(contentType, HttpStatusCode.BadRequest);
    }

    [Then(@"GroupServiceUpdate Response With '(.*)' Should Be Returned")]
    public void ThenGroupServiceUpdateResponseWithShouldBeReturned(string errorMessage)
    {
      GroupServiceSupport.VerifyErrorResponse(errorMessage);
    }

    [When(@"I Set GroupServiceUpdate AssociatedAssetUID To '(.*)'")]
    public void WhenISetGroupServiceUpdateAssociatedAssetUIDTo(string assetUid)
    {
      List<Guid> assetUids = new List<Guid>();
      if (InputGenerator.GetValue(assetUid)!=null)
      assetUids.Add(Guid.Parse(InputGenerator.GetValue(assetUid)));
      GroupServiceSupport.UpdateGroupModel.AssociatedAssetUID = String.IsNullOrEmpty(InputGenerator.GetValue(assetUid)) ? null : assetUids;
    }

    [When(@"I Set GroupServiceUpdate DissociatedAssetUID To '(.*)'")]
    public void WhenISetGroupServiceUpdateDissociatedAssetUIDTo(string assetUid)
    {
      List<Guid> assetUids = new List<Guid>();
      if (InputGenerator.GetValue(assetUid) != null)
      assetUids.Add(Guid.Parse(InputGenerator.GetValue(assetUid)));
      GroupServiceSupport.UpdateGroupModel.DissociatedAssetUID = String.IsNullOrEmpty(InputGenerator.GetValue(assetUid)) ? null : assetUids;
    }

    [When(@"I Set Invalid GroupServiceUpdate GroupUID To '(.*)'")]
    public void WhenISetInvalidGroupServiceUpdateGroupUIDTo(string groupUid)
    {
      GroupServiceSupport.InValidUpdateGroupModel.GroupUID = InputGenerator.GetValue(groupUid);
    }


    [When(@"I Set Invalid GroupServiceUpdate ActionUTC To '(.*)'")]
    public void WhenISetInvalidGroupServiceUpdateActionUTCTo(string actionUtc)
    {
      GroupServiceSupport.InValidUpdateGroupModel.ActionUTC = InputGenerator.GetValue(actionUtc.ToString());
    }

    [Given(@"GroupServiceDelete Request Is Setup With Invalid Default Values")]
    public void GivenGroupServiceDeleteRequestIsSetupWithInvalidDefaultValues()
    {
      GroupServiceSupport.InValidDeleteGroupModel = GetDefaultInValidGroupServiceDeleteRequest();
    }

    [When(@"I Set Invalid GroupServiceDelete UserUID To '(.*)'")]
    public void WhenISetInvalidGroupServiceDeleteUserUIDTo(string userUid)
    {
      GroupServiceSupport.InValidDeleteGroupModel.UserUID = InputGenerator.GetValue(userUid);
    }

    [When(@"I Post Invalid GroupServiceDelete Request")]
    public void WhenIPostInvalidGroupServiceDeleteRequest()
    {
      string contentType = "application/json";
      // GroupServiceSupport.PostInValidDeleteRequestToService(contentType, HttpStatusCode.BadRequest);
      //assetServiceSupport.PostInValidDeleteRequestToService(assetServiceSupport.InValidDeleteAssetModel.AssetUID, assetServiceSupport.InValidDeleteAssetModel.ActionUTC, contentType, HttpStatusCode.BadRequest);
      GroupServiceSupport.PostInValidDeleteRequestToService(GroupServiceSupport.InValidDeleteGroupModel.GroupUID, GroupServiceSupport.InValidDeleteGroupModel.UserUID, GroupServiceSupport.InValidDeleteGroupModel.ActionUTC, contentType, HttpStatusCode.BadRequest);
    }

    [Then(@"GroupServiceDelete Response With '(.*)' Should Be Returned")]
    public void ThenGroupServiceDeleteResponseWithShouldBeReturned(string errorMessage)
    {
      GroupServiceSupport.VerifyErrorResponse(errorMessage);
    }


    [When(@"I Set Invalid GroupServiceDelete GroupUID To '(.*)'")]
    public void WhenISetInvalidGroupServiceDeleteGroupUIDTo(string groupUid)
    {
      GroupServiceSupport.InValidDeleteGroupModel.GroupUID = InputGenerator.GetValue(groupUid);
    }

    [When(@"I Set Invalid GroupServiceDelete ActionUTC To '(.*)'")]
    public void WhenISetInvalidGroupServiceDeleteActionUTCTo(string actionUtc)
    {
      //ScenarioContext.Current.Pending();
      GroupServiceSupport.InValidDeleteGroupModel.ActionUTC = InputGenerator.GetValue(actionUtc.ToString());
    }

    [When(@"I Set GroupServiceUpdate GroupName To '(.*)'")]
    public void WhenISetGroupServiceUpdateGroupNameTo(string groupName)
    {
      GroupServiceSupport.UpdateGroupModel.GroupName = InputGenerator.GetValue(groupName);
    }

    [When(@"I Set Invalid GroupServiceUpdate AssociatedAssetUID To '(.*)'")]
    public void WhenISetInvalidGroupServiceUpdateAssociatedAssetUIDTo(string associatedAssetuid)
    {
      GroupServiceSupport.InValidUpdateGroupModel.AssociatedAssetUID = InputGenerator.GetValue(associatedAssetuid);
    }

    [When(@"I Set Invalid GroupServiceUpdate DissociatedAssetUID To '(.*)'")]
    public void WhenISetInvalidGroupServiceUpdateDissociatedAssetUIDTo(string dissociatedAssetuid)
    {
      GroupServiceSupport.InValidUpdateGroupModel.DissociatedAssetUID = InputGenerator.GetValue(dissociatedAssetuid);
    }

    [When(@"I Set Invalid GroupServiceUpdate GroupName To '(.*)'")]
    public void WhenISetInvalidGroupServiceUpdateGroupNameTo(string groupName)
    {
      GroupServiceSupport.InValidUpdateGroupModel.GroupName = InputGenerator.GetValue(groupName);
    }

    public static CreateGroupServiceEvent GetDefaultValidGroupServiceCreateRequest()
    {
      List<Guid> ItemsIndex = new List<Guid>();
      ItemsIndex.Add(Guid.NewGuid());
      ItemsIndex.Add(Guid.NewGuid());


      CreateGroupServiceEvent defaultValidGroupServiceCreateModel = new CreateGroupServiceEvent();
      defaultValidGroupServiceCreateModel.CustomerUID = Guid.NewGuid();
      defaultValidGroupServiceCreateModel.UserUID = Guid.NewGuid();
      defaultValidGroupServiceCreateModel.AssetUID = ItemsIndex;
      defaultValidGroupServiceCreateModel.GroupUID = Guid.NewGuid();
      defaultValidGroupServiceCreateModel.GroupName = "AutoTestAPICreateGroupName" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultValidGroupServiceCreateModel.ActionUTC = DateTime.UtcNow;
      return defaultValidGroupServiceCreateModel;
    }

    public static InValidCreateGroupServiceEvent GetDefaultInValidGroupServiceCreateRequest()
    {
      //= ItemsIndex.Select(g => g.ToString()).ToList();



      /*    foreach (var item in ItemsIndex)
          {
              stringList.Add(item.ToString());
          }*/
      //  List<string> stringList = ItemsIndex.Select(s => String.Parse(s)).ToList<string>();


      InValidCreateGroupServiceEvent defaultInValidGroupServiceCreateModel = new InValidCreateGroupServiceEvent();
      defaultInValidGroupServiceCreateModel.CustomerUID = Guid.NewGuid().ToString();
      defaultInValidGroupServiceCreateModel.UserUID = Guid.NewGuid().ToString();
      defaultInValidGroupServiceCreateModel.AssetUID = Guid.NewGuid().ToString();
      defaultInValidGroupServiceCreateModel.GroupUID = Guid.NewGuid().ToString();
      defaultInValidGroupServiceCreateModel.GroupName = "AutoTestAPICreateGroupName" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultInValidGroupServiceCreateModel.ActionUTC = DateTime.UtcNow.ToString();
      return defaultInValidGroupServiceCreateModel;
    }

    public static UpdateGroupServiceEvent GetDefaultValidGroupServiceUpdateRequest()
    {

      List<Guid> ItemsIndex = new List<Guid>();
      ItemsIndex.Add(Guid.NewGuid());
      ItemsIndex.Add(Guid.NewGuid());

      List<Guid> ItemsIndex1 = new List<Guid>();
      ItemsIndex1.Add(Guid.NewGuid());
      ItemsIndex1.Add(Guid.NewGuid());

      //Guid GroupsGuid, GroupsGuid2;
      //List<Guid> ItemsIndex = new List<Guid>();
      //GroupsGuid = Guid.NewGuid();
      // GroupsGuid2 = Guid.NewGuid();
      // ItemsIndex.Add(GroupsGuid);

      UpdateGroupServiceEvent defaultValidGroupServiceUpdateModel = new UpdateGroupServiceEvent();
      defaultValidGroupServiceUpdateModel.GroupName = "AutoTestAPICreateGroupName" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultValidGroupServiceUpdateModel.UserUID = Guid.NewGuid();
      defaultValidGroupServiceUpdateModel.AssociatedAssetUID = ItemsIndex;
      defaultValidGroupServiceUpdateModel.DissociatedAssetUID = ItemsIndex1;
      defaultValidGroupServiceUpdateModel.GroupUID = Guid.NewGuid();
      defaultValidGroupServiceUpdateModel.ActionUTC = DateTime.UtcNow;
      return defaultValidGroupServiceUpdateModel;
    }

    public static InValidUpdateGroupServiceEvent GetDefaultInValidGroupServiceUpdateRequest()
    {

      //List<Guid> ItemsIndex = new List<Guid>();
      //ItemsIndex.Add(Guid.NewGuid());
      //ItemsIndex.Add(Guid.NewGuid());

      //List<Guid> ItemsIndex1 = new List<Guid>();
      //ItemsIndex1.Add(Guid.NewGuid());
      //ItemsIndex1.Add(Guid.NewGuid());

      //Guid GroupsGuid, GroupsGuid2;
      //List<Guid> ItemsIndex = new List<Guid>();
      //GroupsGuid = Guid.NewGuid();
      // GroupsGuid2 = Guid.NewGuid();
      // ItemsIndex.Add(GroupsGuid);

      InValidUpdateGroupServiceEvent defaultInValidGroupServiceUpdateModel = new InValidUpdateGroupServiceEvent();
      defaultInValidGroupServiceUpdateModel.GroupName = "AutoTestAPICreateGroupName" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultInValidGroupServiceUpdateModel.UserUID = Guid.NewGuid().ToString();
      defaultInValidGroupServiceUpdateModel.AssociatedAssetUID = Guid.NewGuid().ToString();
      defaultInValidGroupServiceUpdateModel.DissociatedAssetUID = Guid.NewGuid().ToString();
      defaultInValidGroupServiceUpdateModel.GroupUID = Guid.NewGuid().ToString();
      defaultInValidGroupServiceUpdateModel.ActionUTC = DateTime.UtcNow.ToString();
      return defaultInValidGroupServiceUpdateModel;
    }

    public static DeleteGroupServiceEvent GetDefaultValidGroupServiceDeleteRequest()
    {
      Guid GroupsGuid;
      List<Guid> ItemsIndex = new List<Guid>();
      GroupsGuid = Guid.NewGuid();
      // GroupsGuid2 = Guid.NewGuid();
      ItemsIndex.Add(GroupsGuid);
      DeleteGroupServiceEvent defaultValidGroupServiceDeleteModel = new DeleteGroupServiceEvent();
      defaultValidGroupServiceDeleteModel.UserUID = Guid.NewGuid();
      defaultValidGroupServiceDeleteModel.GroupUID = Guid.NewGuid();
      defaultValidGroupServiceDeleteModel.ActionUTC = DateTime.UtcNow;
      return defaultValidGroupServiceDeleteModel;
    }

    public static InValidDeleteGroupServiceEvent GetDefaultInValidGroupServiceDeleteRequest()
    {

      //List<Guid> ItemsIndex = new List<Guid>();
      //ItemsIndex.Add(Guid.NewGuid());
      //ItemsIndex.Add(Guid.NewGuid());

      //List<Guid> ItemsIndex1 = new List<Guid>();
      //ItemsIndex1.Add(Guid.NewGuid());
      //ItemsIndex1.Add(Guid.NewGuid());

      //Guid GroupsGuid, GroupsGuid2;
      //List<Guid> ItemsIndex = new List<Guid>();
      //GroupsGuid = Guid.NewGuid();
      // GroupsGuid2 = Guid.NewGuid();
      // ItemsIndex.Add(GroupsGuid);

      InValidDeleteGroupServiceEvent defaultInValidGroupServiceDeleteModel = new InValidDeleteGroupServiceEvent();
      defaultInValidGroupServiceDeleteModel.UserUID = Guid.NewGuid().ToString();
      defaultInValidGroupServiceDeleteModel.GroupUID = Guid.NewGuid().ToString();
      defaultInValidGroupServiceDeleteModel.ActionUTC = DateTime.UtcNow.ToString();
      return defaultInValidGroupServiceDeleteModel;
    }
  }
}


