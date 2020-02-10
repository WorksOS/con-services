using AutoMapper;
using ClientModel.DeviceConfig.Request;
using ClientModel.DeviceConfig.Request.DeviceConfig;
using ClientModel.DeviceConfig.Request.DeviceConfig.AssetSecurity;
using ClientModel.DeviceConfig.Request.DeviceConfig.FaultCodeReporting;
using ClientModel.DeviceConfig.Request.DeviceConfig.MaintenanceMode;
using ClientModel.DeviceConfig.Request.DeviceConfig.Meters;
using ClientModel.DeviceConfig.Request.DeviceConfig.MovingThresold;
using ClientModel.DeviceConfig.Request.DeviceConfig.Parameter;
using ClientModel.DeviceConfig.Request.DeviceConfig.ParameterGroup;
using ClientModel.DeviceConfig.Request.DeviceConfig.ReportingSchedule;
using ClientModel.DeviceConfig.Request.DeviceConfig.SpeedingThresholds;
using ClientModel.DeviceConfig.Request.DeviceConfig.Switches;
using ClientModel.DeviceConfig.Response.DeviceConfig.Asset_Security;
using ClientModel.DeviceConfig.Response.DeviceConfig.DeviceTypeGroupParameterAttribute;
using ClientModel.DeviceConfig.Response.DeviceConfig.FaultCodeReporting;
using ClientModel.DeviceConfig.Response.DeviceConfig.MaintenanceMode;
using ClientModel.DeviceConfig.Response.DeviceConfig.Meters;
using ClientModel.DeviceConfig.Response.DeviceConfig.MovingThreshold;
using ClientModel.DeviceConfig.Response.DeviceConfig.Parameter;
using ClientModel.DeviceConfig.Response.DeviceConfig.ParameterGroup;
using ClientModel.DeviceConfig.Response.DeviceConfig.Ping;
using ClientModel.DeviceConfig.Response.DeviceConfig.ReportingSchedule;
using ClientModel.DeviceConfig.Response.DeviceConfig.SpeedingThresholds;
using ClientModel.DeviceConfig.Response.DeviceConfig.Switches;
using CommonApiLibrary.Middlewares;
using CommonModel.DeviceSettings;
using CommonModel.DeviceSettings.ConfigNameValues;
using DbModel.DeviceConfig;
using DeviceConfigRepository.MySql;
using DeviceConfigRepository.MySql.DeviceConfig;
using Infrastructure.Cache.Implementations;
using Infrastructure.Cache.Interfaces;
using Infrastructure.Service.DeviceAcknowledgementByPasser;
using Infrastructure.Service.DeviceAcknowledgementByPasser.Interfaces;
using Infrastructure.Service.DeviceConfig.Implementations;
using Infrastructure.Service.DeviceConfig.Interfaces;
using Infrastructure.Service.DeviceConfig.Validators;
using Infrastructure.Service.DeviceMessageConstructor;
using Infrastructure.Service.DeviceMessageConstructor.Interfaces;
using Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using Utilities.Cache;
using Utilities.IOC;
using Utilities.Logging;
using Utilities.Logging.Models;
using VSS.MasterData.WebAPI.Transactions;

namespace DeviceSettings
{
	public class ConfigureApiService : InjectConfigBase
	{
		public ConfigureApiService(IWebHostEnvironment env)
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
				.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

			builder.AddEnvironmentVariables();
			Configuration = builder.Build();
		}

		public IConfigurationRoot Configuration { get; private set; }

		public void ConfigureServices(IServiceCollection services)
		{
			this.GetContainer(services);
			this.AddMappers(services);
			this.InitializeStartupObjects();
			this.BuildServiceProvider();
		}

		private void InitializeStartupObjects()
		{
			var startUpObjects = this.Resolves<IStartUpObject>();

			//TODO: Add async version if needed
			foreach (var startUpObject in startUpObjects)
			{
				startUpObject.Initialize();
			}
		}

		private void AddMappers(IServiceCollection services)
		{
			var configurations = this.Resolve<IOptions<Configurations>>();
			var mapperconfig = new MapperConfiguration(cfg =>
			{
				cfg.CreateMap<DeviceParamGroupDto, ParameterGroupDetails>();
				cfg.CreateMap<DeviceParamDto, ParameterDetails>();
				cfg.CreateMap<DeviceTypeGroupParamAttrDto, DeviceTypeGroupParameterAttributeDetails>();
				cfg.CreateMap<DbModel.Cache.DeviceTypeParameterAttributeDto, DeviceTypeGroupParameterAttributeDetails>();
				cfg.CreateMap<PingRequestStatus, DevicePingStatusResponse>();
			});

			var mapper = mapperconfig.CreateMapper();
			this._serviceCollection.AddSingleton<IMapper>(mapper);
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			//app.UseHttpsRedirection();

			app.UseCors("VSS");

			app.UseRouting();

			app.UseMiddleware<RequestLoggingMiddleware>();
			app.UseMiddleware<ExceptionMiddleware>();

			app.UseSwagger();

			// Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
			// specifying the Swagger JSON endpoint.
			app.UseSwaggerUI(c =>
			{
				c.SwaggerEndpoint("/swagger/v1/swagger.json", "DeviceSettings v1");
			});

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
				endpoints.MapHealthChecks("/healthz");
			});

			app.UseHealthChecks("/healthz");
			app.UseResponseCompression();
		}

		private T BuildConfigNameValueCollection<T>(string sectionName, bool reverse) where T : ConfigNameValueCollection, new()
		{
			T result = new T();
			result.Values = Configuration.GetSection(sectionName).GetChildren()
				.Select(item => reverse ? new KeyValuePair<string, string>(item.Value, item.Key) : new KeyValuePair<string, string>(item.Key, item.Value))
				.ToDictionary(x => x.Key, x => x.Value);
			return result;
		}

		public override void ConfigureServiceCollection(IServiceCollection services)
		{
			services.AddCors(options =>
			{
				options.AddPolicy("VSS", builder => builder.AllowAnyOrigin()
				  .WithHeaders("Origin", "X-Requested-With", "Content-Type", "Accept", "Authorization", "X-VisionLink-CustomerUid", "X-VisionLink-UserUid")
				  .WithMethods("OPTIONS", "TRACE", "GET", "HEAD", "POST", "PUT", "DELETE"));
			});
			services.AddRouting();

			#region Log - Configuration - StartUpObjects

			ILoggerFactory loggerFactory = new LoggerFactory();
			var logger = new LoggerConfiguration().ReadFrom.Configuration(Configuration);
			loggerFactory.AddProvider(new SerilogLoggerProvider(logger.CreateLogger()));

			services.AddSingleton(loggerFactory.CreateLogger(GetType()));

			services.AddSingleton<IInjectConfig>(this);

			services.AddSingleton<IConfiguration>(Configuration);

			services.Configure<Configurations>(Configuration);

			services.AddScoped<LogRequestContext>();

			services.AddScoped<ILoggingService, LoggingService>();

			services.AddSingleton<IStartUpObject, StartUpCacheUpdater>();

			#endregion

			#region Cache 

			services.AddSingleton<ICache>(new ParameterAttributeMemoryCache("ParameterAttributeCache",
				new CacheItemPolicy
				{
					Priority = CacheItemPriority.NotRemovable
				}
			));
			services.AddSingleton<ICache>(new ServiceTypeParameterMemoryCache("ServiceTypeParameterCache",
				new CacheItemPolicy
				{
					Priority = CacheItemPriority.NotRemovable
				}
			));
			services.AddSingleton<ICache>(new DeviceTypeMemoryCache("DeviceTypeCache",
				new CacheItemPolicy
				{
					Priority = CacheItemPriority.NotRemovable
				}
			));
			services.AddSingleton<ICache>(new DeviceParamGroupMemoryCache("DeviceParamGroupCache",
				new CacheItemPolicy
				{
					Priority = CacheItemPriority.NotRemovable
				}
			));

			services.AddScoped<IDeviceTypeParameterAttributeRepository, DeviceTypeParameterAttributeRepository.DeviceTypeParameterAttributeRepository>();
			services.AddScoped<IServiceTypeParameterRepository, ServiceTypeParameterRepository.ServiceTypeParameterRepository>();
			services.AddScoped<IParameterAttributeCache, ParameterAttributeCache>();
			services.AddScoped<IServiceTypeParameterCache, ServiceTypeParameterCache>();
			services.AddScoped<IDeviceTypeCache, DeviceTypeCache>();
			services.AddScoped<IDeviceParamGroupCache, DeviceParamGroupCache>();

			#endregion

			#region ConfigNameValueCollection & ConfigListsCollection

			//UIParameter to DBAttribute Mapper
			services.AddSingleton<DeviceConfigRequestToAttributeMaps>(this.BuildConfigNameValueCollection<DeviceConfigRequestToAttributeMaps>("deviceConfigParameterAttributeMaps", false));
			services.AddSingleton<DeviceConfigAttributeToRequestMaps>(this.BuildConfigNameValueCollection<DeviceConfigAttributeToRequestMaps>("deviceConfigParameterAttributeMaps", true));
			services.AddSingleton<DeviceConfigParameterNames>(this.BuildConfigNameValueCollection<DeviceConfigParameterNames>("deviceConfigParameterName", false));


			services.AddSingleton<ConfigListsCollection>(new ParameterGroupsNonMandatoryLists
			{
				Values = new List<string>(Configuration["AppSettings:DeviceConfigParameterGroupsNonMandatoryLists"].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
			});

			services.AddSingleton<ConfigListsCollection>(new ParametersNonMandatoryLists
			{
				Values = new List<string>(Configuration["AppSettings:DeviceConfigParametersNonMandatoryLists"].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
			});

			services.AddSingleton<ConfigListsCollection>(new AttributesNonMandatoryLists
			{
				Values = new List<string>(Configuration["AppSettings:DeviceConfigAttributesNonMandatoryLists"].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
			});


			#endregion

			#region Transactions

			services.AddScoped<ITransactions, ExecuteTransaction>();

			#endregion

			#region Subscription 

			services.AddScoped<IRequestValidator<DeviceConfigRequestBase>, SubscriptionValidator>();
			services.AddScoped<ISubscriptionServicePlanRepository, SubscriptionServicePlanRepository>();

			#endregion

			#region Validators

			//Service Request Validators

			services.AddScoped<IRequestValidator<IServiceRequest>, CustomerUIDValidator>();
			services.AddScoped<IRequestValidator<IServiceRequest>, UserUIDValidator>();


			//Device Config Request Validators

			services.AddScoped<IRequestValidator<DeviceConfigRequestBase>, DeviceTypeValidator>();
			services.AddScoped<IRequestValidator<DeviceConfigRequestBase>, DeviceTypeParameterGroupValidator>();
			services.AddScoped<IRequestValidator<DeviceConfigRequestBase>, AssetUIDsValidator>();
			services.AddScoped<IRequestValidator<DeviceConfigRequestBase>, AllAttributeAsMandatoryValidator>();

			#endregion

			services.AddScoped<IAssetDeviceRepository, AssetDeviceRepository>();
			services.AddScoped<IMessageConstructor, MessageConstructor>();

			//Acknowledgement Bypasser
			services.AddScoped<IAckBypasser, AcknowledgementBypasser>();

			//Device Config Settings Base
			services.AddScoped<IDeviceConfigSettingConfig, DeviceConfigSettingConfigBase>();

			services.AddScoped<IGroupSettingsConfig, MetersSettingsConfig>();

			#region Device Parameter Groups

			//Service
			services.AddScoped<IDeviceConfigService<DeviceConfigParameterGroupRequest, ParameterGroupDetails>, DeviceConfigParamGroupService>();

			//Repository
			services.AddScoped<IDeviceParamGroupRepository, DeviceParamGroupRepository>();

			#endregion

			#region Device Parameters

			//Service
			services.AddScoped<DeviceConfigParmetersServiceBase, DeviceConfigParamService>();

			//Validators
			services.AddScoped<IRequestValidator<DeviceConfigParameterRequest>, DeviceParameterGroupByIdValidator>();

			//Repository
			services.AddScoped<IDeviceParamRepository, DeviceParamRepository>();


			#endregion

			#region Repository

			//Repository 
			services.AddScoped<IDeviceConfigRepository, DeviceConfigRepository.MySql.DeviceConfig.DeviceConfigRepository>();
			services.AddScoped<IUserAssetRepository, UserAssetRepository>();
			services.AddScoped<IDeviceTypeRepository, DeviceTypeRepository>();
			services.AddScoped<ISubscriptionServicePlanRepository, SubscriptionServicePlanRepository>();

			#endregion

			#region Asset Security

			//Service
			services.AddScoped<IDeviceConfigService<DeviceConfigAssetSecurityRequest, DeviceConfigAssetSecurityDetails>, DeviceConfigAssetSecurityService>();

			//Validator
			services.AddScoped<IRequestValidator<DeviceConfigAssetSecurityRequest>, AssetSecurityValidator>();

			#endregion

			#region Fault Code Reporting

			services.AddScoped<IDeviceConfigService<DeviceConfigFaultCodeReportingRequest, DeviceConfigFaultCodeReportingDetails>, DeviceConfigFaultCodeReportingService>();

			//Validator
			services.AddScoped<IRequestValidator<DeviceConfigFaultCodeReportingRequest>, FaultCodeReportingValidator>();

			#endregion

			#region Reporting Schedule

			//Service
			services.AddScoped<IDeviceConfigService<DeviceConfigReportingScheduleRequest, DeviceConfigReportingScheduleDetails>, DeviceConfigReportingScheduleService>();

			//Validator
			services.AddScoped<IRequestValidator<DeviceConfigReportingScheduleRequest>, ReportingScheduleValidator>();

			#endregion

			#region Moving Threshold

			//Service
			services.AddScoped<IDeviceConfigService<DeviceConfigMovingThresholdRequest, DeviceConfigMovingThresholdDetails>, DeviceConfigMovingThresholdService>();

			//Validator
			services.AddScoped<IRequestValidator<DeviceConfigMovingThresholdRequest>, MovingThresholdValidator>();

			#endregion

			#region Speeding Thresholds

			//Service
			services.AddScoped<IDeviceConfigService<DeviceConfigSpeedingThresholdsRequest, DeviceConfigSpeedingThresholdsDetails>, DeviceConfigSpeedingThresholdsService>();

			//Validator
			services.AddScoped<IRequestValidator<DeviceConfigSpeedingThresholdsRequest>, SpeedingThresholdsValidator>();

			#endregion

			#region Meters

			//Service
			services.AddScoped<IDeviceConfigService<DeviceConfigMetersRequest, DeviceConfigMetersDetails>, DeviceConfigMetersService>();

			//Validator
			services.AddScoped<IRequestValidator<DeviceConfigMetersRequest>, MetersValidator>();

			#endregion

			#region Switches

			services.AddScoped<ISwitchesValidator, SwitchesValidator>();
			services.AddScoped<IDeviceConfigSwitchesService, DeviceConfigSwitchesService>();

			#endregion

			#region Maintenance Mode

			//Service
			services.AddScoped<IDeviceConfigService<DeviceConfigMaintenanceModeRequest, DeviceConfigMaintenanceModeDetails>, DeviceConfigMaintenanceModeService>();

			//Validator
			services.AddScoped<IRequestValidator<DeviceConfigMaintenanceModeRequest>, MaintenanceModeValidator>();

			#endregion

			#region Ping

			services.AddScoped<IServiceRequest, DevicePingLogRequest>();
			services.AddScoped<IDevicePingService, DevicePingService>();
			services.AddScoped<IRequestValidator<DevicePingLogRequest>, PingValidator>();
			services.AddScoped<IDevicePingRepository, DevicePingRepository>();

			#endregion

			#region Switches 

			services.AddScoped<DeviceConfigTemplateBase<DeviceConfigSwitchesRequest, DeviceConfigSwitches>, DeviceConfigSwitchesService>();

			#endregion


			services.AddControllers().AddNewtonsoftJson();

			services.AddHealthChecks();

			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo { Title = "DeviceSettings", Version = "v1" });
			});

			services.AddResponseCompression();

		}
	}
}
