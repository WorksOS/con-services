using Interfaces;
using AutoMapper;
using ClientModel.AssetSettings.Request;
using ClientModel.AssetSettings.Request.AssetSettings;
using ClientModel.AssetSettings.Response.AssetTargets;
using ClientModel.Interfaces;
using DbModel.AssetSettings;
using CommonModel.Enum;
using Infrastructure.Service.AssetSettings.Interfaces;
using Infrastructure.Service.AssetSettings.Service;
using Infrastructure.Service.AssetSettings.Validators;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System;
using System.Collections.Generic;
using Utilities.IOC;
using Utilities.Logging;
using Utilities.Logging.Models;
using VSS.MasterData.WebAPI.Transactions;

namespace AssetSettings.Api.Tests.Registrations
{
	public class AssetConfigRegistrations<T> : InjectConfigBase where T : class
    {
        private List<string> _assetUIds;
		private List<AssetSettingsDto> _assetConfigDtoLists;
		public ILoggingService StubLoggingService { get; set; }
        public IAssetConfigRepository StubAssetConfigRepository { get; set; }
		public IAssetConfigTypeRepository StubAssetConfigTypeRepository { get; set; }
		public ITransactions StubTransactions{ get; set; }
		public List<string> AssetUIDs {
            get
            {
                if (_assetUIds == null)
                {
                    _assetUIds = new List<string>();
                }
                return _assetUIds;          
            }
            set
            {
                _assetUIds = value;
            }
        }
		public List<AssetSettingsDto> AssetConfigDtoLists
		{
			get
			{
				if (_assetConfigDtoLists == null)
				{
					_assetConfigDtoLists = new List<AssetSettingsDto>();
				}
				return _assetConfigDtoLists;
			}
			set
			{
				_assetConfigDtoLists = value;
			}
		}

		/*
		IdletimeHours = 0,
		RuntimeHours = 1,
		OdometerinKmsPerWeek = 2,
		BucketVolumeinCuMeter = 3,
		PayloadinTonnes = 4,
		CycleCount = 5,
		VolumeinCuMeter = 6,
		IdlingBurnRateinLiPerHour = 7,
		WorkingBurnRateinLiPerHour = 8,
		PayloadPerCycleInTonnes = 9
		*/
		private readonly List<AssetConfigTypeDto> assetConfigTypeDtos = new List<AssetConfigTypeDto>
		{
			new AssetConfigTypeDto
			{
				AssetConfigTypeID = 0, 
				ConfigTypeDescr = "IdletimeHours", 
				ConfigTypeName = "IdletimeHours", 
				InsertUTC = DateTime.Now, 
				UpdateUTC = DateTime.Now
			},
			new AssetConfigTypeDto
			{
				AssetConfigTypeID = 1,
				ConfigTypeDescr = "RuntimeHours",
				ConfigTypeName = "RuntimeHours",
				InsertUTC = DateTime.Now,
				UpdateUTC = DateTime.Now
			},
			new AssetConfigTypeDto
			{
				AssetConfigTypeID = 2,
				ConfigTypeDescr = "OdometerinKmsPerWeek",
				ConfigTypeName = "OdometerinKmsPerWeek",
				InsertUTC = DateTime.Now,
				UpdateUTC = DateTime.Now
			},
			new AssetConfigTypeDto
			{
				AssetConfigTypeID = 3,
				ConfigTypeDescr = "BucketVolumeinCuMeter",
				ConfigTypeName = "BucketVolumeinCuMeter",
				InsertUTC = DateTime.Now,
				UpdateUTC = DateTime.Now
			},
			new AssetConfigTypeDto
			{
				AssetConfigTypeID = 4,
				ConfigTypeDescr = "PayloadinTonnes",
				ConfigTypeName = "PayloadinTonnes",
				InsertUTC = DateTime.Now,
				UpdateUTC = DateTime.Now
			},
			new AssetConfigTypeDto
			{
				AssetConfigTypeID = 5,
				ConfigTypeDescr = "CycleCount",
				ConfigTypeName = "CycleCount",
				InsertUTC = DateTime.Now,
				UpdateUTC = DateTime.Now
			},
			new AssetConfigTypeDto
			{
				AssetConfigTypeID = 6,
				ConfigTypeDescr = "VolumeinCuMeter",
				ConfigTypeName = "VolumeinCuMeter",
				InsertUTC = DateTime.Now,
				UpdateUTC = DateTime.Now
			},
			new AssetConfigTypeDto
			{
				AssetConfigTypeID = 7,
				ConfigTypeDescr = "IdlingBurnRateinLiPerHour",
				ConfigTypeName = "IdlingBurnRateinLiPerHour",
				InsertUTC = DateTime.Now,
				UpdateUTC = DateTime.Now
			},
			new AssetConfigTypeDto
			{
				AssetConfigTypeID = 8,
				ConfigTypeDescr = "WorkingBurnRateinLiPerHour",
				ConfigTypeName = "WorkingBurnRateinLiPerHour",
				InsertUTC = DateTime.Now,
				UpdateUTC = DateTime.Now
			},
			new AssetConfigTypeDto
			{
				AssetConfigTypeID = 9,
				ConfigTypeDescr = "PayloadPerCycleInTonnes",
				ConfigTypeName = "PayloadPerCycleInTonnes",
				InsertUTC = DateTime.Now,
				UpdateUTC = DateTime.Now
			}
		};


		public override void ConfigureServiceCollection(IServiceCollection services)
        {

            var mapperconfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<AssetSettingsListDto, AssetSettingsDetails>();
                cfg.CreateMap<AssetSettingsDto, AssetSettingsResponse>();
            });

            var mapper = mapperconfig.CreateMapper();

            var stubAssetSettingsPublisher = Substitute.For<IAssetSettingsPublisher>();

			StubTransactions = Substitute.For<ITransactions>();

			StubLoggingService = Substitute.For<ILoggingService>();

            StubAssetConfigRepository = Substitute.For<IAssetConfigRepository>();

			StubAssetConfigTypeRepository = Substitute.For<IAssetConfigTypeRepository>();
			StubAssetConfigTypeRepository.FetchByConfigTypeNames(Arg.Any<AssetConfigTypeDto>()).ReturnsForAnyArgs(x =>
			{
				return this.assetConfigTypeDtos;

			});
			var stubAssetSettingsListRepository = Substitute.For<IAssetSettingsListRepository>();

            stubAssetSettingsListRepository.FetchValidAssetUIds(Arg.Any<List<string>>(), Arg.Any<AssetSettingsListRequestDto>())
                .ReturnsForAnyArgs(x =>
				{
					return this._assetUIds;
				});

            StubAssetConfigRepository.FetchAssetConfig(Arg.Any<List<string>>() , Arg.Any<AssetSettingsDto>())
                .ReturnsForAnyArgs(x =>
				{
					return new List<AssetSettingsDto>(AssetConfigDtoLists);
				});

			services.AddSingleton<ITransactions>(StubTransactions);

			services.AddSingleton<IMapper>(mapper);

			services.AddSingleton<IInjectConfig>(this);

			services.AddSingleton<LogRequestContext>();

			services.AddSingleton<ILoggingService>(StubLoggingService);

			services.AddSingleton<IAssetConfigRepository>(StubAssetConfigRepository);

			services.AddSingleton<IAssetSettingsListRepository>(stubAssetSettingsListRepository);

			services.AddSingleton<IAssetSettingsPublisher>(stubAssetSettingsPublisher);

			services.AddSingleton<IAssetConfigTypeRepository>(StubAssetConfigTypeRepository);


			#region Asset Settings Fuel Burn Rate

			//Validators
			services.AddSingleton<IRequestValidator<AssetFuelBurnRateSettingRequest>, BurnRateTargetValueValidator>();
			services.AddSingleton<IRequestValidator<AssetFuelBurnRateSettingRequest>, BurnRateTargetValueNullValidator>();

			//Service
			services.AddSingleton<IAssetSettingsService<AssetFuelBurnRateSettingRequest, AssetFuelBurnRateSettingsDetails>, AssetBurnRateSettingsService>();

			#endregion

			#region Asset Settings Mileage & Volume Per Cycle

			//Validators
			services.AddSingleton<IRequestValidator<AssetSettingsRequestBase>,AssetUIDsValidator>();
			services.AddSingleton<IRequestValidator<IServiceRequest>,CustomerUidValidator>();
			services.AddSingleton<IRequestValidator<IServiceRequest>, UserUidValidator>();
			services.AddSingleton<IRequestValidator<AssetSettingsRequestBase>,TargetValueValidator>();

			//Service
			services.AddSingleton< IAssetSettingsService<AssetSettingsRequestBase, AssetSettingsResponse>, AssetSettingsService>();

			#endregion

			services.AddSingleton<T>();
		}


        public void BuildRepositoryStub(int count, AssetSettingsRequestBase request, List<AssetTargetType> assetTargetTypes)
        {
			AssetConfigDtoLists = new List<AssetSettingsDto>();

            for (int i = 0; i < count; i++)
            {
                var guid = Guid.NewGuid();
                this.AssetUIDs.Add(guid.ToString());
                AssetConfigDtoLists.Add(new AssetSettingsDto
                {
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now,
                    InsertUTC = DateTime.UtcNow,
                    UpdateUTC = DateTime.UtcNow,
                    TargetValue = i,
                    TargetType = assetTargetTypes[i % assetTargetTypes.Count].ToString()
                });
            }
        }
    }
}
