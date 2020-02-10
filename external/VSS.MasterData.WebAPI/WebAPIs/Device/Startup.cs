using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Extensions.Logging;
using VSS.MasterData.WebAPI.AssetRepository;
using VSS.MasterData.WebAPI.Device.DeviceConfig;
using VSS.MasterData.WebAPI.Device.Filters;
using VSS.MasterData.WebAPI.Filters;
using VSS.MasterData.WebAPI.Interfaces;
using VSS.MasterData.WebAPI.Interfaces.Device;
using VSS.MasterData.WebAPI.Repository.Device;
using VSS.MasterData.WebAPI.Transactions;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace VSS.MasterData.WebAPI.Device
{
	public class Startup
	{
		public static IConfiguration Configuration { get; set; } //TODO: Remove them if not needed, Changed to static for device config ping controller
		public ILogger Logger { get; }

		private List<string> allowableMethodsToLogRequest;
		private DeviceConfigDependencyInjection deviceConfigDependencyInjection = new DeviceConfigDependencyInjection(); //TODO: Remove them if not needed, Changed to static for device config ping controller


		public Startup(IHostingEnvironment env)
		{
			Configuration = new ConfigurationBuilder().SetBasePath(env.ContentRootPath)
													.AddXmlFile("app.config.xml", true)
													.AddJsonFile("appsettings.json", true)
													.AddJsonFile("appsettings-deviceconfig.json", true) //TODO: Remove them if not needed, Changed to static for device config ping controller
													.AddJsonFile("log.json", true)
													.AddEnvironmentVariables()
													.Build();
			var logger = new LoggerConfiguration().ReadFrom.Configuration(Configuration);
			ILoggerFactory loggerFactory = new LoggerFactory();
			loggerFactory.AddProvider(new SerilogLoggerProvider(logger.CreateLogger()));

			Logger = loggerFactory.CreateLogger(GetType());
			allowableMethodsToLogRequest = Configuration["allowableMethodsToLogRequest"]?.ToString()?.Split(',').ToList();
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			try
			{
				services.AddCors(options =>
				{
					options.AddPolicy("VSS",
						corsBuilder => corsBuilder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
				});
				//services.AddControllersWithViews().AddNewtonsoftJson();
				services.AddSingleton(Logger);
				services.AddSingleton(Configuration);
				services.AddSingleton<IDeviceTypeService, DeviceTypeService>();
				services.AddAutoMapper(typeof(Startup).Assembly);
				services.AddScoped<IAssetServices, AssetServices>();
				services.AddScoped<IDeviceService, DeviceService>();
				services.AddScoped<ITransactions, ExecuteTransaction>();


				services.AddMemoryCache();

				services.AddMvc(options => { options.Filters.Add(typeof(ValidateModelAttribute)); })
					.AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null)
					.AddNewtonsoftJson(options => { options.UseMemberCasing(); });

				services.AddSwaggerGen(c =>
				{
					c.SwaggerDoc("v1", new OpenApiInfo
					{
						Version = "v1",
						Title = "VSS.MasterData.WebAPI.Device",
						Contact = new OpenApiContact { Name = "VSSTeamTitans", Email = "VSSTeamTitans@trimble.com" }
					});


					// Set the comments path for the Swagger JSON and UI.
					var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
					var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
					c.IncludeXmlComments(xmlPath);
				});

				services.AddHealthChecks();

				//TODO: Remove them if not needed, Changed to static for device config ping controller
				deviceConfigDependencyInjection.GetContainer(services);


			}
			catch (Exception ex)
			{
				Logger.LogError(ex.Message + ex.StackTrace);
			}

		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			try
			{
				app.UseSwagger();
				app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "VSS.MasterData.WebAPI V1"); });

				if (env.IsDevelopment())
				{
					app.UseDeveloperExceptionPage();
				}
				else
				{
					// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
					app.UseHsts();
				}

				app.Use(async (context, next) =>
				{
					if (allowableMethodsToLogRequest == null ||
						allowableMethodsToLogRequest.Contains(context.Request.Method)
					) // it will log the request if allowableMethodsToLogRequest not provided in config
					{
						context.Request.EnableBuffering();

						using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, false, 1024, true))
						{
							var body = await reader.ReadToEndAsync();
							Logger.LogInformation(
								$"vlmd_device_request_log : machine{context.Request.HttpContext?.Connection?.RemoteIpAddress} user:{UserInfoParser.Parse(context)} {context.Request.Method} {context.Request.Scheme} {context.Request.Host}{context.Request.Path} {context.Request.QueryString} {body}");
							context.Request.Body.Seek(0, SeekOrigin.Begin);
						}
					}

					await next.Invoke();
				});

				app.UseCors("VSS");
				app.UseCors("VSS");
				app.UseRouting();
				app.UseEndpoints(endpoints =>
				{
					endpoints.MapControllers();
					endpoints.MapHealthChecks("/healthz");
				});

				//TODO: Remove them if not needed, Changed to static for device config ping controller
				deviceConfigDependencyInjection.InitializeStartupObjects();

			}
			catch (Exception ex)
			{
				Logger.LogError(ex.Message + ex.StackTrace);
			}
		}
	}
}