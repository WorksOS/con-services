using AssetSettings.Controller;
using Interfaces;
using AutoMapper;
using CommonModel.AssetSettings;
using ClientModel.AssetSettings.Request;
using ClientModel.AssetSettings.Response.AssetTargets;
using ClientModel.Interfaces;
using DbModel.AssetSettings;
using Infrastructure.Cache.Implementations;
using Infrastructure.Cache.Interfaces;
using Infrastructure.Service.AssetSettings.Interfaces;
using Infrastructure.Service.AssetSettings.Service;
using Infrastructure.Service.AssetSettings.Validators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using Utilities.Cache;
using Utilities.IOC;
using Utilities.Logging;
using Utilities.Logging.Models;
using DbModel.Cache;

namespace AssetSettings.Api.Tests.Registrations
{
	public class AssetSettingsRegistrations : InjectConfigBase
    {
        public ILoggingService StubLoggingService { get; set; }

        public List<AssetSettingsListDto> AssetSettingLists { get; set; }

        public IAssetSettingsListRepository StubAssetSettingsListRepository { get; set; }

		public IConfigurationRoot Configuration { get; private set; }

		public IDeviceTypeParameterAttributeRepository StubDeviceTypeParameterAttributeRepository { get; set; }

		public override void ConfigureServiceCollection(IServiceCollection services)
        {
            var mapperconfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<AssetSettingsListDto, AssetSettingsDetails>();
            });

            var mapper = mapperconfig.CreateMapper();

			Configuration = new ConfigurationBuilder()
			 .AddJsonFile("appsettings.json")
			 .Build();

			StubLoggingService = Substitute.For<ILoggingService>();

            StubAssetSettingsListRepository = Substitute.For<IAssetSettingsListRepository>();

			StubDeviceTypeParameterAttributeRepository = Substitute.For<IDeviceTypeParameterAttributeRepository>();

			StubAssetSettingsListRepository.FetchEssentialAssets(Arg.Any<AssetSettingsListRequestDto>())
                .ReturnsForAnyArgs(x => new Tuple<int, IList<AssetSettingsListDto>>(AssetSettingLists.Count, AssetSettingLists));

            StubAssetSettingsListRepository.FetchDeviceTypesByAssetUID(Arg.Any<AssetDeviceTypeRequest>())
                .ReturnsForAnyArgs(x => new Tuple<int, IEnumerable<DeviceTypeDto>>(3, new List<DeviceTypeDto>() { new DeviceTypeDto() { DeviceType = "PL121", AssetCount = 120 }, new DeviceTypeDto() { DeviceType = "PL142", AssetCount = 24 }, new DeviceTypeDto() { DeviceType = "PL132", AssetCount = 100 } }));

            StubAssetSettingsListRepository.FetchValidAssetUIds(Arg.Any<List<string>>(), Arg.Any<AssetSettingsListRequestDto>())
                .ReturnsForAnyArgs(x => new List<string>(AssetSettingLists.Select(y => y.AssetUIDString).ToList()));

			services.Configure<Configurations>(Configuration);

			#region Cache

			services.AddSingleton<IParameterAttributeCache, ParameterAttributeCache>();

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
			
			#endregion

			services.AddSingleton<IDeviceTypeParameterAttributeRepository>(StubDeviceTypeParameterAttributeRepository);

			services.AddSingleton<IMapper>(mapper);

            services.AddSingleton<IInjectConfig>(this);

            services.AddScoped<LogRequestContext>();

            services.AddSingleton<AssetSettingsListController>();

            services.AddSingleton<DeviceTypesController>();

            services.AddSingleton<ILoggingService>(StubLoggingService);

            services.AddSingleton<IAssetSettingsListRepository>(StubAssetSettingsListRepository);

			//Validators Registration
			services.AddSingleton<IRequestValidator<AssetSettingsListRequest>, AssetSettingsFilterValidator>();
			services.AddSingleton<IRequestValidator<AssetSettingsListRequest>, AssetSettingsSortColumnValidator>();
			services.AddSingleton<IRequestValidator<AssetSettingsListRequest>, AssetSettingsListPageValidator>();
			services.AddSingleton<IRequestValidator<IServiceRequest>, CustomerUidValidator>();
			services.AddSingleton<IAssetSettingsListService, AssetSettingsListService>();
        }

        public void BuildRepositoryStub(int count)
        {
            AssetSettingLists = new List<AssetSettingsListDto>();

            for (int i = 0; i < count; i++)
            {
                AssetSettingLists.Add(new AssetSettingsListDto
                {
                    AssetName = "AssetName" + i,
                    AssetUIDString = Guid.NewGuid().ToString(),
                    DeviceSerialNumber = "DeviceSerialNumber" + i,
                    IconKey = i,
                    MakeCode = "MakeCode" + i,
                    Model = "Model" + i,
                    SerialNumber = "SerialNumber" + i,
                    TargetStatus = i % 2 == 0 ? 1 : 0
                });
            }
        }
    }
}
