using Interfaces;
using CommonModel.AssetSettings;
using DbModel.AssetSettings;
using CommonModel.Enum;
using Infrastructure.Common.Helpers;
using Infrastructure.Service.AssetSettings.Enums;
using Infrastructure.Service.AssetSettings.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Utilities.Logging;

namespace Infrastructure.Service.AssetSettings.Service
{
	public class AssetSettingsOverlapHandler : AssetSettingsOverlapTemplate
    {
		
		public AssetSettingsOverlapHandler(IWeeklyAssetSettingsRepository assetSettingsRepo, IAssetSettingsPublisher assetSettingsPublisher, IAssetSettingsTypeHandler<AssetSettingsBase> assetSettingsOverlapHandler, ILoggingService loggingService)
        {
            _assetSettingsRepo = assetSettingsRepo;
			_assetSettingsPublisher = assetSettingsPublisher;
            _assetOverlapHandler = assetSettingsOverlapHandler;
            _loggingService = loggingService;
            _loggingService.CreateLogger(typeof(AssetSettingsOverlapHandler));
        }

        protected override List<AssetSettingsGetDBResponse> HandleSingleOverallOverlap(IEnumerable<AssetSettingsGetDBResponse> assetTargets, AssetSettingsBase target, GroupType assetTargetType, DateTime startDate, DateTime endDate)
        {
                _loggingService.Debug("SingleOverall Overlap", "AssetSettingsOverlapHandler.HandleSingleOverallOverlap");
                var response = new List<AssetSettingsGetDBResponse>();
                var assetWeeklyConfigUIDs = string.Join(",", assetTargets.Select(assetTarget => assetTarget.AssetWeeklyConfigUID)).WrapCommaSeperatedStringsWithUnhex();
					assetTargets.ToList().ForEach(assestTarget => 
					{
						assestTarget.Status = false;
						assestTarget.OperationType = AssetSettingsOperationType.Delete;
					});
                    response.AddRange(assetTargets);

                //Insert the new one
                var targets = _assetOverlapHandler.GetCommonResponseFromProductivityTargetsAndAssetTargets(target);
				targets.ForEach(assetTarget => 
				{
					assetTarget.Status = true;
					assetTarget.OperationType = AssetSettingsOperationType.Insert;
				});
				
                response.AddRange(targets);


                return response;
        }

        protected override List<AssetSettingsGetDBResponse> HandleSingleOverlapAtEnd(IEnumerable<AssetSettingsGetDBResponse> assetTargets, AssetSettingsBase target, GroupType assetTargetType, DateTime startDate, DateTime endDate)
        {
            _loggingService.Debug("Single Overlap At The End", "AssetSettingsOverlapHandler.HandleSingleOverlapAtEnd");
            var response = new List<AssetSettingsGetDBResponse>();

            var targets = _assetOverlapHandler.UpdateDateForEndDateAndReturn(assetTargets.ToList().AsReadOnly(), target.StartDate, _groupType);
            targets.ForEach(assetTarget => 
			{
				assetTarget.Status = true;
				assetTarget.OperationType = AssetSettingsOperationType.Update;
			});
            response.AddRange(targets);

            targets = _assetOverlapHandler.GetCommonResponseFromProductivityTargetsAndAssetTargets(target);
            targets.ForEach(assetTarget => 
			{
				assetTarget.Status = true;
				assetTarget.OperationType = AssetSettingsOperationType.Update;
			});
            response.AddRange(targets);
            return response;
        }

        protected override List<AssetSettingsGetDBResponse> HandleSingleOVerlapAtMiddle(IEnumerable<AssetSettingsGetDBResponse> assetTargets, AssetSettingsBase target, GroupType assetTargetType, DateTime startDate, DateTime endDate)
        {
            _loggingService.Debug("Single Overlap At The Middle", "AssetSettingsOverlapHandler.HandleSingleOVerlapAtMiddle");
            var response = new List<AssetSettingsGetDBResponse>();
            var targetEndDate = target.EndDate.Value;
            var targetStartDate = target.StartDate;
            var oldAssetEndDate = assetTargets.First().EndDate;
            var targets = _assetOverlapHandler.UpdateDateForEndDateAndReturn(new List<AssetSettingsGetDBResponse>(assetTargets), targetStartDate, _groupType);
            targets.ForEach(assetTarget => 
			{
				assetTarget.Status = true;
				assetTarget.OperationType = AssetSettingsOperationType.Update;
			});
            response.AddRange(targets);

            targets = _assetOverlapHandler.GetCommonResponseFromProductivityTargetsAndAssetTargets(target);
            targets.ForEach(assetTarget => 
			{
				assetTarget.Status = true;
				assetTarget.OperationType = AssetSettingsOperationType.Insert;
			});
            response.AddRange(targets);

            targets = _assetOverlapHandler.UpdateDataForStartDate(new List<AssetSettingsGetDBResponse>(assetTargets), targetEndDate, _groupType);
            targets.ForEach(assetTarget => 
			{
				assetTarget.EndDate = oldAssetEndDate;
				assetTarget.AssetWeeklyConfigUID = Guid.NewGuid().ToStringWithoutHyphens();
				assetTarget.Status = true;
				assetTarget.OperationType = AssetSettingsOperationType.Insert;
			});
            response.AddRange(targets);

            return response;
        }

        protected override List<AssetSettingsGetDBResponse> HandleSingleOverlapAtStart(IEnumerable<AssetSettingsGetDBResponse> assetTargets, AssetSettingsBase target, GroupType assetTargetType, DateTime startDate, DateTime endDate)
        {
			using (var scope = new TransactionScope())
			{
				_loggingService.Debug("Single Overlap At The Start", "AssetSettingsOverlapHandler.HandleSingleOverlapAtStart");
				var response = new List<AssetSettingsGetDBResponse>();


				var targets = _assetOverlapHandler.GetCommonResponseFromProductivityTargetsAndAssetTargets(target);
				targets.ForEach(assetTarget => 
				{
					assetTarget.Status = true;
					assetTarget.OperationType = AssetSettingsOperationType.Insert;
				});
                response.AddRange(targets);
                //InsertintoKafka(targets);

                targets = _assetOverlapHandler.UpdateDataForStartDate(assetTargets.ToList().AsReadOnly(), target.EndDate.Value, _groupType);
				targets.ForEach(assetTarget =>
				{
					assetTarget.Status = true;
					assetTarget.OperationType = AssetSettingsOperationType.Update;
				});
                response.AddRange(targets);

                return response; 
            }
        }

        protected override List<AssetSettingsGetDBResponse> HandleMultipleOverallOverlap(IEnumerable<AssetSettingsGetDBResponse> assetTargets, AssetSettingsBase target, GroupType assetTargetType, DateTime startDate, DateTime endDate)
        {
            _loggingService.Debug("Multiple Overall Overlap", "AssetSettingsOverlapHandler.HandleMultipleOverallOverlap");

            var response = new List<AssetSettingsGetDBResponse>();
            var assetUIDsToBeDeleted = string.Join(",", assetTargets.Select(assetTarget => assetTarget.AssetWeeklyConfigUID)).WrapCommaSeperatedStringsWithUnhex();

			assetTargets.ToList().ForEach(assetTarget =>
			{
				assetTarget.Status = false;
				assetTarget.OperationType = AssetSettingsOperationType.Delete;
			});
            response.AddRange(assetTargets);

            var targets = _assetOverlapHandler.GetCommonResponseFromProductivityTargetsAndAssetTargets(target);
            targets.ForEach(assetTarget => 
			{
				assetTarget.Status = true;
				assetTarget.OperationType = AssetSettingsOperationType.Insert;
			});
            response.AddRange(targets);
            return response; 
        }

        protected override List<AssetSettingsGetDBResponse> HandleMultipleOverlapAtEnd(IEnumerable<AssetSettingsGetDBResponse> assetTargets, AssetSettingsBase target, GroupType assetTargetType, DateTime startDate, DateTime endDate)
        {

                _loggingService.Debug("Overall Overlap At The End", "AssetSettingsOverlapHandler.HandleMultipleOverlapAtEnd");
                var response = new List<AssetSettingsGetDBResponse>();
                var assetUIDsToBeDeleted = string.Join(",", assetTargets.Where(assetTarget => assetTarget.StartDate != startDate).Select(assetTarget => assetTarget.AssetWeeklyConfigUID)).WrapCommaSeperatedStringsWithUnhex();

                    var targetsDeleted = assetTargets.Where(assetTarget => assetTarget.StartDate != startDate);
					targetsDeleted.ToList().ForEach(assetTarget =>
					{
						assetTarget.Status = false;
						assetTarget.OperationType = AssetSettingsOperationType.Delete;
					});
                    response.AddRange(targetsDeleted);

                var targets = _assetOverlapHandler.UpdateDateForEndDateAndReturn(assetTargets.ToList().AsReadOnly(), target.StartDate, _groupType);
				targets.ToList().ForEach(assetTarget =>
				{
					assetTarget.Status = true;
					assetTarget.OperationType = AssetSettingsOperationType.Update;
				});
                response.AddRange(targets);

                targets = _assetOverlapHandler.GetCommonResponseFromProductivityTargetsAndAssetTargets(target);
                targets.ToList().ForEach(assetTarget =>
				{
					assetTarget.Status = true;
					assetTarget.OperationType = AssetSettingsOperationType.Update;
				});
				response.AddRange(targets);


                return response; 
        }

        protected override List<AssetSettingsGetDBResponse> HandleMultipleOverlapAtMiddle(IEnumerable<AssetSettingsGetDBResponse> assetTargets, AssetSettingsBase target, GroupType assetTargetType, DateTime startDate, DateTime endDate)
        {

                _loggingService.Debug("Overall Overlap At The Middle", "AssetSettingsOverlapHandler.HandleMultipleOVerlapAtMiddle");
                var response = new List<AssetSettingsGetDBResponse>();
                var oldAssetEndDate = assetTargets.First().EndDate;
                {
                    var targetsDeleted = assetTargets.Where(assetTarget => assetTarget.EndDate != endDate && assetTarget.StartDate != startDate);
                    targetsDeleted.ToList().ForEach(assetTarget =>
					{
						assetTarget.Status = false;
						assetTarget.OperationType = AssetSettingsOperationType.Delete;
					});
                    response.AddRange(targetsDeleted);
                }


                //Start Insert 
                var smallStartDateSection = assetTargets.Where(at => at.StartDate == startDate).ToList();
                var targets = _assetOverlapHandler.UpdateDateForEndDateAndReturn(smallStartDateSection.AsReadOnly(), target.StartDate, _groupType);
                targets.ToList().ForEach(assetTarget =>
				{
					assetTarget.Status = true;
					assetTarget.OperationType = AssetSettingsOperationType.Update;
				});
                response.AddRange(targets);


                //End Insert
                var largerEndDateSection = assetTargets.Where(at => at.EndDate == endDate).ToList();
                targets = _assetOverlapHandler.UpdateDataForStartDate(largerEndDateSection.AsReadOnly(), target.EndDate.Value, _groupType);
                targets.ForEach(assetTarget => 
				{
					assetTarget.EndDate = endDate;
					assetTarget.Status = true;
					assetTarget.OperationType = AssetSettingsOperationType.Update;
				});
                response.AddRange(targets);

                targets = _assetOverlapHandler.GetCommonResponseFromProductivityTargetsAndAssetTargets(target);
                targets.ForEach(assetTarget => 
				{
					assetTarget.Status = true;
					assetTarget.OperationType = AssetSettingsOperationType.Update;
				});
                response.AddRange(targets);


                return response; 
        }

		protected override List<AssetSettingsGetDBResponse> HandleMultipleOverlapAtStart(IEnumerable<AssetSettingsGetDBResponse> assetTargets, AssetSettingsBase target, GroupType assetTargetType, DateTime startDate, DateTime endDate)
		{
			_loggingService.Debug("Overall Overlap At The Start", "AssetSettingsOverlapHandler.HandleMultipleOverlapAtStart");
			var oldAssetEndDate = assetTargets.First().EndDate;
			//Delete Middle Records
			var response = new List<AssetSettingsGetDBResponse>();
			var assetWeeklyConfigUidsToBeDeleted = assetTargets.Where(assetTarget => assetTarget.EndDate != endDate).Select(assetTarget => assetTarget.AssetWeeklyConfigUID).ToList();
				var targetsDeleted = assetTargets.Where(assetTarget => assetTarget.EndDate != endDate).ToList();
				targetsDeleted.ToList().ForEach(assetTarget =>
				{
					assetTarget.Status = false;
					assetTarget.OperationType = AssetSettingsOperationType.Delete;
				});
				response.AddRange(targetsDeleted);

                assetTargets = assetTargets.Where(at => !assetWeeklyConfigUidsToBeDeleted.Contains(at.AssetWeeklyConfigUID));
                var targets = _assetOverlapHandler.UpdateDataForStartDate(assetTargets.ToList().AsReadOnly(), target.EndDate.Value, _groupType);
                targets.ForEach(assetTarget => assetTarget.EndDate = endDate);
                targets.ForEach(assetTarget => 
				{
					assetTarget.EndDate = endDate;
					assetTarget.Status = true;
					assetTarget.OperationType = AssetSettingsOperationType.Update;
				});
                response.AddRange(targets);

                //Insert Targets
                targets = _assetOverlapHandler.GetCommonResponseFromProductivityTargetsAndAssetTargets(target);
                targets.ForEach(assetTarget => 
				{
					assetTarget.Status = true;
					assetTarget.OperationType = AssetSettingsOperationType.Update;
				});
                response.AddRange(targets);


                return response; 
        }

		
	}
}

