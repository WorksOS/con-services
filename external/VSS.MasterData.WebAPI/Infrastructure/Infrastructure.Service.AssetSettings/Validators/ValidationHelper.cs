using CommonModel.AssetSettings;
using CommonModel.Error;
using Infrastructure.Service.AssetSettings.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.Service.AssetSettings.Validators
{
	public class ValidationHelper : IValidationHelper
    {
        public List<AssetErrorInfo> ValidateAssetUIDParameters(string[] assetUID)
        {
            var isValid = new List<AssetErrorInfo>();
            Guid assetId;
            if ((assetUID == null) || (assetUID.Count() == 0))
            {
                isValid.Add(new AssetErrorInfo { ErrorCode = 400101, Message= "AssetUID is Required" });
            }

            foreach (var asset in assetUID)
            {
                if (!Guid.TryParse(asset, out assetId))
                {
                    isValid.Add(new AssetErrorInfo { ErrorCode = 400103, Message = "AssetUID Is Invalid", AssetUID = asset });
                }
            }

            return isValid;
        }

        public List<AssetErrorInfo> ValidateAssetUIDsForDefaultGuid(string[] assetUID)
        {
            var isValid = new List<AssetErrorInfo>();
            if (assetUID.ToList().Any(assetIds => Guid.Parse(assetIds) == default(Guid)))
            {
                assetUID.ToList().ForEach(asetIds =>
                {
                    if (Guid.Parse(asetIds) == default(Guid))
                        isValid.Add(new AssetErrorInfo { ErrorCode = 400102, Message = "Asset Id must not be an empty GUID", AssetUID = asetIds });
                });
            }
            return isValid;
        }

        public List<AssetErrorInfo> ValidateAssetTargetHours(AssetSettingsWeeklyTargets[] assetTargets)
        {
            var isValid = new List<AssetErrorInfo>();

            var runtimeHours = assetTargets.ToList().Select(targets => new KeyValuePair<string, List<double>>(targets.AssetUID.ToString(), new List<double> { targets.Runtime.Sunday, targets.Runtime.Monday, targets.Runtime.Tuesday, targets.Runtime.Wednesday, targets.Runtime.Thursday,
                targets.Runtime.Friday, targets.Runtime.Saturday }));

            foreach (var runtimehour in runtimeHours)
            {
                foreach (var hours in runtimehour.Value)
                {
                    if (hours < 0 || hours > 24)
                        isValid.Add(new AssetErrorInfo { ErrorCode = 400104,  Message = string.Format("Target {0} hours must be between 0 and 24", "RuntimeHours"), AssetUID = runtimehour.Key });
                }
            }

            var idleHours = assetTargets.ToList().Select(targets => new KeyValuePair<string, List<double>>(targets.AssetUID.ToString(), new List<double> { targets.Idle.Sunday, targets.Idle.Monday, targets.Idle.Tuesday, targets.Idle.Wednesday, targets.Idle.Thursday,
                targets.Idle.Friday, targets.Idle.Saturday }));

            foreach (var idleHour in idleHours)
            {
                foreach (var hours in idleHour.Value)
                {
                    if (hours < 0 || hours > 24)
                        isValid.Add(new AssetErrorInfo { ErrorCode = 400104,  Message = string.Format("Target {0} hours must be between 0 and 24", "IdleHours"), AssetUID = idleHour.Key });
                }
            }
            return isValid;
        }

        public List<AssetErrorInfo> ValidateTargetRuntimeWithIdleHours(AssetSettingsWeeklyTargets[] assetTargets)
        {
            var isValid = new List<AssetErrorInfo>();
            foreach (var assetTarget in assetTargets)
            {
                var assetuidPairs = assetTargets.ToList().Where(targets => targets.AssetUID == assetTarget.AssetUID).Select(targets => new List<Tuple<string, double, double>> { Tuple.Create(targets.AssetUID.ToString(), targets.Runtime.Monday, targets.Idle.Monday ),
                    Tuple.Create(targets.AssetUID.ToString(), targets.Runtime.Tuesday, targets.Idle.Tuesday),
                    Tuple.Create(targets.AssetUID.ToString(), targets.Runtime.Wednesday, targets.Idle.Wednesday),
                    Tuple.Create(targets.AssetUID.ToString(), targets.Runtime.Thursday, targets.Idle.Thursday),
                    Tuple.Create(targets.AssetUID.ToString(), targets.Runtime.Friday, targets.Idle.Friday),
                    Tuple.Create(targets.AssetUID.ToString(), targets.Runtime.Saturday, targets.Idle.Saturday),
                    Tuple.Create(targets.AssetUID.ToString(), targets.Runtime.Sunday, targets.Idle.Sunday)
                    });

                foreach (var assetUidPair in assetuidPairs)
                {
                    foreach (var assetUidCollection in assetUidPair)
                    {
                        if (assetUidCollection.Item2 < assetUidCollection.Item3)
                        {
                            isValid.Add(new AssetErrorInfo { ErrorCode = 400105,  Message = "Target idle hours must be less than target runtime hours" , AssetUID = assetUidCollection.Item1});
                        }
                    }
                }
            }

            return isValid;
        }

        public List<AssetErrorInfo> validateStartDateAndEndDate(AssetSettingsBase[] assetTargets)
        {
            var isValid = new List<AssetErrorInfo>();
            
            foreach(var assetTarget in assetTargets)
            {
                if(assetTarget.EndDate < assetTarget.StartDate)
                {
                    isValid.Add(new AssetErrorInfo { ErrorCode = 400106,  Message = "StartDate Should be Less than the End Date", AssetUID=assetTarget.AssetUID.ToString() });
                }
            }
            return isValid;
        }

        public List<AssetErrorInfo> ValidateProductivityTargetsForNegativeValues(ProductivityWeeklyTargetValues[] values)
        {
            var isValid = new List<AssetErrorInfo>();

            foreach (var value in values)
            {
                var cycleValues = new List<double> { value.Cycles.Sunday, value.Cycles.Monday, value.Cycles.Tuesday, value.Cycles.Wednesday, value.Cycles.Thursday, value.Cycles.Friday, value.Cycles.Saturday };
                if (cycleValues.Any(cycleValue => cycleValue < 0))
                    isValid.Add(new AssetErrorInfo { ErrorCode = 400110, Message = string.Format("Target Cycles Cannot Contain Negative Values"),});

                var payloadValues = new List<double> { value.Payload.Sunday, value.Payload.Monday, value.Payload.Tuesday, value.Payload.Wednesday, value.Payload.Thursday, value.Payload.Friday, value.Payload.Saturday };
                if (payloadValues.Any(payloadValue => payloadValue < 0))
                    isValid.Add(new AssetErrorInfo { ErrorCode = 400111, Message = string.Format("Target Payload Cannot Contain Negative Values"), AssetUID = value.AssetUID.ToString() });

                var volumeValues = new List<double> { value.Volumes.Sunday, value.Volumes.Monday, value.Volumes.Tuesday, value.Volumes.Wednesday, value.Volumes.Thursday, value.Volumes.Friday, value.Volumes.Saturday };
                if (volumeValues.Any(volumeValue => volumeValue < 0))
                    isValid.Add( new AssetErrorInfo { ErrorCode = 400112,  Message = string.Format("Target Volume Cannot Contain Negative Values"), AssetUID = value.AssetUID.ToString() }); 
            }

            return isValid;
        }

        public List<AssetErrorInfo> validateStartDateAndEndDate(DateTime startDate, DateTime endDate)
        {
            if (endDate < startDate)
                return new List<AssetErrorInfo> { new AssetErrorInfo { ErrorCode = 400113, Message = "EndDate Cannot Be Greater than StartDate" } };
            return new List<AssetErrorInfo>();
        }

        public List<AssetErrorInfo> ValidateDatetimeInAssetSettings(AssetSettingsBase[] values)
        {
            var isValid = new List<AssetErrorInfo>();
            foreach(var value in values)
            if (value.StartDate == default(DateTime) || !value.EndDate.HasValue)
                isValid.Add(new AssetErrorInfo { ErrorCode = 400114, Message= "Start and End Date are required", AssetUID = value.AssetUID.ToString() } );
            return isValid;
        }
    }
}
