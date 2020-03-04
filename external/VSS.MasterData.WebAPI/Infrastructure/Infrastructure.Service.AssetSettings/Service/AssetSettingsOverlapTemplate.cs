using Interfaces;
using CommonModel.AssetSettings;
using DbModel.AssetSettings;
using CommonModel.Enum;
using Infrastructure.Service.AssetSettings.Enums;
using Infrastructure.Service.AssetSettings.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities.Logging;

namespace Infrastructure.Service.AssetSettings.Service
{
	public abstract class AssetSettingsOverlapTemplate
    {
        protected IWeeklyAssetSettingsRepository _assetSettingsRepo;
        protected AssetTargetType _assetTargetType;
        protected GroupType _groupType;
        protected IAssetSettingsPublisher _assetSettingsPublisher;
        protected IAssetSettingsTypeHandler<AssetSettingsBase> _assetOverlapHandler;
        protected ILoggingService _loggingService;

        protected abstract List<AssetSettingsGetDBResponse> HandleSingleOverlapAtStart(IEnumerable<AssetSettingsGetDBResponse> assetTargets, AssetSettingsBase targets, GroupType assetTargetType, DateTime startDate, DateTime endDate);

        protected abstract List<AssetSettingsGetDBResponse> HandleSingleOverlapAtEnd(IEnumerable<AssetSettingsGetDBResponse> assetTargets, AssetSettingsBase targets, GroupType assetTargetType, DateTime startDate, DateTime endDate);

        protected abstract List<AssetSettingsGetDBResponse> HandleSingleOVerlapAtMiddle(IEnumerable<AssetSettingsGetDBResponse> assetTargets, AssetSettingsBase targets, GroupType assetTargetType, DateTime startDate, DateTime endDate);

        protected abstract List<AssetSettingsGetDBResponse> HandleSingleOverallOverlap(IEnumerable<AssetSettingsGetDBResponse> assetTargets, AssetSettingsBase targets, GroupType assetTargetType, DateTime startDate, DateTime endDate);

        protected abstract List<AssetSettingsGetDBResponse> HandleMultipleOverlapAtStart(IEnumerable<AssetSettingsGetDBResponse> assetTargets, AssetSettingsBase targets, GroupType assetTargetType, DateTime startDate, DateTime endDate);

        protected abstract List<AssetSettingsGetDBResponse> HandleMultipleOverlapAtEnd(IEnumerable<AssetSettingsGetDBResponse> assetTargets, AssetSettingsBase targets, GroupType assetTargetType, DateTime startDate, DateTime endDate);

        protected abstract List<AssetSettingsGetDBResponse> HandleMultipleOverlapAtMiddle(IEnumerable<AssetSettingsGetDBResponse> assetTargets, AssetSettingsBase targets, GroupType assetTargetType, DateTime startDate, DateTime endDate);

        protected abstract List<AssetSettingsGetDBResponse> HandleMultipleOverallOverlap(IEnumerable<AssetSettingsGetDBResponse> assetTargets, AssetSettingsBase targets, GroupType assetTargetType, DateTime startDate, DateTime endDate);

        public List<AssetSettingsGetDBResponse> HandleOverlap(AssetSettingsBase target, List<AssetSettingsGetDBResponse> assetTargets, GroupType groupType)
        {
            _loggingService.Debug("No Records Insert a New Record", "AssetSettingsOverlapTemplate.HandleOverlap");
            _groupType = groupType;
            var dates = _assetOverlapHandler.GetStartDateAndEndDate(assetTargets, _groupType);
            var assetEndDate = dates.Item2;
            var assetStartDate = dates.Item1;
            var response = new List<AssetSettingsGetDBResponse>();
            //Multiple Overlap Scenarios
            if((_groupType == GroupType.AssetTargets && assetTargets.Count > 2) || (_groupType == GroupType.ProductivityTargets && assetTargets.Count > 3))
            {
                _loggingService.Debug("MultipleOverlaps Exists For The Scenario", "AssetSettingsOverlapTemplate.HandleOverlap");
                return HandleMultipleOverlaps(target, assetStartDate, assetTargets);
            }
            _loggingService.Debug("SingleOverlap Exists For The Scenario", "AssetSettingsOverlapTemplate.HandleOverlap");
            //Single Overlap Scenarios
            if (target.StartDate > assetStartDate && target.EndDate >= assetEndDate)
                return HandleSingleOverlapAtEnd(assetTargets, target, _groupType, assetStartDate, assetEndDate);

            if (target.StartDate > assetStartDate && target.EndDate < assetEndDate)
                return HandleSingleOVerlapAtMiddle(assetTargets, target, _groupType, assetStartDate, assetEndDate);

            if (target.StartDate <= assetStartDate && target.EndDate >= assetEndDate)
                return HandleSingleOverallOverlap(assetTargets, target, _groupType, assetStartDate, assetEndDate);

            if (target.StartDate <= assetStartDate && target.EndDate < assetEndDate)
                return HandleSingleOverlapAtStart(assetTargets, target, _groupType, assetStartDate, assetEndDate);

            return response;
        }

        private List<AssetSettingsGetDBResponse> HandleMultipleOverlaps(AssetSettingsBase target, DateTime startDate, List<AssetSettingsGetDBResponse> assetTargets)
        {
            var response = new List<AssetSettingsGetDBResponse>();
            var largeAssetEndDate = assetTargets.Select(assetTarget => assetTarget.EndDate).OrderByDescending(assetDate => assetDate).First();
            if (target.StartDate > startDate && target.EndDate >= largeAssetEndDate)
                return HandleMultipleOverlapAtEnd(assetTargets, target, _groupType, startDate, largeAssetEndDate);
            if (target.StartDate > startDate && target.EndDate < largeAssetEndDate)
                return HandleMultipleOverlapAtMiddle(assetTargets, target, _groupType, startDate, largeAssetEndDate);
            if (target.StartDate <= startDate && target.EndDate >= largeAssetEndDate)
                return HandleMultipleOverallOverlap(assetTargets, target, _groupType, startDate, largeAssetEndDate);
            if (target.StartDate <= startDate && target.EndDate < largeAssetEndDate)
                return HandleMultipleOverlapAtStart(assetTargets, target, _groupType, startDate, largeAssetEndDate);
            return response;
        }

        
    }
}
