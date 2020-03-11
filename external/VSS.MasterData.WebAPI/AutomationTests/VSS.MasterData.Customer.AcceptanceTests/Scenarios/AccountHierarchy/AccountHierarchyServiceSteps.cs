using System;
using TechTalk.SpecFlow;
using AutomationCore.API.Framework.Library;
using AutomationCore.Shared.Library;
using VSS.MasterData.Customer.AcceptanceTests.Scenarios.CustomerUserService;
using VSS.MasterData.Customer.AcceptanceTests.Scenarios.CustomerService;
using AutomationCore.API.Framework.Common.Config.TPaaSServicesConfig;
using AutomationCore.API.Framework.Common.Features.TPaaS;
using VSS.MasterData.Customer.AcceptanceTests.Scenarios.CustomerRelationship;
using VSS.MasterData.Customer.AcceptanceTests.Utils.Config;
using VSS.MasterData.Customer.AcceptanceTests.Utils.Features.Classes.AccountHierarchy;
using Newtonsoft.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Collections.Generic;

namespace VSS.MasterData.Customer.AcceptanceTests.Scenarios.AccountHierarchy
{
  [Binding]
  public class AccountHierarchyServiceSteps
  {

    #region Variables

    public string TestName;
    private static Log4Net Log = new Log4Net(typeof(AccountHierarchyServiceSteps));
    private static AccountHierarchyServiceSupport accountHierarchyServiceSupport = new AccountHierarchyServiceSupport(Log);


    public static List<Guid> parentUID = new List<Guid>();
    public static List<AccountHierarchyDBComparisonClass> AccountHierarchyNodes = new List<AccountHierarchyDBComparisonClass>();
    public static List<AccountHierarchyDBComparisonClass> AccountHierarchyRemovedNodes = new List<AccountHierarchyDBComparisonClass>();
    public static List<Guid> RemoveNodeUIDList = new List<Guid>();



    private static CustomerUserServiceSupport customerUserServiceSupport = new CustomerUserServiceSupport(Log);
    private static CustomerServiceSupport customerServiceSupport = new CustomerServiceSupport(Log);
    private static CustomerRelationshipServiceSupport customerRelationshipServiceSupport = new CustomerRelationshipServiceSupport(Log);

    Dictionary<String, Guid> CustomerNameWithUID =
            new Dictionary<String, Guid>();


    public string SingleCustomerUserUID = string.Empty;
    public string SingleCustomerUserLoginId = "CustomerListSingleUser" + InputGenerator.GenerateUniqueId() + "@Visionlink.com";
    public string SingleCustomerUserPassword = "VisionLink$6";

    public Guid[] ParentCustomerUID = new Guid[6];
    public String[] ParentCustomerName = new String[6];
    public String[] ParentCustomerType = new String[6];


    public Guid[,] ChildCustomerUID = new Guid[3, 2];
    public String[,] ChildCustomerName = new String[3, 2];
    public String[,] ChildCustomerType = new String[3, 2];
    public Guid[] UserUID = new Guid[3];
    public string response = String.Empty;

    #endregion

    #region Step Definition

    #region Set CustomerRelationship
    [When(@"I Set '(.*)' As RootNode And Has '(.*)' And '(.*)' As Children")]
    public void WhenISetAsRootNodeAndHasAndAsChildren(string rootCustomer, string childCustomer1, string childCustomer2)
    {

      //rootCustomer = rootCustomer + DateTime.UtcNow.ToString();
      //childCustomer1=childCustomer1+ DateTime.UtcNow.ToString();
      //childCustomer2=childCustomer2+ DateTime.UtcNow.ToString();


      bool removeNode = false;

      if (rootCustomer.Contains("Dealer"))
      {
        CreateCustomer("Dealer", rootCustomer);
        SetRootNodeValues();
      }
      else
      {
        CreateCustomer("Customer", rootCustomer);
        SetRootNodeValues();
      }

      if (childCustomer1.Contains("Dealer"))
        CreateCustomer("Dealer", childCustomer1);
      else
        CreateCustomer("Customer", childCustomer1);


      if (childCustomer2.Contains("Dealer"))
        CreateCustomer("Dealer", childCustomer2);
      else
        CreateCustomer("Customer", childCustomer2);



      AssociateCustomer(parentUID[0], CustomerNameWithUID[childCustomer1], parentUID[0]);
      AssociateCustomer(parentUID[0], CustomerNameWithUID[childCustomer2], parentUID[0]);

      //AssignNodePosition(removeNode);
    }


    [When(@"I Set '(.*)' Has '(.*)' And '(.*)' As Children")]
    public void WhenISetHasAndAsChildren(string parentCustomer, string childCustomer1, string childCustomer2)
    {
      bool removeNode = false;

      //parentCustomer =parentCustomer+ DateTime.UtcNow.ToString();
      //childCustomer1=childCustomer1+ DateTime.UtcNow.ToString();
      //childCustomer2=childCustomer2+ DateTime.UtcNow.ToString();

     

      if (!CustomerNameWithUID.ContainsKey(parentCustomer))
      {
        if (parentCustomer.Contains("Dealer"))
        {
          CreateCustomer("Dealer", parentCustomer);
          //SetRootNodeValues();
        }
        else
        {
          CreateCustomer("Customer", parentCustomer);
          //SetRootNodeValues();
        }
      }


      if (!CustomerNameWithUID.ContainsKey(childCustomer1))
      {
        if (childCustomer1.Contains("Dealer"))
          CreateCustomer("Dealer", childCustomer1);
        else
          CreateCustomer("Customer", childCustomer1);
      }

      if (!CustomerNameWithUID.ContainsKey(childCustomer2))
      {
        if (childCustomer2.Contains("Dealer"))
          CreateCustomer("Dealer", childCustomer2);
        else
          CreateCustomer("Customer", childCustomer2);
      }



      AssociateCustomer(CustomerNameWithUID[parentCustomer], CustomerNameWithUID[childCustomer1], parentUID[0]);
      AssociateCustomer(CustomerNameWithUID[parentCustomer], CustomerNameWithUID[childCustomer2], parentUID[0]);

     // AssignNodePosition(removeNode);
    }




    [When(@"I Set '(.*)'  Has '(.*)' As Children")]
    public void WhenISetHasAsChildren(string parentCustomer, string childCustomer)
    {
      bool removeNode = false;

      //parentCustomer = parentCustomer + DateTime.UtcNow.ToString();
      //childCustomer = childCustomer + DateTime.UtcNow.ToString();

      if (!CustomerNameWithUID.ContainsKey(parentCustomer))
      {
        if (parentCustomer.Contains("Dealer"))
          CreateCustomer("Dealer", parentCustomer);
        else
          CreateCustomer("Customer", parentCustomer);
      }

      if (!CustomerNameWithUID.ContainsKey(childCustomer))
      {
        if (childCustomer.Contains("Dealer"))
          CreateCustomer("Dealer", childCustomer);
        else
          CreateCustomer("Customer", childCustomer);
      }
      AssociateCustomer(CustomerNameWithUID[parentCustomer], CustomerNameWithUID[childCustomer]);

    //  AssignNodePosition(removeNode);

    }


    //[When(@"I Set '(.*)' With Parent '(.*)' LeftNode As '(.*)' And RightNode As '(.*)'")]
    [When(@"I Set '(.*)' With Parent '(.*)' LeftNode As '(.*)' And RightNode As '(.*)'")]
    public void WhenISetWithParentLeftNodeAsAndRightNodeAs(string childNode, string parentNode, int childLeftPosition, int childRightPosition)
    {
      Guid nodeUID = CustomerNameWithUID[childNode];
      Guid parentNodeUID= CustomerNameWithUID[parentNode];
      foreach (var node in AccountHierarchyNodes)
        if (node.NodeUID == nodeUID && node.ParentNodeUID == parentNodeUID)
        {
          node.LeftPosition = childLeftPosition;
          node.RightPosition = childRightPosition;
          break;
        }

    }

   


    public AccountHierarchyDBComparisonClass GetNodeByNodeUIDAndParentUID(Guid nodeUID, Guid parentUID)
    {
      AccountHierarchyDBComparisonClass Node = new AccountHierarchyDBComparisonClass();
      foreach (var node in AccountHierarchyNodes)
        if (node.NodeUID == nodeUID && node.ParentNodeUID == parentUID)
        {
          Node = node;
          return Node;
        }
      return Node;
    }

    #endregion

    #region Validate DB
    [Then(@"The AccountHierarchyRelationship Should Stored In VSSDB")]
    public void ThenTheAccountHierarchyRelationshipShouldStoredInVSSDB()
    {
      //int expectedResult = 1;
      int totalNodes = AccountHierarchyNodes.Count();
      //Assert.IsTrue(AccountHierarchyServiceSupport.ValidateDB(totalNodes), "DB Validation Failure");
      foreach (AccountHierarchyDBComparisonClass customernode in AccountHierarchyNodes)
        Assert.IsTrue(AccountHierarchyServiceSupport.ValidateDB(customernode), "DB Validation Failure");
    }

    #endregion

    #region Remove Customer




    [When(@"I Remove '(.*)' From '(.*)' As Child")]
    public void WhenIRemoveFromAsChild(string childNode, string parentNode)
    {
      Guid childNodeUID = CustomerNameWithUID[childNode];
      Guid parentNodeUID = CustomerNameWithUID[parentNode];
      DissociateCustomerToDealer(parentNodeUID, childNodeUID);
      RemoveNode(childNodeUID);
    }

    public void RemoveNode(Guid Node)
    {
      bool removeNode = false;
      RemoveNodeList(Node);
      //RemoveNodeUIDList = RemoveNodeUIDList.Distinct().ToList();
      //foreach (var node in AccountHierarchyNodes)
      //{
      //  if (RemoveNodeUIDList.Contains(node.NodeUID))
      //  {
      //    //AccountHierarchyNodes.Remove(node);
      //    node.LeftPosition = 0;
      //    node.RightPosition = 0;
      //    node.RootNodeUID = Node;
      //    if (node.NodeUID == Node)
      //      node.ParentNodeUID = Node;

          

      //    //AccountHierarchyRemovedNodes.Add(node);
      //  }

      //}


      //AccountHierarchyRemovedNodes[0].RootNodeUID = AccountHierarchyRemovedNodes[0].NodeUID;
      //AccountHierarchyRemovedNodes[0].ParentNodeUID = AccountHierarchyRemovedNodes[0].NodeUID;

      //if (AccountHierarchyRemovedNodes.Count > 0)
        //removeNode = true;
      //AssignNodePosition(removeNode);
      //ChangeParentHierarchy();
      //removeNode = true;
      //AssignNodePosition(removeNode);
    }

    //public void ChangeParentHierarchy()
    //{
    //  int count = AccountHierarchyNodes.Count();
    //  List<AccountHierarchyDBComparisonClass> TempAccHierarchyNode = new List<AccountHierarchyDBComparisonClass>();
    //  TempAccHierarchyNode = AccountHierarchyNodes;
    //  for (int i = 0; i < count; i++)
    //  {
    //    if (RemoveNodeUIDList.Contains(AccountHierarchyNodes[i].NodeUID))
    //      TempAccHierarchyNode.Remove(GetNodeByNodeUID(RemoveNodeUIDList[i].ToString()));
    //  }
    //  AccountHierarchyNodes = TempAccHierarchyNode;
    //}


    public void RemoveNodeList(Guid nodeUID)
    {
      AccountHierarchyDBComparisonClass tempNode = new AccountHierarchyDBComparisonClass();
      for (int i= 0;i< AccountHierarchyNodes.Count();i++)
      {
        tempNode = AccountHierarchyNodes[i];
        if (AccountHierarchyNodes[i].NodeUID == nodeUID)
        {
          tempNode.ParentNodeUID = nodeUID;
          tempNode.RootNodeUID = nodeUID;
          tempNode.NodeUID = AccountHierarchyNodes[i].NodeUID;
          RemoveNodeUIDList.Add(AccountHierarchyNodes[i].NodeUID);
        }
        if (AccountHierarchyNodes[i].ParentNodeUID == nodeUID)
        {


          tempNode.RootNodeUID = nodeUID;

          RemoveNodeUIDList.Add(AccountHierarchyNodes[i].ParentNodeUID);
        }

        if ((RemoveNodeUIDList.Contains(AccountHierarchyNodes[i].ParentNodeUID)) )
        {
          RemoveNodeUIDList.Add(AccountHierarchyNodes[i].NodeUID);
          tempNode.RootNodeUID = nodeUID;
        }
        AccountHierarchyNodes[i] = tempNode;


        // 


      }

    }
    public void SetNodeValue(Guid rootnode, Guid parentnode, Guid childnode,int leftNodePosition,int rightNodePosition)
    {
      AccountHierarchyDBComparisonClass node = new AccountHierarchyDBComparisonClass();
      node.RootNodeUID = rootnode;
      node.ParentNodeUID = parentnode;
      node.NodeUID = childnode;
      node.LeftPosition = leftNodePosition;
      node.RightPosition = rightNodePosition;

    }

    #endregion

    #region Create Customer And Add/Remove Relationship 
    public void CreateCustomer(string customerType, string customerName)
    {
      if (!CustomerNameWithUID.ContainsKey(customerName))
      {

        customerServiceSupport.CreateCustomerModel = CustomerServiceSteps.GetDefaultValidCustomerServiceCreateRequest();
        customerServiceSupport.CreateCustomerModel.CustomerType = customerType;
        customerServiceSupport.CreateCustomerModel.CustomerName = customerName;
        customerServiceSupport.PostValidCreateRequestToService();

        parentUID.Add(customerServiceSupport.CreateCustomerModel.CustomerUID);
        CustomerNameWithUID.Add(customerName, customerServiceSupport.CreateCustomerModel.CustomerUID);
      }

    }

    public void AssociateCustomer(Guid parentCustomerUID, Guid childCustomerUID, Guid rootNodeUID)
    {
      customerRelationshipServiceSupport.CreateCustomerRelationshipModel = CustomerRelationshipServiceSteps.GetDefaultValidCustomerRelationshipServiceCreateRequest();


      customerRelationshipServiceSupport.CreateCustomerRelationshipModel.ParentCustomerUID = parentCustomerUID;
      customerRelationshipServiceSupport.CreateCustomerRelationshipModel.ChildCustomerUID = childCustomerUID;

      customerRelationshipServiceSupport.PostValidCreateCustomerRelationshipRequestToService();
      AccountHierarchyDBComparisonClass NodeDetails = new AccountHierarchyDBComparisonClass();
      NodeDetails.NodeUID = childCustomerUID;
      NodeDetails.ParentNodeUID = parentCustomerUID;
      NodeDetails.RootNodeUID = rootNodeUID;
      AccountHierarchyNodes.Add(NodeDetails);
    }

    public void AssociateCustomer(Guid parentCustomerUID, Guid childCustomerUID)
    {
      customerRelationshipServiceSupport.CreateCustomerRelationshipModel = CustomerRelationshipServiceSteps.GetDefaultValidCustomerRelationshipServiceCreateRequest();


      customerRelationshipServiceSupport.CreateCustomerRelationshipModel.ParentCustomerUID = parentCustomerUID;
      customerRelationshipServiceSupport.CreateCustomerRelationshipModel.ChildCustomerUID = childCustomerUID;

      customerRelationshipServiceSupport.PostValidCreateCustomerRelationshipRequestToService();
      AccountHierarchyDBComparisonClass NodeDetails = new AccountHierarchyDBComparisonClass();
      NodeDetails.NodeUID = childCustomerUID;
      NodeDetails.ParentNodeUID = parentCustomerUID;
      NodeDetails.RootNodeUID = parentUID[0];
      AccountHierarchyNodes.Add(NodeDetails);
    }

    public AccountHierarchyDBComparisonClass GetNodeByNodeUID(string nodeUID)
    {
      AccountHierarchyDBComparisonClass NodeByNodeUID = new AccountHierarchyDBComparisonClass();
      foreach (var node in AccountHierarchyNodes)
      {
        if (nodeUID == node.NodeUID.ToString())
        {
          NodeByNodeUID = node;
          break;
        }
      }
      return NodeByNodeUID;
    }

    public void DissociateCustomerToDealer(Guid parentCustomerUID, Guid childCustomerUID)
    {
      customerRelationshipServiceSupport.DeleteCustomerRelationshipModel = CustomerRelationshipServiceSteps.GetDefaultValidCustomerRelationshipServiceDeleteRequest();


      customerRelationshipServiceSupport.DeleteCustomerRelationshipModel.ParentCustomerUID = parentCustomerUID;
      customerRelationshipServiceSupport.DeleteCustomerRelationshipModel.ChildCustomerUID = childCustomerUID;
      customerRelationshipServiceSupport.DeleteCustomerRelationshipModel.ActionUTC = DateTime.UtcNow;

      customerRelationshipServiceSupport.PostValidDeleteCustomerRelationshipRequestToService(customerRelationshipServiceSupport.DeleteCustomerRelationshipModel.ParentCustomerUID, customerRelationshipServiceSupport.DeleteCustomerRelationshipModel.ChildCustomerUID, customerRelationshipServiceSupport.DeleteCustomerRelationshipModel.ActionUTC);


    }

    #endregion

    #region Assign Node Position

    public void SetRootNodeValues()
    {
      AccountHierarchyDBComparisonClass NodeDetails = new AccountHierarchyDBComparisonClass();
      NodeDetails.NodeUID = parentUID[0];
      NodeDetails.ParentNodeUID = parentUID[0];
      NodeDetails.RootNodeUID = parentUID[0];
      AccountHierarchyNodes.Add(NodeDetails);
    }

    public void AssignNodePosition(bool removeNode)
    {
      List<int> values = new List<int>();
      int leftPosition = 1;
      int rightPosition = AccountHierarchyNodes.Count * 2;
      int siblingRightNodePosition = 0;
      int leftNodePosition = 0;

      if (removeNode == true)
      {
        rightPosition = AccountHierarchyRemovedNodes.Count * 2;
        foreach (var nodeDetail in AccountHierarchyRemovedNodes)
        {
          if (nodeDetail.NodeUID == nodeDetail.ParentNodeUID && nodeDetail.NodeUID == nodeDetail.RootNodeUID)
          {
            nodeDetail.LeftPosition = leftPosition;
            nodeDetail.RightPosition = rightPosition;
            values.Add(leftPosition);
          }
          else
          {
            leftNodePosition = GetParentLeftNodePosition(nodeDetail.ParentNodeUID.ToString()) + 1;
            while (values.Contains(leftNodePosition))
            {
              siblingRightNodePosition = GetSiblingRightNodePosition(leftNodePosition);
              leftNodePosition = siblingRightNodePosition + 1;
            }
            nodeDetail.LeftPosition = leftNodePosition;
            nodeDetail.RightPosition = nodeDetail.LeftPosition + ((GetChildCounts(nodeDetail.NodeUID.ToString()) * 2) + 1);
            values.Add(nodeDetail.LeftPosition);
          }
        }
      }
      else
      {
        foreach (var nodeDetail in AccountHierarchyNodes)
        {
          if (nodeDetail.NodeUID == nodeDetail.ParentNodeUID && nodeDetail.NodeUID == nodeDetail.RootNodeUID)
          {
            nodeDetail.LeftPosition = leftPosition;
            nodeDetail.RightPosition = rightPosition;
            values.Add(leftPosition);
          }
          else
          {
            leftNodePosition = GetParentLeftNodePosition(nodeDetail.ParentNodeUID.ToString()) + 1;
            while (values.Contains(leftNodePosition))
            {
              siblingRightNodePosition = GetSiblingRightNodePosition(leftNodePosition);
              leftNodePosition = siblingRightNodePosition + 1;
            }
            nodeDetail.LeftPosition = leftNodePosition;
            nodeDetail.RightPosition = nodeDetail.LeftPosition + ((GetChildCounts(nodeDetail.NodeUID.ToString()) * 2) + 1);
            values.Add(nodeDetail.LeftPosition);
          }
        }
      }
      
    }

    public int GetParentLeftNodePosition(string nodeUid)
    {
      int leftPosition = 0;

      foreach (var nodeDetail in AccountHierarchyNodes)
      {
        if (nodeDetail.NodeUID.ToString() == nodeUid)
        {
          leftPosition = nodeDetail.LeftPosition;
        }
      }
      return leftPosition;
    }




    public int GetSiblingRightNodePosition(int leftNodeValue)
    {
      int rightPosition = 0;

      foreach (var nodeDetail in AccountHierarchyNodes)
      {
        if (nodeDetail.LeftPosition == leftNodeValue)
        {
          rightPosition = nodeDetail.RightPosition;
        }
      }
      return rightPosition;
    }

    public int GetChildCounts(string nodeUID)
    {
      int count = 0;

      foreach (var nodeDetail in AccountHierarchyNodes)
      {
        if (nodeDetail.ParentNodeUID.ToString() == nodeUID)
        {
          count++;
        }
      }
      return count;
    }

    #endregion



    [When(@"I Post Valid AccountHierarchyService GetAccountHierarchy Request")]
    public void WhenIPostValidAccountHierarchyServiceGetAccountHierarchyRequest()
    {
      string accessToken = GetValidUserAccessToken(SingleCustomerUserLoginId);
      response = accountHierarchyServiceSupport.PostValidReadRequestToService(accessToken);
    }

    [Then(@"The GetAccountHierarchy Response Should Return The UserAccountHierarchy")]
    public void ThenTheGetAccountHierarchyResponseShouldReturnTheUserAccountHierarchy()
    {
      VerifyAccountHierarchyServiceReadResponse();
    }
    #endregion

    #region Helper Methods

    public void VerifyAccountHierarchyServiceReadResponse()
    {
      AccountHierarchyServiceReadResponseModel responseObject = JsonConvert.DeserializeObject<AccountHierarchyServiceReadResponseModel>(response);
      Assert.AreEqual(SingleCustomerUserUID, responseObject.UserUID);

      for (int responseIndex = 0; responseIndex < responseObject.Customers.Count; responseIndex++)
      {
        bool matchingUID = false;
        for (int customerIndex = 0; customerIndex < responseObject.Customers.Count; customerIndex++)
        {
          if (responseObject.Customers[responseIndex].CustomerUID == ParentCustomerUID[customerIndex].ToString())
          {
            matchingUID = true;
            Assert.AreEqual(responseObject.Customers[responseIndex].Name, ParentCustomerName[customerIndex]);
            Assert.AreEqual(responseObject.Customers[responseIndex].CustomerType, ParentCustomerType[customerIndex]);
            if (ParentCustomerType[customerIndex] == "Dealer")
            {
              for (int childCustomerIndex = 0; childCustomerIndex < responseObject.Customers[customerIndex].Children.Count; childCustomerIndex++)
              {
                bool matchingcustomerUID = false;
                for (int j = 0; j < responseObject.Customers[customerIndex].Children.Count; j++)
                {
                  if (responseObject.Customers[customerIndex].Children[childCustomerIndex].CustomerUID == ChildCustomerUID[responseIndex, j].ToString())
                  {
                    matchingcustomerUID = true;
                    Assert.AreEqual(responseObject.Customers[customerIndex].Children[childCustomerIndex].Name, ChildCustomerName[responseIndex, j]);
                    Assert.AreEqual(responseObject.Customers[customerIndex].Children[childCustomerIndex].CustomerType, ChildCustomerType[responseIndex, j]);
                    break;
                  }
                }
                Assert.IsTrue(matchingcustomerUID);
              }
            }
            break;
          }
        }
        Assert.IsTrue(matchingUID);
      }
    }





    //To Associate 'N' Dealers/Customers To A User
    public void AssociateUsersToACustomer(int numberOfCustomers, string customerType = null)
    {
      UserUID[0] = new Guid(CreateValueSingleCustomerUserWithUID());
      customerUserServiceSupport.AssociateCustomerUserModel = CustomerUserServiceSteps.GetDefaultValidAssociateCustomerUserServiceRequest();
      customerUserServiceSupport.AssociateCustomerUserModel.UserUID = UserUID[0];

      for (int i = 0; i < numberOfCustomers; i++)
      {
        customerServiceSupport.CreateCustomerModel =
        CustomerServiceSteps.GetDefaultValidCustomerServiceCreateRequest();
        customerServiceSupport.CreateCustomerModel.CustomerType = customerType;
        customerServiceSupport.PostValidCreateRequestToService();

        ParentCustomerUID[i] = customerServiceSupport.CreateCustomerModel.CustomerUID;
        ParentCustomerName[i] = customerServiceSupport.CreateCustomerModel.CustomerName;
        ParentCustomerType[i] = customerServiceSupport.CreateCustomerModel.CustomerType;

        customerUserServiceSupport.AssociateCustomerUserModel.CustomerUID = ParentCustomerUID[i];
        customerUserServiceSupport.PostValidCustomerUserAssociateRequestToService();
      }
    }

    //To Associate Dealer And Customer





    public string CreateValueSingleCustomerUserWithUID()
    {
      if (SingleCustomerUserUID == string.Empty)
      {
        TPaaSServicesConfig.SetupEnvironment();
        SingleCustomerUserUID = UserIdentityService.CreateUser(SingleCustomerUserLoginId, SingleCustomerUserPassword);
      }
      return SingleCustomerUserUID;
    }

    public string GetValidUserAccessToken(string userLoginId)
    {
      string accessToken = string.Empty;
      string customerUserLoginId = string.Empty;
      string customerUserPassword = string.Empty;


      CreateValueSingleCustomerUserWithUID();
      customerUserLoginId = SingleCustomerUserLoginId;
      customerUserPassword = SingleCustomerUserPassword;


      string userTokenEndpoint = TokenService.GetTokenAPIEndpointUpdated(TPaaSServicesConfig.TPaaSTokenEndpoint, customerUserLoginId, customerUserPassword);
      accessToken = TokenService.GetAccessToken(userTokenEndpoint, CustomerServiceConfig.WebAPIConsumerKey, CustomerServiceConfig.WebAPIConsumerSecret);

      return accessToken;
    }
   

    //[Given(@"I Set '(.*)' Has '(.*)' And '(.*)' As Children")]
    //public void GivenISetHasAndAsChildren(string rootNode, string childDealer, string childCustomer)
    //{
    //  CreateCustomer("Dealer", rootNode);
    //  CreateCustomer("Dealer", childDealer);
    //  CreateCustomer("Customer", childCustomer);

    //  SetRootNodeValues();

    //  AssociateCustomerToDealer(parentUID[0], parentUID[1], parentUID[0]);
    //  AssociateCustomerToDealer(parentUID[0], parentUID[2], parentUID[0]);

    //  AssignNodePosition();

    //}
    //[Given(@"I Set '(.*)' With '(.*)' As Child")]
    //public void GivenISetWithAsChild(string parentNode, string childNode)
    //{
    //  CreateCustomer("Customer", childNode);
    //  AssociateCustomer(parentUID[1], parentUID[3], parentUID[0]);
    //  AssignNodePosition();
    //}






    [When(@"I Get Valid GetAccountHierarchy Request")]
    public void WhenIGetValidGetAccountHierarchyRequest()
    {



    }







    #endregion

  }
}
