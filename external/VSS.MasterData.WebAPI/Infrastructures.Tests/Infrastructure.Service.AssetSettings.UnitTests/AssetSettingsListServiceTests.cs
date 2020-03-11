using Interfaces;
using AutoMapper;
using CommonModel.AssetSettings;
using ClientModel.AssetSettings.Request;
using ClientModel.AssetSettings.Response.AssetTargets;
using CommonModel.Exceptions;
using ClientModel.Interfaces;
using DbModel.AssetSettings;
using Infrastructure.Cache.Interfaces;
using CommonModel.Enum;
using Infrastructure.Common.Helpers;
using Infrastructure.Service.AssetSettings.Interfaces;
using Infrastructure.Service.AssetSettings.Service;
using Infrastructure.Service.AssetSettings.Validators;
using Microsoft.Extensions.Options;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities.Logging;
using Xunit;
using DbModel.Cache;
using CustomerRepository;

namespace VSP.MasterData.Asset.Domain.UnitTests
{
	public class AssetSettingsListServiceTests
    {
        private readonly IAssetSettingsListRepository _stubAssetSettingsListRepository;
		private readonly ICustomerRepository _stubCustomerRepository;
		private readonly IParameterAttributeCache _stubParameterAttributeCache;
		private readonly IEnumerable<IRequestValidator<AssetSettingsListRequest>> _assetSettingsListValidators;
        private readonly IEnumerable<IRequestValidator<IServiceRequest>> _serviceRequestParametersValidators;
        private readonly IMapper _mapper;
        private readonly ILoggingService _stubLoggingService;
        private readonly Guid _customerUid;
        private readonly Guid _userUid;
        private readonly List<Guid> _assetUIDs;
        private readonly IAssetSettingsListService _assetSettingsListService;
		private readonly IOptions<Configurations> _stubConfigurations;
        public AssetSettingsListServiceTests()
        {
            _customerUid = Guid.NewGuid();
            _userUid = Guid.NewGuid();
            _assetUIDs = new List<Guid>();

			_stubLoggingService = Substitute.For<ILoggingService>();
			_stubCustomerRepository = Substitute.For<ICustomerRepository>();
			_stubLoggingService.CreateLogger(typeof(AssetSettingsListService));

            _assetSettingsListValidators = new List<IRequestValidator<AssetSettingsListRequest>>
            {
                new AssetSettingsListPageValidator(_stubLoggingService),
				new AssetSettingsFilterValidator(_stubLoggingService),
                new AssetSettingsSortColumnValidator(_stubLoggingService)
            };

            _serviceRequestParametersValidators = new List<IRequestValidator<IServiceRequest>>
            {
                new CustomerUidValidator(_stubCustomerRepository, _stubLoggingService),
                new UserUidValidator(_stubLoggingService)
            };

            for (int i = 0; i < 10; i++)
            {
                _assetUIDs.Add(Guid.NewGuid());
            }
			_stubParameterAttributeCache = Substitute.For<IParameterAttributeCache>();
			_stubAssetSettingsListRepository = Substitute.For<IAssetSettingsListRepository>();
            _stubAssetSettingsListRepository.FetchEssentialAssets(Arg.Any<AssetSettingsListRequestDto>()).Returns(
            x =>
            {
                var request = (AssetSettingsListRequestDto)x[0];
                if (request.CustomerUid == _customerUid.ToString("N") && request.UserUid == _userUid.ToString("N"))
                {
                    if ((_assetUIDs.Count / request.PageSize) < request.PageNumber)
                    {
                        return new Tuple<int, IList<AssetSettingsListDto>>(0, new List<AssetSettingsListDto>());
                    }
                    return new Tuple<int, IList<AssetSettingsListDto>>(_assetUIDs.Count, _assetUIDs.Select(y => new AssetSettingsListDto
                    {
                        AssetUIDString = y.ToString("N"),
                        AssetName = y.ToString("N")
                    }).ToList());
                }
                else
                {
                    return new Tuple<int, IList<AssetSettingsListDto>>(0, new List<AssetSettingsListDto>());
                }
            });

            _stubAssetSettingsListRepository.FetchDeviceTypesByAssetUID(Arg.Any<AssetDeviceTypeRequest>()).Returns(x =>
            {
                var request = (AssetDeviceTypeRequest)x[0];
                return Tuple.Create<int, IEnumerable<DeviceTypeDto>>(3, new List<DeviceTypeDto>() {
                    new DeviceTypeDto() { DeviceType = "PL121", AssetCount = 120 },
                    new DeviceTypeDto() { DeviceType = "PL142", AssetCount = 24 },
                    new DeviceTypeDto() { DeviceType = "PL132", AssetCount = 100 } });
            });

            var mapperconfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<AssetSettingsListDto, AssetSettingsDetails>();
            });
			this._stubConfigurations = Options.Create<Configurations>(Substitute.For<Configurations>());
			_mapper = mapperconfig.CreateMapper();
            _assetSettingsListService = new AssetSettingsListService(_stubAssetSettingsListRepository, _assetSettingsListValidators, _serviceRequestParametersValidators, _stubParameterAttributeCache, _mapper, _stubLoggingService, _stubConfigurations);
        }

        [Theory]
        [MemberData(nameof(GetAssetDeviceTypeRequest))]
        public void FetchDeviceTypes_Test(AssetDeviceTypeRequest request, int errorCode, string errorMessage)
        {
            try
            {
                var response = _assetSettingsListService.FetchDeviceTypes(request).Result;

                Assert.NotNull(response);
                Assert.Equal(response.Count, 3);
            }
            catch (AggregateException aggregateEx)
            {
                Assert.NotNull(aggregateEx.InnerException);
                var domainEx = (DomainException)aggregateEx.InnerException;
                Assert.NotNull(domainEx);
                Assert.NotNull(domainEx.Errors);
                Assert.True(domainEx.Errors.Any());
                Assert.Equal(domainEx.Errors.First().ErrorCode, errorCode);
                Assert.Equal(domainEx.Errors.First().Message, errorMessage);
            }
        }

        public static IEnumerable<object[]> GetAssetDeviceTypeRequest()
        {
            return new List<object[]>
            {
                new object[] { new AssetDeviceTypeRequest()
                    {
                        UserUid = Guid.NewGuid()
                    },
                    (int)ErrorCodes.CustomerUIDNull,
					UtilHelpers.GetEnumDescription(ErrorCodes.CustomerUIDNull)
                },
                new object[] { new AssetDeviceTypeRequest()
                    {
                        CustomerUid = Guid.NewGuid(),
                    },
                    (int)ErrorCodes.UserUIDNull,
					UtilHelpers.GetEnumDescription(ErrorCodes.UserUIDNull)
                },
                new object[] { new AssetDeviceTypeRequest()
                    {
                        UserUid = Guid.NewGuid(),
                        CustomerUid = Guid.NewGuid()
                    },
                    0,
                    null
                }
            };
        }

        [Fact]
        public void FetchEssentialAssets_ValidPageSizeAndNumber_GivesOk()
        {
            var response = _assetSettingsListService.FetchEssentialAssets(new AssetSettingsListRequest
            {
                PageSize = 10,
                PageNumber = 1,
                CustomerUid = _customerUid,
                UserUid = _userUid
            }).Result;

            Assert.NotNull(response);

            Assert.Equal(response.Lists.Count, 10);
            Assert.NotNull(response.RecordInfo);

            Assert.Equal(response.RecordInfo.TotalRecords, 10);

            Assert.Equal(response.RecordInfo.CurrentPageSize, 10);
            Assert.Equal(response.RecordInfo.CurrentPageNumber, 1);
        }

        [Fact]
        public void FetchEssentialAssets_InvalidPageSizeAndNumber_GivesOk()
        {
            var response = _assetSettingsListService.FetchEssentialAssets(new AssetSettingsListRequest
            {
                PageSize = 10,
                PageNumber = 2,
                CustomerUid = _customerUid,
                UserUid = _userUid
            }).Result;

            Assert.NotNull(response);

            Assert.Equal(response.Lists.Count, 0);
            Assert.NotNull(response.RecordInfo);

            Assert.Equal(response.RecordInfo.TotalRecords, 0);

            Assert.Equal(response.RecordInfo.CurrentPageSize, 10);
            Assert.Equal(response.RecordInfo.CurrentPageNumber, 2);
        }

        [Fact]
        public void FetchEssentialAssets_InvalidPageNumber_ThrowsDomainException()
        {
            try
            {
                var response = _assetSettingsListService.FetchEssentialAssets(new AssetSettingsListRequest
                {
                    PageSize = 10,
                    PageNumber = 0,
                    CustomerUid = _customerUid,
                    UserUid = _userUid
                }).Result;
            }
            catch (AggregateException aggregateEx)
            {
                Assert.NotNull(aggregateEx.InnerException);
                var domainEx = (DomainException)aggregateEx.InnerException;
                Assert.NotNull(domainEx);
                Assert.NotNull(domainEx.Errors);
                Assert.True(domainEx.Errors.Any());
                Assert.Equal(domainEx.Errors.First().ErrorCode, (int)ErrorCodes.PageNumberLessThanOne);
                Assert.Equal(domainEx.Errors.First().Message, UtilHelpers.GetEnumDescription(ErrorCodes.PageNumberLessThanOne));
            }
        }

        [Fact]
        public void FetchEssentialAssets_InvalidPageSize_ThrowsDomainException()
        {
            try
            {
                var response = _assetSettingsListService.FetchEssentialAssets(new AssetSettingsListRequest
                {
                    PageSize = 0,
                    PageNumber = 1,
                    CustomerUid = _customerUid,
                    UserUid = _userUid
                }).Result;
            }
            catch (AggregateException aggregateEx)
            {
                Assert.NotNull(aggregateEx.InnerException);
                var domainEx = (DomainException)aggregateEx.InnerException;
                Assert.NotNull(domainEx);
                Assert.NotNull(domainEx.Errors);
                Assert.True(domainEx.Errors.Any());
                Assert.Equal(domainEx.Errors.First().ErrorCode, (int)ErrorCodes.PageSizeLessThanOne);
                Assert.Equal(domainEx.Errors.First().Message, UtilHelpers.GetEnumDescription(ErrorCodes.PageSizeLessThanOne));
            }
        }

        [Fact]
        public void FetchEssentialAssets_InvalidFilterName_ThrowsDomainException()
        {
            try
            {
                var response = _assetSettingsListService.FetchEssentialAssets(new AssetSettingsListRequest
                {
                    PageSize = 10,
                    PageNumber = 1,
                    CustomerUid = _customerUid,
                    UserUid = _userUid,
                    FilterName = "InvalidFilter",
                    FilterValue = "Fuel"
                }).Result;
            }
            catch (AggregateException aggregateEx)
            {
                Assert.NotNull(aggregateEx.InnerException);
                var domainEx = (DomainException)aggregateEx.InnerException;
                Assert.NotNull(domainEx);
                Assert.NotNull(domainEx.Errors);
                Assert.True(domainEx.Errors.Any());
                Assert.Equal(domainEx.Errors.First().ErrorCode, (int)ErrorCodes.InvalidFilterName);
                Assert.Equal(domainEx.Errors.First().Message, string.Format(UtilHelpers.GetEnumDescription(ErrorCodes.InvalidFilterName), "InvalidFilter"));
            }
        }

        [Fact]
        public void FetchEssentialAssets_ValidDeviceType_GivesOK()
        {
            var response = _assetSettingsListService.FetchEssentialAssets(new AssetSettingsListRequest
            {
                PageSize = 10,
                PageNumber = 1,
                CustomerUid = _customerUid,
                UserUid = _userUid,
                DeviceType = "DeviceType"
            }).Result;

            Assert.NotNull(response);

            Assert.Equal(response.Lists.Count, 10);
            Assert.NotNull(response.RecordInfo);

            Assert.Equal(response.RecordInfo.TotalRecords, 10);

            Assert.Equal(response.RecordInfo.CurrentPageSize, 10);
            Assert.Equal(response.RecordInfo.CurrentPageNumber, 1);
        }

        [Fact]
        public void FetchEssentialAssets_ValidFilterAndDeviceType_GivesOK()
        {
            var response = _assetSettingsListService.FetchEssentialAssets(new AssetSettingsListRequest
            {
                PageSize = 10,
                PageNumber = 1,
                CustomerUid = _customerUid,
                UserUid = _userUid,
                FilterName = "AssetId",
                FilterValue = "Asset",
                DeviceType = "DeviceType"
            }).Result;
            Assert.NotNull(response);

            Assert.Equal(response.Lists.Count, 10);
            Assert.NotNull(response.RecordInfo);

            Assert.Equal(response.RecordInfo.TotalRecords, 10);

            Assert.Equal(response.RecordInfo.CurrentPageSize, 10);
            Assert.Equal(response.RecordInfo.CurrentPageNumber, 1);
        }

        [Fact]
        public void FetchEssentialAssets_ValidFilterNameNullFilterValue_ThrowsDomainException()
        {
            try
            {
                var response = _assetSettingsListService.FetchEssentialAssets(new AssetSettingsListRequest
                {
                    PageSize = 10,
                    PageNumber = 1,
                    CustomerUid = _customerUid,
                    UserUid = _userUid,
                    FilterName = "AssetId"
                }).Result;
            }
            catch (AggregateException aggregateEx)
            {
                Assert.NotNull(aggregateEx.InnerException);
                var domainEx = (DomainException)aggregateEx.InnerException;
                Assert.NotNull(domainEx);
                Assert.NotNull(domainEx.Errors);
                Assert.True(domainEx.Errors.Any());
                Assert.Equal(domainEx.Errors.First().ErrorCode, (int)ErrorCodes.FilterValueNull);
                Assert.Equal(domainEx.Errors.First().Message, UtilHelpers.GetEnumDescription(ErrorCodes.FilterValueNull));
            }
        }


        [Fact]
        public void FetchEssentialAssets_ValidFilterValueNullFilterName_ThrowsDomainException()
        {
            try
            {
                var response = _assetSettingsListService.FetchEssentialAssets(new AssetSettingsListRequest
                {
                    PageSize = 10,
                    PageNumber = 1,
                    CustomerUid = _customerUid,
                    UserUid = _userUid,
                    FilterValue = "Fuel"
                }).Result;
            }
            catch (AggregateException aggregateEx)
            {
                Assert.NotNull(aggregateEx.InnerException);
                var domainEx = (DomainException)aggregateEx.InnerException;
                Assert.NotNull(domainEx);
                Assert.NotNull(domainEx.Errors);
                Assert.True(domainEx.Errors.Any());
                Assert.Equal(domainEx.Errors.First().ErrorCode, (int)ErrorCodes.FilterNameNull);
                Assert.Equal(domainEx.Errors.First().Message, UtilHelpers.GetEnumDescription(ErrorCodes.FilterNameNull));
            }
        }

        [Fact]
        public void FetchEssentialAssets_ValidFilterValueAndFilterName_GivesOK()
        {
            var response = _assetSettingsListService.FetchEssentialAssets(new AssetSettingsListRequest
            {
                PageSize = 10,
                PageNumber = 1,
                CustomerUid = _customerUid,
                UserUid = _userUid,
                FilterValue = "Fuel",
                FilterName = "AssetId"
            }).Result;

            Assert.NotNull(response);

            Assert.Equal(response.Lists.Count, 10);
            Assert.NotNull(response.RecordInfo);

            Assert.Equal(response.RecordInfo.TotalRecords, 10);

            Assert.Equal(response.RecordInfo.CurrentPageSize, 10);
            Assert.Equal(response.RecordInfo.CurrentPageNumber, 1);
        }

        [Fact]
        public void FetchEssentialAssets_ValidFilterValueAndFilterNameAndSortColumn_GivesOK()
        {
            var response = _assetSettingsListService.FetchEssentialAssets(new AssetSettingsListRequest
            {
                PageSize = 10,
                PageNumber = 1,
                CustomerUid = _customerUid,
                UserUid = _userUid,
                FilterValue = "Fuel",
                FilterName = "AssetId",
                SortColumn = "AssetId"
            }).Result;

            Assert.NotNull(response);

            Assert.Equal(response.Lists.Count, 10);
            Assert.NotNull(response.RecordInfo);

            Assert.Equal(response.RecordInfo.TotalRecords, 10);

            Assert.Equal(response.RecordInfo.CurrentPageSize, 10);
            Assert.Equal(response.RecordInfo.CurrentPageNumber, 1);
        }

        [Fact]
        public void FetchEssentialAssets_InvalidSortColumn_ThrowsDomainException()
        {
            try
            {
                var response = _assetSettingsListService.FetchEssentialAssets(new AssetSettingsListRequest
                {
                    PageSize = 10,
                    PageNumber = 1,
                    CustomerUid = _customerUid,
                    UserUid = _userUid,
                    SortColumn = "InvalidSortColumn"
                }).Result;
            }
            catch (AggregateException aggregateEx)
            {
                Assert.NotNull(aggregateEx.InnerException);
                var domainEx = (DomainException)aggregateEx.InnerException;
                Assert.NotNull(domainEx);
                Assert.NotNull(domainEx.Errors);
                Assert.True(domainEx.Errors.Any());
                Assert.Equal(domainEx.Errors.First().ErrorCode, (int)ErrorCodes.InvalidSortColumn);
                Assert.Equal(domainEx.Errors.First().Message, string.Format(UtilHelpers.GetEnumDescription(ErrorCodes.InvalidSortColumn), "InvalidSortColumn"));
            }
        }
    }
}
