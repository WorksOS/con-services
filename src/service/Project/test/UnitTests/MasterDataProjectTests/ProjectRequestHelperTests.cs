using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Serilog;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Serilog.Extensions;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;
using Xunit;

namespace VSS.MasterData.ProjectTests
{
  public class ProjectRequestHelperTests : UnitTestsDIFixture<ProjectRequestHelperTests>
  {
    /// <summary>
    /// cws Filename format is: "trn::profilex:us-west-2:project:5d2ab210-5fb4-4e77-90f9-b0b41c9e6e3f||2020-03-25 23:03:45.314||BootCamp 2012.dc",
    /// </summary>
    private readonly HeaderDictionary _customHeaders;
    private readonly Microsoft.Extensions.Logging.ILogger _logger;
    private readonly IServiceExceptionHandler _serviceExceptionHandler;

    public ProjectRequestHelperTests()
    {
      var loggerFactory = new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.Project.WebApi.log"));
      var serviceCollection = new ServiceCollection();

      serviceCollection.AddLogging();
      serviceCollection.AddSingleton(loggerFactory);
      serviceCollection
        .AddSingleton<IConfigurationStore, GenericConfiguration>()
        .AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>()
        .AddTransient<IErrorCodesProvider, ProjectErrorCodesProvider>();

      IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
      _serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();
      _logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<ProjectRequestHelperTests>();
      _serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();
      _customHeaders = new HeaderDictionary();
    }

    [Fact]
    public void ExtractCalibrationFileDetails_NullConfigurations_Failed()
    {
      List<ProjectConfigurationModel> projectConfigurations = null;
      var extractedCalibrationFileOk = ProjectRequestHelper.ExtractCalibrationFileDetails(projectConfigurations, out string fileName, out DateTime? fileDateUtc);
      Assert.False(extractedCalibrationFileOk);
    }

    [Fact]
    public void ExtractCalibrationFileDetails_NoConfiguration_Failed()
    {
      var projectConfigurations = new List<ProjectConfigurationModel>();
      var extractedCalibrationFileOk = ProjectRequestHelper.ExtractCalibrationFileDetails(projectConfigurations, out string fileName, out DateTime? fileDateUtc);
      Assert.False(extractedCalibrationFileOk);
    }

    [Fact]
    public void ExtractCalibrationFileDetails_NoCalibrationCnfiguration_Failed()
    {
      var projectConfigurations = new List<ProjectConfigurationModel>();
      projectConfigurations.Add(new ProjectConfigurationModel() { FileType = ProjectConfigurationFileType.AVOIDANCE_ZONE.ToString() });
      var extractedCalibrationFileOk = ProjectRequestHelper.ExtractCalibrationFileDetails(projectConfigurations, out string fileName, out DateTime? fileDateUtc);
      Assert.False(extractedCalibrationFileOk);
    }

    [Fact]
    public void ExtractCalibrationFileDetails_InvalidFullName_Failed()
    {
      var projectConfigurations = new List<ProjectConfigurationModel>();
      projectConfigurations.Add(new ProjectConfigurationModel() { FileType = ProjectConfigurationFileType.AVOIDANCE_ZONE.ToString() });

      var invalidFullName = "2020-03-25 23:03:45.314||BootCamp 2012.dc";
      projectConfigurations.Add(new ProjectConfigurationModel() { FileType = ProjectConfigurationFileType.CALIBRATION.ToString(), FileName = invalidFullName });
      var extractedCalibrationFileOk = ProjectRequestHelper.ExtractCalibrationFileDetails(projectConfigurations, out var fileName, out var fileDateUtc);
      Assert.False(extractedCalibrationFileOk);
    }

    [Fact]
    public void ExtractCalibrationFileDetails_InvalidFileExtension_Failed()
    {
      var projectConfigurations = new List<ProjectConfigurationModel>();
      projectConfigurations.Add(new ProjectConfigurationModel() { FileType = ProjectConfigurationFileType.AVOIDANCE_ZONE.ToString() });

      var invalidFullName = "trn::profilex:us-west-2:project:5d2ab210-5fb4-4e77-90f9-b0b41c9e6e3f||2020-03-25 23:03:45.314||BootCamp 2012.aa";
      projectConfigurations.Add(new ProjectConfigurationModel() { FileType = ProjectConfigurationFileType.CALIBRATION.ToString(), FileName = invalidFullName });
      var extractedCalibrationFileOk = ProjectRequestHelper.ExtractCalibrationFileDetails(projectConfigurations, out var fileName, out var fileDateUtc);
      Assert.False(extractedCalibrationFileOk);
    }

    [Fact]
    public void ExtractCalibrationFileDetails_HappyPath()
    {
      var projectConfigurations = new List<ProjectConfigurationModel>();
      projectConfigurations.Add(new ProjectConfigurationModel() { FileType = ProjectConfigurationFileType.AVOIDANCE_ZONE.ToString() });

      var fullName = "trn::profilex:us-west-2:project:5d2ab210-5fb4-4e77-90f9-b0b41c9e6e3f||2020-03-25 23:03:45.314||BootCamp 2012.dc";
      projectConfigurations.Add(new ProjectConfigurationModel() { FileType = ProjectConfigurationFileType.CALIBRATION.ToString(), FileName = fullName });
      var extractedCalibrationFileOk = ProjectRequestHelper.ExtractCalibrationFileDetails(projectConfigurations, out var fileName, out var fileDateUtc);
      Assert.True(extractedCalibrationFileOk);
      Assert.Equal("BootCamp 2012.dc", fileName);
      Assert.Equal(new DateTime(2020, 03, 25, 23, 03, 45, 314), fileDateUtc);
    }

    [Fact]
    public void GetProjectListForCustomer_HappyPath()
    {
      var customerUid = Guid.NewGuid().ToString();
      var projectUid = Guid.NewGuid().ToString();
      var projectName = "the project name";
      var lastUpdate = DateTime.UtcNow.AddDays(-1);

      var projectConfigurations = new List<ProjectConfigurationModel>();
      var fullName = "trn::profilex:us-west-2:project:5d2ab210-5fb4-4e77-90f9-b0b41c9e6e3f||2020-03-25 23:03:45.314||BootCamp 2012.dc";
      projectConfigurations.Add(new ProjectConfigurationModel() { FileType = ProjectConfigurationFileType.CALIBRATION.ToString(), FileName = fullName });

      var projectDetailResponseModel = new ProjectDetailListResponseModel()
      {
        Projects = new List<ProjectDetailResponseModel>()
          {new ProjectDetailResponseModel()
            {AccountId = customerUid, ProjectId = projectUid, ProjectName = projectName,
              LastUpdate = lastUpdate, 
              ProjectSettings = new ProjectSettingsModel()
              {
                ProjectId = projectUid,
                TimeZone = "Pacific/Auckland",
                Boundary = new ProjectBoundary() {type = "Polygon", coordinates = new List<double[,]>() {{new double[,] {{150.3, 1.2}, {150.4, 1.2}, {150.4, 1.3}, {150.4, 1.4}, {150.3, 1.2}}}}},
                Config = projectConfigurations
              }
            }
          }
      };

      var mockCwsProjectClient = new Mock<ICwsProjectClient>();
      mockCwsProjectClient.Setup(pr => pr.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<HeaderDictionary>())).ReturnsAsync(projectDetailResponseModel);

      var projectDatabaseModelList = ProjectRequestHelper.GetProjectListForCustomer(new Guid(customerUid), Guid.NewGuid(),
        _logger, _serviceExceptionHandler, mockCwsProjectClient.Object, _customHeaders);

      var result = projectDatabaseModelList.Result;
      Assert.NotNull(result);
      Assert.Single(result);
      Assert.Equal(projectUid, result[0].ProjectUID);
      Assert.Equal(customerUid, result[0].CustomerUID);
      Assert.Equal(projectName, result[0].Name);
      Assert.Equal(ProjectType.Standard, result[0].ProjectType);
      Assert.Equal("New Zealand Standard Time", result[0].ProjectTimeZone);
      Assert.Equal("Pacific/Auckland", result[0].ProjectTimeZoneIana);
      Assert.Equal("POLYGON((150.3 1.2,150.4 1.2,150.4 1.3,150.4 1.4,150.3 1.2))", result[0].Boundary);
      Assert.Equal("BootCamp 2012.dc", result[0].CoordinateSystemFileName);
      Assert.Equal(new DateTime(2020, 03, 25, 23, 03, 45, 314), result[0].CoordinateSystemLastActionedUTC);
      Assert.False(result[0].IsArchived);
      Assert.Equal(lastUpdate, result[0].LastActionedUTC);
    }

  }
}
