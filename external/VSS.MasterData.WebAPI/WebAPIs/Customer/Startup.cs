using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Serilog;
using Serilog.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Text;
using VSS.MasterData.WebAPI.Customer.Filters;
using VSS.MasterData.WebAPI.CustomerRepository;
using VSS.MasterData.WebAPI.Interfaces;
using VSS.MasterData.WebAPI.Transactions;

namespace VSS.MasterData.WebAPI.Customer
{
	[ExcludeFromCodeCoverage]
	public class Startup
	{
		/// <summary>
		/// Startup constructor
		/// </summary>
		/// <param name="env"></param>
		public Startup(IWebHostEnvironment env)
		{
			Configuration = new ConfigurationBuilder().SetBasePath(env.ContentRootPath)
				.AddXmlFile("app.config.xml", true)
				.AddJsonFile("appsettings.json", true)
				.AddJsonFile("log.json", true)
				.AddEnvironmentVariables()
				.Build();
			ILoggerFactory loggerFactory = new LoggerFactory();
			var logger = new LoggerConfiguration().ReadFrom.Configuration(Configuration);
			loggerFactory.AddProvider(new SerilogLoggerProvider(logger.CreateLogger()));
			Logger = loggerFactory.CreateLogger(GetType());
		}

		/// <summary>
		/// 
		/// </summary>
		public IConfiguration Configuration { get; }

		/// <summary>
		/// 
		/// </summary>
		public Microsoft.Extensions.Logging.ILogger Logger { get; }

		/// This method gets called by the runtime. Use this method to add services to the container.
		/// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddCors(options =>
			{
				options.AddPolicy("VSS",
					corsBuilder => corsBuilder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
			});

			services.AddSingleton(Logger);
			services.AddSingleton(Configuration);
			services.AddScoped<ITransactions, ExecuteTransaction>();
			services.AddScoped<ICustomerService, CustomerService>();
			services.AddScoped<IAccountService, AccountService>();
			services.AddScoped<ICustomerAssetService, CustomerAssetService>();
			services.AddScoped<IUserCustomerService, UserCustomerService>();

			services.AddMvc(options =>
				{
					options.Filters.Add(typeof(ValidateModelAttribute));
				})
				.AddNewtonsoftJson(options =>
				{
					options.UseMemberCasing();
				})
				.SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

			services.AddSwaggerGen(s =>
			{
				s.SwaggerDoc("v1", new OpenApiInfo
				{
					Version = "v1",
					Title = "VSS.MasterData.WebAPI.Customer",
					Contact = new OpenApiContact { Name = "VSSTeamTitans", Email = "VSSTeamTitans@trimble.com" }
				});

				// Set the comments path for the Swagger JSON and UI.
				var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
				var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
				s.IncludeXmlComments(xmlPath);
			});

			services.AddHealthChecks();
		}

		/// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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

			//Request logging on entry
			app.Use(async (context, next) =>
			{
				context.Request.EnableBuffering();
				using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, false, 1024, true))
				{
					if (context.Request.Path != null &&
						!context.Request.Path.ToString().ToLower().Contains("healthz") &&
						!context.Request.Path.ToString().ToLower().Contains("swagger"))
					{
						var body = await reader.ReadToEndAsync();
						Logger.LogInformation(
							$"vlmd_customer_Request : {context.Request.Method} {context.Request.Scheme}" +
							$"://{context.Request.Host}{context.Request.Path}{context.Request.QueryString} {body}");
						context.Request.Body.Seek(0, SeekOrigin.Begin);
					}
				}

				await next.Invoke();
			});

			app.UseCors("VSS");
			app.UseRouting();
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
				endpoints.MapHealthChecks("/healthz");
			});
		}
	}
}