using AssetSettingsRepository;
using Interfaces;
using DbModel.AssetSettings;
using CommonModel.Enum;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities.Logging;
using VSS.MasterData.WebAPI.Transactions;
using Xunit;
using AssetConfigRepository;

namespace VSP.MasterData.Asset.Data.Tests
{
	public class AssetSettingsRepositoryTests
    {
        private readonly ITransactions _transactions;
        private readonly ILoggingService _stubLoggingService;
        private readonly IAssetConfigRepository _assetSettingsRepository;
        private readonly List<string> _assetUIDs;

        public AssetSettingsRepositoryTests()
        {
            _transactions = Substitute.For<ITransactions>();
            _stubLoggingService = Substitute.For<ILoggingService>();
            _assetSettingsRepository = new AssetConfigRepository.AssetConfigRepository(_transactions, _stubLoggingService);
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
        public void FetchAssetConfig_ValidRequestWithFilterValueString_ValidQuery()
        {
            AssetSettingsDto request = new AssetSettingsDto
            {               
                TargetValues = new Dictionary<AssetTargetType, Tuple<Guid, double>>
                {
                    { AssetTargetType.IdlingBurnRateinLiPerHour, new Tuple<Guid, double>(Guid.NewGuid(), 12) },
                    { AssetTargetType.WorkingBurnRateinLiPerHour, new Tuple<Guid, double>(Guid.NewGuid(), 22) }
                },
                FilterCriteria = new List<KeyValuePair<string, Tuple<string, object>>>
                {
                    new KeyValuePair<string, Tuple<string, object>>("LIKE", Tuple.Create<string, object>("a.AssetName", "fuel"))
                }
            };

            var result = _assetSettingsRepository.FetchAssetConfig(_assetUIDs, request).Result;
            
            var queryExpected = string.Format(
					AssetConfigRepository.Queries.FetchAssetConfig,
                    string.Join(",", _assetUIDs.Select(x => "UNHEX('" + x + "')")).Replace("-", string.Empty), //Asset UIDs Lists
                    "a.AssetName LIKE 'fuel' AND ",
                    string.Join(",", request.TargetValues.Keys.Select(x => string.Format("'{0}'", x.ToString())))
                    );

            _transactions.Received(1).GetAsync<AssetSettingsDto>(Arg.Is<string>(queryExpected), Arg.Is<dynamic>(request));
        }

        [Fact]
        public void FetchSingleAssetConfig_ValidRequestWithFilterValueDateTime_ValidQuery()
        {
            var currentDateTime = DateTime.Now;
            AssetSettingsDto request = new AssetSettingsDto
            {
                TargetValues = new Dictionary<AssetTargetType, Tuple<Guid, double>>
                {
                    { AssetTargetType.WorkingBurnRateinLiPerHour, new Tuple<Guid, double>(Guid.NewGuid(), 22) }
                },
                FilterCriteria = new List<KeyValuePair<string, Tuple<string, object>>>
                {
                    new KeyValuePair<string, Tuple<string, object>>("LIKE", Tuple.Create<string, object>("a.AssetName", currentDateTime))
                }
            };

            var result = _assetSettingsRepository.FetchAssetConfig(_assetUIDs, request).Result;

            var queryExpected = string.Format(
					AssetConfigRepository.Queries.FetchAssetConfig,
                    string.Join(",", _assetUIDs.Select(x => "UNHEX('" + x + "')")).Replace("-", string.Empty), //Asset UIDs Lists
                    "a.AssetName LIKE '" + currentDateTime + "' AND ",
                    string.Join(",", request.TargetValues.Keys.Select(x => string.Format("'{0}'", x.ToString())))
                    );

            _transactions.Received(1).GetAsync<AssetSettingsDto>(Arg.Is<string>(queryExpected), Arg.Is<dynamic>(request));
        }

        [Fact]
        public void FetchTwoAssetConfig_ValidRequestWithFilterValueDateTime_ValidQuery()
        {
            var currentDateTime = DateTime.Now;
            AssetSettingsDto request = new AssetSettingsDto
            {
                TargetValues = new Dictionary<AssetTargetType, Tuple<Guid, double>>
                {
                    { AssetTargetType.IdlingBurnRateinLiPerHour, new Tuple<Guid, double>(Guid.NewGuid(), 12) },
                    { AssetTargetType.WorkingBurnRateinLiPerHour, new Tuple<Guid, double>(Guid.NewGuid(), 22) }
                },
                FilterCriteria = new List<KeyValuePair<string, Tuple<string, object>>>
                {
                    new KeyValuePair<string, Tuple<string, object>>("LIKE", Tuple.Create<string, object>("a.AssetName", currentDateTime))
                }
            };

            var result = _assetSettingsRepository.FetchAssetConfig(_assetUIDs, request).Result;

            var queryExpected = string.Format(
					AssetConfigRepository.Queries.FetchAssetConfig,
                    string.Join(",", _assetUIDs.Select(x => "UNHEX('" + x + "')")).Replace("-", string.Empty), //Asset UIDs Lists
                    "a.AssetName LIKE '"+ currentDateTime + "' AND ",
                    string.Join(",", request.TargetValues.Keys.Select(x => string.Format("'{0}'", x.ToString())))
                    );

            _transactions.Received(1).GetAsync<AssetSettingsDto>(Arg.Is<string>(queryExpected), Arg.Is<dynamic>(request));
        }

        [Fact]
        public void FetchTwoAssetConfig_ValidRequestWithFilterValueNull_ValidQuery()
        {
            var currentDateTime = DateTime.Now;
            AssetSettingsDto request = new AssetSettingsDto
            {
                TargetValues = new Dictionary<AssetTargetType, Tuple<Guid, double>>
                {
                    { AssetTargetType.IdlingBurnRateinLiPerHour, new Tuple<Guid, double>(Guid.NewGuid(), 12) },
                    { AssetTargetType.WorkingBurnRateinLiPerHour, new Tuple<Guid, double>(Guid.NewGuid(), 22) }
                },
                FilterCriteria = new List<KeyValuePair<string, Tuple<string, object>>>
                {
                    new KeyValuePair<string, Tuple<string, object>>("LIKE", Tuple.Create<string, object>("a.AssetName", null))
                }
            };

            var result = _assetSettingsRepository.FetchAssetConfig(_assetUIDs, request).Result;

            var queryExpected = string.Format(
					AssetConfigRepository.Queries.FetchAssetConfig,
                    string.Join(",", _assetUIDs.Select(x => "UNHEX('" + x + "')")).Replace("-", string.Empty), //Asset UIDs Lists
                    "a.AssetName is null AND ",
                    string.Join(",", request.TargetValues.Keys.Select(x => string.Format("'{0}'", x.ToString())))
                    );

            _transactions.Received(1).GetAsync<AssetSettingsDto>(Arg.Is<string>(queryExpected), Arg.Is<dynamic>(request));
        }
    }
}
