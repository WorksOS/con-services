using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.ProjectTests
{
  [TestClass]
  public class ProjectGeofenceExecutorTests : ExecutorBaseTests
  {
    protected ProjectErrorCodesProvider _projectErrorCodesProvider = new ProjectErrorCodesProvider();

    private static string _validBoundary;

    public ProjectGeofenceExecutorTests()
    {
      _validBoundary =
        "POLYGON((172.595831670724 -43.5427038560109,172.594630041089 -43.5438859356773,172.59329966542 -43.542486101965, 172.595831670724 -43.5427038560109))";
    }

    [ClassInitialize]
    public static void ClassInitialize(TestContext testContext)
    {
      AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();
    }

    [TestMethod]
    public async Task ProjectGeofence_HappyPath_OneNewGeofenceAssociation()
    {
      var customerUid = Guid.NewGuid().ToString();
      var projectUid = Guid.NewGuid().ToString();
      var testGeofencesForCustomer = CreateGeofenceWithAssociations(customerUid, projectUid);

      var projectRepo = new Mock<IProjectRepository>();
      var project = new Repositories.DBModels.Project()
      {
        CustomerUID = customerUid,
        ProjectUID = projectUid,
        ProjectType = ProjectType.LandFill
      };
      var projectList = new List<Repositories.DBModels.Project>();
      projectList.Add(project);
      projectRepo.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<string>())).ReturnsAsync(projectList);
      projectRepo.Setup(gr => gr.GetCustomerGeofences(It.IsAny<string>()))
        .ReturnsAsync(testGeofencesForCustomer);
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<AssociateProjectGeofence>())).ReturnsAsync(1);

      var geofenceTypes = new List<GeofenceType>() {GeofenceType.Landfill};

      // 0= not associated 2= associated to this project
      var geofences = new List<Guid>()
      {
        Guid.Parse(testGeofencesForCustomer[0].GeofenceUID),
        Guid.Parse(testGeofencesForCustomer[2].GeofenceUID)
      };
      var request =
        UpdateProjectGeofenceRequest.CreateUpdateProjectGeofenceRequest(Guid.Parse(projectUid), geofenceTypes,
          geofences);
      request.Validate();

      var configStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
      var producer = new Mock<IKafka>();
      producer.Setup(p => p.InitProducer(It.IsAny<IConfigurationStore>()));
      producer.Setup(p => p.Send(It.IsAny<string>(), It.IsAny<List<KeyValuePair<string, string>>>()));

      var executor = RequestExecutorContainerFactory.Build<UpdateProjectGeofenceExecutor>
      (logger, configStore, serviceExceptionHandler,
        customerUid, null, null, null,
        producer.Object, KafkaTopicName,
        null, null, null, 
        projectRepo.Object);
      await executor.ProcessAsync(request);
    }

    [TestMethod]
    public async Task ProjectGeofence_Error_InvalidProjectType()
    {
      var customerUid = Guid.NewGuid().ToString();
      var projectUid = Guid.NewGuid().ToString();
      var testGeofencesForCustomer = CreateGeofenceWithAssociations(customerUid, projectUid);

      var projectRepo = new Mock<IProjectRepository>();
      var project = new Repositories.DBModels.Project()
      {
        CustomerUID = customerUid,
        ProjectUID = projectUid,
        ProjectType = ProjectType.Standard
      };
      var projectList = new List<Repositories.DBModels.Project>();
      projectList.Add(project);
      projectRepo.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<string>())).ReturnsAsync(projectList);
      projectRepo.Setup(gr => gr.GetCustomerGeofences(It.IsAny<string>()))
        .ReturnsAsync(testGeofencesForCustomer);
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<AssociateProjectGeofence>())).ReturnsAsync(1);

      var geofenceTypes = new List<GeofenceType>() {GeofenceType.Landfill};

      // 0= not associated 2= associated to this project
      var geofences = new List<Guid>()
      {
        Guid.Parse(testGeofencesForCustomer[0].GeofenceUID),
        Guid.Parse(testGeofencesForCustomer[2].GeofenceUID)
      };
      var request =
        UpdateProjectGeofenceRequest.CreateUpdateProjectGeofenceRequest(Guid.Parse(projectUid), geofenceTypes,
          geofences);
      request.Validate();

      var configStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
      var producer = new Mock<IKafka>();
      producer.Setup(p => p.InitProducer(It.IsAny<IConfigurationStore>()));
      producer.Setup(p => p.Send(It.IsAny<string>(), It.IsAny<List<KeyValuePair<string, string>>>()));

      var executor = RequestExecutorContainerFactory.Build<UpdateProjectGeofenceExecutor>
      (logger, configStore, serviceExceptionHandler,
        customerUid, null, null, null,
        producer.Object, KafkaTopicName,
        null, null, null, 
        projectRepo.Object);
      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(async () =>
        await executor.ProcessAsync(request));

      Assert.AreNotEqual(-1,
        ex.GetContent.IndexOf(_projectErrorCodesProvider.FirstNameWithOffset(102), StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task ProjectGeofence_Error_ExistingAssociationMissingFromList()
    {
      var customerUid = Guid.NewGuid().ToString();
      var projectUid = Guid.NewGuid().ToString();
      var testGeofencesForCustomer = CreateGeofenceWithAssociations(customerUid, projectUid);

      var projectRepo = new Mock<IProjectRepository>();
      var project = new Repositories.DBModels.Project()
      {
        CustomerUID = customerUid,
        ProjectUID = projectUid,
        ProjectType = ProjectType.LandFill
      };
      var projectList = new List<Repositories.DBModels.Project>();
      projectList.Add(project);
      projectRepo.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<string>())).ReturnsAsync(projectList);
      projectRepo.Setup(gr => gr.GetCustomerGeofences(It.IsAny<string>()))
        .ReturnsAsync(testGeofencesForCustomer);
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<AssociateProjectGeofence>())).ReturnsAsync(1);

      var geofenceTypes = new List<GeofenceType>() {GeofenceType.Landfill};

      // 0= not associated 2= associated to this project
      var geofences = new List<Guid>() {Guid.Parse(testGeofencesForCustomer[0].GeofenceUID)};
      var request =
        UpdateProjectGeofenceRequest.CreateUpdateProjectGeofenceRequest(Guid.Parse(projectUid), geofenceTypes,
          geofences);
      request.Validate();

      var configStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
      var producer = new Mock<IKafka>();
      producer.Setup(p => p.InitProducer(It.IsAny<IConfigurationStore>()));
      producer.Setup(p => p.Send(It.IsAny<string>(), It.IsAny<List<KeyValuePair<string, string>>>()));

      var executor = RequestExecutorContainerFactory.Build<UpdateProjectGeofenceExecutor>
      (logger, configStore, serviceExceptionHandler,
        customerUid, null, null, null,
        producer.Object, KafkaTopicName,
        null, null, null, 
        projectRepo.Object);
      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(async () =>
        await executor.ProcessAsync(request));

      Assert.AreNotEqual(-1,
        ex.GetContent.IndexOf(_projectErrorCodesProvider.FirstNameWithOffset(107), StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task ProjectGeofence_Error_NoNewAssociationsToAdd()
    {
      var customerUid = Guid.NewGuid().ToString();
      var projectUid = Guid.NewGuid().ToString();
      var testGeofencesForCustomer = CreateGeofenceWithAssociations(customerUid, projectUid);

      var projectRepo = new Mock<IProjectRepository>();
      var project = new Repositories.DBModels.Project()
      {
        CustomerUID = customerUid,
        ProjectUID = projectUid,
        ProjectType = ProjectType.LandFill
      };
      var projectList = new List<Repositories.DBModels.Project>();
      projectList.Add(project);
      projectRepo.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<string>())).ReturnsAsync(projectList);
      projectRepo.Setup(gr => gr.GetCustomerGeofences(It.IsAny<string>()))
        .ReturnsAsync(testGeofencesForCustomer);
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<AssociateProjectGeofence>())).ReturnsAsync(1);

      var geofenceTypes = new List<GeofenceType>() {GeofenceType.Landfill};

      // 0= not associated 2= associated to this project
      var geofences = new List<Guid>() {Guid.Parse(testGeofencesForCustomer[2].GeofenceUID)};
      var request =
        UpdateProjectGeofenceRequest.CreateUpdateProjectGeofenceRequest(Guid.Parse(projectUid), geofenceTypes,
          geofences);
      request.Validate();

      var configStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
      var producer = new Mock<IKafka>();
      producer.Setup(p => p.InitProducer(It.IsAny<IConfigurationStore>()));
      producer.Setup(p => p.Send(It.IsAny<string>(), It.IsAny<List<KeyValuePair<string, string>>>()));

      var executor = RequestExecutorContainerFactory.Build<UpdateProjectGeofenceExecutor>
      (logger, configStore, serviceExceptionHandler,
        customerUid, null, null, null,
        producer.Object, KafkaTopicName,
        null, null, null, 
        projectRepo.Object);
      await executor.ProcessAsync(request);
    }

    [TestMethod]
    public async Task ProjectGeofence_Error_ProjectNotFound()
    {
      var customerUid = Guid.NewGuid().ToString();
      var projectUid = Guid.NewGuid().ToString();
      var testGeofencesForCustomer = CreateGeofenceWithAssociations(customerUid, projectUid);

      var projectRepo = new Mock<IProjectRepository>();
      var projectList = new List<Repositories.DBModels.Project>();
      projectRepo.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<string>())).ReturnsAsync(projectList);
      projectRepo.Setup(gr => gr.GetCustomerGeofences(It.IsAny<string>()))
        .ReturnsAsync(testGeofencesForCustomer);
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<AssociateProjectGeofence>())).ReturnsAsync(1);

      var geofenceTypes = new List<GeofenceType>() {GeofenceType.Landfill};

      // 0= not associated 2= associated to this project
      var geofences = new List<Guid>()
      {
        Guid.Parse(testGeofencesForCustomer[0].GeofenceUID),
        Guid.Parse(testGeofencesForCustomer[2].GeofenceUID)
      };
      var request =
        UpdateProjectGeofenceRequest.CreateUpdateProjectGeofenceRequest(Guid.Parse(projectUid), geofenceTypes,
          geofences);
      request.Validate();

      var configStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
      var producer = new Mock<IKafka>();
      producer.Setup(p => p.InitProducer(It.IsAny<IConfigurationStore>()));
      producer.Setup(p => p.Send(It.IsAny<string>(), It.IsAny<List<KeyValuePair<string, string>>>()));

      var executor = RequestExecutorContainerFactory.Build<UpdateProjectGeofenceExecutor>
      (logger, configStore, serviceExceptionHandler,
        customerUid, null, null, null,
        producer.Object, KafkaTopicName,
        null, null, null, 
        projectRepo.Object);
      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(async () =>
        await executor.ProcessAsync(request));

      Assert.AreNotEqual(-1,
        ex.GetContent.IndexOf(_projectErrorCodesProvider.FirstNameWithOffset(1), StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task ProjectGeofence_Error_GeofenceUidNotInDatabaseForCustomer()
    {
      var customerUid = Guid.NewGuid().ToString();
      var projectUid = Guid.NewGuid().ToString();
      var testGeofencesForCustomer = CreateGeofenceWithAssociations(customerUid, projectUid);

      var projectRepo = new Mock<IProjectRepository>();
      var project = new Repositories.DBModels.Project()
      {
        CustomerUID = customerUid,
        ProjectUID = projectUid,
        ProjectType = ProjectType.LandFill
      };
      var projectList = new List<Repositories.DBModels.Project>();
      projectList.Add(project);
      projectRepo.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<string>())).ReturnsAsync(projectList);
      projectRepo.Setup(gr => gr.GetCustomerGeofences(It.IsAny<string>()))
        .ReturnsAsync(testGeofencesForCustomer);
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<AssociateProjectGeofence>())).ReturnsAsync(1);

      var geofenceTypes = new List<GeofenceType>() {GeofenceType.Landfill};

      // 0= not associated 2= associated to this project
      var geofences = new List<Guid>() {Guid.NewGuid(), Guid.Parse(testGeofencesForCustomer[2].GeofenceUID)};
      var request =
        UpdateProjectGeofenceRequest.CreateUpdateProjectGeofenceRequest(Guid.Parse(projectUid), geofenceTypes,
          geofences);
      request.Validate();

      var configStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
      var producer = new Mock<IKafka>();
      producer.Setup(p => p.InitProducer(It.IsAny<IConfigurationStore>()));
      producer.Setup(p => p.Send(It.IsAny<string>(), It.IsAny<List<KeyValuePair<string, string>>>()));

      var executor = RequestExecutorContainerFactory.Build<UpdateProjectGeofenceExecutor>
      (logger, configStore, serviceExceptionHandler,
        customerUid, null, null, null,
        producer.Object, KafkaTopicName,
        null, null, null, 
        projectRepo.Object);
      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(async () =>
        await executor.ProcessAsync(request));

      Assert.AreNotEqual(-1,
        ex.GetContent.IndexOf(_projectErrorCodesProvider.FirstNameWithOffset(104), StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task Get_UnassignedLandfillGeofencesAsync()
    {
      var log = ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<ProjectGeofenceValidationTests>();
      var customerUid = Guid.NewGuid().ToString();
      var projectUid = Guid.NewGuid().ToString();
      var testGeofencesForCustomer = CreateGeofenceWithAssociations(customerUid, projectUid);

      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(gr => gr.GetCustomerGeofences(It.IsAny<string>()))
        .ReturnsAsync(testGeofencesForCustomer);

      var geofenceTypes = new List<GeofenceType>() {GeofenceType.Landfill};

      var geofences = await ProjectRequestHelper
        .GetGeofenceList(customerUid, string.Empty, geofenceTypes, log, projectRepo.Object)
        .ConfigureAwait(false);

      Assert.AreEqual(1, geofences.Count, "Should be 1 landfill");

      Assert.AreEqual(testGeofencesForCustomer[0].GeofenceUID, geofences[0].GeofenceUID, "Unexpected GeofenceUid");
      Assert.AreEqual(testGeofencesForCustomer[0].Name, geofences[0].Name, "Unexpected project name");
      Assert.AreEqual(testGeofencesForCustomer[0].GeofenceType, geofences[0].GeofenceType, "Should be Landfill type");
      Assert.AreEqual(testGeofencesForCustomer[0].GeometryWKT, geofences[0].GeometryWKT, "Unexpected GeometryWKT");
      Assert.AreEqual(testGeofencesForCustomer[0].FillColor, geofences[0].FillColor, "Unexpected FillColor");
      Assert.AreEqual(testGeofencesForCustomer[0].IsTransparent, geofences[0].IsTransparent,
        "Unexpected IsTransparent");
      Assert.AreEqual(testGeofencesForCustomer[0].Description, geofences[0].Description, "Unexpected Description");
      Assert.AreEqual(testGeofencesForCustomer[0].CustomerUID, geofences[0].CustomerUID, "Unexpected CustomerUid");
      Assert.AreEqual(testGeofencesForCustomer[0].UserUID, geofences[0].UserUID, "Unexpected UserUid");
      Assert.AreEqual(testGeofencesForCustomer[0].AreaSqMeters, geofences[0].AreaSqMeters, "Unexpected AreaSqMeters");
    }

    [TestMethod]
    public async Task Get_AssignedLandfillGeofences_FromProject()
    {
      var log = ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<ProjectGeofenceValidationTests>();
      var customerUid = Guid.NewGuid().ToString();
      var projectUid = Guid.NewGuid().ToString();
      var testGeofencesForCustomer = CreateGeofenceWithAssociations(customerUid, projectUid);

      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(gr => gr.GetCustomerGeofences(It.IsAny<string>()))
        .ReturnsAsync(testGeofencesForCustomer);

      var geofenceTypes = new List<GeofenceType> {GeofenceType.Landfill};

      var geofences = await ProjectRequestHelper
        .GetGeofenceList(customerUid, projectUid, geofenceTypes, log, projectRepo.Object)
        .ConfigureAwait(false);

      Assert.AreEqual(1, geofences.Count, "Should be 1 landfills");

      Assert.AreEqual(testGeofencesForCustomer[2].GeofenceUID, geofences[0].GeofenceUID, "Unexpected GeofenceUid");
      Assert.AreEqual(testGeofencesForCustomer[2].Name, geofences[0].Name, "Unexpected project name");
      Assert.AreEqual(testGeofencesForCustomer[2].GeofenceType, geofences[0].GeofenceType, "Should be Landfill type");
      Assert.AreEqual(testGeofencesForCustomer[2].GeometryWKT, geofences[0].GeometryWKT, "Unexpected GeometryWKT");
      Assert.AreEqual(testGeofencesForCustomer[2].FillColor, geofences[0].FillColor, "Unexpected FillColor");
      Assert.AreEqual(testGeofencesForCustomer[2].IsTransparent, geofences[0].IsTransparent,
        "Unexpected IsTransparent");
      Assert.AreEqual(testGeofencesForCustomer[2].Description, geofences[0].Description, "Unexpected Description");
      Assert.AreEqual(testGeofencesForCustomer[2].CustomerUID, geofences[0].CustomerUID, "Unexpected CustomerUid");
      Assert.AreEqual(testGeofencesForCustomer[2].UserUID, geofences[0].UserUID, "Unexpected UserUid");
      Assert.AreEqual(testGeofencesForCustomer[2].AreaSqMeters, geofences[0].AreaSqMeters, "Unexpected AreaSqMeters");
    }

    #region private

    private List<GeofenceWithAssociation> CreateGeofenceWithAssociations(string customerUid, string projectUid)
    {
      var geofencesWithAssociation = new List<GeofenceWithAssociation>()
      {
        new GeofenceWithAssociation()
        {
          CustomerUID = customerUid,
          Name = "geofence Name",
          Description = "geofence Description",
          GeofenceType = GeofenceType.Landfill,
          GeometryWKT = _validBoundary,
          FillColor = 4555,
          IsTransparent = false,
          GeofenceUID = Guid.NewGuid().ToString(),
          UserUID = Guid.NewGuid().ToString(),
          AreaSqMeters = 12.45
        },
        new GeofenceWithAssociation()
        {
          CustomerUID = customerUid,
          Name = "geofence Name2",
          Description = "geofence Description2",
          GeofenceType = GeofenceType.Project,
          GeometryWKT = _validBoundary,
          FillColor = 4555,
          IsTransparent = false,
          GeofenceUID = Guid.NewGuid().ToString(),
          UserUID = Guid.NewGuid().ToString(),
          AreaSqMeters = 223.45
        },
        new GeofenceWithAssociation()
        {
          CustomerUID = customerUid,
          Name = "geofence Name3",
          Description = "geofence Description3",
          GeofenceType = GeofenceType.Landfill,
          GeometryWKT = _validBoundary,
          FillColor = 4555,
          IsTransparent = false,
          GeofenceUID = Guid.NewGuid().ToString(),
          UserUID = Guid.NewGuid().ToString(),
          AreaSqMeters = 43.45,
          ProjectUID = projectUid
        },
        new GeofenceWithAssociation()
        {
          CustomerUID = customerUid,
          Name = "geofence Name4",
          Description = "geofence Description4",
          GeofenceType = GeofenceType.CutZone,
          GeometryWKT = _validBoundary,
          FillColor = 4555,
          IsTransparent = false,
          GeofenceUID = Guid.NewGuid().ToString(),
          UserUID = Guid.NewGuid().ToString(),
          AreaSqMeters = 43.45
        }
      };
      return geofencesWithAssociation;
    }

    #endregion private
  }
}