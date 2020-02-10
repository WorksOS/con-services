using Interfaces;
using CommonModel.AssetSettings;
using DbModel.AssetSettings;
using CommonModel.Enum;
using Infrastructure.Common.Helpers;
using Infrastructure.Service.AssetSettings.Interfaces;
using Infrastructure.Service.AssetSettings.Service;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities.Logging;
using Xunit;

namespace VSP.MasterData.Asset.Domain.UnitTests
{
	public class OverLapHandlerTests
    {
        private AssetSettingsOverlapTemplate _overlapTemplate;
        private IWeeklyAssetSettingsRepository _weeklyRepo;
        private IAssetSettingsPublisher _assetSettingsPublisher;
        private IAssetSettingsTypeHandler<AssetSettingsBase> _assetSettingsTypeHandler;
        private ILoggingService _loggingService;

        public OverLapHandlerTests()
        {
            _assetSettingsTypeHandler = new AssetSettingsTypeHandler<AssetSettingsBase>();
            _weeklyRepo = Substitute.For<IWeeklyAssetSettingsRepository>();
			_assetSettingsPublisher = Substitute.For<IAssetSettingsPublisher>();
            _loggingService = Substitute.For<ILoggingService>();
            _overlapTemplate = new AssetSettingsOverlapHandler(_weeklyRepo, _assetSettingsPublisher, _assetSettingsTypeHandler, _loggingService);
        }

        #region "AssetTargets"
        [Fact]
        public void AssetSettingsOverlapHandler_TestHandleOverlap_ForSingleOVerlapScenarios_AtTheStart_ExpectSuccess()
        {
            var sessionAssetGuid = Guid.NewGuid();
            var target = PrepareDataForInitialInsert_AssetTargets(sessionAssetGuid);
            var assetTargets = prepareDataForAssetTargets(sessionAssetGuid);
            var settingsHandled = _overlapTemplate.HandleOverlap(target, assetTargets, Infrastructure.Service.AssetSettings.Enums.GroupType.AssetTargets);
            var startDates = settingsHandled.Select(settings => settings.StartDate).ToList();
            var endDates = settingsHandled.Select(settings => settings.EndDate).ToList();
            
            Assert.True(settingsHandled != null);
            Assert.True(startDates.Contains(Convert.ToDateTime("2016.12.31")));
            Assert.True(startDates.Contains(Convert.ToDateTime("2017.01.07")));
            Assert.True(endDates.Contains(Convert.ToDateTime("2017.01.06")));
            Assert.True(endDates.Contains(Convert.ToDateTime("2017.04.10")));
        }

        [Fact]
        public void AssetSettingsOverlapHandler_TestHandleOverlap_ForSingleOVerlapScenarios_AtTheMiddle_ExpectSuccess()
        {
            var sessionAssetGuid = Guid.NewGuid();
            var target = PrepareDataForMiddleInsert_AssetTargets(sessionAssetGuid);
            var assetTargets = prepareDataForAssetTargetsForMiddleInsert(sessionAssetGuid);
            var settingsHandled = _overlapTemplate.HandleOverlap(target, assetTargets, Infrastructure.Service.AssetSettings.Enums.GroupType.AssetTargets);
            var startDates = settingsHandled.Select(settings => settings.StartDate).Distinct().ToList();
            var endDates = settingsHandled.Select(settings => settings.EndDate).Distinct().ToList();
            Assert.True(startDates.Contains(Convert.ToDateTime("2016-12-31")));
            Assert.True(startDates.Contains(Convert.ToDateTime("2017-01-15")));
            Assert.True(startDates.Contains(Convert.ToDateTime("2017-01-19")));
            Assert.True(endDates.Contains(Convert.ToDateTime("2017-03-10")));
            Assert.True(endDates.Contains(Convert.ToDateTime("2017-01-14")));
            Assert.True(endDates.Contains(Convert.ToDateTime("2017-01-18")));
            Assert.True(settingsHandled != null);
        }

        [Fact]
        public void AssetSettingsOverlapHandler_TestHandleOverlap_ForSingleOverlapScenarios_AtTheEnd_ExpectSuccess()
        {
            var sessionAssetGuid = Guid.NewGuid();
            var target = PrepareUserDataForEndInsert_AssetTargets(sessionAssetGuid);
            var assetTargets = prepareDataForAssetTargetsForEndInsert(sessionAssetGuid);
            var settingsHandled = _overlapTemplate.HandleOverlap(target, assetTargets, Infrastructure.Service.AssetSettings.Enums.GroupType.AssetTargets);

            var startDates = settingsHandled.Select(settings => settings.StartDate).Distinct().ToList();
            var endDates = settingsHandled.Select(settings => settings.EndDate).Distinct().ToList();
            Assert.True(startDates.Contains(Convert.ToDateTime("2016-12-31")));
            Assert.True(startDates.Contains(Convert.ToDateTime("2017-04-05")));

            Assert.True(endDates.Contains(Convert.ToDateTime("2017-04-04")));
            Assert.True(endDates.Contains(Convert.ToDateTime("2017-04-13")));
            Assert.True(settingsHandled != null);
        }

        [Fact]
        public void AssetSettingsOverlapHandler_TestHandleOVerlap_ForSingleOVerlapScenarios_FullOverlap_ExpectSuccess()
        {
            var sessionAssetGuid = Guid.NewGuid();
            var target = PrepareUserDataForEndInsert_AssetTargets(sessionAssetGuid);
            var assetTargets = prepareDataForAssetTargetsForEndInsert(sessionAssetGuid);
            var settingsHandled = _overlapTemplate.HandleOverlap(target, assetTargets, Infrastructure.Service.AssetSettings.Enums.GroupType.AssetTargets);
            var startDates = settingsHandled.Select(settings => settings.StartDate).Distinct().ToList();
            var endDates = settingsHandled.Select(settings => settings.EndDate).Distinct().ToList();

            Assert.True(startDates.Contains(Convert.ToDateTime("2016-12-31")));
            Assert.True(endDates.Contains(Convert.ToDateTime("2017-04-13")));
        }

        [Fact]
        public void AssetSettingsOverlapHandler_TestHandleOverlap_ForMultipleUpdateScenarios_OverlapAtTheEnd_ExpectSuccess()
        {
            var sessionAssetGuid = Guid.NewGuid();
            var target = PrepareDataForEndInsertMultipleOverlapScenarios_AssetTargets(sessionAssetGuid);
            var assetTargets = prepareDataForAssetTargetsFoorMultipleOverlapScenariosAtTheEnd(sessionAssetGuid);

            var settingsHandled = _overlapTemplate.HandleOverlap(target, assetTargets, Infrastructure.Service.AssetSettings.Enums.GroupType.AssetTargets);
            var startDates = settingsHandled.Select(settings => settings.StartDate).Distinct().ToList();
            var endDates = settingsHandled.Select(settings => settings.EndDate).Distinct().ToList();

            Assert.True(startDates.Contains(Convert.ToDateTime("2017-01-01")));
            Assert.True(startDates.Contains(Convert.ToDateTime("2017-01-12")));
            Assert.True(endDates.Contains(Convert.ToDateTime("2017-01-11")));
            Assert.True(endDates.Contains(Convert.ToDateTime("2017-01-19")));
        }

        [Fact]
        public void AssetSettingsOverlapHandler_TestHandleOverlap_ForMultipleUpdateScenarios_OverlapAtTheMiddle_ExpectSuccess()
        {
            var sessionAssetGuid = Guid.NewGuid();
            var target = PrepareDataForMiddleInsertMultipleOverlapScenariosMiddleInsert_AssetTargets(sessionAssetGuid);
            var assetTargets = prepareDataForAssetTargetsFoorMultipleOverlapScenarios(sessionAssetGuid);
            var settingsHandled = _overlapTemplate.HandleOverlap(target, assetTargets, Infrastructure.Service.AssetSettings.Enums.GroupType.AssetTargets);
            var startDates = settingsHandled.Select(settings => settings.StartDate).Distinct().ToList();
            var endDates = settingsHandled.Select(settings => settings.EndDate).Distinct().ToList();

            Assert.True(startDates.Contains(Convert.ToDateTime("2017-01-01")));
            Assert.True(startDates.Contains(Convert.ToDateTime("2017-01-13")));
            Assert.True(startDates.Contains(Convert.ToDateTime("2017-01-17")));
            Assert.True(endDates.Contains(Convert.ToDateTime("2017-01-12")));
            Assert.True(endDates.Contains(Convert.ToDateTime("2017-01-16")));//TODO: Assert.True(endDates.Contains(Convert.ToDateTime("2017-01-16"))) Assertion failed. tempfix to deploy in alpha.
            Assert.True(endDates.Contains(Convert.ToDateTime("2017-01-18")));
        }

        [Fact]
        public void AssetSettingsOverlapHandler_TestHandleOverlap_ForMultipleUpdateScenarios_OverlapAtTheStart_ExpectSuccess()
        {
            var sessionAssetGuid = Guid.NewGuid();
            var target = PrepareDataForMiddleInsertMultipleOverlapScenariosStartInsert_AssetTargets(sessionAssetGuid);
            var assetTargets = prepareDataForAssetTargetsFoorMultipleOverlapScenarios(sessionAssetGuid);
            var settingsHandled = _overlapTemplate.HandleOverlap(target, assetTargets, Infrastructure.Service.AssetSettings.Enums.GroupType.AssetTargets);
            var startDates = settingsHandled.Select(settings => settings.StartDate).Distinct().ToList();
            var endDates = settingsHandled.Select(settings => settings.EndDate).Distinct().ToList();

            Assert.True(startDates.Contains(Convert.ToDateTime("2016-12-31")));
            Assert.True(startDates.Contains(Convert.ToDateTime("2017-01-17")));
            Assert.True(endDates.Contains(Convert.ToDateTime("2017-01-16")));//TODO: Assert.True(endDates.Contains(Convert.ToDateTime("2017-01-16"))) assertion failed. tempfix to deploy in alpha.
            Assert.True(endDates.Contains(Convert.ToDateTime("2017-01-18")));
        }

        [Fact]
        public void AssetSettingsOverlapHandler_TestHandleOverlap_ForMultipleOverlaps_AtTheStart_ExpectSuccess()
        {
            var sessionAssetGuid = Guid.NewGuid();
            var target = PrepareUserDataForEndInsert_AssetTargets(sessionAssetGuid);
            var assetTargets = prepareDataForAssetTargetsForEndInsert(sessionAssetGuid);
            var settingsHandled = _overlapTemplate.HandleOverlap(target, assetTargets, Infrastructure.Service.AssetSettings.Enums.GroupType.AssetTargets);
            var startDates = settingsHandled.Select(settings => settings.StartDate).Distinct().ToList();
            var endDates = settingsHandled.Select(settings => settings.EndDate).Distinct().ToList();
        }

        [Fact]
        public void AssetSettingsOverlapHandler_TestHandleOverlap_ForMultipleOverlaps_FullOverlap_ExpectSuccess()
        {
            var sessionAssetGuid = Guid.NewGuid();
            var target = PrepareDataForMiddleInsertMultipleOverlapScenariosFullOverlap_MultipleOverlap_AssetTargets(sessionAssetGuid);
            var assetTargets = prepareDataForAssetTargetsFoorMultipleOverlapScenarios(sessionAssetGuid);
            var settingsHandled = _overlapTemplate.HandleOverlap(target, assetTargets, Infrastructure.Service.AssetSettings.Enums.GroupType.AssetTargets);
            var startDates = settingsHandled.Select(settings => settings.StartDate).Distinct().ToList();
            var endDates = settingsHandled.Select(settings => settings.EndDate).Distinct().ToList();

            Assert.True(startDates.Contains(target.StartDate));
            Assert.True(endDates.Contains(target.EndDate.Value));
        }

        [Fact]
        public void AssetSettingsOverlapHandler_TestHandleOverlap_ForMultipleUpdateScenarios_OverlapAt_ExpectSuccess()
        {
            var sessionAssetGuid = Guid.NewGuid();
            var target = PrepareDataForMiddleInsertMultipleOverlapScenariosAtInsert_AssetTargets(sessionAssetGuid);
            var assetTargets = prepareDataForAssetTargetsFoorMultipleOverlapScenariosTo(sessionAssetGuid);
            var settingsHandled = _overlapTemplate.HandleOverlap(target, assetTargets, Infrastructure.Service.AssetSettings.Enums.GroupType.AssetTargets);
            var startDates = settingsHandled.Select(settings => settings.StartDate).Distinct().ToList();
            var endDates = settingsHandled.Select(settings => settings.EndDate).Distinct().ToList();

            Assert.True(startDates.Contains(Convert.ToDateTime("2017-04-12")));
            Assert.True(endDates.Contains(Convert.ToDateTime("2017-04-20")));//TODO: Assert.True(endDates.Contains(Convert.ToDateTime("2017-04-20"))) Assertion failed. tempfix to deploy in alpha.
            Assert.True(startDates.Contains(Convert.ToDateTime("2017-04-21")));
            Assert.True(endDates.Contains(Convert.ToDateTime("2017-04-27")));
        }

        #endregion

        #region "ProductivityTargetsOverlapTests"
        [Fact]
        public void AssetSettingsOverlapHandler_TestHandleOverlap_ForProductivityTargets_ForSingleOVerlapScenarios_AtTheStart_ExpectSuccess()
        {
            var sessionAssetGuid = Guid.NewGuid();
            var target = PrepareDataForInitialInsert_ProductivityTargets(sessionAssetGuid);
            var assetTargets = prepareDataForProductivityTargets(sessionAssetGuid);
            var settingsHandled = _overlapTemplate.HandleOverlap(target, assetTargets, Infrastructure.Service.AssetSettings.Enums.GroupType.ProductivityTargets);
            //_weeklyRepo.Received(2).InsertAssetTargets(Arg.Any<List<AssetSettingsGetDBResponse>>());
            var startDates = settingsHandled.Select(settings => settings.StartDate).ToList();
            var endDates = settingsHandled.Select(settings => settings.EndDate).ToList();

            Assert.True(settingsHandled != null);
            Assert.True(startDates.Contains(Convert.ToDateTime("2016.12.31")));
            Assert.True(startDates.Contains(Convert.ToDateTime("2017.01.07")));
            Assert.True(endDates.Contains(Convert.ToDateTime("2017.01.06")));
            Assert.True(endDates.Contains(Convert.ToDateTime("2017.04.10")));
        }

        [Fact]
        public void AssetSettingsOverlapHandler_TestHandleOverlap_ForProductivityTargets_ForSingleOVerlapScenarios_AtTheMiddle_ExpectSuccess()
        {
            var sessionAssetGuid = Guid.NewGuid();
            var target = PrepareDataForMiddleInsert_ProductivityTargets(sessionAssetGuid);
            var assetTargets = prepareDataForProductivityTargetsForMiddleInsert(sessionAssetGuid);
            var settingsHandled = _overlapTemplate.HandleOverlap(target, assetTargets, Infrastructure.Service.AssetSettings.Enums.GroupType.ProductivityTargets);
            //_weeklyRepo.Received(2).InsertAssetTargets(Arg.Any<List<AssetSettingsGetDBResponse>>());
            var startDates = settingsHandled.Select(settings => settings.StartDate).Distinct().ToList();
            var endDates = settingsHandled.Select(settings => settings.EndDate).Distinct().ToList();
            Assert.True(startDates.Contains(Convert.ToDateTime("2016-12-31")));
            Assert.True(startDates.Contains(Convert.ToDateTime("2017-01-15")));
            Assert.True(startDates.Contains(Convert.ToDateTime("2017-01-19")));
            Assert.True(endDates.Contains(Convert.ToDateTime("2017-03-10")));
            Assert.True(endDates.Contains(Convert.ToDateTime("2017-01-14")));
            Assert.True(endDates.Contains(Convert.ToDateTime("2017-01-18")));
            Assert.True(settingsHandled != null);
        }

        [Fact]
        public void AssetSettingsOverlapHandler_TestHandleOverlap_ForProductivityTargets_ForSingleOverlapScenarios_AtTheEnd_ExpectSuccess()
        {
            var sessionAssetGuid = Guid.NewGuid();
            var target = PrepareUserDataForEndInsert_ProductivityTargets(sessionAssetGuid);
            var assetTargets = prepareDataForProductivityTargetsForEndInsert(sessionAssetGuid);
            var settingsHandled = _overlapTemplate.HandleOverlap(target, assetTargets, Infrastructure.Service.AssetSettings.Enums.GroupType.ProductivityTargets);

            var startDates = settingsHandled.Select(settings => settings.StartDate).Distinct().ToList();
            var endDates = settingsHandled.Select(settings => settings.EndDate).Distinct().ToList();
            Assert.True(startDates.Contains(Convert.ToDateTime("2016-12-31")));
            Assert.True(startDates.Contains(Convert.ToDateTime("2017-04-05")));

            Assert.True(endDates.Contains(Convert.ToDateTime("2017-04-04")));
            Assert.True(endDates.Contains(Convert.ToDateTime("2017-04-13")));
            Assert.True(settingsHandled != null);
        }

        [Fact]
        public void AssetSettingsOverlapHandler_TestHandleOVerlap_ForProductivityTargets_ForSingleOVerlapScenarios_FullOverlap_ExpectSuccess()
        {
            var sessionAssetGuid = Guid.NewGuid();
            var target = PrepareUserDataForEndInsert_ProductivityTargets(sessionAssetGuid);
            var assetTargets = prepareDataForProductivityTargetsForEndInsert(sessionAssetGuid);
            var settingsHandled = _overlapTemplate.HandleOverlap(target, assetTargets, Infrastructure.Service.AssetSettings.Enums.GroupType.ProductivityTargets);
            var startDates = settingsHandled.Select(settings => settings.StartDate).Distinct().ToList();
            var endDates = settingsHandled.Select(settings => settings.EndDate).Distinct().ToList();

            Assert.True(startDates.Contains(Convert.ToDateTime("2016-12-31")));
            Assert.True(endDates.Contains(Convert.ToDateTime("2017-04-13")));
        }

        [Fact]
        public void AssetSettingsOverlapHandler_TestHandleOverlap_ForProductivityTargets_ForMultipleUpdateScenarios_OverlapAtTheEnd_ExpectSuccess()
        {
            var sessionAssetGuid = Guid.NewGuid();
            var target = PrepareDataForEndInsertMultipleOverlapScenarios_ProductivityTargets(sessionAssetGuid);
            var assetTargets = prepareDataForProductivityTargetsFoorMultipleOverlapScenariosAtTheEnd(sessionAssetGuid);

            var settingsHandled = _overlapTemplate.HandleOverlap(target, assetTargets, Infrastructure.Service.AssetSettings.Enums.GroupType.ProductivityTargets);
            var startDates = settingsHandled.Select(settings => settings.StartDate).Distinct().ToList();
            var endDates = settingsHandled.Select(settings => settings.EndDate).Distinct().ToList();

            Assert.True(startDates.Contains(Convert.ToDateTime("2017-01-01")));
            Assert.True(startDates.Contains(Convert.ToDateTime("2017-01-12")));
            Assert.True(endDates.Contains(Convert.ToDateTime("2017-01-11")));
            Assert.True(endDates.Contains(Convert.ToDateTime("2017-01-19")));
        }

        [Fact]
        public void AssetSettingsOverlapHandler_TestHandleOverlap_ForProductivityTargets_ForMultipleUpdateScenarios_OverlapAtTheMiddle_ExpectSuccess()
        {
            var sessionAssetGuid = Guid.NewGuid();
            var target = PrepareDataForMiddleInsertMultipleOverlapScenariosMiddleInsert_ProductivityTargets(sessionAssetGuid);
            var assetTargets = prepareDataForProductivityTargetsFoorMultipleOverlapScenarios(sessionAssetGuid);
            var settingsHandled = _overlapTemplate.HandleOverlap(target, assetTargets, Infrastructure.Service.AssetSettings.Enums.GroupType.ProductivityTargets);
            var startDates = settingsHandled.Select(settings => settings.StartDate).Distinct().ToList();
            var endDates = settingsHandled.Select(settings => settings.EndDate).Distinct().ToList();

            Assert.True(startDates.Contains(Convert.ToDateTime("2017-01-01")));
            Assert.True(startDates.Contains(Convert.ToDateTime("2017-01-13")));
            Assert.True(startDates.Contains(Convert.ToDateTime("2017-01-17")));
            Assert.True(endDates.Contains(Convert.ToDateTime("2017-01-12")));
            Assert.True(endDates.Contains(Convert.ToDateTime("2017-01-16"))); 
            Assert.True(endDates.Contains(Convert.ToDateTime("2017-01-18")));
        }

        [Fact]
        public void AssetSettingsOverlapHandler_TestHandleOverlap_ForProductivityTargets_ForMultipleUpdateScenarios_OverlapAtTheStart_ExpectSuccess()
        {
            var sessionAssetGuid = Guid.NewGuid();
            var target = PrepareDataForMiddleInsertMultipleOverlapScenariosStartInsert_ProductivityTargets(sessionAssetGuid);
            var assetTargets = prepareDataForProductivityTargetsFoorMultipleOverlapScenarios(sessionAssetGuid);
            var settingsHandled = _overlapTemplate.HandleOverlap(target, assetTargets, Infrastructure.Service.AssetSettings.Enums.GroupType.ProductivityTargets);
            var startDates = settingsHandled.Select(settings => settings.StartDate).Distinct().ToList();
            var endDates = settingsHandled.Select(settings => settings.EndDate).Distinct().ToList();

            Assert.True(startDates.Contains(Convert.ToDateTime("2016-12-31")));
            Assert.True(startDates.Contains(Convert.ToDateTime("2017-01-17")));
            Assert.True(endDates.Contains(Convert.ToDateTime("2017-01-16"))); 
            Assert.True(endDates.Contains(Convert.ToDateTime("2017-01-18")));
        }

        [Fact]
        public void AssetSettingsOverlapHandler_TestHandleOverlap_ForProductivityTargets_ForMultipleOverlaps_AtTheStart_ExpectSuccess()
        {
            var sessionAssetGuid = Guid.NewGuid();
            var target = PrepareUserDataForEndInsert_ProductivityTargets(sessionAssetGuid);
            var assetTargets = prepareDataForProductivityTargetsForEndInsert(sessionAssetGuid);
            var settingsHandled = _overlapTemplate.HandleOverlap(target, assetTargets, Infrastructure.Service.AssetSettings.Enums.GroupType.ProductivityTargets);
            var startDates = settingsHandled.Select(settings => settings.StartDate).Distinct().ToList();
            var endDates = settingsHandled.Select(settings => settings.EndDate).Distinct().ToList();
        }

        [Fact]
        public void AssetSettingsOverlapHandler_TestHandleOverlap_ForProductivityTargets_ForMultipleOverlaps_FullOverlap_ExpectSuccess()
        {
            var sessionAssetGuid = Guid.NewGuid();
            var target = PrepareDataForMiddleInsertMultipleOverlapScenariosFullOverlap_MultipleOverlap_ProductivityTargets(sessionAssetGuid);
            var assetTargets = prepareDataForProductivityTargetsFoorMultipleOverlapScenarios(sessionAssetGuid);
            var settingsHandled = _overlapTemplate.HandleOverlap(target, assetTargets, Infrastructure.Service.AssetSettings.Enums.GroupType.ProductivityTargets);
            var startDates = settingsHandled.Select(settings => settings.StartDate).Distinct().ToList();
            var endDates = settingsHandled.Select(settings => settings.EndDate).Distinct().ToList();

            Assert.True(startDates.Contains(target.StartDate));
            Assert.True(endDates.Contains(target.EndDate.Value));
        }

        #endregion

        #region "Private Methods"

        #region "DBDataMocks
        private static List<AssetSettingsGetDBResponse> prepareDataForAssetTargets(Guid assetUID)
        {
            return new List<AssetSettingsGetDBResponse> {
                new AssetSettingsGetDBResponse
            {
                AssetID = assetUID.ToStringWithoutHyphens(),
                AssetWeeklyConfigUID = Guid.NewGuid().ToStringWithoutHyphens(),
                ConfigType = AssetTargetType.RuntimeHours.ToString(),
                StartDate = Convert.ToDateTime("2017-01-01"),
                EndDate = Convert.ToDateTime("2017-04-10"),
                Sunday = 0.0,
                Monday = 8.0,
                Tuesday = 8.0,
                Wednesday = 8.0,
                Thursday = 8.0,
                Friday = 8.0,
                Saturday = 0.0
            },
                new AssetSettingsGetDBResponse
            {
                AssetID = assetUID.ToStringWithoutHyphens(),
                AssetWeeklyConfigUID = Guid.NewGuid().ToStringWithoutHyphens(),
                ConfigType = AssetTargetType.IdletimeHours.ToString(),
                StartDate = Convert.ToDateTime("2017-01-01"),
                EndDate = Convert.ToDateTime("2017-04-10"),
                Sunday = 0.0,
                Monday = 0.0,
                Tuesday = 0.0,
                Wednesday = 0.0,
                Thursday = 0.0,
                Friday = 0.0,
                Saturday = 0.0
            },
            };
        }
        private static List<AssetSettingsGetDBResponse> prepareDataForProductivityTargets(Guid assetUID)
        {
            return new List<AssetSettingsGetDBResponse> {
                new AssetSettingsGetDBResponse
            {
                AssetID = assetUID.ToStringWithoutHyphens(),
                AssetWeeklyConfigUID = Guid.NewGuid().ToStringWithoutHyphens(),
                ConfigType = AssetTargetType.CycleCount.ToString(),
                StartDate = Convert.ToDateTime("2017-01-01"),
                EndDate = Convert.ToDateTime("2017-04-10"),
                Sunday = 0.0,
                Monday = 8.0,
                Tuesday = 8.0,
                Wednesday = 8.0,
                Thursday = 8.0,
                Friday = 8.0,
                Saturday = 0.0
            },
                new AssetSettingsGetDBResponse
            {
                AssetID = assetUID.ToStringWithoutHyphens(),
                AssetWeeklyConfigUID = Guid.NewGuid().ToStringWithoutHyphens(),
                ConfigType = AssetTargetType.PayloadinTonnes.ToString(),
                StartDate = Convert.ToDateTime("2017-01-01"),
                EndDate = Convert.ToDateTime("2017-04-10"),
                Sunday = 0.0,
                Monday = 0.0,
                Tuesday = 0.0,
                Wednesday = 0.0,
                Thursday = 0.0,
                Friday = 0.0,
                Saturday = 0.0
            },
                new AssetSettingsGetDBResponse
            {
                AssetID = assetUID.ToStringWithoutHyphens(),
                AssetWeeklyConfigUID = Guid.NewGuid().ToStringWithoutHyphens(),
                ConfigType = AssetTargetType.VolumeinCuMeter.ToString(),
                StartDate = Convert.ToDateTime("2017-01-01"),
                EndDate = Convert.ToDateTime("2017-04-10"),
                Sunday = 0.0,
                Monday = 0.0,
                Tuesday = 0.0,
                Wednesday = 0.0,
                Thursday = 0.0,
                Friday = 0.0,
                Saturday = 0.0
            }
            };
        }
        private static List<AssetSettingsGetDBResponse> prepareDataForAssetTargetsForMiddleInsert(Guid assetUID)
        {
            return new List<AssetSettingsGetDBResponse> {
                new AssetSettingsGetDBResponse
            {
                AssetID = assetUID.ToStringWithoutHyphens(),
                AssetWeeklyConfigUID = Guid.NewGuid().ToStringWithoutHyphens(),
                ConfigType = AssetTargetType.RuntimeHours.ToString(),
                StartDate = Convert.ToDateTime("2016-12-31"),
                EndDate = Convert.ToDateTime("2017-03-10"),
                Sunday = 0.0,
                Monday = 8.0,
                Tuesday = 8.0,
                Wednesday = 8.0,
                Thursday = 8.0,
                Friday = 8.0,
                Saturday = 0.0
            },
                new AssetSettingsGetDBResponse
            {
                AssetID = assetUID.ToStringWithoutHyphens(),
                AssetWeeklyConfigUID = Guid.NewGuid().ToStringWithoutHyphens(),
                ConfigType = AssetTargetType.IdletimeHours.ToString(),
                StartDate = Convert.ToDateTime("2016-12-31"),
                EndDate = Convert.ToDateTime("2017-03-10"),
                Sunday = 0.0,
                Monday = 0.0,
                Tuesday = 0.0,
                Wednesday = 0.0,
                Thursday = 0.0,
                Friday = 0.0,
                Saturday = 0.0
            }
            };

        }
        private static List<AssetSettingsGetDBResponse> prepareDataForProductivityTargetsForMiddleInsert(Guid assetUID)
        {
            return new List<AssetSettingsGetDBResponse> {
                new AssetSettingsGetDBResponse
            {
                AssetID = assetUID.ToStringWithoutHyphens(),
                AssetWeeklyConfigUID = Guid.NewGuid().ToStringWithoutHyphens(),
                ConfigType = AssetTargetType.CycleCount.ToString(),
                StartDate = Convert.ToDateTime("2016-12-31"),
                EndDate = Convert.ToDateTime("2017-03-10"),
                Sunday = 0.0,
                Monday = 8.0,
                Tuesday = 8.0,
                Wednesday = 8.0,
                Thursday = 8.0,
                Friday = 8.0,
                Saturday = 0.0
            },
                new AssetSettingsGetDBResponse
            {
                AssetID = assetUID.ToStringWithoutHyphens(),
                AssetWeeklyConfigUID = Guid.NewGuid().ToStringWithoutHyphens(),
                ConfigType = AssetTargetType.PayloadinTonnes.ToString(),
                StartDate = Convert.ToDateTime("2016-12-31"),
                EndDate = Convert.ToDateTime("2017-03-10"),
                Sunday = 0.0,
                Monday = 0.0,
                Tuesday = 0.0,
                Wednesday = 0.0,
                Thursday = 0.0,
                Friday = 0.0,
                Saturday = 0.0
            },
                new AssetSettingsGetDBResponse
            {
                AssetID = assetUID.ToStringWithoutHyphens(),
                AssetWeeklyConfigUID = Guid.NewGuid().ToStringWithoutHyphens(),
                ConfigType = AssetTargetType.VolumeinCuMeter.ToString(),
                StartDate = Convert.ToDateTime("2016-12-31"),
                EndDate = Convert.ToDateTime("2017-03-10"),
                Sunday = 0.0,
                Monday = 0.0,
                Tuesday = 0.0,
                Wednesday = 0.0,
                Thursday = 0.0,
                Friday = 0.0,
                Saturday = 0.0
            }
            };

        }
        private static List<AssetSettingsGetDBResponse> prepareDataForAssetTargetsForEndInsert(Guid assetUID)
        {
            return new List<AssetSettingsGetDBResponse> {
                new AssetSettingsGetDBResponse
            {
                AssetID = assetUID.ToStringWithoutHyphens(),
                AssetWeeklyConfigUID = Guid.NewGuid().ToStringWithoutHyphens(),
                ConfigType = AssetTargetType.RuntimeHours.ToString(),
                StartDate = Convert.ToDateTime("2016-12-31"),
                EndDate = Convert.ToDateTime("2017-04-10"),
                Sunday = 0.0,
                Monday = 8.0,
                Tuesday = 8.0,
                Wednesday = 8.0,
                Thursday = 8.0,
                Friday = 8.0,
                Saturday = 0.0
            },
                new AssetSettingsGetDBResponse
            {
                AssetID = assetUID.ToStringWithoutHyphens(),
                AssetWeeklyConfigUID = Guid.NewGuid().ToStringWithoutHyphens(),
                ConfigType = AssetTargetType.IdletimeHours.ToString(),
                StartDate = Convert.ToDateTime("2016-12-31"),
                EndDate = Convert.ToDateTime("2017-04-10"),
                Sunday = 0.0,
                Monday = 0.0,
                Tuesday = 0.0,
                Wednesday = 0.0,
                Thursday = 0.0,
                Friday = 0.0,
                Saturday = 0.0
            }
            };

        }
        private static List<AssetSettingsGetDBResponse> prepareDataForProductivityTargetsForEndInsert(Guid assetUID)
        {
            return new List<AssetSettingsGetDBResponse> {
                new AssetSettingsGetDBResponse
            {
                AssetID = assetUID.ToStringWithoutHyphens(),
                AssetWeeklyConfigUID = Guid.NewGuid().ToStringWithoutHyphens(),
                ConfigType = AssetTargetType.CycleCount.ToString(),
                StartDate = Convert.ToDateTime("2016-12-31"),
                EndDate = Convert.ToDateTime("2017-04-10"),
                Sunday = 0.0,
                Monday = 8.0,
                Tuesday = 8.0,
                Wednesday = 8.0,
                Thursday = 8.0,
                Friday = 8.0,
                Saturday = 0.0
            },
                new AssetSettingsGetDBResponse
            {
                AssetID = assetUID.ToStringWithoutHyphens(),
                AssetWeeklyConfigUID = Guid.NewGuid().ToStringWithoutHyphens(),
                ConfigType = AssetTargetType.PayloadinTonnes.ToString(),
                StartDate = Convert.ToDateTime("2016-12-31"),
                EndDate = Convert.ToDateTime("2017-04-10"),
                Sunday = 0.0,
                Monday = 0.0,
                Tuesday = 0.0,
                Wednesday = 0.0,
                Thursday = 0.0,
                Friday = 0.0,
                Saturday = 0.0
            },
                new AssetSettingsGetDBResponse
            {
                AssetID = assetUID.ToStringWithoutHyphens(),
                AssetWeeklyConfigUID = Guid.NewGuid().ToStringWithoutHyphens(),
                ConfigType = AssetTargetType.VolumeinCuMeter.ToString(),
                StartDate = Convert.ToDateTime("2016-12-31"),
                EndDate = Convert.ToDateTime("2017-04-10"),
                Sunday = 0.0,
                Monday = 0.0,
                Tuesday = 0.0,
                Wednesday = 0.0,
                Thursday = 0.0,
                Friday = 0.0,
                Saturday = 0.0
            }
            };

        }
        private static List<AssetSettingsGetDBResponse> prepareDataForAssetTargetsFoorMultipleOverlapScenariosAtTheEnd(Guid assetUID)
        {
            return new List<AssetSettingsGetDBResponse> {
                new AssetSettingsGetDBResponse
            {
                AssetID = assetUID.ToStringWithoutHyphens(),
                AssetWeeklyConfigUID = Guid.NewGuid().ToStringWithoutHyphens(),
                ConfigType = AssetTargetType.RuntimeHours.ToString(),
                StartDate = Convert.ToDateTime("2017-01-01"),
                EndDate = Convert.ToDateTime("2017-01-14"),
                Sunday = 0.0,
                Monday = 8.0,
                Tuesday = 8.0,
                Wednesday = 8.0,
                Thursday = 8.0,
                Friday = 8.0,
                Saturday = 0.0
            },
                new AssetSettingsGetDBResponse
            {
                AssetID = assetUID.ToStringWithoutHyphens(),
                AssetWeeklyConfigUID = Guid.NewGuid().ToStringWithoutHyphens(),
                ConfigType = AssetTargetType.IdletimeHours.ToString(),
                StartDate = Convert.ToDateTime("2017-01-01"),
                EndDate = Convert.ToDateTime("2017-01-14"),
                Sunday = 0.0,
                Monday = 0.0,
                Tuesday = 0.0,
                Wednesday = 0.0,
                Thursday = 0.0,
                Friday = 0.0,
                Saturday = 0.0
            },
                new AssetSettingsGetDBResponse
            {
                AssetID = assetUID.ToStringWithoutHyphens(),
                AssetWeeklyConfigUID = Guid.NewGuid().ToStringWithoutHyphens(),
                ConfigType = AssetTargetType.RuntimeHours.ToString(),
                StartDate = Convert.ToDateTime("2017-01-15"),
                EndDate = Convert.ToDateTime("2017-01-18"),
                Sunday = 0.0,
                Monday = 8.0,
                Tuesday = 8.0,
                Wednesday = 8.0,
                Thursday = 8.0,
                Friday = 8.0,
                Saturday = 0.0
            },
                new AssetSettingsGetDBResponse
            {
                AssetID = assetUID.ToStringWithoutHyphens(),
                AssetWeeklyConfigUID = Guid.NewGuid().ToStringWithoutHyphens(),
                ConfigType = AssetTargetType.IdletimeHours.ToString(),
                StartDate = Convert.ToDateTime("2017-01-15"),
                EndDate = Convert.ToDateTime("2017-01-18"),
                Sunday = 0.0,
                Monday = 0.0,
                Tuesday = 0.0,
                Wednesday = 0.0,
                Thursday = 0.0,
                Friday = 0.0,
                Saturday = 0.0
            }
            };
        }
        private static List<AssetSettingsGetDBResponse> prepareDataForProductivityTargetsFoorMultipleOverlapScenariosAtTheEnd(Guid assetUID)
        {
            return new List<AssetSettingsGetDBResponse> {
                new AssetSettingsGetDBResponse
            {
                AssetID = assetUID.ToStringWithoutHyphens(),
                AssetWeeklyConfigUID = Guid.NewGuid().ToStringWithoutHyphens(),
                ConfigType = AssetTargetType.CycleCount.ToString(),
                StartDate = Convert.ToDateTime("2017-01-01"),
                EndDate = Convert.ToDateTime("2017-01-14"),
                Sunday = 0.0,
                Monday = 8.0,
                Tuesday = 8.0,
                Wednesday = 8.0,
                Thursday = 8.0,
                Friday = 8.0,
                Saturday = 0.0
            },
                new AssetSettingsGetDBResponse
            {
                AssetID = assetUID.ToStringWithoutHyphens(),
                AssetWeeklyConfigUID = Guid.NewGuid().ToStringWithoutHyphens(),
                ConfigType = AssetTargetType.PayloadinTonnes.ToString(),
                StartDate = Convert.ToDateTime("2017-01-01"),
                EndDate = Convert.ToDateTime("2017-01-14"),
                Sunday = 0.0,
                Monday = 0.0,
                Tuesday = 0.0,
                Wednesday = 0.0,
                Thursday = 0.0,
                Friday = 0.0,
                Saturday = 0.0
            },
                new AssetSettingsGetDBResponse
            {
                AssetID = assetUID.ToStringWithoutHyphens(),
                AssetWeeklyConfigUID = Guid.NewGuid().ToStringWithoutHyphens(),
                ConfigType = AssetTargetType.VolumeinCuMeter.ToString(),
                StartDate = Convert.ToDateTime("2017-01-01"),
                EndDate = Convert.ToDateTime("2017-01-14"),
                Sunday = 0.0,
                Monday = 0.0,
                Tuesday = 0.0,
                Wednesday = 0.0,
                Thursday = 0.0,
                Friday = 0.0,
                Saturday = 0.0
            },
                new AssetSettingsGetDBResponse
            {
                AssetID = assetUID.ToStringWithoutHyphens(),
                AssetWeeklyConfigUID = Guid.NewGuid().ToStringWithoutHyphens(),
                ConfigType = AssetTargetType.CycleCount.ToString(),
                StartDate = Convert.ToDateTime("2017-01-15"),
                EndDate = Convert.ToDateTime("2017-01-18"),
                Sunday = 0.0,
                Monday = 8.0,
                Tuesday = 8.0,
                Wednesday = 8.0,
                Thursday = 8.0,
                Friday = 8.0,
                Saturday = 0.0
            },
                new AssetSettingsGetDBResponse
            {
                AssetID = assetUID.ToStringWithoutHyphens(),
                AssetWeeklyConfigUID = Guid.NewGuid().ToStringWithoutHyphens(),
                ConfigType = AssetTargetType.PayloadinTonnes.ToString(),
                StartDate = Convert.ToDateTime("2017-01-15"),
                EndDate = Convert.ToDateTime("2017-01-18"),
                Sunday = 0.0,
                Monday = 0.0,
                Tuesday = 0.0,
                Wednesday = 0.0,
                Thursday = 0.0,
                Friday = 0.0,
                Saturday = 0.0
            },
                new AssetSettingsGetDBResponse
            {
                AssetID = assetUID.ToStringWithoutHyphens(),
                AssetWeeklyConfigUID = Guid.NewGuid().ToStringWithoutHyphens(),
                ConfigType = AssetTargetType.VolumeinCuMeter.ToString(),
                StartDate = Convert.ToDateTime("2017-01-15"),
                EndDate = Convert.ToDateTime("2017-01-18"),
                Sunday = 0.0,
                Monday = 0.0,
                Tuesday = 0.0,
                Wednesday = 0.0,
                Thursday = 0.0,
                Friday = 0.0,
                Saturday = 0.0
            }
            };
        }

        private static List<AssetSettingsGetDBResponse> prepareDataForAssetTargetsFoorMultipleOverlapScenariosTo(Guid assetUID)
        {
            return new List<AssetSettingsGetDBResponse> {
                new AssetSettingsGetDBResponse
            {
                AssetID = assetUID.ToStringWithoutHyphens(),
                AssetWeeklyConfigUID = Guid.NewGuid().ToStringWithoutHyphens(),
                ConfigType = AssetTargetType.RuntimeHours.ToString(),
                StartDate = Convert.ToDateTime("2017-04-12"),
                EndDate = Convert.ToDateTime("2017-04-18"),
                Sunday = 0.0,
                Monday = 8.0,
                Tuesday = 8.0,
                Wednesday = 8.0,
                Thursday = 8.0,
                Friday = 8.0,
                Saturday = 0.0
            },
                new AssetSettingsGetDBResponse
            {
                AssetID = assetUID.ToStringWithoutHyphens(),
                AssetWeeklyConfigUID = Guid.NewGuid().ToStringWithoutHyphens(),
                ConfigType = AssetTargetType.IdletimeHours.ToString(),
                StartDate = Convert.ToDateTime("2017-04-12"),
                EndDate = Convert.ToDateTime("2017-04-18"),
                Sunday = 0.0,
                Monday = 0.0,
                Tuesday = 0.0,
                Wednesday = 0.0,
                Thursday = 0.0,
                Friday = 0.0,
                Saturday = 0.0
            },
                new AssetSettingsGetDBResponse
            {
                AssetID = assetUID.ToStringWithoutHyphens(),
                AssetWeeklyConfigUID = Guid.NewGuid().ToStringWithoutHyphens(),
                ConfigType = AssetTargetType.RuntimeHours.ToString(),
                StartDate = Convert.ToDateTime("2017-04-19"),
                EndDate = Convert.ToDateTime("2017-04-19"),
                Sunday = 0.0,
                Monday = 8.0,
                Tuesday = 8.0,
                Wednesday = 8.0,
                Thursday = 8.0,
                Friday = 8.0,
                Saturday = 0.0
            },
                new AssetSettingsGetDBResponse
            {
                AssetID = assetUID.ToStringWithoutHyphens(),
                AssetWeeklyConfigUID = Guid.NewGuid().ToStringWithoutHyphens(),
                ConfigType = AssetTargetType.IdletimeHours.ToString(),
                StartDate = Convert.ToDateTime("2017-04-19"),
                EndDate = Convert.ToDateTime("2017-04-19"),
                Sunday = 0.0,
                Monday = 0.0,
                Tuesday = 0.0,
                Wednesday = 0.0,
                Thursday = 0.0,
                Friday = 0.0,
                Saturday = 0.0
            },
                new AssetSettingsGetDBResponse
            {
                AssetID = assetUID.ToStringWithoutHyphens(),
                AssetWeeklyConfigUID = Guid.NewGuid().ToStringWithoutHyphens(),
                ConfigType = AssetTargetType.RuntimeHours.ToString(),
                StartDate = Convert.ToDateTime("2017-04-20"),
                EndDate = Convert.ToDateTime("2017-04-27"),
                Sunday = 0.0,
                Monday = 8.0,
                Tuesday = 8.0,
                Wednesday = 8.0,
                Thursday = 8.0,
                Friday = 8.0,
                Saturday = 0.0
            },
                new AssetSettingsGetDBResponse
            {
                AssetID = assetUID.ToStringWithoutHyphens(),
                AssetWeeklyConfigUID = Guid.NewGuid().ToStringWithoutHyphens(),
                ConfigType = AssetTargetType.IdletimeHours.ToString(),
                StartDate = Convert.ToDateTime("2017-04-20"),
                EndDate = Convert.ToDateTime("2017-04-27"),
                Sunday = 0.0,
                Monday = 0.0,
                Tuesday = 0.0,
                Wednesday = 0.0,
                Thursday = 0.0,
                Friday = 0.0,
                Saturday = 0.0
            }
            };
        }

        private static List<AssetSettingsGetDBResponse> prepareDataForAssetTargetsFoorMultipleOverlapScenarios(Guid assetUID)
        {
            return new List<AssetSettingsGetDBResponse> {
                new AssetSettingsGetDBResponse
            {
                AssetID = assetUID.ToStringWithoutHyphens(),
                AssetWeeklyConfigUID = Guid.NewGuid().ToStringWithoutHyphens(),
                ConfigType = AssetTargetType.RuntimeHours.ToString(),
                StartDate = Convert.ToDateTime("2017-01-01"),
                EndDate = Convert.ToDateTime("2017-01-14"),
                Sunday = 0.0,
                Monday = 8.0,
                Tuesday = 8.0,
                Wednesday = 8.0,
                Thursday = 8.0,
                Friday = 8.0,
                Saturday = 0.0
            },
                new AssetSettingsGetDBResponse
            {
                AssetID = assetUID.ToStringWithoutHyphens(),
                AssetWeeklyConfigUID = Guid.NewGuid().ToStringWithoutHyphens(),
                ConfigType = AssetTargetType.IdletimeHours.ToString(),
                StartDate = Convert.ToDateTime("2017-01-01"),
                EndDate = Convert.ToDateTime("2017-01-14"),
                Sunday = 0.0,
                Monday = 0.0,
                Tuesday = 0.0,
                Wednesday = 0.0,
                Thursday = 0.0,
                Friday = 0.0,
                Saturday = 0.0
            },
                new AssetSettingsGetDBResponse
            {
                AssetID = assetUID.ToStringWithoutHyphens(),
                AssetWeeklyConfigUID = Guid.NewGuid().ToStringWithoutHyphens(),
                ConfigType = AssetTargetType.RuntimeHours.ToString(),
                StartDate = Convert.ToDateTime("2017-01-15"),
                EndDate = Convert.ToDateTime("2017-01-18"),
                Sunday = 0.0,
                Monday = 8.0,
                Tuesday = 8.0,
                Wednesday = 8.0,
                Thursday = 8.0,
                Friday = 8.0,
                Saturday = 0.0
            },
                new AssetSettingsGetDBResponse
            {
                AssetID = assetUID.ToStringWithoutHyphens(),
                AssetWeeklyConfigUID = Guid.NewGuid().ToStringWithoutHyphens(),
                ConfigType = AssetTargetType.IdletimeHours.ToString(),
                StartDate = Convert.ToDateTime("2017-01-15"),
                EndDate = Convert.ToDateTime("2017-01-18"),
                Sunday = 0.0,
                Monday = 0.0,
                Tuesday = 0.0,
                Wednesday = 0.0,
                Thursday = 0.0,
                Friday = 0.0,
                Saturday = 0.0
            }
            };
        }
        private static List<AssetSettingsGetDBResponse> prepareDataForProductivityTargetsFoorMultipleOverlapScenarios(Guid assetUID)
        {
            return new List<AssetSettingsGetDBResponse> {
                new AssetSettingsGetDBResponse
            {
                AssetID = assetUID.ToStringWithoutHyphens(),
                AssetWeeklyConfigUID = Guid.NewGuid().ToStringWithoutHyphens(),
                ConfigType = AssetTargetType.CycleCount.ToString(),
                StartDate = Convert.ToDateTime("2017-01-01"),
                EndDate = Convert.ToDateTime("2017-01-14"),
                Sunday = 0.0,
                Monday = 8.0,
                Tuesday = 8.0,
                Wednesday = 8.0,
                Thursday = 8.0,
                Friday = 8.0,
                Saturday = 0.0
            },
                new AssetSettingsGetDBResponse
            {
                AssetID = assetUID.ToStringWithoutHyphens(),
                AssetWeeklyConfigUID = Guid.NewGuid().ToStringWithoutHyphens(),
                ConfigType = AssetTargetType.PayloadinTonnes.ToString(),
                StartDate = Convert.ToDateTime("2017-01-01"),
                EndDate = Convert.ToDateTime("2017-01-14"),
                Sunday = 0.0,
                Monday = 0.0,
                Tuesday = 0.0,
                Wednesday = 0.0,
                Thursday = 0.0,
                Friday = 0.0,
                Saturday = 0.0
            },
                new AssetSettingsGetDBResponse
            {
                AssetID = assetUID.ToStringWithoutHyphens(),
                AssetWeeklyConfigUID = Guid.NewGuid().ToStringWithoutHyphens(),
                ConfigType = AssetTargetType.VolumeinCuMeter.ToString(),
                StartDate = Convert.ToDateTime("2017-01-01"),
                EndDate = Convert.ToDateTime("2017-01-14"),
                Sunday = 0.0,
                Monday = 0.0,
                Tuesday = 0.0,
                Wednesday = 0.0,
                Thursday = 0.0,
                Friday = 0.0,
                Saturday = 0.0
            },
                new AssetSettingsGetDBResponse
            {
                AssetID = assetUID.ToStringWithoutHyphens(),
                AssetWeeklyConfigUID = Guid.NewGuid().ToStringWithoutHyphens(),
                ConfigType = AssetTargetType.CycleCount.ToString(),
                StartDate = Convert.ToDateTime("2017-01-15"),
                EndDate = Convert.ToDateTime("2017-01-18"),
                Sunday = 0.0,
                Monday = 8.0,
                Tuesday = 8.0,
                Wednesday = 8.0,
                Thursday = 8.0,
                Friday = 8.0,
                Saturday = 0.0
            },
                new AssetSettingsGetDBResponse
            {
                AssetID = assetUID.ToStringWithoutHyphens(),
                AssetWeeklyConfigUID = Guid.NewGuid().ToStringWithoutHyphens(),
                ConfigType = AssetTargetType.PayloadinTonnes.ToString(),
                StartDate = Convert.ToDateTime("2017-01-15"),
                EndDate = Convert.ToDateTime("2017-01-18"),
                Sunday = 0.0,
                Monday = 0.0,
                Tuesday = 0.0,
                Wednesday = 0.0,
                Thursday = 0.0,
                Friday = 0.0,
                Saturday = 0.0
            },
                new AssetSettingsGetDBResponse
            {
                AssetID = assetUID.ToStringWithoutHyphens(),
                AssetWeeklyConfigUID = Guid.NewGuid().ToStringWithoutHyphens(),
                ConfigType = AssetTargetType.VolumeinCuMeter.ToString(),
                StartDate = Convert.ToDateTime("2017-01-15"),
                EndDate = Convert.ToDateTime("2017-01-18"),
                Sunday = 0.0,
                Monday = 0.0,
                Tuesday = 0.0,
                Wednesday = 0.0,
                Thursday = 0.0,
                Friday = 0.0,
                Saturday = 0.0
            }
            };
        }
        #endregion

        #region "InputUserData"
        #region "AssetTargets"
        private static AssetSettingsBase PrepareUserDataForEndInsert_AssetTargets(Guid assetUID)
        {
            AssetSettingsBase assetSettings = new AssetSettingsWeeklyTargets
            {

                AssetUID = assetUID,
                EndDate = Convert.ToDateTime("2017-04-13"),
                Idle = new WeekDays
                {
                    Sunday = 0.0,
                    Monday = 0.0,
                    Tuesday = 0.0,
                    Wednesday = 0.0,
                    Thursday = 0.0,
                    Friday = 0.0,
                    Saturday = 0.0
                },
                Runtime = new WeekDays
                {
                    Sunday = 8.0,
                    Monday = 8.0,
                    Tuesday = 8.0,
                    Wednesday = 8.0,
                    Thursday = 8.0,
                    Friday = 8.0,
                    Saturday = 8.0
                },
                StartDate = Convert.ToDateTime("2017-04-05")
            };

            return assetSettings;

        }

        private static AssetSettingsBase PrepareDataForMiddleInsert_AssetTargets(Guid assetUID)
        {
            AssetSettingsBase assetSettings = new AssetSettingsWeeklyTargets()
            {

                AssetUID = assetUID,
                EndDate = Convert.ToDateTime("2017-01-18"),
                Idle = new WeekDays
                {
                    Sunday = 0.0,
                    Monday = 0.0,
                    Tuesday = 0.0,
                    Wednesday = 0.0,
                    Thursday = 0.0,
                    Friday = 0.0,
                    Saturday = 0.0
                },
                Runtime = new WeekDays
                {
                    Sunday = 8.0,
                    Monday = 8.0,
                    Tuesday = 8.0,
                    Wednesday = 8.0,
                    Thursday = 8.0,
                    Friday = 8.0,
                    Saturday = 8.0
                },
                StartDate = Convert.ToDateTime("2017-01-15")
            };

            return assetSettings;
        }

        private static AssetSettingsBase PrepareDataForInitialInsert_AssetTargets(Guid assetUID)
        {
            AssetSettingsBase assetSettings = new AssetSettingsWeeklyTargets()
            {

                AssetUID = assetUID,
                EndDate = Convert.ToDateTime("2017-01-06"),
                Idle = new WeekDays
                {
                    Sunday = 0.0,
                    Monday = 0.0,
                    Tuesday = 0.0,
                    Wednesday = 0.0,
                    Thursday = 0.0,
                    Friday = 0.0,
                    Saturday = 0.0
                },
                Runtime = new WeekDays
                {
                    Sunday = 8.0,
                    Monday = 8.0,
                    Tuesday = 8.0,
                    Wednesday = 8.0,
                    Thursday = 8.0,
                    Friday = 8.0,
                    Saturday = 8.0
                },
                StartDate = Convert.ToDateTime("2016-12-31")
            };

            return assetSettings;
        }

        private static AssetSettingsBase PrepareDataForEndInsertMultipleOverlapScenarios_AssetTargets(Guid assetUID)
        {
            AssetSettingsBase assetSettings = new AssetSettingsWeeklyTargets()
            {
                AssetUID = assetUID,
                EndDate = Convert.ToDateTime("2017-01-19"),
                Idle = new WeekDays
                {
                    Sunday = 0.0,
                    Monday = 0.0,
                    Tuesday = 0.0,
                    Wednesday = 0.0,
                    Thursday = 0.0,
                    Friday = 0.0,
                    Saturday = 0.0
                },
                Runtime = new WeekDays
                {
                    Sunday = 8.0,
                    Monday = 8.0,
                    Tuesday = 8.0,
                    Wednesday = 8.0,
                    Thursday = 8.0,
                    Friday = 8.0,
                    Saturday = 8.0
                },
                StartDate = Convert.ToDateTime("2017-01-12")
            };

            return assetSettings;
        }

        private static AssetSettingsBase PrepareDataForMiddleInsertMultipleOverlapScenariosMiddleInsert_AssetTargets(Guid assetUID)
        {
            AssetSettingsBase assetSettings = new AssetSettingsWeeklyTargets()
            {
                AssetUID = assetUID,
                EndDate = Convert.ToDateTime("2017-01-16"),
                Idle = new WeekDays
                {
                    Sunday = 0.0,
                    Monday = 0.0,
                    Tuesday = 0.0,
                    Wednesday = 0.0,
                    Thursday = 0.0,
                    Friday = 0.0,
                    Saturday = 0.0
                },
                Runtime = new WeekDays
                {
                    Sunday = 8.0,
                    Monday = 8.0,
                    Tuesday = 8.0,
                    Wednesday = 8.0,
                    Thursday = 8.0,
                    Friday = 8.0,
                    Saturday = 8.0
                },
                StartDate = Convert.ToDateTime("2017-01-13")
            };
            return assetSettings;
        }

        private static AssetSettingsBase PrepareDataForMiddleInsertMultipleOverlapScenariosStartInsert_AssetTargets(Guid assetUID)
        {
            AssetSettingsBase assetSettings = new AssetSettingsWeeklyTargets()
            {
                AssetUID = assetUID,
                EndDate = Convert.ToDateTime("2017-01-16"),
                Idle = new WeekDays
                {
                    Sunday = 0.0,
                    Monday = 0.0,
                    Tuesday = 0.0,
                    Wednesday = 0.0,
                    Thursday = 0.0,
                    Friday = 0.0,
                    Saturday = 0.0
                },
                Runtime = new WeekDays
                {
                    Sunday = 8.0,
                    Monday = 8.0,
                    Tuesday = 8.0,
                    Wednesday = 8.0,
                    Thursday = 8.0,
                    Friday = 8.0,
                    Saturday = 8.0
                },
                StartDate = Convert.ToDateTime("2016-12-31")
            };
            return assetSettings;
        }

        private static AssetSettingsBase PrepareDataForMiddleInsertMultipleOverlapScenariosAtInsert_AssetTargets(Guid assetUID)
        {
            AssetSettingsBase assetSettings = new AssetSettingsWeeklyTargets()
            {
                AssetUID = assetUID,
                EndDate = Convert.ToDateTime("2017-04-20"),
                Idle = new WeekDays
                {
                    Sunday = 0.0,
                    Monday = 0.0,
                    Tuesday = 0.0,
                    Wednesday = 0.0,
                    Thursday = 0.0,
                    Friday = 0.0,
                    Saturday = 0.0
                },
                Runtime = new WeekDays
                {
                    Sunday = 8.0,
                    Monday = 8.0,
                    Tuesday = 8.0,
                    Wednesday = 8.0,
                    Thursday = 8.0,
                    Friday = 8.0,
                    Saturday = 8.0
                },
                StartDate = Convert.ToDateTime("2017-04-12")
            };
            return assetSettings;
        }

        private static AssetSettingsBase PrepareDataForMiddleInsertMultipleOverlapScenariosFullOverlap_MultipleOverlap_AssetTargets(Guid assetUID)
        {
            AssetSettingsBase assetSettings = new AssetSettingsWeeklyTargets()
            {
                AssetUID = assetUID,
                EndDate = Convert.ToDateTime("2017-03-10"),
                Idle = new WeekDays
                {
                    Sunday = 0.0,
                    Monday = 0.0,
                    Tuesday = 0.0,
                    Wednesday = 0.0,
                    Thursday = 0.0,
                    Friday = 0.0,
                    Saturday = 0.0
                },
                Runtime = new WeekDays
                {
                    Sunday = 8.0,
                    Monday = 8.0,
                    Tuesday = 8.0,
                    Wednesday = 8.0,
                    Thursday = 8.0,
                    Friday = 8.0,
                    Saturday = 8.0
                },
                StartDate = Convert.ToDateTime("2016-12-31")
            };
            return assetSettings;
        }

        #endregion

        #region "ProductivityTargets"
        private static AssetSettingsBase PrepareUserDataForEndInsert_ProductivityTargets(Guid assetUID)
        {
            AssetSettingsBase assetSettings = new ProductivityWeeklyTargetValues
            {
                AssetUID = assetUID,
                EndDate = Convert.ToDateTime("2017-04-13"),
                Cycles = new WeekDays
                {
                    Sunday = 0.0,
                    Monday = 0.0,
                    Tuesday = 0.0,
                    Wednesday = 0.0,
                    Thursday = 0.0,
                    Friday = 0.0,
                    Saturday = 0.0
                },
                Payload = new WeekDays
                {
                    Sunday = 8.0,
                    Monday = 8.0,
                    Tuesday = 8.0,
                    Wednesday = 8.0,
                    Thursday = 8.0,
                    Friday = 8.0,
                    Saturday = 8.0
                },
                Volumes = new WeekDays
                {
                    Sunday = 8.0,
                    Monday = 8.0,
                    Tuesday = 8.0,
                    Wednesday = 8.0,
                    Thursday = 8.0,
                    Friday = 8.0,
                    Saturday = 8.0
                },
                StartDate = Convert.ToDateTime("2017-04-05")
            };

            return assetSettings;

        }

        private static AssetSettingsBase PrepareDataForMiddleInsert_ProductivityTargets(Guid assetUID)
        {
            AssetSettingsBase assetSettings = new ProductivityWeeklyTargetValues
            {

                AssetUID = assetUID,
                EndDate = Convert.ToDateTime("2017-01-18"),
                Cycles = new WeekDays
                {
                    Sunday = 0.0,
                    Monday = 0.0,
                    Tuesday = 0.0,
                    Wednesday = 0.0,
                    Thursday = 0.0,
                    Friday = 0.0,
                    Saturday = 0.0
                },
                Payload = new WeekDays
                {
                    Sunday = 8.0,
                    Monday = 8.0,
                    Tuesday = 8.0,
                    Wednesday = 8.0,
                    Thursday = 8.0,
                    Friday = 8.0,
                    Saturday = 8.0
                },
                Volumes = new WeekDays
                {
                    Sunday = 8.0,
                    Monday = 8.0,
                    Tuesday = 8.0,
                    Wednesday = 8.0,
                    Thursday = 8.0,
                    Friday = 8.0,
                    Saturday = 8.0
                },
                StartDate = Convert.ToDateTime("2017-01-15")
            };

            return assetSettings;
        }

        private static AssetSettingsBase PrepareDataForInitialInsert_ProductivityTargets(Guid assetUID)
        {
            AssetSettingsBase assetSettings = new ProductivityWeeklyTargetValues
            {

                AssetUID = assetUID,
                EndDate = Convert.ToDateTime("2017-01-06"),
                Cycles = new WeekDays
                {
                    Sunday = 0.0,
                    Monday = 0.0,
                    Tuesday = 0.0,
                    Wednesday = 0.0,
                    Thursday = 0.0,
                    Friday = 0.0,
                    Saturday = 0.0
                },
                Payload = new WeekDays
                {
                    Sunday = 8.0,
                    Monday = 8.0,
                    Tuesday = 8.0,
                    Wednesday = 8.0,
                    Thursday = 8.0,
                    Friday = 8.0,
                    Saturday = 8.0
                },
                Volumes = new WeekDays
                {
                    Sunday = 8.0,
                    Monday = 8.0,
                    Tuesday = 8.0,
                    Wednesday = 8.0,
                    Thursday = 8.0,
                    Friday = 8.0,
                    Saturday = 8.0
                },
                StartDate = Convert.ToDateTime("2016-12-31")
            };

            return assetSettings;
        }

        private static AssetSettingsBase PrepareDataForEndInsertMultipleOverlapScenarios_ProductivityTargets(Guid assetUID)
        {
            AssetSettingsBase assetSettings = new ProductivityWeeklyTargetValues
            {
                AssetUID = assetUID,
                EndDate = Convert.ToDateTime("2017-01-19"),
                Cycles = new WeekDays
                {
                    Sunday = 0.0,
                    Monday = 0.0,
                    Tuesday = 0.0,
                    Wednesday = 0.0,
                    Thursday = 0.0,
                    Friday = 0.0,
                    Saturday = 0.0
                },
                Payload = new WeekDays
                {
                    Sunday = 8.0,
                    Monday = 8.0,
                    Tuesday = 8.0,
                    Wednesday = 8.0,
                    Thursday = 8.0,
                    Friday = 8.0,
                    Saturday = 8.0
                },
                Volumes = new WeekDays
                {
                    Sunday = 8.0,
                    Monday = 8.0,
                    Tuesday = 8.0,
                    Wednesday = 8.0,
                    Thursday = 8.0,
                    Friday = 8.0,
                    Saturday = 8.0
                },
                StartDate = Convert.ToDateTime("2017-01-12")
            };

            return assetSettings;
        }

        private static AssetSettingsBase PrepareDataForMiddleInsertMultipleOverlapScenariosMiddleInsert_ProductivityTargets(Guid assetUID)
        {
            AssetSettingsBase assetSettings = new ProductivityWeeklyTargetValues
            {
                AssetUID = assetUID,
                EndDate = Convert.ToDateTime("2017-01-16"),
                Cycles = new WeekDays
                {
                    Sunday = 0.0,
                    Monday = 0.0,
                    Tuesday = 0.0,
                    Wednesday = 0.0,
                    Thursday = 0.0,
                    Friday = 0.0,
                    Saturday = 0.0
                },
                Payload = new WeekDays
                {
                    Sunday = 8.0,
                    Monday = 8.0,
                    Tuesday = 8.0,
                    Wednesday = 8.0,
                    Thursday = 8.0,
                    Friday = 8.0,
                    Saturday = 8.0
                },
                Volumes = new WeekDays
                {
                    Sunday = 8.0,
                    Monday = 8.0,
                    Tuesday = 8.0,
                    Wednesday = 8.0,
                    Thursday = 8.0,
                    Friday = 8.0,
                    Saturday = 8.0
                },
                StartDate = Convert.ToDateTime("2017-01-13")
            };
            return assetSettings;
        }

        private static AssetSettingsBase PrepareDataForMiddleInsertMultipleOverlapScenariosStartInsert_ProductivityTargets(Guid assetUID)
        {
            AssetSettingsBase assetSettings = new ProductivityWeeklyTargetValues
            {
                AssetUID = assetUID,
                EndDate = Convert.ToDateTime("2017-01-16"),
                Cycles = new WeekDays
                {
                    Sunday = 0.0,
                    Monday = 0.0,
                    Tuesday = 0.0,
                    Wednesday = 0.0,
                    Thursday = 0.0,
                    Friday = 0.0,
                    Saturday = 0.0
                },
                Payload = new WeekDays
                {
                    Sunday = 8.0,
                    Monday = 8.0,
                    Tuesday = 8.0,
                    Wednesday = 8.0,
                    Thursday = 8.0,
                    Friday = 8.0,
                    Saturday = 8.0
                },
                Volumes = new WeekDays
                {
                    Sunday = 8.0,
                    Monday = 8.0,
                    Tuesday = 8.0,
                    Wednesday = 8.0,
                    Thursday = 8.0,
                    Friday = 8.0,
                    Saturday = 8.0
                },
                StartDate = Convert.ToDateTime("2016-12-31")
            };
            return assetSettings;
        }

        private static AssetSettingsBase PrepareDataForMiddleInsertMultipleOverlapScenariosFullOverlap_MultipleOverlap_ProductivityTargets(Guid assetUID)
        {
            AssetSettingsBase assetSettings = new ProductivityWeeklyTargetValues
            {
                AssetUID = assetUID,
                EndDate = Convert.ToDateTime("2017-03-10"),
                Cycles = new WeekDays
                {
                    Sunday = 0.0,
                    Monday = 0.0,
                    Tuesday = 0.0,
                    Wednesday = 0.0,
                    Thursday = 0.0,
                    Friday = 0.0,
                    Saturday = 0.0
                },
                Payload = new WeekDays
                {
                    Sunday = 8.0,
                    Monday = 8.0,
                    Tuesday = 8.0,
                    Wednesday = 8.0,
                    Thursday = 8.0,
                    Friday = 8.0,
                    Saturday = 8.0
                },
                Volumes = new WeekDays
                {
                    Sunday = 8.0,
                    Monday = 8.0,
                    Tuesday = 8.0,
                    Wednesday = 8.0,
                    Thursday = 8.0,
                    Friday = 8.0,
                    Saturday = 8.0
                },
                StartDate = Convert.ToDateTime("2016-12-31")
            };
            return assetSettings;
        }
        #endregion

        #endregion

        #endregion
    }
}
