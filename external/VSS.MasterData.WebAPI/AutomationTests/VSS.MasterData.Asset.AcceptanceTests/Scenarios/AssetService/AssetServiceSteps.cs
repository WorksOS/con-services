using System;
using System.Collections.Generic;
using VSS.MasterData.Asset.AcceptanceTests.Utils.Config;
using VSS.MasterData.Asset.AcceptanceTests.Utils.Features.Classes;
using AutomationCore.Shared.Library;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using System.Net;
using Newtonsoft.Json;
using AutomationCore.API.Framework.Library;
using AutomationCore.API.Framework.Common.Features.TPaaS;
using AutomationCore.API.Framework.Common.Config.TPaaSServicesConfig;
using VSS.MasterData.Asset.AcceptanceTests.Utils.Features.Classes.AssetService;


namespace VSS.MasterData.Asset.AcceptanceTests.Scenarios.AssetService
{
  [Binding]
  public class AssetServiceSteps
  {

    #region Variables
    public string TestName;


    //DB Configuration
    public static string MySqlConnectionString;
    public static string MySqlDBName = AssetServiceConfig.MySqlDBName;


    private static Log4Net Log = new Log4Net(typeof(AssetServiceSteps));
    public static AssetServiceSupport assetServiceSupport = new AssetServiceSupport(Log);

    public static string ObjectTypeValue;
    public static string CategoryValue;
    public static string ProjectValue;
    public static string ProjectStatusValue;
    public static string SortFieldValue;
    public static string SourceValue;
    public static string UserEnteredRuntimeHoursValue;
    public static string ClassificationValue;
    public static bool IsUpdate = false;

    public const int ObjectTypeMaxValue = 50;
    public const int CategoryMaxValue = 10;
    public const int ProjectMaxValue = 150;
    public const int ProjectStatusMaxValue = 10;
    public const int SortFieldMaxValue = 50;
    public const int SourceMaxValue = 50;
    public const int UserEnteredRuntimeHoursMaxValue = 100;
    public const int ClassificationMaxValue = 50;





    #endregion

    #region StepDefinition
    //[BeforeFeature()]
    //public static void InitializeKafka()
    //{
    //  if (FeatureContext.Current.FeatureInfo.Title.Equals("AssetService"))
    //  {
    //    //KafkaServicesConfig.InitializeKafkaConsumer(assetServiceSupport);
    //  }
    //}




    public AssetServiceSteps()
    {

      MySqlConnectionString = AssetServiceConfig.MySqlConnection + MySqlDBName;
    }



    [Given(@"AssetService Is Ready To Verify '(.*)'")]
    public void GivenAssetServiceIsReadyToVerify(string TestDescription)
    {
      //log the scenario info
      TestName = (ScenarioContext.Current.ScenarioInfo).Title + "_" + TestDescription;
      //TestName = TestDescription;
      LogResult.Report(Log, "log_ForInfo", "Execution started for Test Scenario" + TestName);
    }

    [Given(@"AssetServiceCreate Request Is Setup With Default Values")]
    public void GivenAssetServiceCreateRequestIsSetupWithDefaultValues()
    {
      assetServiceSupport.CreateAssetModel = GetDefaultValidAssetServiceCreateRequest();
    }

    [When(@"I Set AssetServiceCreate ObjectType To '(.*)'")]
    public void WhenISetAssetServiceCreateObjectTypeTo(string objectType)
    {
      if (objectType == "EMPTY")
      {
        assetServiceSupport.CreateAssetModel.ObjectType = "";
      }
      else if (objectType == "NULL")
      {
        assetServiceSupport.CreateAssetModel.ObjectType = null;
      }
      else if (objectType == "MinValue")
      {
        assetServiceSupport.CreateAssetModel.ObjectType = AssetServiceSupport.RandomString(1);
      }
      else if (objectType == "MaxValue")
      {
        assetServiceSupport.CreateAssetModel.ObjectType = AssetServiceSupport.RandomString(ObjectTypeMaxValue);
      }
      else if (objectType == "Valid")
      {
        assetServiceSupport.CreateAssetModel.ObjectType = AssetServiceSupport.RandomString(10);
      }
    }

    [When(@"I Set AssetServiceCreate Category To '(.*)'")]
    public void WhenISetAssetServiceCreateCategoryTo(string category)
    {
      if (category == "EMPTY")
      {
        assetServiceSupport.CreateAssetModel.Category = "";
      }
      else if (category == "NULL")
      {
        assetServiceSupport.CreateAssetModel.Category = null;
      }
      else if (category == "MinValue")
      {
        assetServiceSupport.CreateAssetModel.Category = AssetServiceSupport.RandomString(1);
      }
      else if (category == "MaxValue")
      {
        assetServiceSupport.CreateAssetModel.Category = AssetServiceSupport.RandomString(CategoryMaxValue);
      }
      else if (category == "Valid")
      {
        assetServiceSupport.CreateAssetModel.Category = AssetServiceSupport.RandomString(10);
      }
    }

    //[When(@"I Set AssetServiceCreate Project To '(.*)'")]
    //public void WhenISetAssetServiceCreateProjectTo(string project)
    //{
    //  if (project == "EMPTY")
    //  {
    //    assetServiceSupport.CreateAssetModel.Project = "";
    //  }
    //  else if (project == "NULL")
    //  {
    //    assetServiceSupport.CreateAssetModel.Project = null;
    //  }
    //  else if (project == "MinValue")
    //  {
    //    assetServiceSupport.CreateAssetModel.Project = AssetServiceSupport.RandomString(1);
    //  }
    //  else if (project == "MaxValue")
    //  {
    //    assetServiceSupport.CreateAssetModel.Project = AssetServiceSupport.RandomString(ProjectMaxValue);
    //  }
    //  else if (project == "Valid")
    //  {
    //    assetServiceSupport.CreateAssetModel.Project = AssetServiceSupport.RandomString(10);
    //  }
    //}

    [When(@"I Set AssetServiceCreate ProjectStatus To '(.*)'")]
    public void WhenISetAssetServiceCreateProjectStatusTo(string projectStatus)
    {
      if (projectStatus == "EMPTY")
      {
        assetServiceSupport.CreateAssetModel.ProjectStatus = "";
      }
      else if (projectStatus == "NULL")
      {
        assetServiceSupport.CreateAssetModel.ProjectStatus = null;
      }
      else if (projectStatus == "MinValue")
      {
        assetServiceSupport.CreateAssetModel.ProjectStatus = AssetServiceSupport.RandomString(1);
      }
      else if (projectStatus == "MaxValue")
      {
        assetServiceSupport.CreateAssetModel.ProjectStatus = AssetServiceSupport.RandomString(ProjectStatusMaxValue);
      }
      else if (projectStatus == "Valid")
      {
        assetServiceSupport.CreateAssetModel.ProjectStatus = AssetServiceSupport.RandomString(10);
      }
    }

    [When(@"I Set AssetServiceCreate SortField To '(.*)'")]
    public void WhenISetAssetServiceCreateSortFieldTo(string sortField)
    {
      if (sortField == "EMPTY")
      {
        assetServiceSupport.CreateAssetModel.SortField = "";
      }
      else if (sortField == "NULL")
      {
        assetServiceSupport.CreateAssetModel.SortField = null;
      }
      else if (sortField == "MinValue")
      {
        assetServiceSupport.CreateAssetModel.SortField = AssetServiceSupport.RandomString(1);
      }
      else if (sortField == "MaxValue")
      {
        assetServiceSupport.CreateAssetModel.SortField = AssetServiceSupport.RandomString(SortFieldMaxValue);
      }
      else if (sortField == "Valid")
      {
        assetServiceSupport.CreateAssetModel.SortField = AssetServiceSupport.RandomString(10);
      }
    }

    [When(@"I Set AssetServiceCreate Source To '(.*)'")]
    public void WhenISetAssetServiceCreateSourceTo(string source)
    {
      if (source == "EMPTY")
      {
        assetServiceSupport.CreateAssetModel.Source = "";
      }
      else if (source == "NULL")
      {
        assetServiceSupport.CreateAssetModel.Source = null;
      }
      else if (source == "MinValue")
      {
        assetServiceSupport.CreateAssetModel.Source = AssetServiceSupport.RandomString(1);
      }
      else if (source == "MaxValue")
      {
        assetServiceSupport.CreateAssetModel.Source = AssetServiceSupport.RandomString(SourceMaxValue);
      }
      else if (source == "Valid")
      {
        assetServiceSupport.CreateAssetModel.Source = AssetServiceSupport.RandomString(10);
      }
    }

    [When(@"I Set AssetServiceCreate UserEnteredRuntimeHours To '(.*)'")]
    public void WhenISetAssetServiceCreateUserEnteredRuntimeHoursTo(string userEnteredRuntimeHours)
    {
      if (userEnteredRuntimeHours == "EMPTY")
      {
        assetServiceSupport.CreateAssetModel.UserEnteredRuntimeHours = "";
      }
      else if (userEnteredRuntimeHours == "NULL")
      {
        assetServiceSupport.CreateAssetModel.UserEnteredRuntimeHours = null;
      }
      else if (userEnteredRuntimeHours == "MinValue")
      {
        assetServiceSupport.CreateAssetModel.UserEnteredRuntimeHours = AssetServiceSupport.RandomString(1);
      }
      else if (userEnteredRuntimeHours == "MaxValue")
      {
        assetServiceSupport.CreateAssetModel.UserEnteredRuntimeHours = AssetServiceSupport.RandomString(UserEnteredRuntimeHoursMaxValue);
      }
      else if (userEnteredRuntimeHours == "Valid")
      {
        assetServiceSupport.CreateAssetModel.UserEnteredRuntimeHours = AssetServiceSupport.RandomString(10);
      }
    }

    [When(@"I Set AssetServiceCreate Classification To '(.*)'")]
    public void WhenISetAssetServiceCreateClassificationTo(string classification)
    {
      if (classification == "EMPTY")
      {
        assetServiceSupport.CreateAssetModel.Classification = "";
      }
      else if (classification == "NULL")
      {
        assetServiceSupport.CreateAssetModel.Classification = null;
      }
      else if (classification == "MinValue")
      {
        assetServiceSupport.CreateAssetModel.Classification = AssetServiceSupport.RandomString(1);
      }
      else if (classification == "MaxValue")
      {
        assetServiceSupport.CreateAssetModel.Classification = AssetServiceSupport.RandomString(ClassificationMaxValue);
      }
      else if (classification == "Valid")
      {
        assetServiceSupport.CreateAssetModel.Classification = AssetServiceSupport.RandomString(10);
      }
    }


    [When(@"I Set AssetServiceUpate ObjectType To '(.*)'")]
    public void WhenISetAssetServiceUpateObjectTypeTo(string objectType)
    {
      IsUpdate = true;
      if (objectType == "EMPTY")
      {
        assetServiceSupport.UpdateAssetModel.ObjectType = "";
        ObjectTypeValue = assetServiceSupport.CreateAssetModel.ObjectType;
      }
      else if (objectType == "NULL")
      {
        assetServiceSupport.UpdateAssetModel.ObjectType = null;
        ObjectTypeValue = assetServiceSupport.CreateAssetModel.ObjectType;
      }
      else if (objectType == "MinValue")
      {
        assetServiceSupport.UpdateAssetModel.ObjectType = AssetServiceSupport.RandomString(1);
      }
      else if (objectType == "MaxValue")
      {
        assetServiceSupport.UpdateAssetModel.ObjectType = AssetServiceSupport.RandomString(ObjectTypeMaxValue);
      }
      else if (objectType == "Valid")
      {
        assetServiceSupport.UpdateAssetModel.ObjectType = AssetServiceSupport.RandomString(10);
      }
    }

    [When(@"I Set AssetServiceUpate Category To '(.*)'")]
    public void WhenISetAssetServiceUpateCategoryTo(string Category)
    {
      IsUpdate = true;
      if (Category == "EMPTY")
      {
        assetServiceSupport.UpdateAssetModel.Category = "";
        CategoryValue = assetServiceSupport.CreateAssetModel.Category;
      }
      else if (Category == "NULL")
      {
        assetServiceSupport.UpdateAssetModel.Category = null;
        CategoryValue = assetServiceSupport.CreateAssetModel.Category;
      }
      else if (Category == "MinValue")
      {
        assetServiceSupport.UpdateAssetModel.Category = AssetServiceSupport.RandomString(1);
      }
      else if (Category == "MaxValue")
      {
        assetServiceSupport.UpdateAssetModel.Category = AssetServiceSupport.RandomString(CategoryMaxValue);
      }
      else if (Category == "Valid")
      {
        assetServiceSupport.UpdateAssetModel.Category = AssetServiceSupport.RandomString(10);
      }
    }

    //[When(@"I Set AssetServiceUpate Project To '(.*)'")]
    //public void WhenISetAssetServiceUpateProjectTo(string Project)
    //{
    //  IsUpdate = true;
    //  if (Project == "EMPTY")
    //  {
    //    assetServiceSupport.UpdateAssetModel.Project = "";
    //    ProjectValue = assetServiceSupport.CreateAssetModel.Project;
    //  }
    //  else if (Project == "NULL")
    //  {
    //    assetServiceSupport.UpdateAssetModel.Project = null;
    //    ProjectValue = assetServiceSupport.CreateAssetModel.Project;
    //  }
    //  else if (Project == "MinValue")
    //  {
    //    assetServiceSupport.UpdateAssetModel.Project = AssetServiceSupport.RandomString(1);
    //  }
    //  else if (Project == "MaxValue")
    //  {
    //    assetServiceSupport.UpdateAssetModel.Project = AssetServiceSupport.RandomString(ProjectMaxValue);
    //  }
    //  else if (Project == "Valid")
    //  {
    //    assetServiceSupport.UpdateAssetModel.Project = AssetServiceSupport.RandomString(10);
    //  }
    //}

    [When(@"I Set AssetServiceUpate ProjectStatus To '(.*)'")]
    public void WhenISetAssetServiceUpateProjectStatusTo(string ProjectStatus)
    {
      IsUpdate = true;
      if (ProjectStatus == "EMPTY")
      {
        assetServiceSupport.UpdateAssetModel.ProjectStatus = "";
        ProjectStatusValue = assetServiceSupport.CreateAssetModel.ProjectStatus;
      }
      else if (ProjectStatus == "NULL")
      {
        assetServiceSupport.UpdateAssetModel.ProjectStatus = null;
        ProjectStatusValue = assetServiceSupport.CreateAssetModel.ProjectStatus;
      }
      else if (ProjectStatus == "MinValue")
      {
        assetServiceSupport.UpdateAssetModel.ProjectStatus = AssetServiceSupport.RandomString(1);
      }
      else if (ProjectStatus == "MaxValue")
      {
        assetServiceSupport.UpdateAssetModel.ProjectStatus = AssetServiceSupport.RandomString(ProjectStatusMaxValue);
      }
      else if (ProjectStatus == "Valid")
      {
        assetServiceSupport.UpdateAssetModel.ProjectStatus = AssetServiceSupport.RandomString(10);
      }
    }

    [When(@"I Set AssetServiceUpate SortField To '(.*)'")]
    public void WhenISetAssetServiceUpateSortFieldTo(string SortField)
    {
      IsUpdate = true;
      if (SortField == "EMPTY")
      {
        assetServiceSupport.UpdateAssetModel.SortField = "";
        SortFieldValue = assetServiceSupport.CreateAssetModel.SortField;
      }
      else if (SortField == "NULL")
      {
        assetServiceSupport.UpdateAssetModel.SortField = null;
        SortFieldValue = assetServiceSupport.CreateAssetModel.SortField;
      }
      else if (SortField == "MinValue")
      {
        assetServiceSupport.UpdateAssetModel.SortField = AssetServiceSupport.RandomString(1);
      }
      else if (SortField == "MaxValue")
      {
        assetServiceSupport.UpdateAssetModel.SortField = AssetServiceSupport.RandomString(SortFieldMaxValue);
      }
      else if (SortField == "Valid")
      {
        assetServiceSupport.UpdateAssetModel.SortField = AssetServiceSupport.RandomString(10);
      }
    }

    [When(@"I Set AssetServiceUpate Source To '(.*)'")]
    public void WhenISetAssetServiceUpateSourceTo(string Source)
    {
      IsUpdate = true;
      if (Source == "EMPTY")
      {
        assetServiceSupport.UpdateAssetModel.Source = "";
        SourceValue = assetServiceSupport.CreateAssetModel.Source;
      }
      else if (Source == "NULL")
      {
        assetServiceSupport.UpdateAssetModel.Source = null;
        SourceValue = assetServiceSupport.CreateAssetModel.Source;
      }
      else if (Source == "MinValue")
      {
        assetServiceSupport.UpdateAssetModel.Source = AssetServiceSupport.RandomString(1);
      }
      else if (Source == "MaxValue")
      {
        assetServiceSupport.UpdateAssetModel.Source = AssetServiceSupport.RandomString(SourceMaxValue);
      }
      else if (Source == "Valid")
      {
        assetServiceSupport.UpdateAssetModel.Source = AssetServiceSupport.RandomString(10);
      }
    }

    [When(@"I Set AssetServiceUpate UserEnteredRuntimeHours To '(.*)'")]
    public void WhenISetAssetServiceUpateUserEnteredRuntimeHoursTo(string UserEnteredRuntimeHours)
    {
      IsUpdate = true;
      if (UserEnteredRuntimeHours == "EMPTY")
      {
        assetServiceSupport.UpdateAssetModel.UserEnteredRuntimeHours = "";
        UserEnteredRuntimeHoursValue = assetServiceSupport.CreateAssetModel.UserEnteredRuntimeHours;
      }
      else if (UserEnteredRuntimeHours == "NULL")
      {
        assetServiceSupport.UpdateAssetModel.UserEnteredRuntimeHours = null;
        UserEnteredRuntimeHoursValue = assetServiceSupport.CreateAssetModel.UserEnteredRuntimeHours;
      }
      else if (UserEnteredRuntimeHours == "MinValue")
      {
        assetServiceSupport.UpdateAssetModel.UserEnteredRuntimeHours = AssetServiceSupport.RandomString(1);
      }
      else if (UserEnteredRuntimeHours == "MaxValue")
      {
        assetServiceSupport.UpdateAssetModel.UserEnteredRuntimeHours = AssetServiceSupport.RandomString(UserEnteredRuntimeHoursMaxValue);
      }
      else if (UserEnteredRuntimeHours == "Valid")
      {
        assetServiceSupport.UpdateAssetModel.UserEnteredRuntimeHours = AssetServiceSupport.RandomString(10);
      }
    }

    [When(@"I Set AssetServiceUpate Classification To '(.*)'")]
    public void WhenISetAssetServiceUpateClassificationTo(string classification)
    {
      if (classification == "EMPTY")
      {
        assetServiceSupport.UpdateAssetModel.Classification = "";
        ClassificationValue = assetServiceSupport.CreateAssetModel.Classification;
      }
      else if (classification == "NULL")
      {
        assetServiceSupport.UpdateAssetModel.Classification = null;
        ClassificationValue = assetServiceSupport.CreateAssetModel.Classification;
      }
      else if (classification == "MinValue")
      {
        assetServiceSupport.UpdateAssetModel.Classification = AssetServiceSupport.RandomString(1);
      }
      else if (classification == "MaxValue")
      {
        assetServiceSupport.UpdateAssetModel.Classification = AssetServiceSupport.RandomString(ClassificationMaxValue);
      }
      else if (classification == "Valid")
      {
        assetServiceSupport.UpdateAssetModel.Classification = AssetServiceSupport.RandomString(10);
      }
    }



    [Given(@"AssetServiceCreate Request Is Setup With Default Values and Valid MakeCode")]
    public void GivenAssetServiceCreateRequestIsSetupWithDefaultValuesAndValidMakeCode()
    {
      assetServiceSupport.CreateAssetModel = GetDefaultValidAssetServiceCreateRequest();
    }

    [Given(@"AssetServiceCreate Request Is Setup With Default Values and Invalid MakeCode")]
    public void GivenAssetServiceCreateRequestIsSetupWithDefaultValuesAndInvalidMakeCode()
    {
      assetServiceSupport.CreateAssetModel = GetDefaultValidAssetServiceCreateRequest();
      assetServiceSupport.CreateAssetModel.MakeCode = "INV";
    }



    [When(@"I Post Valid AssetServiceCreate Request")]
    public void WhenIPostValidAssetServiceCreateRequest()
    {
      // assetServiceSupport.SetupCreateAssetKafkaConsumer(assetServiceSupport.CreateAssetModel.AssetUID, assetServiceSupport.CreateAssetModel.ActionUTC);
      assetServiceSupport.PostValidCreateRequestToService();
    }

    [Then(@"The Processed AssetServiceCreate Message must be available in Kafka topic")]
    public void ThenTheProcessedAssetServiceCreateMessageMustBeAvailableInKafkaTopic()
    {
      assetServiceSupport.VerifyAssetServiceCreateResponse();
    }

    [Given(@"AssetServiceUpdate Request Is Setup With Default Values")]
    public void GivenAssetServiceUpdateRequestIsSetupWithDefaultValues()
    {
      assetServiceSupport.CreateAssetModel = GetDefaultValidAssetServiceCreateRequest();
      assetServiceSupport.PostValidCreateRequestToService();
      assetServiceSupport.UpdateAssetModel = GetDefaultValidAssetServiceUpdateRequest();
    }

    [When(@"I Post Valid AssetServiceUpdate Request")]
    public void WhenIPostValidAssetServiceUpdateRequest()
    {

      assetServiceSupport.PostValidUpdateRequestToService();
    }

    [Then(@"The Processed AssetServiceUpdate Message must be available in Kafka topic")]
    public void ThenTheProcessedAssetServiceUpdateMessageMustBeAvailableInKafkaTopic()
    {
      assetServiceSupport.VerifyAssetServiceUpdateResponse();
    }


    [Given(@"AssetServiceDelete Request Is Setup With Default Values")]
    public void GivenAssetServiceDeleteRequestIsSetupWithDefaultValues()
    {
      assetServiceSupport.CreateAssetModel = GetDefaultValidAssetServiceCreateRequest();
      assetServiceSupport.PostValidCreateRequestToService();
      assetServiceSupport.DeleteAssetModel = GetDefaultValidAssetServiceDeleteRequest();
    }

    [Given(@"AssetServiceCreate Request Is Setup With Invalid Default Values")]
    public void GivenAssetServiceCreateRequestIsSetupWithInvalidDefaultValues()
    {
      assetServiceSupport.InValidCreateAssetModel = GetDefaultInValidAssetServiceCreateRequest();
    }

    [Given(@"AssetServiceUpdate Request Is Setup With Invalid Default Values")]
    public void GivenAssetServiceUpdateRequestIsSetupWithInvalidDefaultValues()
    {
      assetServiceSupport.InValidUpdateAssetModel = GetDefaultInValidAssetServiceUpdateRequest();
    }

    [Given(@"AssetServiceDelete Request Is Setup With Invalid Default Values")]
    public void GivenAssetServiceDeleteRequestIsSetupWithInvalidDefaultValues()
    {
      assetServiceSupport.InValidDeleteAssetModel = GetDefaultInValidAssetServiceDeleteRequest();
    }


    [When(@"I Post Valid AssetServiceDelete Request")]
    public void WhenIPostValidAssetServiceDeleteRequest()
    {
      assetServiceSupport.PostValidDeleteRequestToService(assetServiceSupport.DeleteAssetModel.AssetUID, assetServiceSupport.DeleteAssetModel.ActionUTC);
    }

    [Then(@"The Processed AssetServiceDelete Message must be available in Kafka topic")]
    public void ThenTheProcessedAssetServiceDeleteMessageMustBeAvailableInKafkaTopic()
    {
      assetServiceSupport.VerifyAssetServiceDeleteResponse();
    }

    [When(@"I Set AssetServiceCreate AssetName To '(.*)'")]
    public void WhenISetAssetServiceCreateAssetNameTo(string assetName)
    {
      assetServiceSupport.CreateAssetModel.AssetName = InputGenerator.GetValue(assetName);
    }

    [When(@"I Set AssetServiceCreate AssetType To '(.*)'")]
    public void WhenISetAssetServiceCreateAssetTypeTo(string assetType)
    {
      assetServiceSupport.CreateAssetModel.AssetType = InputGenerator.GetValue(assetType);
    }

    [When(@"I Set AssetServiceCreate Model To '(.*)'")]
    public void WhenISetAssetServiceCreateModelTo(string model)
    {
      assetServiceSupport.CreateAssetModel.Model = InputGenerator.GetValue(model);
    }

    [When(@"I Set AssetServiceCreate EquipmentVIN To '(.*)'")]
    public void WhenISetAssetServiceCreateEquipmentVINTo(string equipmentVin)
    {
      assetServiceSupport.CreateAssetModel.EquipmentVIN = InputGenerator.GetValue(equipmentVin);
    }

    [When(@"I Set AssetServiceCreate IconKey To '(.*)'")]
    public void WhenISetAssetServiceCreateIconKeyTo(string iconKey)
    {
      assetServiceSupport.CreateAssetModel.IconKey = String.IsNullOrEmpty(InputGenerator.GetValue(iconKey)) ? null : (int?)Convert.ToInt32(InputGenerator.GetValue(iconKey));
    }

    [When(@"I Set AssetServiceCreate ModelYear To '(.*)'")]
    public void WhenISetAssetServiceCreateModelYearTo(string modelYear)
    {
      assetServiceSupport.CreateAssetModel.ModelYear = String.IsNullOrEmpty(InputGenerator.GetValue(modelYear)) ? null : (int?)Convert.ToInt32(InputGenerator.GetValue(modelYear));
    }

    [When(@"I Set AssetServiceCreate MakeCode To '(.*)'")]
    public void WhenISetAssetServiceCreateMakeCodeTo(string makeCode)
    {
      assetServiceSupport.CreateAssetModel.MakeCode = InputGenerator.GetValue(makeCode);

    }

    [When(@"I Set Invalid AssetServiceCreate AssetName To '(.*)'")]
    public void WhenISetInvalidAssetServiceCreateAssetNameTo(string assetName)
    {
      assetServiceSupport.InValidCreateAssetModel.AssetName = InputGenerator.GetValue(assetName);
    }

    [When(@"I Set Invalid AssetServiceCreate AssetType To '(.*)'")]
    public void WhenISetInvalidAssetServiceCreateAssetTypeTo(string assetType)
    {
      assetServiceSupport.InValidCreateAssetModel.AssetType = InputGenerator.GetValue(assetType);
    }

    [When(@"I Set Invalid AssetServiceCreate Model To '(.*)'")]
    public void WhenISetInvalidAssetServiceCreateModelTo(string model)
    {
      assetServiceSupport.InValidCreateAssetModel.Model = InputGenerator.GetValue(model);
    }

    [When(@"I Set Invalid AssetServiceCreate EquipmentVIN To '(.*)'")]
    public void WhenISetInvalidAssetServiceCreateEquipmentVINTo(string equipmentVin)
    {
      assetServiceSupport.InValidCreateAssetModel.EquipmentVIN = InputGenerator.GetValue(equipmentVin);
    }

    [When(@"I Set Invalid AssetServiceCreate IconKey To '(.*)'")]
    public void WhenISetInvalidAssetServiceCreateIconKeyTo(string iconKey)
    {
      assetServiceSupport.InValidCreateAssetModel.IconKey = InputGenerator.GetValue(iconKey);
    }

    [When(@"I Set Invalid AssetServiceCreate ModelYear To '(.*)'")]
    public void WhenISetInvalidAssetServiceCreateModelYearTo(string modelYear)
    {
      assetServiceSupport.InValidCreateAssetModel.ModelYear = InputGenerator.GetValue(modelYear);
    }

    [When(@"I Set Invalid AssetServiceCreate MakeCode To '(.*)'")]
    public void WhenISetInvalidAssetServiceCreateMakeCodeTo(string makeCode)
    {
      assetServiceSupport.InValidCreateAssetModel.MakeCode = InputGenerator.GetValue(makeCode);
    }

    [When(@"I Set Invalid AssetServiceUpdate AssetUID To '(.*)'")]
    public void WhenISetInvalidAssetServiceUpdateAssetUIDTo(string assetUid)
    {
      assetServiceSupport.InValidUpdateAssetModel.AssetUID = InputGenerator.GetValue(assetUid);
    }

    [When(@"I Set AssetServiceCreate LegacyAssetID To '(.*)'")]
    public void WhenISetAssetServiceCreateLegacyAssetIDTo(string legacyAssetid)
    {
      assetServiceSupport.CreateAssetModel.LegacyAssetID = String.IsNullOrEmpty(InputGenerator.GetValue(legacyAssetid)) ? null : (long?)Convert.ToInt64(InputGenerator.GetValue(legacyAssetid));
    }

    [When(@"I Post Valid AssetServiceCreate Request With The Below Values")]
    public void WhenIPostValidAssetServiceCreateRequestWithTheBelowValues(Table table)
    {
      assetServiceSupport.CreateAssetModel.AssetName = InputGenerator.GetValue(((string[])(table.Rows[0].Values))[0]);
      assetServiceSupport.CreateAssetModel.AssetType = InputGenerator.GetValue(((string[])(table.Rows[0].Values))[1]);
      assetServiceSupport.CreateAssetModel.Model = InputGenerator.GetValue(((string[])(table.Rows[0].Values))[2]);
      assetServiceSupport.CreateAssetModel.ModelYear = String.IsNullOrEmpty(InputGenerator.GetValue(((string[])(table.Rows[0].Values))[3])) ? null : (int?)Convert.ToInt32(InputGenerator.GetValue(((string[])(table.Rows[0].Values))[3]));
      assetServiceSupport.CreateAssetModel.EquipmentVIN = InputGenerator.GetValue(((string[])(table.Rows[0].Values))[4]);
      assetServiceSupport.CreateAssetModel.IconKey = String.IsNullOrEmpty(InputGenerator.GetValue(((string[])(table.Rows[0].Values))[5])) ? null : (int?)Convert.ToInt32(InputGenerator.GetValue(((string[])(table.Rows[0].Values))[5]));
      assetServiceSupport.CreateAssetModel.LegacyAssetID = String.IsNullOrEmpty(InputGenerator.GetValue(((string[])(table.Rows[0].Values))[6])) ? null : (long?)Convert.ToInt64(InputGenerator.GetValue(((string[])(table.Rows[0].Values))[6]));
      assetServiceSupport.CreateAssetModel.ActionUTC = DateTime.UtcNow;
      assetServiceSupport.PostValidCreateRequestToService();
    }


    [When(@"I Post Valid AssetServiceUpdate Request With The Below Values")]
    public void WhenIPostValidAssetServiceUpdateRequestWithTheBelowValues(Table table)
    {
      assetServiceSupport.UpdateAssetModel.AssetName = InputGenerator.GetValue(((string[])(table.Rows[0].Values))[0]);
      assetServiceSupport.UpdateAssetModel.AssetType = InputGenerator.GetValue(((string[])(table.Rows[0].Values))[1]);
      assetServiceSupport.UpdateAssetModel.Model = InputGenerator.GetValue(((string[])(table.Rows[0].Values))[2]);
      assetServiceSupport.UpdateAssetModel.ModelYear = String.IsNullOrEmpty(InputGenerator.GetValue(((string[])(table.Rows[0].Values))[3])) ? null : (int?)Convert.ToInt32(InputGenerator.GetValue(((string[])(table.Rows[0].Values))[3]));
      assetServiceSupport.UpdateAssetModel.EquipmentVIN = InputGenerator.GetValue(((string[])(table.Rows[0].Values))[4]);
      assetServiceSupport.UpdateAssetModel.IconKey = String.IsNullOrEmpty(InputGenerator.GetValue(((string[])(table.Rows[0].Values))[5])) ? null : (int?)Convert.ToInt32(InputGenerator.GetValue(((string[])(table.Rows[0].Values))[5]));
      assetServiceSupport.UpdateAssetModel.LegacyAssetID = String.IsNullOrEmpty(InputGenerator.GetValue(((string[])(table.Rows[0].Values))[6])) ? null : (long?)Convert.ToInt64(InputGenerator.GetValue(((string[])(table.Rows[0].Values))[6]));
      assetServiceSupport.UpdateAssetModel.ActionUTC = DateTime.UtcNow;
      assetServiceSupport.PostValidUpdateRequestToService();
    }

    [When(@"I Post Invalid AssetServiceUpdate Request With The Below Values")]
    public void WhenIPostInvalidAssetServiceUpdateRequestWithTheBelowValues(Table table)
    {
      assetServiceSupport.InValidUpdateAssetModel.AssetName = InputGenerator.GetValue(((string[])(table.Rows[0].Values))[0]);
      assetServiceSupport.InValidUpdateAssetModel.AssetType = InputGenerator.GetValue(((string[])(table.Rows[0].Values))[1]);
      assetServiceSupport.InValidUpdateAssetModel.Model = InputGenerator.GetValue(((string[])(table.Rows[0].Values))[2]);
      assetServiceSupport.InValidUpdateAssetModel.ModelYear = String.IsNullOrEmpty(InputGenerator.GetValue(((string[])(table.Rows[0].Values))[3])) ? null : (InputGenerator.GetValue(((string[])(table.Rows[0].Values))[3]));
      assetServiceSupport.InValidUpdateAssetModel.EquipmentVIN = InputGenerator.GetValue(((string[])(table.Rows[0].Values))[4]);
      assetServiceSupport.InValidUpdateAssetModel.IconKey = String.IsNullOrEmpty(InputGenerator.GetValue(((string[])(table.Rows[0].Values))[5])) ? null : (InputGenerator.GetValue(((string[])(table.Rows[0].Values))[5]));
      assetServiceSupport.InValidUpdateAssetModel.LegacyAssetID = String.IsNullOrEmpty(InputGenerator.GetValue(((string[])(table.Rows[0].Values))[6])) ? null : (InputGenerator.GetValue(((string[])(table.Rows[0].Values))[6]));
      assetServiceSupport.InValidUpdateAssetModel.ActionUTC = DateTime.UtcNow.ToString();
      string contentType = "application/json";
      assetServiceSupport.PostInValidUpdateRequestToService(contentType, HttpStatusCode.BadRequest);
    }


    [When(@"I Set Invalid AssetServiceCreate AssetUID To '(.*)'")]
    public void WhenISetInvalidAssetServiceCreateAssetUIDTo(string assetUid)
    {
      assetServiceSupport.InValidCreateAssetModel.AssetUID = InputGenerator.GetValue(assetUid);
    }

    [When(@"I Set Invalid AssetServiceCreate ActionUTC To '(.*)'")]
    public void WhenISetInvalidAssetServiceCreateActionUTCTo(string actionUtc)
    {
      assetServiceSupport.InValidCreateAssetModel.ActionUTC = InputGenerator.GetValue(actionUtc.ToString());
    }

    [When(@"I Set Invalid AssetServiceUpdate ActionUTC To '(.*)'")]
    public void WhenISetInvalidAssetServiceUpdateActionUTCTo(string actionUtc)
    {
      assetServiceSupport.InValidUpdateAssetModel.ActionUTC = InputGenerator.GetValue(actionUtc.ToString());
    }

    [When(@"I Set Invalid AssetServiceDelete ActionUTC To '(.*)'")]
    public void WhenISetInvalidAssetServiceDeleteActionUTCTo(string actionUtc)
    {
      assetServiceSupport.InValidDeleteAssetModel.ActionUTC = InputGenerator.GetValue(actionUtc.ToString());
    }

    [When(@"I Post Invalid AssetServiceCreate Request")]
    public void WhenIPostInvalidAssetServiceCreateRequest()
    {
      string contentType = "application/json";
      assetServiceSupport.PostInValidCreateRequestToService(contentType, HttpStatusCode.BadRequest);
    }

    [Then(@"AssetService Response With '(.*)' Should Be Returned")]
    public void ThenAssetServiceResponseWithShouldBeReturned(string errorMessage)
    {
      assetServiceSupport.VerifyErrorResponse(errorMessage);
    }

    [When(@"I Set Invalid AssetServiceCreate SerialNumber To '(.*)'")]
    public void WhenISetInvalidAssetServiceCreateSerialNumberTo(string serialNumber)
    {
      assetServiceSupport.InValidCreateAssetModel.SerialNumber = InputGenerator.GetValue(serialNumber);
    }

    [When(@"I Set Invalid AssetServiceUpdate ModelYear To '(.*)'")]
    public void WhenISetInvalidAssetServiceUpdateModelYearTo(string modelYear)
    {
      assetServiceSupport.InValidUpdateAssetModel.ModelYear = InputGenerator.GetValue(modelYear);
    }

    [When(@"I Set Invalid AssetServiceUpdate IconKey To '(.*)'")]
    public void WhenISetInvalidAssetServiceUpdateIconKeyTo(string iconKey)
    {
      assetServiceSupport.InValidUpdateAssetModel.IconKey = InputGenerator.GetValue(iconKey);
    }

    [When(@"I Post AssetServiceCreate Request With Invalid ContentType '(.*)'")]
    public void WhenIPostAssetServiceCreateRequestWithInvalidContentType(string contentType)
    {
      assetServiceSupport.PostInValidCreateRequestToService(contentType, HttpStatusCode.UnsupportedMediaType);
    }

    [When(@"I Post Invalid AssetServiceUpdate Request")]
    public void WhenIPostInvalidAssetServiceUpdateRequest()
    {
      string contentType = "application/json";
      assetServiceSupport.PostInValidUpdateRequestToService(contentType, HttpStatusCode.BadRequest);
    }

    [When(@"I Post Invalid AssetServiceUpdate Request With Invalid ContentType '(.*)'")]
    public void WhenIPostInvalidAssetServiceUpdateRequestWithInvalidContentType(string contentType)
    {
      assetServiceSupport.PostInValidUpdateRequestToService(contentType, HttpStatusCode.UnsupportedMediaType);
    }

    [When(@"I Set Invalid AssetServiceDelete AssetUID To '(.*)'")]
    public void WhenISetInvalidAssetServiceDeleteAssetUIDTo(string assetUid)
    {
      assetServiceSupport.InValidDeleteAssetModel.AssetUID = assetUid;
    }

    [When(@"I Post Invalid AssetServiceDelete Request")]
    public void WhenIPostInvalidDeleteAssetRequest()
    {
      string contentType = "application/json";
      assetServiceSupport.PostInValidDeleteRequestToService(assetServiceSupport.InValidDeleteAssetModel.AssetUID, assetServiceSupport.InValidDeleteAssetModel.ActionUTC, contentType, HttpStatusCode.BadRequest);
    }

    [When(@"I Set AssetServiceUpdate AssetName To '(.*)'")]
    public void WhenISetAssetServiceUpdateAssetNameTo(string assetName)
    {
      assetServiceSupport.UpdateAssetModel.AssetName = InputGenerator.GetValue(assetName);
    }

    [When(@"I Set AssetServiceCreate SerialNumber To '(.*)'")]
    public void WhenISetAssetServiceCreateSerialNumberTo(string serialNumber)
    {
      assetServiceSupport.CreateAssetModel.SerialNumber = InputGenerator.GetValue(serialNumber);
    }

    [When(@"I Set AssetServiceUpdate AssetType To '(.*)'")]
    public void WhenISetAssetServiceUpdateAssetTypeTo(string assetType)
    {
      assetServiceSupport.UpdateAssetModel.AssetType = InputGenerator.GetValue(assetType);
    }

    [When(@"I Set AssetServiceUpdate Model To '(.*)'")]
    public void WhenISetAssetServiceUpdateModelTo(string model)
    {
      assetServiceSupport.UpdateAssetModel.Model = InputGenerator.GetValue(model);
    }

    [When(@"I Set AssetServiceUpdate EquipmentVIN To '(.*)'")]
    public void WhenISetAssetServiceUpdateEquipmentVINTo(string equipmentVin)
    {
      assetServiceSupport.UpdateAssetModel.EquipmentVIN = InputGenerator.GetValue(equipmentVin);
    }


    [Then(@"The AssetUpdated Details must be stored in MySql DB")]
    public void ThenTheAssetUpdatedDetailsMustBeStoredInMySqlDB()
    {
      CommonUtil.WaitToProcess("2"); //Wait for the data to get persisted in DB

      string query = AssetServiceMySqlQueries.AssetUpdatedDetailsByAssetUID + assetServiceSupport.CreateAssetModel.AssetUID.ToString().Replace("-", "") + "'";
      List<string> columnList = new List<string>() { "AssetName", "LegacyAssetID", "Model", "AssetTypeName", "IconKey", "EquipmentVIN", "ModelYear" };
      List<string> assetDetails = new List<string>();
      if (assetServiceSupport.UpdateAssetModel.AssetName == null)
      {
        if (assetServiceSupport.CreateAssetModel.AssetName == null)
        {
          assetDetails.Add("");
        }
        assetDetails.Add(assetServiceSupport.CreateAssetModel.AssetName);
      }
      else
      {
        assetDetails.Add(assetServiceSupport.UpdateAssetModel.AssetName);
      }
      if (assetServiceSupport.UpdateAssetModel.LegacyAssetID == null)
      {
        assetDetails.Add(assetServiceSupport.CreateAssetModel.LegacyAssetID.ToString());
      }
      else
      {
        assetDetails.Add(assetServiceSupport.UpdateAssetModel.LegacyAssetID.ToString());
      }
      if (assetServiceSupport.UpdateAssetModel.Model == null)
      {
        assetDetails.Add(assetServiceSupport.CreateAssetModel.Model);
      }
      else
      { assetDetails.Add(assetServiceSupport.UpdateAssetModel.Model); }
      if (assetServiceSupport.UpdateAssetModel.AssetType == null)
      { assetDetails.Add(assetServiceSupport.CreateAssetModel.AssetType); }
      else { assetDetails.Add(assetServiceSupport.UpdateAssetModel.AssetType); }
      if (assetServiceSupport.UpdateAssetModel.IconKey == null)
      { assetDetails.Add(assetServiceSupport.CreateAssetModel.IconKey.ToString()); }
      else { assetDetails.Add(assetServiceSupport.UpdateAssetModel.IconKey.ToString()); }
      if (assetServiceSupport.UpdateAssetModel.EquipmentVIN == null)
      { assetDetails.Add(assetServiceSupport.CreateAssetModel.EquipmentVIN); }
      else { assetDetails.Add(assetServiceSupport.UpdateAssetModel.EquipmentVIN); }
      if (assetServiceSupport.UpdateAssetModel.ModelYear == null)
      { assetDetails.Add(assetServiceSupport.CreateAssetModel.ModelYear.ToString()); }
      else { assetDetails.Add(assetServiceSupport.UpdateAssetModel.ModelYear.ToString()); }

      if (assetServiceSupport.UpdateAssetModel.ObjectType == null)
      {
        assetDetails.Add(ObjectTypeValue);
      }
      else if (assetServiceSupport.UpdateAssetModel.ObjectType == "")
      {
        assetDetails.Add("");
      }
      else
      {
        assetDetails.Add(assetServiceSupport.UpdateAssetModel.ObjectType);
      }

      if (assetServiceSupport.UpdateAssetModel.Category == null)
      {
        assetDetails.Add(CategoryValue);
      }
      else if (assetServiceSupport.UpdateAssetModel.Category == "")
      {
        assetDetails.Add("");
      }
      else
      {
        assetDetails.Add(assetServiceSupport.UpdateAssetModel.Category);
      }

      //if (assetServiceSupport.UpdateAssetModel.Project == null)
      //{
      //  assetDetails.Add(ProjectValue);
      //}
      //else if (assetServiceSupport.UpdateAssetModel.Project == "")
      //{
      //  assetDetails.Add("");
      //}
      //else
      //{
      //  assetDetails.Add(assetServiceSupport.UpdateAssetModel.Project);
      //}

      if (assetServiceSupport.UpdateAssetModel.ProjectStatus == null)
      {
        assetDetails.Add(ProjectStatusValue);
      }
      else if (assetServiceSupport.UpdateAssetModel.ProjectStatus == "")
      {
        assetDetails.Add("");
      }
      else
      {
        assetDetails.Add(assetServiceSupport.UpdateAssetModel.ProjectStatus);
      }

      if (assetServiceSupport.UpdateAssetModel.SortField == null)
      {
        assetDetails.Add(SortFieldValue);
      }
      else if (assetServiceSupport.UpdateAssetModel.SortField == "")
      {
        assetDetails.Add("");
      }
      else
      {
        assetDetails.Add(assetServiceSupport.UpdateAssetModel.SortField);
      }

      if (assetServiceSupport.UpdateAssetModel.Source == null)
      {
        assetDetails.Add(SourceValue);
      }
      else if (assetServiceSupport.UpdateAssetModel.Source == "")
      {
        assetDetails.Add("");
      }
      else
      {
        assetDetails.Add(assetServiceSupport.UpdateAssetModel.Source);
      }

      if (assetServiceSupport.UpdateAssetModel.UserEnteredRuntimeHours == null)
      {
        assetDetails.Add(UserEnteredRuntimeHoursValue);
      }
      else if (assetServiceSupport.UpdateAssetModel.UserEnteredRuntimeHours == "")
      {
        assetDetails.Add("");
      }
      else
      {
        assetDetails.Add(assetServiceSupport.UpdateAssetModel.UserEnteredRuntimeHours);
      }

      if (assetServiceSupport.UpdateAssetModel.Classification == null)
      {
        assetDetails.Add(ClassificationValue);
      }
      else if (assetServiceSupport.UpdateAssetModel.Classification == "")
      {
        assetDetails.Add("");
      }
      else
      {
        assetDetails.Add(assetServiceSupport.UpdateAssetModel.Classification);
      }

      MySqlUtil.ValidateMySQLQuery(MySqlConnectionString, query, assetDetails);
    }

    [Then(@"The AssetCreated Details must be stored in MySql DB")]
    public void ThenTheAssetCreatedDetailsMustBeStoredInMySqlDB()
    {
      CommonUtil.WaitToProcess("2"); //Wait for the data to get persisted in DB

      string query = AssetServiceMySqlQueries.AssetDetailsByAssetUID + assetServiceSupport.CreateAssetModel.AssetUID.ToString().Replace("-", "") + "'";
      //List<string> columnList = new List<string>() { "AssetName", "LegacyAssetID", "SerialNumber", "MakeCode", "Model", "AssetTypeName", "IconKey", "EquipmentVIN", "ModelYear", "OwningCustomerUID" };
      List<string> assetDetails = new List<string>();
      if (assetServiceSupport.CreateAssetModel.AssetName == null)
      {
        assetDetails.Add("");
      }
      else
      { assetDetails.Add(assetServiceSupport.CreateAssetModel.AssetName); }
      if (assetServiceSupport.CreateAssetModel.LegacyAssetID == null)
      {
        assetDetails.Add("0");
      }
      else
      {
        assetDetails.Add(assetServiceSupport.CreateAssetModel.LegacyAssetID.ToString());
      }
      assetDetails.Add(assetServiceSupport.CreateAssetModel.SerialNumber.ToString());
      assetDetails.Add(assetServiceSupport.CreateAssetModel.MakeCode);
      if (assetServiceSupport.CreateAssetModel.Model == null)
      {
        assetDetails.Add("");
      }
      else
      {
        assetDetails.Add(assetServiceSupport.CreateAssetModel.Model);
      }
      if (assetServiceSupport.CreateAssetModel.AssetType == null)
      {
        assetDetails.Add("");
      }
      else { assetDetails.Add(assetServiceSupport.CreateAssetModel.AssetType); }
      assetDetails.Add(assetServiceSupport.CreateAssetModel.IconKey.ToString());
      if (assetServiceSupport.CreateAssetModel.EquipmentVIN == null)
      {
        assetDetails.Add("");
      }
      else
      {
        assetDetails.Add(assetServiceSupport.CreateAssetModel.EquipmentVIN);
      }
      assetDetails.Add(assetServiceSupport.CreateAssetModel.ModelYear.ToString());
      if (assetServiceSupport.CreateAssetModel.OwningCustomerUID == Guid.Empty)
      {
        assetDetails.Add("");
      }
      else { assetDetails.Add(assetServiceSupport.CreateAssetModel.OwningCustomerUID.ToString().Replace("-", "").ToUpper()); }

      if (assetServiceSupport.CreateAssetModel.ObjectType == null || assetServiceSupport.CreateAssetModel.ObjectType == "")
      {
        assetDetails.Add("");
      }
      else
      {
        assetDetails.Add(assetServiceSupport.CreateAssetModel.ObjectType);
      }

      if (assetServiceSupport.CreateAssetModel.Category == null || assetServiceSupport.CreateAssetModel.Category == "")
      {
        assetDetails.Add("");
      }
      else
      {
        assetDetails.Add(assetServiceSupport.CreateAssetModel.Category);
      }

      //if (assetServiceSupport.CreateAssetModel.Project == null || assetServiceSupport.CreateAssetModel.Project == "")
      //{
      //  assetDetails.Add("");
      //}
      //else
      //{
      //  assetDetails.Add(assetServiceSupport.CreateAssetModel.Project);
      //}

      if (assetServiceSupport.CreateAssetModel.ProjectStatus == null || assetServiceSupport.CreateAssetModel.ProjectStatus == "")
      {
        assetDetails.Add("");
      }
      else
      {
        assetDetails.Add(assetServiceSupport.CreateAssetModel.ProjectStatus);
      }

      if (assetServiceSupport.CreateAssetModel.SortField == null || assetServiceSupport.CreateAssetModel.SortField == "")
      {
        assetDetails.Add("");
      }
      else
      {
        assetDetails.Add(assetServiceSupport.CreateAssetModel.SortField);
      }

      if (assetServiceSupport.CreateAssetModel.Source == null || assetServiceSupport.CreateAssetModel.Source == "")
      {
        assetDetails.Add("");
      }
      else
      {
        assetDetails.Add(assetServiceSupport.CreateAssetModel.Source);
      }

      if (assetServiceSupport.CreateAssetModel.UserEnteredRuntimeHours == null || assetServiceSupport.CreateAssetModel.UserEnteredRuntimeHours == "")
      {
        assetDetails.Add("");
      }
      else
      {
        assetDetails.Add(assetServiceSupport.CreateAssetModel.UserEnteredRuntimeHours);
      }


      MySqlUtil.ValidateMySQLQuery(MySqlConnectionString, query, assetDetails);
    }

    [Then(@"The AssetDeleted Details must be stored in MySql DB")]
    public void ThenTheAssetDeletedDetailsMustBeStoredInMySqlDB()
    {
      CommonUtil.WaitToProcess("2"); //Wait for the data to get persisted in DB

      string query = AssetServiceMySqlQueries.AssetDeletedDetailsByAssetUID + assetServiceSupport.DeleteAssetModel.AssetUID.ToString().Replace("-", "") + "'";
      List<string> columnList = new List<string>() { "StatusInd" };
      List<string> assetDetails = new List<string>();
      assetDetails.Add("0");
      MySqlUtil.ValidateMySQLQuery(MySqlConnectionString, query, assetDetails);
    }

    [When(@"I Set Invalid AssetServiceCreate AssetUID To Duplicate AssetUID")]
    public void WhenISetInvalidAssetServiceCreateAssetUIDToDuplicateAssetUID()
    {
      string assetUid = MySqlUtil.ExecuteMySQLQueryResult(MySqlConnectionString, AssetServiceMySqlQueries.AssetDetails);
      assetServiceSupport.InValidCreateAssetModel.AssetUID = assetUid;
    }

    #endregion

    #region Helpers
    public static CreateAssetEvent defaultValidAssetServiceCreateModel = new CreateAssetEvent();

    public static CreateAssetEvent GetDefaultValidAssetServiceCreateRequest(string assetType = null, string makecode = null,
                                                                            string model = null, string serialNumber = null)
    {
      defaultValidAssetServiceCreateModel.AssetUID = Guid.NewGuid();
      defaultValidAssetServiceCreateModel.LegacyAssetID = assetServiceSupport.RandomLongNumber();
      defaultValidAssetServiceCreateModel.AssetName = "AutoTestAPICreateAssetName" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");

      if (serialNumber == string.Empty || serialNumber == null)
        defaultValidAssetServiceCreateModel.SerialNumber = "AutoTestAPICreateAssetSerial" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      else
        defaultValidAssetServiceCreateModel.SerialNumber = serialNumber;

      if (makecode == string.Empty || makecode == null)
        defaultValidAssetServiceCreateModel.MakeCode = "CAT";
      else
        defaultValidAssetServiceCreateModel.MakeCode = makecode;

      if (model == string.Empty || model == null)
        defaultValidAssetServiceCreateModel.Model = "AutoTestAPICreateAssetModel" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      else
        defaultValidAssetServiceCreateModel.Model = model;


      if (assetType == string.Empty || assetType == null)
        defaultValidAssetServiceCreateModel.AssetType = "WHEEL LOADERS";
      else
        defaultValidAssetServiceCreateModel.AssetType = assetType;

      defaultValidAssetServiceCreateModel.IconKey = assetServiceSupport.RandomNumber();
      defaultValidAssetServiceCreateModel.EquipmentVIN = "AutoTestAPICreateAssetSerial" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultValidAssetServiceCreateModel.ModelYear = assetServiceSupport.RandomNumber();
      defaultValidAssetServiceCreateModel.ActionUTC = DateTime.UtcNow;
      defaultValidAssetServiceCreateModel.OwningCustomerUID = Guid.NewGuid();
      defaultValidAssetServiceCreateModel.ReceivedUTC = null;
      defaultValidAssetServiceCreateModel.ObjectType = AssetServiceSupport.RandomString(10);
      defaultValidAssetServiceCreateModel.Category = AssetServiceSupport.RandomString(10);
      //defaultValidAssetServiceCreateModel.Project = AssetServiceSupport.RandomString(10);
      defaultValidAssetServiceCreateModel.ProjectStatus = AssetServiceSupport.RandomString(10);
      defaultValidAssetServiceCreateModel.SortField = AssetServiceSupport.RandomString(10);
      defaultValidAssetServiceCreateModel.Source = AssetServiceSupport.RandomString(10);
      defaultValidAssetServiceCreateModel.UserEnteredRuntimeHours = AssetServiceSupport.RandomString(10);
      defaultValidAssetServiceCreateModel.Classification = AssetServiceSupport.RandomString(10);

      return defaultValidAssetServiceCreateModel;
    }



    public static UpdateAssetEvent GetDefaultValidAssetServiceUpdateRequest()
    {
      UpdateAssetEvent defaultValidAssetServiceUpdateModel = new UpdateAssetEvent();
      defaultValidAssetServiceUpdateModel.AssetUID = defaultValidAssetServiceCreateModel.AssetUID;
      defaultValidAssetServiceUpdateModel.LegacyAssetID = assetServiceSupport.RandomLongNumber();
      defaultValidAssetServiceUpdateModel.AssetName = "AutoTestAPIUpdateAssetName" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultValidAssetServiceUpdateModel.Model = "A60";
      defaultValidAssetServiceUpdateModel.AssetType = "GENSET";
      defaultValidAssetServiceUpdateModel.IconKey = assetServiceSupport.RandomNumber(); ;
      defaultValidAssetServiceUpdateModel.EquipmentVIN = "AutoTestAPIUpdateEquipmentVIN" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultValidAssetServiceUpdateModel.ModelYear = assetServiceSupport.RandomNumber();
      defaultValidAssetServiceUpdateModel.ActionUTC = DateTime.UtcNow;
      defaultValidAssetServiceUpdateModel.OwningCustomerUID = defaultValidAssetServiceCreateModel.OwningCustomerUID;
      defaultValidAssetServiceUpdateModel.ReceivedUTC = null;
      defaultValidAssetServiceUpdateModel.ObjectType = AssetServiceSupport.RandomString(10);
      defaultValidAssetServiceUpdateModel.Category = AssetServiceSupport.RandomString(10);
      //defaultValidAssetServiceUpdateModel.Project = AssetServiceSupport.RandomString(10);
      defaultValidAssetServiceUpdateModel.ProjectStatus = AssetServiceSupport.RandomString(10);
      defaultValidAssetServiceUpdateModel.SortField = AssetServiceSupport.RandomString(10);
      defaultValidAssetServiceUpdateModel.Source = AssetServiceSupport.RandomString(10);
      defaultValidAssetServiceUpdateModel.UserEnteredRuntimeHours = AssetServiceSupport.RandomString(10);
      defaultValidAssetServiceUpdateModel.Classification = AssetServiceSupport.RandomString(10);
      return defaultValidAssetServiceUpdateModel;
    }

    public static DeleteAssetEvent GetDefaultValidAssetServiceDeleteRequest()
    {
      DeleteAssetEvent defaultValidAssetServiceDeleteModel = new DeleteAssetEvent();
      defaultValidAssetServiceDeleteModel.AssetUID = defaultValidAssetServiceCreateModel.AssetUID;
      defaultValidAssetServiceDeleteModel.ActionUTC = DateTime.UtcNow;
      defaultValidAssetServiceDeleteModel.ReceivedUTC = null;
      return defaultValidAssetServiceDeleteModel;
    }

    public static InValidCreateAssetEvent GetDefaultInValidAssetServiceCreateRequest()
    {
      InValidCreateAssetEvent defaultInValidAssetServiceCreateModel = new InValidCreateAssetEvent();
      defaultInValidAssetServiceCreateModel.AssetUID = Guid.NewGuid().ToString();
      defaultInValidAssetServiceCreateModel.LegacyAssetID = assetServiceSupport.RandomLongNumber().ToString();
      defaultInValidAssetServiceCreateModel.AssetName = "AutoTestAPICreateAssetName" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultInValidAssetServiceCreateModel.SerialNumber = "AutoTestAPICreateAssetSerial" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultInValidAssetServiceCreateModel.MakeCode = "AutoTestAPICreateAssetSerial" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultInValidAssetServiceCreateModel.Model = "AutoTestAPICreateAssetSerial" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultInValidAssetServiceCreateModel.AssetType = "WHEEL LOADERS";
      defaultInValidAssetServiceCreateModel.IconKey = assetServiceSupport.RandomNumber().ToString();
      defaultInValidAssetServiceCreateModel.EquipmentVIN = "AutoTestAPICreateAssetSerial" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultInValidAssetServiceCreateModel.ModelYear = assetServiceSupport.RandomNumber().ToString();
      defaultInValidAssetServiceCreateModel.OwningCustomerUID = "AutoTestAPICreateAssetOwningCustomer" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultInValidAssetServiceCreateModel.ActionUTC = DateTime.UtcNow.ToString();
      defaultInValidAssetServiceCreateModel.ReceivedUTC = null;
      return defaultInValidAssetServiceCreateModel;
    }

    public static InValidUpdateAssetEvent GetDefaultInValidAssetServiceUpdateRequest()
    {
      InValidUpdateAssetEvent defaultInValidAssetServiceUpdateModel = new InValidUpdateAssetEvent();
      defaultInValidAssetServiceUpdateModel.AssetUID = Guid.NewGuid().ToString();
      defaultInValidAssetServiceUpdateModel.LegacyAssetID = assetServiceSupport.RandomLongNumber().ToString();
      defaultInValidAssetServiceUpdateModel.AssetName = "AutoTestAPIUpdateAssetName" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultInValidAssetServiceUpdateModel.Model = "A60";
      defaultInValidAssetServiceUpdateModel.AssetType = "GENSET";
      defaultInValidAssetServiceUpdateModel.IconKey = assetServiceSupport.RandomNumber().ToString();
      defaultInValidAssetServiceUpdateModel.EquipmentVIN = "AutoTestAPIUpdateEquipmentVIN" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultInValidAssetServiceUpdateModel.ModelYear = assetServiceSupport.RandomNumber().ToString();
      defaultInValidAssetServiceUpdateModel.OwningCustomerUID = "AutoTestAPIUpdateAssetOwningCustomer" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultInValidAssetServiceUpdateModel.ActionUTC = DateTime.UtcNow.ToString();
      defaultInValidAssetServiceUpdateModel.ReceivedUTC = null;
      return defaultInValidAssetServiceUpdateModel;
    }

    public static InValidDeleteAssetEvent GetDefaultInValidAssetServiceDeleteRequest()
    {
      InValidDeleteAssetEvent defaultInValidAssetServiceDeleteModel = new InValidDeleteAssetEvent();
      defaultInValidAssetServiceDeleteModel.AssetUID = Guid.NewGuid().ToString();
      defaultInValidAssetServiceDeleteModel.ActionUTC = DateTime.UtcNow.ToString();
      defaultInValidAssetServiceDeleteModel.ReceivedUTC = null;
      return defaultInValidAssetServiceDeleteModel;
    }
    #endregion

  }
}