using AutomationCore.Shared.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using VSS.MasterData.Subscription.AcceptanceTests.Utils.Config;
using VSS.SubscriptionService.AcceptanceTests.Scenarios.SubscriptionService;
using VSS.MasterData.Subscription.AcceptanceTests.Utils.Features.Classes.SubscriptionService;
using VSS.MasterData.Subscription.AcceptanceTests.Scenarios.SubscriptionService;
using MySql.Data.MySqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VSS.MasterData.Subscription.AcceptanceTests.Scenarios.SAVSubscription
{
  [Binding]
  public class SAVSubscriptionSteps
  {
    public string TestName;
    public string TestDescription;

    public static Log4Net Log = new Log4Net(typeof(SAVSubscriptionSteps));

    public SubscriptionServiceSupport SubscriptionServiceSupport = new SubscriptionServiceSupport(Log);
    public List<CreateAssetSubscriptionEvent> CreateAssetSubscriptionEvent = new List<CreateAssetSubscriptionEvent>();
    public List<UpdateAssetSubscriptionEvent> UpdateAssetSubscriptionEvent = new List<UpdateAssetSubscriptionEvent>();

    private static bool UpdateExisting = false;
    private static List<string> SubscriptionType = new List<string>();
    private static string TerminateServiceType;
    private static Dictionary<string, string> ServicePlan = new Dictionary<string, string>();


    public List<SAVSubscriptionDBVAlidationModel> SAVSubscriptionDBValidation = new List<SAVSubscriptionDBVAlidationModel>();
    public SAVSubscriptionDBVAlidationModel SAVSubscription;

    public static string SubscriptionUID;


    public SAVSubscriptionSteps()
    {
      SubscriptionServiceConfig.SetupEnvironment();
      ServicePlan.Add("Core Basic", "Essentials");
      ServicePlan.Add("Addon", "CAT Health");
      ServicePlan.Add("Compound Service Plan", "CAT Basic - 6 Hours");
    }

    [Given(@"SAVSubscription Service Is Ready To Verify '(.*)'")]
    public void GivenSAVSubscriptionServiceIsReadyToVerify(string testDescription)
    {
      TestDescription = testDescription;
      TestName = (ScenarioContext.Current.ScenarioInfo).Title + "_" + testDescription;
      //TestName = TestDescription;
      LogResult.Report(Log, "log_ForInfo", "Execution started for Test Scenario" + TestName);
      SubscriptionType.Add("Essentials");
    }

    public void CreateSubscription(int count, string source, bool updateExistingAsset, List<string> subscriptionType)
    {
      for (int i = 0; i < count; i++)
      {
        SAVSubscription = new SAVSubscriptionDBVAlidationModel();
        if (!updateExistingAsset)
        {
          SubscriptionServiceSupport.CreateAssetSubscriptionModel = SubscriptionServiceSteps.GetDefaultValidAssetSubscriptionServiceCreateRequest();
          SubscriptionServiceSupport.CreateAssetSubscriptionModel.SubscriptionType = subscriptionType[i];
          SubscriptionServiceSupport.CreateAssetSubscriptionModel.Source = source;
          SubscriptionUID = SubscriptionServiceSupport.CreateAssetSubscriptionModel.SubscriptionUID.ToString();

        }
        else if (updateExistingAsset)
        {
          if (source == "SAV")
          {

            if (TestDescription == "ServiceViewTerminationWithEndDateOfSAVLessThanEndDateOfOwnerCustomer")
              SubscriptionServiceSupport.CreateAssetSubscriptionModel.EndDate = DateTime.UtcNow.AddYears(1);
            else if (TestDescription == "ServiceViewTerminationWithEndDateOfSAVMoreThanEndDateOfOwnerCustomer")
              SubscriptionServiceSupport.CreateAssetSubscriptionModel.EndDate = DateTime.UtcNow.AddYears(11);

            SubscriptionServiceSupport.CreateAssetSubscriptionModel.SubscriptionUID = Guid.NewGuid();
            SubscriptionServiceSupport.CreateAssetSubscriptionModel.Source = source;
            SubscriptionServiceSupport.CreateAssetSubscriptionModel.SubscriptionType = subscriptionType[i];
            SubscriptionServiceSupport.CreateAssetSubscriptionModel.CustomerUID = Guid.NewGuid();
          }
          if (source == "Store")
          {
            SubscriptionServiceSupport.CreateAssetSubscriptionModel.SubscriptionType = subscriptionType[i];
            SubscriptionServiceSupport.CreateAssetSubscriptionModel.SubscriptionUID = Guid.NewGuid();
            SubscriptionServiceSupport.CreateAssetSubscriptionModel.Source = source;
            SubscriptionServiceSupport.CreateAssetSubscriptionModel.StartDate = DateTime.UtcNow;
            SubscriptionServiceSupport.CreateAssetSubscriptionModel.EndDate = DateTime.UtcNow.AddYears(10);

            for (int j = 0; j < SAVSubscriptionDBValidation.Count(); j++)
            {
              if (SAVSubscriptionDBValidation[j].SubscriptionSource == 1)
                SubscriptionServiceSupport.CreateAssetSubscriptionModel.CustomerUID = Guid.Parse(SAVSubscriptionDBValidation[j].CustomerUID);


            }
          }
        }

        SAVSubscription.SubscriptionUID = SubscriptionServiceSupport.CreateAssetSubscriptionModel.SubscriptionUID.ToString();
        SAVSubscription.AssetUID = SubscriptionServiceSupport.CreateAssetSubscriptionModel.AssetUID.ToString();
        SAVSubscription.CustomerUID = SubscriptionServiceSupport.CreateAssetSubscriptionModel.CustomerUID.ToString();
        SAVSubscription.DeviceUID = SubscriptionServiceSupport.CreateAssetSubscriptionModel.DeviceUID.ToString();
        SAVSubscription.StartDate = SubscriptionServiceSupport.CreateAssetSubscriptionModel.StartDate.ToString();
        SAVSubscription.EndDate = SubscriptionServiceSupport.CreateAssetSubscriptionModel.EndDate.ToString();
        SAVSubscription.SubscriptionType = SubscriptionServiceSupport.CreateAssetSubscriptionModel.SubscriptionType;

        if (source == "Store")
          SAVSubscription.SubscriptionSource = 1;
        else if (source == "SAV")
          SAVSubscription.SubscriptionSource = 2;

        SAVSubscriptionDBValidation.Add(SAVSubscription);

        CreateAssetSubscriptionEvent.Add(SubscriptionServiceSupport.CreateAssetSubscriptionModel);

        SubscriptionServiceSupport.PostValidAssetSubscriptionCreateRequestToService();

        if (count > 1)
          updateExistingAsset = true;
      }

    }

    public void CreateSubscription(int count, string source, bool updateExistingAsset, string subscriptionType)
    {
      for (int i = 0; i < count; i++)
      {
        SAVSubscription = new SAVSubscriptionDBVAlidationModel();
        if (!updateExistingAsset)
        {
          SubscriptionServiceSupport.CreateAssetSubscriptionModel = SubscriptionServiceSteps.GetDefaultValidAssetSubscriptionServiceCreateRequest();
          SubscriptionServiceSupport.CreateAssetSubscriptionModel.SubscriptionType = subscriptionType;
          SubscriptionServiceSupport.CreateAssetSubscriptionModel.Source = source;
          SubscriptionUID = SubscriptionServiceSupport.CreateAssetSubscriptionModel.SubscriptionUID.ToString();
        }
        if (updateExistingAsset)
        {
          if (source == "SAV")
          {

            if (TestDescription == "ServiceViewTerminationWithEndDateOfSAVLessThanEndDateOfOwnerCustomer")
              SubscriptionServiceSupport.CreateAssetSubscriptionModel.EndDate = DateTime.UtcNow.AddYears(1);
            else if (TestDescription == "ServiceViewTerminationWithEndDateOfSAVMoreThanEndDateOfOwnerCustomer")
              SubscriptionServiceSupport.CreateAssetSubscriptionModel.EndDate = DateTime.UtcNow.AddYears(11);

            SubscriptionServiceSupport.CreateAssetSubscriptionModel.SubscriptionUID = Guid.NewGuid();
            SubscriptionServiceSupport.CreateAssetSubscriptionModel.Source = source;
            SubscriptionServiceSupport.CreateAssetSubscriptionModel.CustomerUID = Guid.NewGuid();
            SubscriptionServiceSupport.CreateAssetSubscriptionModel.SubscriptionType = subscriptionType;
          }
          if (source == "Store")
          {
            SubscriptionServiceSupport.CreateAssetSubscriptionModel.SubscriptionType = subscriptionType;
            SubscriptionServiceSupport.CreateAssetSubscriptionModel.SubscriptionUID = Guid.NewGuid();
            SubscriptionServiceSupport.CreateAssetSubscriptionModel.Source = source;
            SubscriptionServiceSupport.CreateAssetSubscriptionModel.StartDate = DateTime.UtcNow;
            SubscriptionServiceSupport.CreateAssetSubscriptionModel.EndDate = DateTime.UtcNow.AddYears(10);

            for (int j = 0; j < SAVSubscriptionDBValidation.Count(); j++)
            {
              if (SAVSubscriptionDBValidation[j].SubscriptionSource == 1)
                SubscriptionServiceSupport.CreateAssetSubscriptionModel.CustomerUID = Guid.Parse(SAVSubscriptionDBValidation[j].CustomerUID);


            }
          }

        }

        SAVSubscription.SubscriptionUID = SubscriptionServiceSupport.CreateAssetSubscriptionModel.SubscriptionUID.ToString();
        SAVSubscription.AssetUID = SubscriptionServiceSupport.CreateAssetSubscriptionModel.AssetUID.ToString();
        SAVSubscription.CustomerUID = SubscriptionServiceSupport.CreateAssetSubscriptionModel.CustomerUID.ToString();
        SAVSubscription.StartDate = SubscriptionServiceSupport.CreateAssetSubscriptionModel.StartDate.ToString();
        SAVSubscription.EndDate = SubscriptionServiceSupport.CreateAssetSubscriptionModel.EndDate.ToString();
        SAVSubscription.SubscriptionType = SubscriptionServiceSupport.CreateAssetSubscriptionModel.SubscriptionType;

        if (source == "Store")
          SAVSubscription.SubscriptionSource = 1;
        else if (source == "SAV")
          SAVSubscription.SubscriptionSource = 2;

        SAVSubscriptionDBValidation.Add(SAVSubscription);

        CreateAssetSubscriptionEvent.Add(SubscriptionServiceSupport.CreateAssetSubscriptionModel);

        SubscriptionServiceSupport.PostValidAssetSubscriptionCreateRequestToService();
      }

    }

    [Given(@"I Have Asset With Shared Asset View With '(.*)' Service Plan")]
    public void GivenIHaveAssetWithSharedAssetViewWithServicePlan(string servicePlanCount)
    {
      switch (servicePlanCount)
      {
        case "Single":


          CreateSubscription(1, "Store", UpdateExisting, SubscriptionType);

          // SubscriptionServiceSupport.PostValidAssetSubscriptionCreateRequestToService();

          UpdateExisting = true;
          CreateSubscription(1, "SAV", UpdateExisting, SubscriptionType);

          // CreateAssetSubscriptionEvent.Add(SubscriptionServiceSupport.CreateAssetSubscriptionModel);


          break;

        case "Multiple":
          SubscriptionType.Add("CAT Health");
          CreateSubscription(2, "Store", UpdateExisting, SubscriptionType);
          UpdateExisting = true;
          CreateSubscription(2, "SAV", UpdateExisting, SubscriptionType);
          break;

        default:
          CreateSubscription(1, "Store", UpdateExisting, SubscriptionType);
          break;
      }
    }

    public void TerminateSubscription(string terminateSubscription)
    {

      //SubscriptionServiceSupport.UpdateAssetSubscriptionModel.SubscriptionUID = Guid.Parse(SubscriptionUID);
      //SubscriptionServiceSupport.UpdateAssetSubscriptionModel.CustomerUID = Guid.Parse(SAVSubscriptionDBValidation[0].CustomerUID);
      //SubscriptionServiceSupport.UpdateAssetSubscriptionModel.AssetUID = Guid.Parse(SAVSubscriptionDBValidation[0].AssetUID);
      //SubscriptionServiceSupport.UpdateAssetSubscriptionModel.DeviceUID = SubscriptionServiceSupport.CreateAssetSubscriptionModel.DeviceUID;
      //SubscriptionServiceSupport.UpdateAssetSubscriptionModel.SubscriptionType = SAVSubscriptionDBValidation[0].SubscriptionType;
      //SubscriptionServiceSupport.UpdateAssetSubscriptionModel.Source = "Store";
      //SubscriptionServiceSupport.UpdateAssetSubscriptionModel.StartDate = Convert.ToDateTime(SAVSubscriptionDBValidation[0].StartDate);
      //SubscriptionServiceSupport.UpdateAssetSubscriptionModel.EndDate = DateTime.UtcNow;
      //SubscriptionServiceSupport.UpdateAssetSubscriptionModel.ActionUTC = DateTime.UtcNow;
      //SubscriptionServiceSupport.UpdateAssetSubscriptionModel.ReceivedUTC = DateTime.UtcNow;
      

      for (int i = 0; i < SAVSubscriptionDBValidation.Count(); i++)
      {
        if (SAVSubscriptionDBValidation[i].SubscriptionType == terminateSubscription && SAVSubscriptionDBValidation[i].SubscriptionSource==1)
        {
          SubscriptionServiceSupport.UpdateAssetSubscriptionModel.SubscriptionUID = Guid.Parse(SAVSubscriptionDBValidation[i].SubscriptionUID);
          SubscriptionServiceSupport.UpdateAssetSubscriptionModel.CustomerUID = Guid.Parse(SAVSubscriptionDBValidation[i].CustomerUID);
          SubscriptionServiceSupport.UpdateAssetSubscriptionModel.AssetUID = Guid.Parse(SAVSubscriptionDBValidation[i].AssetUID);
          SubscriptionServiceSupport.UpdateAssetSubscriptionModel.DeviceUID = Guid.Parse(SAVSubscriptionDBValidation[i].DeviceUID);
          SubscriptionServiceSupport.UpdateAssetSubscriptionModel.SubscriptionType = SAVSubscriptionDBValidation[i].SubscriptionType;
          SubscriptionServiceSupport.UpdateAssetSubscriptionModel.Source = "Store";
          SubscriptionServiceSupport.UpdateAssetSubscriptionModel.StartDate = Convert.ToDateTime(SAVSubscriptionDBValidation[i].StartDate);
          SubscriptionServiceSupport.UpdateAssetSubscriptionModel.EndDate = DateTime.UtcNow;
          SubscriptionServiceSupport.UpdateAssetSubscriptionModel.ActionUTC = DateTime.UtcNow;
          SubscriptionServiceSupport.UpdateAssetSubscriptionModel.ReceivedUTC = DateTime.UtcNow;

          SAVSubscriptionDBValidation[i].EndDate = Convert.ToString(SubscriptionServiceSupport.UpdateAssetSubscriptionModel.EndDate);
          SubscriptionServiceSupport.PostValidAssetSubscriptionUpdateRequestToService();

        }
        if (SAVSubscriptionDBValidation[i].SubscriptionType == terminateSubscription && SAVSubscriptionDBValidation[i].SubscriptionSource == 2)
        {


          SAVSubscriptionDBValidation[i].EndDate = Convert.ToString(SubscriptionServiceSupport.UpdateAssetSubscriptionModel.EndDate);
         // SubscriptionServiceSupport.PostValidAssetSubscriptionUpdateRequestToService();

        }
      }
      
    }



    [When(@"I '(.*)' ServicePlan To Existing Shared Asset")]
    public void WhenIServicePlanToExistingSharedAsset(string updateAction)
    {
      switch (updateAction)
      {
        case "Terminate":
          TerminateServiceType = ServicePlan["Core Basic"];
          TerminateSubscription(TerminateServiceType);
          break;
        case "Add":
          UpdateExisting = true;
          SubscriptionType.Add("CAT Health");
          CreateSubscription(1, "Store", UpdateExisting, "CAT Health");
          break;
      }

    }

    //[When(@"I Terminate One Of The ServicePlans To Existing Shared Asset")]
    //public void WhenITerminateOneOfTheServicePlansToExistingSharedAsset()
    //{
    //  TerminateSubscription();
    //}

    [When(@"I Terminate '(.*)' ServicePlan To Existing Shared Asset")]
    public void WhenITerminateServicePlanToExistingSharedAsset(string servicePlanType)
    {
      TerminateServiceType = ServicePlan[servicePlanType];
      TerminateSubscription(TerminateServiceType);


    }





    [Then(@"Subscription Update Should Reflect in VSS DB")]
    public void ThenSubscriptionUpdateShouldReflectInVSSDB()
    {
      Assert.IsTrue(ValidateDB(), "DB Validation Failure");
    }



    public bool ValidateDB()
    {
      string query;
      bool dbResult = false;
      string DBResultCustomerUID;
      string DBResultStartDate;
      string DBResultEndDate;
      string DBResultSubscriptionSource;

      for (int i = 0; i < SAVSubscriptionDBValidation.Count(); i++)
      {
        MySqlDataReader dataReader = null;
        //List<string> dbResult = new List<string>();
        query = string.Format(SAVSubscriptionSqlQueries.SAVSubscriptionByAssetUID, SAVSubscriptionDBValidation[i].AssetUID.Replace("-", ""), SAVSubscriptionDBValidation[i].SubscriptionUID.Replace("-", ""));
        using (MySqlConnection mySqlConnection = new MySqlConnection(SubscriptionServiceConfig.MySqlConnection))
        {
          try
          {
            //Open connection 
            mySqlConnection.Open();
            //Execute the SQL query
            MySqlCommand mySqlCommand = new MySqlCommand(query, mySqlConnection);
            dataReader = mySqlCommand.ExecuteReader();
            while (dataReader != null && dataReader.Read())
            {
              if (dataReader.HasRows)
              {

                DBResultCustomerUID = dataReader[0].ToString();
                DBResultStartDate = dataReader[1].ToString();
                DBResultEndDate = dataReader[2].ToString();
                DBResultSubscriptionSource = dataReader[3].ToString();

                Assert.AreEqual(SAVSubscriptionDBValidation[i].CustomerUID.Replace("-", "").ToUpper(), DBResultCustomerUID, "Customer DB Validation Fail");
                Assert.AreEqual(SAVSubscriptionDBValidation[i].StartDate, DBResultStartDate, "Start Date VAlidation Fail");
                Assert.AreEqual(SAVSubscriptionDBValidation[i].EndDate, DBResultEndDate, "End Date Validation Fail");
                Assert.AreEqual(SAVSubscriptionDBValidation[i].SubscriptionSource.ToString(), DBResultSubscriptionSource, "Subscription Source Validation Fail");

                dbResult = true;
              }
            }
          }
          catch (Exception e)
          {
            LogResult.Report(SAVSubscriptionSteps.Log, "log_ForError", "Got error while executing db query", e);
            return dbResult;
          }

        }
      }
      return dbResult;
    }

  }
}



