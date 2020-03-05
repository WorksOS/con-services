using AssetSettingsRepository;
using AssetSettingsRepository.Helpers;
using Interfaces;
using ClientModel.AssetSettings.Request;
using DbModel.AssetSettings;
using CommonModel.Enum;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities.Logging;
using VSS.MasterData.WebAPI.Transactions;
using Xunit;
using DbModel.Cache;

namespace VSP.MasterData.Asset.Data.Tests
{
	public class AssetSettingsListRepositoryTests
    {
        private readonly ILoggingService _stubLoggingService;
        private readonly IAssetSettingsListRepository _assetSettingsListRepository;
        private readonly List<string> _assetUIDs;
		private readonly ITransactions _stubTransactions;

		public AssetSettingsListRepositoryTests()
        {
            _stubLoggingService = Substitute.For<ILoggingService>();
			_stubTransactions = Substitute.For<ITransactions>();
			_assetSettingsListRepository = new AssetSettingsListRepository(_stubTransactions, _stubLoggingService);

			_assetUIDs = new List<string>
            {
                Guid.NewGuid().ToString("N"),
                Guid.NewGuid().ToString("N"),
                Guid.NewGuid().ToString("N"),
                Guid.NewGuid().ToString("N"),
                Guid.NewGuid().ToString("N"),
                Guid.NewGuid().ToString("N")
            };
        }

        [Fact]
        public void FetchValidAssetUIds_ValidRequest_ValidQuery()
        {
            AssetSettingsListRequestDto request = new AssetSettingsListRequestDto
            {
                CustomerUid = Guid.NewGuid().ToString("N"),
                UserUid = Guid.NewGuid().ToString("N")
            };

            _assetSettingsListRepository.FetchValidAssetUIds(_assetUIDs, request);

            _stubTransactions.Received(1).GetAsync<string>(Arg.Is<string>(string.Format(Queries.FetchAssetUIdsWithUserCustomerAndAssets,
                string.Join(",", _assetUIDs.Select(x => "UNHEX('" + x + "')")).Replace("-", string.Empty))), Arg.Is<dynamic>(request));
        }

        [Fact]
        public void FetchEssentialAssets_ValidRequestWithAssetIdFilter_ValidQuery()
        {
            AssetSettingsListRequestDto request = new AssetSettingsListRequestDto
            {
                CustomerUid = Guid.NewGuid().ToString("N"),
                UserUid = Guid.NewGuid().ToString("N"),
                PageNumber = 1,
                PageSize = 10,
                SortColumn = "AssetId",
                SortDirection = "ASC",
                FilterName = "AssetId",
                FilterValue = "Fuel"
            };

            _assetSettingsListRepository.FetchEssentialAssets(request);

            var queryExpected = string.Format(Queries.FetchAssetsForCustomerAndUserUId + Queries.SelectFoundRows,
                (request.PageNumber > 0) ? string.Format(Queries.LimitClause, (request.PageNumber - 1) * request.PageSize, request.PageSize) : string.Empty, // Limit offset and limit page size
                string.Format(Queries.OrderByClause, Constants.AssetSettingsSortConfig[(AssetSettingsSortColumns)Enum.Parse(typeof(AssetSettingsSortColumns), request.SortColumn)], request.SortDirection), // order by clause                
                string.Format("AND " + Constants.AssetSettingsFilterConfig[(AssetSettingsFilters)Enum.Parse(typeof(AssetSettingsFilters), request.FilterName)], request.FilterValue),
                request.DeviceType); // where clause

            _stubTransactions.Received(1).GetMultipleResultSetAsync<AssetSettingsListDto, long>(Arg.Is<string>(queryExpected), Arg.Any<dynamic>());
        }

        [Fact]
        public void FetchEssentialAssets_ValidRequestWithAssetSerialNumberFilter_ValidQuery()
        {
            AssetSettingsListRequestDto request = new AssetSettingsListRequestDto
            {
                CustomerUid = Guid.NewGuid().ToString("N"),
                UserUid = Guid.NewGuid().ToString("N"),
                PageNumber = 1,
                PageSize = 10,
                SortColumn = "AssetId",
                SortDirection = "ASC",
                FilterName = "AssetSerialNumber",
                FilterValue = "Fuel"
            };

            _assetSettingsListRepository.FetchEssentialAssets(request);

            var queryExpected = string.Format(Queries.FetchAssetsForCustomerAndUserUId + Queries.SelectFoundRows,
                (request.PageNumber > 0) ? string.Format(Queries.LimitClause, (request.PageNumber - 1) * request.PageSize, request.PageSize) : string.Empty, // Limit offset and limit page size
                string.Format(Queries.OrderByClause, Constants.AssetSettingsSortConfig[(AssetSettingsSortColumns)Enum.Parse(typeof(AssetSettingsSortColumns), request.SortColumn)], request.SortDirection), // order by clause                
                string.Format("AND " + Constants.AssetSettingsFilterConfig[(AssetSettingsFilters)Enum.Parse(typeof(AssetSettingsFilters), request.FilterName)], request.FilterValue),
                request.DeviceType); // where clause

            _stubTransactions.Received(1).GetMultipleResultSetAsync<AssetSettingsListDto, long>(Arg.Is<string>(queryExpected), Arg.Any<dynamic>());
        }

        [Fact]
        public void FetchEssentialAssets_InvalidRequestWithAssetSerialNumberFilter_ThrowsException()
        {
            AssetSettingsListRequestDto request = new AssetSettingsListRequestDto
            {
                CustomerUid = Guid.NewGuid().ToString("N"),
                UserUid = Guid.NewGuid().ToString("N"),
                PageNumber = 1,
                PageSize = 10,
                SortColumn = "AssetId",
                SortDirection = "ASC",
                FilterName = "InvalidFilterName",
                FilterValue = "Fuel"
            };
            try
            {
                var values = _assetSettingsListRepository.FetchEssentialAssets(request).Result;
            }
            catch(AggregateException ex)
            {
                Assert.NotNull(ex.InnerException);
                var exception = ex.InnerException;
                var queryExpected = string.Format(Queries.FetchAssetsForCustomerAndUserUId + Queries.SelectFoundRows,
                (request.PageNumber > 0) ? string.Format(Queries.LimitClause, (request.PageNumber - 1) * request.PageSize, request.PageSize) : string.Empty, // Limit offset and limit page size
                string.Format(Queries.OrderByClause, Constants.AssetSettingsSortConfig[(AssetSettingsSortColumns)Enum.Parse(typeof(AssetSettingsSortColumns), request.SortColumn)], request.SortDirection), // order by clause                
                string.Format("AND a.InvalidFilterName LIKE '%{0}%'", request.FilterValue),
                request.DeviceType);

                _stubTransactions.DidNotReceive().GetMultipleResultSetAsync<AssetSettingsListDto, long>(Arg.Any<string>(), Arg.Any<dynamic>());

                Assert.NotNull(exception.Message);
                Assert.Equal(exception.Message, "Requested value 'InvalidFilterName' was not found.");
            }
        }

        [Fact]
        public void FetchEssentialAssets_FilterValueWithPercentageWildCardOperators_ValidQuery()
        {
            AssetSettingsListRequestDto request = new AssetSettingsListRequestDto
            {
                CustomerUid = Guid.NewGuid().ToString("N"),
                UserUid = Guid.NewGuid().ToString("N"),
                PageNumber = 1,
                PageSize = 10,
                SortColumn = "AssetId",
                SortDirection = "ASC",
                FilterName = "AssetId",
                FilterValue = "%"
            };


			_stubTransactions
				.GetMultipleResultSetAsync<AssetSettingsListDto, long>(Arg.Any<string>(), Arg.Any<object>())
				.ReturnsForAnyArgs(x =>
				{
					return new Tuple<IEnumerable<AssetSettingsListDto>, IEnumerable<long>>(
						new List<AssetSettingsListDto>(), new List<long> { 0 });
				});

			var values = _assetSettingsListRepository.FetchEssentialAssets(request).Result;

            var queryExpected = string.Format(Queries.FetchAssetsForCustomerAndUserUId + Queries.SelectFoundRows,
                (request.PageNumber > 0) ? string.Format(Queries.LimitClause, (request.PageNumber - 1) * request.PageSize, request.PageSize) : string.Empty, // Limit offset and limit page size
                string.Format(Queries.OrderByClause, Constants.AssetSettingsSortConfig[(AssetSettingsSortColumns)Enum.Parse(typeof(AssetSettingsSortColumns), request.SortColumn)], request.SortDirection), // order by clause                
                string.Format("AND a.AssetName LIKE '%{0}%'", @"\%"),
                request.DeviceType);

            _stubTransactions.Received(1).GetMultipleResultSetAsync<AssetSettingsListDto, long>(Arg.Is<string>(queryExpected), Arg.Any<dynamic>());
        }

        [Fact]
        public void FetchEssentialAssets_FilterValueWithUnderscoreWildCardOperators_ValidQuery()
        {
            AssetSettingsListRequestDto request = new AssetSettingsListRequestDto
            {
                CustomerUid = Guid.NewGuid().ToString("N"),
                UserUid = Guid.NewGuid().ToString("N"),
                PageNumber = 1,
                PageSize = 10,
                SortColumn = "AssetId",
                SortDirection = "ASC",
                FilterName = "AssetId",
                FilterValue = "_"
            };


			_stubTransactions
				.GetMultipleResultSetAsync<AssetSettingsListDto, long>(Arg.Any<string>(), Arg.Any<object>())
				.ReturnsForAnyArgs(x =>
				{
					return new Tuple<IEnumerable<AssetSettingsListDto>, IEnumerable<long>>(
						new List<AssetSettingsListDto>(), new List<long> { 0 });
				});

			var values = _assetSettingsListRepository.FetchEssentialAssets(request).Result;

            var queryExpected = string.Format(Queries.FetchAssetsForCustomerAndUserUId + Queries.SelectFoundRows,
                (request.PageNumber > 0) ? string.Format(Queries.LimitClause, (request.PageNumber - 1) * request.PageSize, request.PageSize) : string.Empty, // Limit offset and limit page size
                string.Format(Queries.OrderByClause, Constants.AssetSettingsSortConfig[(AssetSettingsSortColumns)Enum.Parse(typeof(AssetSettingsSortColumns), request.SortColumn)], request.SortDirection), // order by clause                
                string.Format("AND a.AssetName LIKE '%{0}%'", @"\_"),
                request.DeviceType);

            _stubTransactions.Received(1).GetMultipleResultSetAsync<AssetSettingsListDto, long>(Arg.Is<string>(queryExpected), Arg.Any<dynamic>());
        }

   //     [Fact]
   //     public void FetchEssentialAssets_FilterValueAndDeviceTypeFilter_ValidQuery()
   //     {
   //         AssetSettingsListRequestDto request = new AssetSettingsListRequestDto
   //         {
   //             CustomerUid = Guid.NewGuid().ToString("N"),
   //             UserUid = Guid.NewGuid().ToString("N"),
   //             PageNumber = 1,
   //             PageSize = 10,
   //             SortColumn = "AssetId",
   //             SortDirection = "ASC",
   //             FilterName = "AssetId",
   //             FilterValue = "Fuel",
   //             DeviceType = "PL"
   //         };

			//_stubTransactions
			//	.GetMultipleResultSetAsync<AssetSettingsListDto, long>(Arg.Any<string>(), Arg.Any<object>())
			//	.ReturnsForAnyArgs(x =>
			//	{
			//		return new Tuple<IEnumerable<AssetSettingsListDto>, IEnumerable<long>>(
			//			new List<AssetSettingsListDto>(), new List<long> { 0 });
			//	});

			//var values = _assetSettingsListRepository.FetchEssentialAssets(request).Result;

   //         var queryExpected = string.Format(string.Concat(Queries.FetchAssetsForCustomerAndUserUId, Queries.SelectFoundRows),
			//		(request.PageNumber > 0) ? string.Format(Queries.LimitClause, (request.PageNumber - 1) * request.PageSize, request.PageSize) : string.Empty, // Limit offset and limit page size
			//		!string.IsNullOrEmpty(request.SortColumn) ? string.Format(Queries.OrderByClause, "AssetName", request.SortDirection) : string.Empty, // order by clause                
			//		"AND a.AssetName LIKE '%Fuel%' AND d.DeviceType = 'PL'", // FilterCriteria
			//		request.DeviceType);

   //         _stubTransactions.Received(1).GetMultipleResultSetAsync<AssetSettingsListDto, long>(Arg.Is<string>(queryExpected), Arg.Any<dynamic>());
   //     }

   //     [Fact]
   //     public void FetchDeviceTypes_ValidRequest_ValidQuery()
   //     {
   //         AssetDeviceTypeRequest request = new AssetDeviceTypeRequest
   //         {
   //             CustomerUid = Guid.NewGuid(),
   //             UserUid = Guid.NewGuid(),
   //             StatusInd = 1,
			//	AssetUIDs = new List<Guid> { Guid.NewGuid() }
   //         };

			//_stubTransactions
			//	.GetMultipleResultSetAsync<DeviceTypeDto, long>(Arg.Any<string>(), Arg.Any<object>())
			//	.ReturnsForAnyArgs(x => 
			//	{ 
			//		return new Tuple<IEnumerable<DeviceTypeDto>, IEnumerable<long>>(
			//			new List<DeviceTypeDto> 
			//			{
			//				new DeviceTypeDto
			//				{
			//					AssetCount = 12,
			//					DeviceType = "PL121"
			//				}
			//			}, new List<long> { 20 }); 
			//	});

   //         var values = _assetSettingsListRepository.FetchDeviceTypesByAssetUID(request).Result;

   //         var queryExpected = string.Format(Queries.FetchDeviceTypesForCustomerAndUser, string.Empty, string.Format(Queries.OrderByClause, "DeviceType", "ASC"), Queries.SelectFoundRows, "And ua.fk_AssetUID IN (UNHEX('" + request.AssetUIDs.First().ToString("N") +"'))");

   //         _stubTransactions.Received(1).GetMultipleResultSetAsync<DeviceTypeDto, long>(Arg.Is(queryExpected), Arg.Any<dynamic>());
   //     }

        //[Fact]
        public void FetchValidAssetUIDs_ValidRequest_ValidQuery()
        {
            AssetSettingsListRequestDto request = new AssetSettingsListRequestDto
            {
                CustomerUid = Guid.NewGuid().ToString("N"),
                UserUid = Guid.NewGuid().ToString("N"),
                StatusInd = 1
            };

            List<string> assetUIDs = new List<string> { Guid.NewGuid().ToString("N"), Guid.NewGuid().ToString("N"), Guid.NewGuid().ToString("N") };
            var values = _assetSettingsListRepository.FetchValidAssetUIds(assetUIDs, request).Result;

            var queryExpected = string.Format(Queries.FetchAssetUIdsWithUserCustomerAndAssets, string.Join(",", assetUIDs.Select(x => "UNHEX('" + x + "')")).Replace("-", string.Empty));

            _stubTransactions.Received(1).GetAsync<string>(Arg.Is<string>(queryExpected), Arg.Any<dynamic>());
        }
    }
}
