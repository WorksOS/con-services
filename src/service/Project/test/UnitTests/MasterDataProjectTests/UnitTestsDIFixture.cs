using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Serilog;
using VSS.Common.Abstractions.Clients.CWS;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Productivity3D.Proxy;
using VSS.Productivity3D.Project.Abstractions.Interfaces.Repository;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Productivity3D.Project.Repository;
using VSS.Serilog.Extensions;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace VSS.MasterData.ProjectTests
{
  public class UnitTestsDIFixture<T> : IDisposable
  {
    public IServiceProvider ServiceProvider;
    protected readonly IServiceExceptionHandler ServiceExceptionHandler;
    protected readonly ILogger _log;
    protected readonly ILoggerFactory _loggerFactory;
    public IHeaderDictionary _customHeaders;
    protected IServiceCollection ServiceCollection;
    protected IConfigurationStore _configStore;

    protected Guid _userUid;
    protected Guid _customerUid;
    protected string _customerTrn;
    protected Guid _projectUid;
    protected string _projectTrn;

    public UnitTestsDIFixture()
    {
      AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();

      var loggerFactory = new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.Project.UnitTests.log"));
      ServiceCollection = new ServiceCollection();

      ServiceCollection.AddLogging();
      ServiceCollection.AddSingleton(loggerFactory);
      ServiceCollection
        .AddTransient<IProjectRepository, ProjectRepository>()
        .AddSingleton<IConfigurationStore, GenericConfiguration>()
        .AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>()
        .AddTransient<IProductivity3dV1ProxyCoord, Productivity3dV1ProxyCoord>()
        .AddTransient<IProductivity3dV2ProxyNotification, Productivity3dV2ProxyNotification>()
        .AddTransient<IProductivity3dV2ProxyCompaction, Productivity3dV2ProxyCompaction>()
        .AddTransient<IErrorCodesProvider, ProjectErrorCodesProvider>();

      ServiceProvider = ServiceCollection.BuildServiceProvider();
      ServiceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();

      _log = ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<T>();
      _loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();
      _customHeaders = new HeaderDictionary();
      _configStore = ServiceProvider.GetRequiredService<IConfigurationStore>();

      _userUid = Guid.NewGuid();
      _customerUid = Guid.NewGuid();
      _customerTrn = TRNHelper.MakeTRN(_customerUid, TRNHelper.TRN_ACCOUNT);
      _projectUid = Guid.NewGuid();
      _projectTrn = TRNHelper.MakeTRN(_projectUid);
      _customHeaders.Add("X-VisionLink-CustomerUID", new StringValues(_customerUid.ToString()));
    }

    protected ProjectDetailListResponseModel CreateProjectListModel(string customerTrn, string projectTrn, string projectName = "the project name",
      DateTime? lastUpdate = null, List<ProjectConfigurationModel> projectConfigurations = null)
    {
      var lastUpdateUtc = lastUpdate ?? DateTime.UtcNow.AddDays(-1);
      var projectConfigurationList = projectConfigurations ?? new List<ProjectConfigurationModel>();
      return new ProjectDetailListResponseModel()
      {
        Projects = new List<ProjectDetailResponseModel>()
        {
          new ProjectDetailResponseModel()
          {
            AccountTRN = customerTrn,
            ProjectTRN = projectTrn,
            ProjectName = projectName,
            ProjectType = CwsProjectType.AcceptsTagFiles,
            UserProjectRole = UserProjectRoleEnum.Admin,
            LastUpdate = lastUpdateUtc,
            ProjectSettings = new ProjectSettingsModel()
            {
              ProjectTRN = projectTrn, TimeZone = "Pacific/Auckland",
              Boundary = CreateProjectBoundary(),
              Config = projectConfigurationList
            }
          }
        }
      };
    }

    protected ProjectDetailResponseModel CreateProjectDetailModel(string customerTrn, string projectTrn, string projectName = "the project name",
      DateTime? lastUpdate = null, UserProjectRoleEnum userProjectRole = UserProjectRoleEnum.Admin,  List<ProjectConfigurationModel> projectConfigurations = null)
    {
      var lastUpdateUtc = lastUpdate ?? DateTime.UtcNow.AddDays(-1);
      var projectConfigurationList = projectConfigurations ?? new List<ProjectConfigurationModel>();
      return new ProjectDetailResponseModel()
      {
        AccountTRN = customerTrn,
        ProjectTRN = projectTrn,
        ProjectName = projectName,
        ProjectType = CwsProjectType.AcceptsTagFiles,
        UserProjectRole = userProjectRole,
        LastUpdate = lastUpdateUtc,
        ProjectSettings = new ProjectSettingsModel()
        {
          ProjectTRN = projectTrn,
          TimeZone = "Pacific/Auckland",
          Boundary = CreateProjectBoundary(),
          Config = projectConfigurationList
        }
      };
    }

    protected ProjectBoundary CreateProjectBoundary()
    {
      return new ProjectBoundary()
      {
        type = "Polygon",
        coordinates = new List<List<double[]>>
        {
          new List<double[]>
          {
            new[] {150.3, 1.2},
            new[] {150.4, 1.2},
            new[] {150.4, 1.3},
            new[] {150.4, 1.4},
            new[] {150.3, 1.2}
          }
        }
      };
    }

    public void Dispose()
    { }
  }
}
