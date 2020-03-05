using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging;
using Swashbuckle.AspNetCore.Swagger;
using System;
using Serilog;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using VSS.MasterData.WebAPI.Asset.Filters;
using VSS.MasterData.WebAPI.Asset.Helpers;
using VSS.MasterData.WebAPI.AssetRepository;
using VSS.MasterData.WebAPI.Interfaces;
using VSS.MasterData.WebAPI.Transactions;
using VSS.MasterData.WebAPI.Utilities;
using Microsoft.AspNetCore.Mvc.Formatters;
using System.Text;
using Microsoft.AspNetCore.Http;
using VSS.VisionLink.SearchAndFilter.Client.v1_6.Interfaces;
using VSS.VisionLink.SearchAndFilter.Client.v1_6.Implementations;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace VSS.MasterData.WebAPI.Asset
{
	[ExcludeFromCodeCoverage]
	public class Startup
	{
		public IConfiguration Configuration { get; }
		public Microsoft.Extensions.Logging.ILogger Logger { get; }
		public ConfigParams configParams { get; }
		private ILoggerFactory loggerFactory;
		public Startup(IHostEnvironment env)
		{
			Configuration = new ConfigurationBuilder().SetBasePath(env.ContentRootPath)
													.AddXmlFile("app.config.xml", true)
													.AddJsonFile("appsettings.json", true)
													.AddJsonFile("log.json", true)
													.AddEnvironmentVariables()
													.Build();

			loggerFactory = new LoggerFactory();
			var logger = new LoggerConfiguration().ReadFrom.Configuration(Configuration);
			loggerFactory.AddProvider(new SerilogLoggerProvider(logger.CreateLogger()));
			Logger = loggerFactory.CreateLogger(GetType());
			configParams = new ConfigParams {
				SearchAndQuerySvcUri = new Uri(Configuration["SearchAndFilterSvcUri"]),
				ServiceClientTriesMax = int.Parse(Configuration["ServiceClientTriesMax"])
			};
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		[ExcludeFromCodeCoverage]
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddCors(options =>
			{
				options.AddPolicy("VSS",
					corsBuilder => corsBuilder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
			});
			services.AddControllersWithViews().AddNewtonsoftJson();
			services.AddSingleton(Logger);
			services.AddSingleton(Configuration);
			services.AddSingleton<ISearchAndFilter>(x=> new SearchAndFilterClient(configParams.SearchAndQuerySvcUri, loggerFactory, configParams.ServiceClientTriesMax));
			services.AddSingleton<ITransactions, ExecuteTransaction>();
			services.AddSingleton<ISupportAssetServices, SupportAssetServices>();
			services.AddSingleton<IAssetServices, AssetServices>();
			services.AddSingleton<IControllerUtilities, ControllerUtilities>();
			services.AddSingleton<IAssetOwnerServices, AssetOwnerServices>();
			services.AddSingleton<IAssetECMInfoServices, AssetECMServices>();
			services.AddHealthChecks();

			services.AddMvc(config =>
			{
				config.Filters.Add(typeof(ValidateModelStateAttribute));
				config.RespectBrowserAcceptHeader = true;
				config.ReturnHttpNotAcceptable = true;
				//config.OutputFormatters.Add(new XmlSerializerOutputFormatter());
			}).AddXmlSerializerFormatters()
			.AddNewtonsoftJson(options =>
			{
				options.UseMemberCasing();
			});

			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo{	Version = "v1", Title = "VSS.MasterData.WebAPI.Asset"});

				// Set the comments path for the Swagger JSON and UI.
				var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
				var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
				c.IncludeXmlComments(xmlPath);
			});
		}

		[ExcludeFromCodeCoverage]
		public void Configure(IApplicationBuilder app, IHostEnvironment env)
		{

			// Enable middleware to serve generated Swagger as a JSON endpoint.
			app.UseSwagger();

			// specifying the Swagger JSON endpoint.
			app.UseSwaggerUI(c =>{c.SwaggerEndpoint("/swagger/v1/swagger.json", "VSS.MasterData.WebAPI V1");});

			app.Use(async (context, next) =>
			{
				context.Request.EnableBuffering();

				using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, false, 1024, true))
				{
					if (context.Request.Path != null && !context.Request.Path.ToString().ToLower().Contains("healthz") &&
					!context.Request.Path.ToString().ToLower().Contains("swagger"))
					{
						var body = await reader.ReadToEndAsync();
						Logger.LogInformation($"vlmd_AssetService_Request:{context.Request.Scheme} {context.Request.Host}{context.Request.Path} {context.Request.QueryString} {body}");
						context.Request.Body.Seek(0, SeekOrigin.Begin);
					}
					
				}

				await next.Invoke();
			});

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}			
			app.UseCors("VSS");
			app.UseRouting();
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
			app.UseHealthChecks("/healthz");
		}
	}
}
