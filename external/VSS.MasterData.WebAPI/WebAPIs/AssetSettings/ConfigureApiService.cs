using AssetSettings.Controller;
using AssetSettingsRepository;
using AssetWeeklyConfigRepository;
using AutoMapper;
using ClientModel.AssetSettings.Request;
using ClientModel.AssetSettings.Request.AssetSettings;
using ClientModel.AssetSettings.Response.AssetTargets;
using ClientModel.AssetSettings.Response.ProductivityTargetsResponse;
using ClientModel.Interfaces;
using CommonApiLibrary.Middlewares;
using CommonModel.AssetSettings;
using CustomerRepository;
using DbModel.AssetSettings;
using Infrastructure.Cache.Implementations;
using Infrastructure.Cache.Interfaces;
using Infrastructure.Service.AssetSettings.Interfaces;
using Infrastructure.Service.AssetSettings.Mapper;
using Infrastructure.Service.AssetSettings.Publisher;
using Infrastructure.Service.AssetSettings.Service;
using Infrastructure.Service.AssetSettings.Validators;
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
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Runtime.Caching;
using Utilities.Cache;
using Utilities.IOC;
using Utilities.Logging;
using Utilities.Logging.Models;
using VSS.MasterData.WebAPI.Transactions;
using WorkDefinitionRepository;

namespace AssetSettings
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
				cfg.CreateMap<AssetSettingsDto, AssetSettingsResponse>();
				cfg.CreateMap<GetAssetWeeklyTargetsResponse, GetProductivityTargetsResponse>();
				cfg.CreateMap<AssetSettingsListDto, AssetSettingsDetails>().AfterMap(new AssetSettingsListMapper(configurations).Process);
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

			//app.UseHttpsRedirection();

			app.UseSwagger();

			// Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
			// specifying the Swagger JSON endpoint.
			app.UseSwaggerUI(c =>
			{
				c.SwaggerEndpoint("/swagger/v1/swagger.json", "AssetSettings v1");
			});

			app.UseMiddleware<RequestLoggingMiddleware>();
			app.UseMiddleware<ExceptionMiddleware>();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});

			app.UseHealthChecks("/healthz");
			app.UseResponseCompression();
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
			//TODO: Need to check if we can update dependency to ILoggingService 
			ILoggerFactory loggerFactory = new LoggerFactory();
			var logger = new LoggerConfiguration().ReadFrom.Configuration(Configuration);
			loggerFactory.AddProvider(new SerilogLoggerProvider(logger.CreateLogger()));

			services.AddSingleton(loggerFactory.CreateLogger(GetType()));

			services.AddSingleton<IInjectConfig>(this);

			services.AddSingleton<IConfiguration>(Configuration);

			services.Configure<Configurations>(Configuration);

			services.AddScoped<LogRequestContext>((sp) =>
				new LogRequestContext
				{
					CorrelationId = Guid.NewGuid(),
					TraceId = new HttpContextAccessor().HttpContext.TraceIdentifier
				});

			services.AddScoped<ILoggingService, LoggingService>();

			services.AddScoped<IStartUpObject, StartUpCacheUpdater>();

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

			services.AddScoped<IDeviceTypeParameterAttributeRepository, DeviceTypeParameterAttributeRepository.DeviceTypeParameterAttributeRepository>();
			services.AddScoped<IServiceTypeParameterRepository, ServiceTypeParameterRepository.ServiceTypeParameterRepository>();
			services.AddScoped<IParameterAttributeCache, ParameterAttributeCache>();
			services.AddScoped<IServiceTypeParameterCache, ServiceTypeParameterCache>();

			#endregion

			#region Transactions

			services.AddScoped<ITransactions, ExecuteTransaction>();

			#endregion

			#region Subscription 

			services.AddScoped<IRequestValidator<AssetSettingValidationRequestBase>, SubscriptionValidator>();
			services.AddScoped<ISubscriptionServicePlanRepository, SubscriptionServicePlanRepository>();

			#endregion

			#region WorkDefinition

			services.AddScoped<IWorkDefinitionServices, WorkDefinitionServices>();

			#endregion

			#region Asset Settings

			services.AddScoped<IRequestValidator<IServiceRequest>, CustomerUidValidator>();
			services.AddScoped<IRequestValidator<IServiceRequest>, UserUidValidator>();
			services.AddScoped<ICustomerRepository, CustomerRepository.CustomerRepository>();

			#endregion

			#region Asset Settings List

			//Validators Registration
			services.AddScoped<IRequestValidator<AssetSettingsListRequest>, AssetSettingsFilterValidator>();
			services.AddScoped<IRequestValidator<AssetSettingsListRequest>, AssetSettingsSortColumnValidator>();
			services.AddScoped<IRequestValidator<AssetSettingsListRequest>, AssetSettingsListPageValidator>();

			//DB Repository Registration
			services.AddScoped<IAssetConfigTypeRepository, AssetConfigTypeRepository.AssetConfigTypeRepository>();
			services.AddScoped<IAssetSettingsListRepository, AssetSettingsListRepository>();

			//Domain Service Registration
			services.AddScoped<IAssetSettingsListService, AssetSettingsListService>();

			//Mapper helpers
			services.AddScoped<AssetSettingsListMapper>();

			#endregion

			#region Asset Settings Mileage & Volume Per Cycle

			//Validators
			services.AddScoped<IRequestValidator<AssetSettingsRequestBase>, AssetUIDsValidator>();
			services.AddScoped<IRequestValidator<AssetSettingsRequestBase>, TargetValueValidator>();

			//Service
			services.AddScoped<IAssetSettingsService<AssetSettingsRequestBase, AssetSettingsResponse>, AssetSettingsService>();
			services.AddScoped<IAssetConfigRepository, AssetConfigRepository.AssetConfigRepository>();


			#endregion

			#region Asset Settings Burn Rate


			//Validators
			services.AddScoped<IRequestValidator<AssetFuelBurnRateSettingRequest>, BurnRateTargetValueValidator>();
			services.AddScoped<IRequestValidator<AssetFuelBurnRateSettingRequest>, BurnRateTargetValueNullValidator>();

			//Service
			services.AddScoped<IAssetSettingsService<AssetFuelBurnRateSettingRequest, AssetFuelBurnRateSettingsDetails>, AssetBurnRateSettingsService>();

			#endregion

			#region Assset Weekly Targets
			//TODO : Pending device config
			//services.AddSingleton<DeviceConfigRepository>().As<IDeviceConfigRepository>().InstancePerDependency();
			services.AddScoped<IWeeklyAssetSettingsRepository, WeeklyAssetSettingsRepository>();
			services.AddScoped<IValidationHelper, ValidationHelper>();
			services.AddScoped<AssetTargetSettingsController>();
			services.AddScoped<AssetProductivitySettingsController>();
			services.AddTransient<Func<string, AssetWeeklySettingsService>>(serviceProvider => key =>
			{
				switch (key)
				{
					case "AssetSettings":
						return this.Resolve<AssetSettingsTargets>();
					case "ProductivityTargets":
						return this.Resolve<AssetSettingsProductivityTargets>();
					default:
						return null;
				}
			});
			services.AddScoped<AssetSettingsTargets>();
			services.AddScoped<AssetSettingsProductivityTargets>();
			services.AddScoped<IAssetSettingsTypeHandler<AssetSettingsBase>, AssetSettingsTypeHandler<AssetSettingsBase>>();
			services.AddScoped<AssetSettingsOverlapTemplate, AssetSettingsOverlapHandler>();


			#endregion

			#region Kafka Publishers

			services.AddScoped<IAssetSettingsPublisher, AssetSettingsPublisher>();

			#endregion

			services.AddControllers().AddNewtonsoftJson();

			services.AddHealthChecks();

			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo { Title = "AssetSettings", Version = "v1" });
			});

			services.AddResponseCompression();
		}
	}
}
