using CommonModel.AssetSettings;
using DbModel.AssetSettings;
using CommonModel.Enum;
using Infrastructure.Common.Helpers;
using Infrastructure.Service.AssetSettings.Enums;
using Infrastructure.Service.AssetSettings.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.Service.AssetSettings.Service
{
	public class AssetSettingsTypeHandler<T> : IAssetSettingsTypeHandler<T> where T : AssetSettingsBase
    {

        public List<AssetSettingsGetDBResponse> GetCommonResponseFromProductivityTargetsAndAssetTargets(T targets)
        {
            var response = new List<AssetSettingsGetDBResponse>();
            if (targets is AssetSettingsWeeklyTargets)
            {
                return (GetAssetTargetsFromAssetUtilizationTargets(targets as AssetSettingsWeeklyTargets));
            }
            if (targets is ProductivityWeeklyTargetValues)
            {
                return GetProductivityTargetsFromProductivityTargetValues(targets as ProductivityWeeklyTargetValues);
            }
            return response;
        }
        public List<AssetSettingsGetDBResponse> UpdateDataForStartDate(IEnumerable<AssetSettingsGetDBResponse> assetSettings, DateTime dateTime, GroupType targetType)
        {
            var response = new List<AssetSettingsGetDBResponse>();

            if (targetType == GroupType.AssetTargets)
            {
                var originalStartRuntime = assetSettings.First(assetTarget => assetTarget.ConfigType.Equals(AssetTargetType.RuntimeHours.ToString()));
                var originalStartIdleTime = assetSettings.First(assetTarget => assetTarget.ConfigType.Equals(AssetTargetType.IdletimeHours.ToString()));
                var startRuntime = UtilHelpers.Clone(originalStartRuntime);
                var startIdletime = UtilHelpers.Clone(originalStartIdleTime);

                startRuntime.StartDate = dateTime.AddDays(1);
                startIdletime.StartDate = dateTime.AddDays(1);
                var targets = new List<AssetSettingsGetDBResponse>() { startRuntime, startIdletime };
                return targets;
            }

            if (targetType == GroupType.ProductivityTargets)
            {
                var originalCycle = assetSettings.First(assetTarget => assetTarget.ConfigType.Equals(AssetTargetType.CycleCount.ToString()));
                var targetCycle = UtilHelpers.Clone(originalCycle);
                var originalPayload = assetSettings.First(assetTarget => assetTarget.ConfigType.Equals(AssetTargetType.PayloadinTonnes.ToString()));
                var targetPayload = UtilHelpers.Clone(originalPayload);
                var originalVolume = assetSettings.First(assetTarget => assetTarget.ConfigType.Equals(AssetTargetType.VolumeinCuMeter.ToString()));
                var targetVolume = UtilHelpers.Clone(originalVolume);

                targetCycle.StartDate = dateTime.AddDays(1);
                targetPayload.StartDate = dateTime.AddDays(1);
                targetVolume.StartDate = dateTime.AddDays(1);

                var targets = new List<AssetSettingsGetDBResponse>() { targetCycle, targetPayload, targetVolume };
                return targets;
            }

            return response;
        }
        public List<AssetSettingsGetDBResponse> UpdateDateForEndDateAndReturn(IEnumerable<AssetSettingsGetDBResponse> assetSettings, DateTime dateTime, GroupType targetType)
        {
            var response = new List<AssetSettingsGetDBResponse>();

            if (targetType == GroupType.AssetTargets)
            {
                var originalRuntimeHours = assetSettings.First(assetTarget => assetTarget.ConfigType.Equals(AssetTargetType.RuntimeHours.ToString()));
                var originalIdleHours = assetSettings.First(assetTarget => assetTarget.ConfigType.Equals(AssetTargetType.IdletimeHours.ToString()));
                var targetRuntimeHours = UtilHelpers.Clone(originalRuntimeHours);
                var targetIdleHours = UtilHelpers.Clone(originalIdleHours);

                targetRuntimeHours.EndDate = dateTime.AddDays(-1);
                targetIdleHours.EndDate = dateTime.AddDays(-1);

                var targets = new List<AssetSettingsGetDBResponse>() { targetRuntimeHours, targetIdleHours };
                return targets;
            }

            if (targetType == GroupType.ProductivityTargets)
            {
                var originalCycle = assetSettings.First(assetTarget => assetTarget.ConfigType.Equals(AssetTargetType.CycleCount.ToString()));
                var originalPayload = assetSettings.First(assetTarget => assetTarget.ConfigType.Equals(AssetTargetType.PayloadinTonnes.ToString()));
                var originalVolume = assetSettings.First(assetTarget => assetTarget.ConfigType.Equals(AssetTargetType.VolumeinCuMeter.ToString()));

                var targetCycle = UtilHelpers.Clone(originalCycle);
                var targetPayload = UtilHelpers.Clone(originalPayload);
                var targetVolume = UtilHelpers.Clone(originalVolume);

                targetCycle.EndDate = dateTime.AddDays(-1);
                targetPayload.EndDate = dateTime.AddDays(-1);
                targetVolume.EndDate = dateTime.AddDays(-1);

                var targets = new List<AssetSettingsGetDBResponse>() { targetCycle, targetPayload, targetVolume };
                return targets;
            }
            return response;
        }

        private List<AssetSettingsGetDBResponse> GetAssetTargetsFromAssetUtilizationTargets(AssetSettingsWeeklyTargets target, bool isInsert = true)
        {
            var targets = new List<AssetSettingsGetDBResponse>() {
            new AssetSettingsGetDBResponse
            {
                AssetID = target.AssetUID.ToStringWithoutHyphens(),
                AssetWeeklyConfigUID = isInsert? Guid.NewGuid().ToStringWithoutHyphens() : target.Runtime.AssetWeeklyConfigUID.ToStringWithoutHyphens(),
                ConfigType = AssetTargetType.RuntimeHours.ToString(),
                EndDate = target.EndDate.Value,
                StartDate = target.StartDate,
                Sunday = target.Runtime.Sunday,
                Monday = target.Runtime.Monday,
                Tuesday = target.Runtime.Tuesday,
                Wednesday = target.Runtime.Wednesday,
                Thursday = target.Runtime.Thursday,
                Friday = target.Runtime.Friday,
                Saturday = target.Runtime.Saturday
            },
            new AssetSettingsGetDBResponse
            {
                AssetID = target.AssetUID.ToStringWithoutHyphens(),
                AssetWeeklyConfigUID = isInsert? Guid.NewGuid().ToStringWithoutHyphens() : target.Idle.AssetWeeklyConfigUID.ToStringWithoutHyphens(),
                ConfigType = AssetTargetType.IdletimeHours.ToString(),
                EndDate = target.EndDate.Value,
                StartDate = target.StartDate,
                Sunday = target.Idle.Sunday,
                Monday = target.Idle.Monday,
                Tuesday = target.Idle.Tuesday,
                Wednesday = target.Idle.Wednesday,
                Thursday = target.Idle.Thursday,
                Friday = target.Idle.Friday,
                Saturday = target.Idle.Saturday
            },
            };

            //_targetHours.InsertAssetTargets(targets);
            return targets;
        }

        private List<AssetSettingsGetDBResponse> GetProductivityTargetsFromProductivityTargetValues(ProductivityWeeklyTargetValues values)
        {
            var targetsToBeStored = new List<AssetSettingsGetDBResponse>();

            var targetCycles = new AssetSettingsGetDBResponse
            {
                AssetID = values.AssetUID.ToStringWithoutHyphens(),
                AssetWeeklyConfigUID = values.Cycles.AssetWeeklyConfigUID == Guid.Empty ? Guid.NewGuid().ToStringWithoutHyphens() : values.Cycles.AssetWeeklyConfigUID.ToStringWithoutHyphens(),
                ConfigType = AssetTargetType.CycleCount.ToString(),
                EndDate = values.EndDate.Value,
                StartDate = values.StartDate,
                Sunday = values.Cycles.Sunday,
                Monday = values.Cycles.Monday,
                Tuesday = values.Cycles.Tuesday,
                Wednesday = values.Cycles.Wednesday,
                Thursday = values.Cycles.Thursday,
                Friday = values.Cycles.Friday,
                Saturday = values.Cycles.Saturday
            };

            var targetPayload = new AssetSettingsGetDBResponse
            {
                AssetID = values.AssetUID.ToStringWithoutHyphens(),
                AssetWeeklyConfigUID = values.Payload.AssetWeeklyConfigUID == Guid.Empty ? Guid.NewGuid().ToStringWithoutHyphens() : values.Payload.AssetWeeklyConfigUID.ToStringWithoutHyphens(),
                ConfigType = AssetTargetType.PayloadinTonnes.ToString(),
                EndDate = values.EndDate.Value,
                StartDate = values.StartDate,
                Sunday = values.Payload.Sunday,
                Monday = values.Payload.Monday,
                Tuesday = values.Payload.Tuesday,
                Wednesday = values.Payload.Wednesday,
                Thursday = values.Payload.Thursday,
                Friday = values.Payload.Friday,
                Saturday = values.Payload.Saturday
            };

            var targetVolume = new AssetSettingsGetDBResponse
            {
                AssetID = values.AssetUID.ToStringWithoutHyphens(),
                AssetWeeklyConfigUID = values.Volumes.AssetWeeklyConfigUID == Guid.Empty ? Guid.NewGuid().ToStringWithoutHyphens() : values.Volumes.AssetWeeklyConfigUID.ToStringWithoutHyphens(),
                ConfigType = AssetTargetType.VolumeinCuMeter.ToString(),
                EndDate = values.EndDate.Value,
                StartDate = values.StartDate,
                Sunday = values.Volumes.Sunday,
                Monday = values.Volumes.Monday,
                Tuesday = values.Volumes.Tuesday,
                Wednesday = values.Volumes.Wednesday,
                Thursday = values.Volumes.Thursday,
                Friday = values.Volumes.Friday,
                Saturday = values.Volumes.Saturday
            };

            targetsToBeStored.Add(targetCycles);
            targetsToBeStored.Add(targetPayload);
            targetsToBeStored.Add(targetVolume);

            return targetsToBeStored;
        }

        public List<T> ExtractToAssetUtilizationTargets(List<AssetSettingsGetDBResponse> assetTargets, GroupType groupType)
        {
            var targets = new List<T>();
            var assetUIDs = assetTargets.Select(target => target.AssetUID.ToString()).Distinct().ToArray();
            if (assetUIDs.Any())
            {
                if (groupType == GroupType.AssetTargets)
                {
                    assetUIDs.ToList().ForEach(assetUID =>
                    {
                        var assetDates = assetTargets.Any() ? assetTargets.Where(target => target.AssetUID == Guid.Parse(assetUID)).Select(target => new { target.StartDate, target.EndDate }).Distinct() : null;
                        if (assetDates.Any())
                        {
                            assetDates.ToList().ForEach(assetDate =>
                            {
                                var assetStartDate = assetDate.StartDate;
                                var assetEndDate = assetDate.EndDate;
                                var totalRuntimeHours = assetTargets.Any(target => target.AssetUID == Guid.Parse(assetUID) && target.ConfigType.Equals(AssetTargetType.RuntimeHours.ToString())) ?
                                GetWeekDaysFromAssetTargetResponse(assetTargets.First(target => target.AssetUID == Guid.Parse(assetUID) && target.ConfigType.Equals(AssetTargetType.RuntimeHours.ToString())
                                && target.StartDate == assetStartDate && target.EndDate == assetEndDate))
                                : new WeekDays
                                {
                                    Sunday = 0,
                                    Monday = 0,
                                    Tuesday = 0,
                                    Wednesday = 0,
                                    Thursday = 0,
                                    Friday = 0,
                                    Saturday = 0,
                                };

                                var totalIdleHours = assetTargets.Any(target => target.AssetUID == Guid.Parse(assetUID) && target.ConfigType.Equals(AssetTargetType.IdletimeHours.ToString())) ?
                                GetWeekDaysFromAssetTargetResponse(assetTargets.First(target => target.AssetUID == Guid.Parse(assetUID) && target.ConfigType.Equals(AssetTargetType.IdletimeHours.ToString())
                                && target.StartDate == assetStartDate && target.EndDate == assetEndDate))
                                : new WeekDays
                                {
                                    Sunday = 0,
                                    Monday = 0,
                                    Tuesday = 0,
                                    Wednesday = 0,
                                    Thursday = 0,
                                    Friday = 0,
                                    Saturday = 0
                                };

                                targets.Add(new AssetSettingsWeeklyTargets { AssetUID = Guid.Parse(assetUID), Idle = totalIdleHours, Runtime = totalRuntimeHours, StartDate = assetStartDate, EndDate = assetEndDate } as T);
                            });
                        }
                    });
                }

                if (groupType == GroupType.ProductivityTargets)
                {
                    assetUIDs.ToList().ForEach(assetUID =>
                    {
                        var assetDates = assetTargets.Any() ? assetTargets.Where(target => target.AssetUID == Guid.Parse(assetUID)).Select(target => new { target.StartDate, target.EndDate }).Distinct() : null;
                        if (assetDates.Any())
                        {
                            assetDates.ToList().ForEach(assetDate =>
                            {
                                var assetStartDate = assetDate.StartDate;
                                var assetEndDate = assetDate.EndDate;
                                var targetCycle = assetTargets.Any(target => target.AssetUID == Guid.Parse(assetUID) && target.ConfigType.Equals(AssetTargetType.CycleCount.ToString())) ?
                                GetWeekDaysFromAssetTargetResponse(assetTargets.First(target => target.AssetUID == Guid.Parse(assetUID) && target.ConfigType.Equals(AssetTargetType.CycleCount.ToString())
                                && target.StartDate == assetStartDate && target.EndDate == assetEndDate))
                                : new WeekDays
                                {
                                    Sunday = 0,
                                    Monday = 0,
                                    Tuesday = 0,
                                    Wednesday = 0,
                                    Thursday = 0,
                                    Friday = 0,
                                    Saturday = 0,
                                };

                                var targetPayload = assetTargets.Any(target => target.AssetUID == Guid.Parse(assetUID) && target.ConfigType.Equals(AssetTargetType.PayloadinTonnes.ToString())) ?
                                GetWeekDaysFromAssetTargetResponse(assetTargets.First(target => target.AssetUID == Guid.Parse(assetUID) && target.ConfigType.Equals(AssetTargetType.PayloadinTonnes.ToString())
                                && target.StartDate == assetStartDate && target.EndDate == assetEndDate))
                                : new WeekDays
                                {
                                    Sunday = 0,
                                    Monday = 0,
                                    Tuesday = 0,
                                    Wednesday = 0,
                                    Thursday = 0,
                                    Friday = 0,
                                    Saturday = 0
                                };

                                var targetVolume = assetTargets.Any(target => target.AssetUID == Guid.Parse(assetUID) && target.ConfigType.Equals(AssetTargetType.VolumeinCuMeter.ToString())) ?
                                GetWeekDaysFromAssetTargetResponse(assetTargets.First(target => target.AssetUID == Guid.Parse(assetUID) && target.ConfigType.Equals(AssetTargetType.VolumeinCuMeter.ToString())
                                && target.StartDate == assetStartDate && target.EndDate == assetEndDate))
                                : new WeekDays
                                {
                                    Sunday = 0,
                                    Monday = 0,
                                    Tuesday = 0,
                                    Wednesday = 0,
                                    Thursday = 0,
                                    Friday = 0,
                                    Saturday = 0
                                };

                                targets.Add(new ProductivityWeeklyTargetValues { AssetUID = Guid.Parse(assetUID), Cycles = targetCycle, Payload = targetPayload, Volumes = targetVolume, StartDate = assetStartDate, EndDate = assetEndDate } as T);
                            });
                        }
                    });
                } 
            }
            return targets;
        }

        private static WeekDays GetWeekDaysFromAssetTargetResponse(AssetSettingsGetDBResponse response)
        {
            return new WeekDays
            {
                Sunday = response.Sunday,
                Monday = response.Monday,
                Tuesday = response.Tuesday,
                Wednesday = response.Wednesday,
                Thursday = response.Thursday,
                Friday = response.Friday,
                Saturday = response.Saturday,
                AssetWeeklyConfigUID = Guid.Parse(response.AssetWeeklyConfigUID)
            };
        }

        public Tuple<DateTime, DateTime> GetStartDateAndEndDate(List<AssetSettingsGetDBResponse> assetTargets, GroupType groupType)
        {
            switch (groupType)
            {
                case GroupType.AssetTargets:
                    return Tuple.Create<DateTime, DateTime>(assetTargets.Where(assetTarget => assetTarget.ConfigType.Equals(AssetTargetType.RuntimeHours.ToString())).OrderBy(targets => targets.StartDate).First().StartDate,
                        assetTargets.Where(assetTarget => assetTarget.ConfigType.Equals(AssetTargetType.RuntimeHours.ToString())).OrderBy(targets => targets.StartDate).First().EndDate);
                case GroupType.ProductivityTargets:
                    return Tuple.Create<DateTime, DateTime>(assetTargets.Where(assetTarget => assetTarget.ConfigType.Equals(AssetTargetType.CycleCount.ToString())).OrderBy(targets => targets.EndDate).First().StartDate,
                        assetTargets.Where(assetTarget => assetTarget.ConfigType.Equals(AssetTargetType.CycleCount.ToString())).OrderBy(targets => targets.EndDate).First().EndDate);
                default:
                    return Tuple.Create<DateTime, DateTime>(DateTime.Now, DateTime.Now);
            }
        }
    }
}
