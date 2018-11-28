using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace ExecutorTests
{
    [TestClass]
  public class ProjectSettingsTests : ExecutorTestsBase
  {
    [TestMethod]
    [DataRow(ProjectSettingsType.Targets)]
    [DataRow(ProjectSettingsType.ImportedFiles)]
    public void GetProjectSettingsExecutor_InvalidProjectUid(ProjectSettingsType settingsType)
    {
      string projectUid = string.Empty;
      string settings = string.Empty;
      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, settings, settingsType);
      var ex = Assert.ThrowsException<ServiceException>(() => projectSettingsRequest.Validate());
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("2005", StringComparison.Ordinal), "executor threw exception but incorrect code");
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("Missing ProjectUID.", StringComparison.Ordinal), "executor threw exception but incorrect messaage");
    }

    [TestMethod]
    [DataRow(ProjectSettingsType.Targets)]
    //[DataRow(ProjectSettingsType.ImportedFiles)]
    [DataRow(ProjectSettingsType.Colors)]
    public async Task GetProjectSettingsExecutor_InvalidCustomerProjectRelationship(ProjectSettingsType settingsType)
    {
      string customerUidOfProject = Guid.NewGuid().ToString();
      string customerUidSomeOther = Guid.NewGuid().ToString();
      string userId = Guid.NewGuid().ToString();
      string userEmailAddress = "whatever@here.there.com";
      string projectUid = Guid.NewGuid().ToString();
      string settings = string.Empty;
      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, settings, settingsType);

      var isCreatedOk = CreateCustomerProject(customerUidOfProject, projectUid);
      Assert.IsTrue(isCreatedOk, "unable to create project for Customer");

      var executor =
        RequestExecutorContainerFactory.Build<GetProjectSettingsExecutor>
        (logger, configStore, serviceExceptionHandler,
          customerUidSomeOther, userId, userEmailAddress, CustomHeaders(customerUidOfProject),
          null, null, 
          null, null, null, null, null,
          projectRepo);
      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(projectSettingsRequest)).ConfigureAwait(false);
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("2001", StringComparison.Ordinal), "executor threw exception but incorrect code");
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("No access to the project for a customer or the project does not exist.", StringComparison.Ordinal), "executor threw exception but incorrect messaage");
    }

    [TestMethod]
    [DataRow(ProjectSettingsType.Targets)]
    //[DataRow(ProjectSettingsType.ImportedFiles)]
    [DataRow(ProjectSettingsType.Colors)]
    public async Task GetProjectSettingsExecutor_NoSettingsExists(ProjectSettingsType settingsType)
    {
      string customerUid = Guid.NewGuid().ToString();
      string userId = Guid.NewGuid().ToString();
      string userEmailAddress = "whatever@here.there.com";
      string projectUid = Guid.NewGuid().ToString();
      string settings = string.Empty;
      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, settings, settingsType);

      var isCreatedOk = CreateCustomerProject(customerUid, projectUid);
      Assert.IsTrue(isCreatedOk, "unable to create project for Customer");

      var executor =
        RequestExecutorContainerFactory.Build<GetProjectSettingsExecutor>
          ( logger, configStore, serviceExceptionHandler,
            customerUid, userId, userEmailAddress, CustomHeaders(customerUid),
            null, null, 
            null, null, null, null, null,
           projectRepo);
      var result = await executor.ProcessAsync(projectSettingsRequest) as ProjectSettingsResult;
      
      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(projectUid, result.projectUid, "executor returned incorrect ProjectUid");
      Assert.IsNull(result.settings, "executor should have returned null Settings");
      Assert.AreEqual(settingsType, result.projectSettingsType, $"executor should have returned {settingsType} SettingsType");
    }

    [TestMethod]
    [DataRow(ProjectSettingsType.Targets)]
    //[DataRow(ProjectSettingsType.ImportedFiles)]
    [DataRow(ProjectSettingsType.Colors)]
    public async Task GetProjectSettingsExecutor_Exists(ProjectSettingsType settingsType)
    {
      string customerUid = Guid.NewGuid().ToString();
      string userId = Guid.NewGuid().ToString();
      string userEmailAddress = "whatever@here.there.com";
      string projectUid = Guid.NewGuid().ToString();
      string settings = @"{firstValue: 10, lastValue: 20}";//"blah";

      var isCreatedOk = CreateCustomerProject(customerUid, projectUid);
      Assert.IsTrue(isCreatedOk, "unable to create project for Customer");

      isCreatedOk = CreateProjectSettings(projectUid, userId, settings, settingsType);
      Assert.IsTrue(isCreatedOk, "created projectSettings");
      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, settings, settingsType);

      var executor =
        RequestExecutorContainerFactory.Build<GetProjectSettingsExecutor>
        (logger, configStore, serviceExceptionHandler,
          customerUid, userId, userEmailAddress, CustomHeaders(customerUid),
          null, null, 
          null, null, null, null, null,
          projectRepo);
      var result = await executor.ProcessAsync(projectSettingsRequest) as ProjectSettingsResult;
      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(projectUid, result.projectUid, "executor returned incorrect ProjectUid");
      Assert.AreEqual(settingsType, result.projectSettingsType, "executor returned incorrect projectSettingsType");

      if (settingsType == ProjectSettingsType.Targets || settingsType == ProjectSettingsType.Colors)
      {
        var tempSettings = JsonConvert.DeserializeObject<JObject>(settings);

        Assert.AreEqual(tempSettings["firstValue"], result.settings["firstValue"],
          "executor returned incorrect firstValue of settings");
        Assert.AreEqual(tempSettings["lastValue"], result.settings["lastValue"],
          "executor should have returned lastValue of settings");
      }
      else
      {
        var tempObj = JsonConvert.DeserializeObject<JArray>(settings);
        var tempJObject = new JObject { ["importedFiles"] = tempObj };

        Assert.AreEqual(tempJObject["importedFiles"][0]["firstValue"], result.settings["importedFiles"][0]["firstValue"], "executor returned incorrect firstValue of the first object of the settings");
        Assert.AreEqual(tempJObject["importedFiles"][0]["lastValue"], result.settings["importedFiles"][0]["lastValue"], "executor returned incorrect lastValue of the first object of the settings");
        Assert.AreEqual(tempJObject["importedFiles"][1]["firstValue"], result.settings["importedFiles"][1]["firstValue"], "executor returned incorrect firstValue of the last object of the settings");
        Assert.AreEqual(tempJObject["importedFiles"][1]["lastValue"], result.settings["importedFiles"][1]["lastValue"], "executor returned incorrect lastValue of the last object of the settings");
      }
    }

    [TestMethod]
    [DataRow(ProjectSettingsType.Targets)]
    [DataRow(ProjectSettingsType.ImportedFiles)]
    public void UpsertProjectSettingsExecutor_InvalidProjectSettings(ProjectSettingsType settingsType)
    {
      string projectUid = Guid.NewGuid().ToString();
      string settings = null;
      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, settings, settingsType);
      var ex = Assert.ThrowsException<ServiceException>(() => projectSettingsRequest.Validate());
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("2073", StringComparison.Ordinal), "executor threw exception but incorrect code");
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("ProjectSettings cannot be null.", StringComparison.Ordinal), "executor threw exception but incorrect messaage");
    }

    [TestMethod]
    [DataRow(ProjectSettingsType.Targets)]
    [DataRow(ProjectSettingsType.ImportedFiles)]
    public async Task UpsertProjectSettingsExecutor_InvalidCustomerProjectRelationship(ProjectSettingsType settingsType)
    {
      string customerUidOfProject = Guid.NewGuid().ToString();
      string customerUidSomeOther = Guid.NewGuid().ToString();
      string userId = Guid.NewGuid().ToString();
      string userEmailAddress = "whatever@here.there.com";
      var projectUid = Guid.NewGuid();
      string settings = "blah";
      string settingsUpdated = "blah Is Updated";

      var isCreatedOk = CreateCustomerProject(customerUidOfProject, projectUid.ToString());
      Assert.IsTrue(isCreatedOk, "unable to create project for Customer");

      isCreatedOk = CreateProjectSettings(projectUid.ToString(), userId, settings, settingsType);
      Assert.IsTrue(isCreatedOk, "created projectSettings");

      var projectSettingsRequest =
        ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid.ToString(), settingsUpdated, settingsType);

      var executor = RequestExecutorContainerFactory.Build<UpsertProjectSettingsExecutor>
      (logger, configStore, serviceExceptionHandler,
        customerUidSomeOther, userId, userEmailAddress, CustomHeaders(customerUidOfProject),
        producer, kafkaTopicName,
        raptorProxy, null, null, null, null,
        projectRepo);
      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(projectSettingsRequest)).ConfigureAwait(false);
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("2001", StringComparison.Ordinal), "executor threw exception but incorrect code");
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("No access to the project for a customer or the project does not exist.", StringComparison.Ordinal), "executor threw exception but incorrect messaage");
    }

    [TestMethod]
    [DataRow(ProjectSettingsType.Targets)]
    [DataRow(ProjectSettingsType.ImportedFiles)]
    [DataRow(ProjectSettingsType.Colors)]
    public async Task UpsertProjectSettingsExecutor_NoProjectSettingsExists(ProjectSettingsType settingsType)
    {
      string customerUid = Guid.NewGuid().ToString();
      string userId = Guid.NewGuid().ToString();
      string userEmailAddress = "whatever@here.there.com";
      var projectUid = Guid.NewGuid();
      string settings = settingsType != ProjectSettingsType.ImportedFiles ? @"{firstValue: 10, lastValue: 20}" : @"[{firstValue: 10, lastValue: 20}, {firstValue: 20, lastValue: 40}]";//"blah";
      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid.ToString(), settings, settingsType);

      var isCreatedOk = CreateCustomerProject(customerUid, projectUid.ToString());
      Assert.IsTrue(isCreatedOk, "unable to create project for Customer");
      
      var executor = RequestExecutorContainerFactory.Build<UpsertProjectSettingsExecutor>
        (logger, configStore, serviceExceptionHandler,
        customerUid, userId, userEmailAddress, CustomHeaders(customerUid),
        producer, kafkaTopicName,
        raptorProxy, null, null, null, null,
        projectRepo);
      var result = await executor.ProcessAsync(projectSettingsRequest) as ProjectSettingsResult;
      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(projectUid.ToString(), result.projectUid, "executor returned incorrect ProjectUid");
      Assert.AreEqual(settingsType, result.projectSettingsType, "executor returned incorrect projectSettingsType");

      if (settingsType == ProjectSettingsType.Targets || settingsType == ProjectSettingsType.Colors)
      {
        var tempSettings = JsonConvert.DeserializeObject<JObject>(settings);

        Assert.AreEqual(tempSettings["firstValue"], result.settings["firstValue"],
          "executor returned incorrect firstValue of settings");
        Assert.AreEqual(tempSettings["lastValue"], result.settings["lastValue"],
          "executor should have returned lastValue of settings");
      }
      else
      {
        var tempObj = JsonConvert.DeserializeObject<JArray>(settings);
        var tempJObject = new JObject { ["importedFiles"] = tempObj };

        Assert.AreEqual(tempJObject["importedFiles"][0]["firstValue"], result.settings["importedFiles"][0]["firstValue"], "executor returned incorrect firstValue of the first object of the settings");
        Assert.AreEqual(tempJObject["importedFiles"][0]["lastValue"], result.settings["importedFiles"][0]["lastValue"], "executor returned incorrect lastValue of the first object of the settings");
        Assert.AreEqual(tempJObject["importedFiles"][1]["firstValue"], result.settings["importedFiles"][1]["firstValue"], "executor returned incorrect firstValue of the last object of the settings");
        Assert.AreEqual(tempJObject["importedFiles"][1]["lastValue"], result.settings["importedFiles"][1]["lastValue"], "executor returned incorrect lastValue of the last object of the settings");
      }
    }

    [TestMethod]
    [DataRow(ProjectSettingsType.Targets)]
    [DataRow(ProjectSettingsType.ImportedFiles)]
    [DataRow(ProjectSettingsType.Colors)]
    public async Task UpsertProjectSettingsExecutor_Exists(ProjectSettingsType settingsType)
    {
      string customerUid = Guid.NewGuid().ToString();
      string userId = Guid.NewGuid().ToString();
      string userEmailAddress = "whatever@here.there.com";
      var projectUid = Guid.NewGuid();
      string settings = "blah";
      //string settingsUpdated = "blah Is Updated";
      string settingsUpdated = settingsType != ProjectSettingsType.ImportedFiles ? @"{firstValue: 10, lastValue: 20}" : @"[{firstValue: 10, lastValue: 20}, {firstValue: 20, lastValue: 40}]";

      var isCreatedOk = CreateCustomerProject(customerUid, projectUid.ToString());
      Assert.IsTrue(isCreatedOk, "unable to create project for Customer");

      isCreatedOk = CreateProjectSettings(projectUid.ToString(), userId, settings, settingsType);
      Assert.IsTrue(isCreatedOk, "created projectSettings");

      var projectSettingsRequest =
        ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid.ToString(), settingsUpdated, settingsType);

      var executor = RequestExecutorContainerFactory.Build<UpsertProjectSettingsExecutor>
      (logger, configStore, serviceExceptionHandler,
        customerUid, userId, userEmailAddress, CustomHeaders(customerUid),
        producer, kafkaTopicName,
        raptorProxy, null, null, null, null,
        projectRepo);
      var result = await executor.ProcessAsync(projectSettingsRequest) as ProjectSettingsResult;
      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(projectUid.ToString(), result.projectUid, "executor returned incorrect ProjectUid");
      Assert.AreEqual(settingsType, result.projectSettingsType, "executor returned incorrect projectSettingsType");

      if (settingsType == ProjectSettingsType.Targets || settingsType == ProjectSettingsType.Colors)
      {
        var tempSettings = JsonConvert.DeserializeObject<JObject>(settingsUpdated);

        Assert.AreEqual(tempSettings["firstValue"], result.settings["firstValue"],
          "executor returned incorrect firstValue of settings");
        Assert.AreEqual(tempSettings["lastValue"], result.settings["lastValue"],
          "executor should have returned lastValue of settings");
      }
      else
      {
        var tempObj = JsonConvert.DeserializeObject<JArray>(settingsUpdated);
        var tempJObject = new JObject { ["importedFiles"] = tempObj };

        Assert.AreEqual(tempJObject["importedFiles"][0]["firstValue"], result.settings["importedFiles"][0]["firstValue"], "executor returned incorrect firstValue of the first object of the settings");
        Assert.AreEqual(tempJObject["importedFiles"][0]["lastValue"], result.settings["importedFiles"][0]["lastValue"], "executor returned incorrect lastValue of the first object of the settings");
        Assert.AreEqual(tempJObject["importedFiles"][1]["firstValue"], result.settings["importedFiles"][1]["firstValue"], "executor returned incorrect firstValue of the last object of the settings");
        Assert.AreEqual(tempJObject["importedFiles"][1]["lastValue"], result.settings["importedFiles"][1]["lastValue"], "executor returned incorrect lastValue of the last object of the settings");
      }
    }
  }
}