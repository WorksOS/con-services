using CommonModel.AssetSettings;
using Infrastructure.Service.AssetSettings.Interfaces;
using Infrastructure.Service.AssetSettings.Validators;
using System;
using System.Collections.Generic;
using Xunit;

namespace VSP.MasterData.Asset.Domain.UnitTests
{
	public class ValidationTests
	{
		private IValidationHelper _validationHelper;

		public ValidationTests()
		{
			_validationHelper = new ValidationHelper();
		}

		[Fact]
		public void TestvalidateTargetCyclesWithWrongValue_ExpectFailure()
		{
			var errorInfos = _validationHelper.ValidateAssetTargetHours(
				new List<AssetSettingsWeeklyTargets>
				{
					new AssetSettingsWeeklyTargets
					{
						Idle = new WeekDays{ Friday = 25, Monday = 25, Saturday = 25, Sunday = 25, Thursday = 25, Tuesday = 25, Wednesday = 25 },
						Runtime =  new WeekDays{ Friday = 2, Monday = 2, Saturday = 2, Sunday = 2, Thursday = 2, Tuesday = 2, Wednesday = 2 }
					},
					new AssetSettingsWeeklyTargets
					{
						Runtime = new WeekDays{ Friday = 9, Monday = 9, Saturday = 9, Sunday = 9, Thursday = 9, Tuesday = 9, Wednesday = 9 },
						Idle =  new WeekDays{ Friday = 1, Monday = 1, Saturday = 1, Sunday = 1, Thursday =1, Tuesday = 1, Wednesday = 1 }
					}
				}.ToArray());
			Assert.True(errorInfos.Count == 7);
		}

		//[Fact]
		//public void TestvalidateTargetCyclesWithWrongValues_ExpectFailure()
		//{
		//	var isValid = _validationHelper.ValidateProductivityTargetsForNegativeValues(new List<double> { 9999.0, 8888.0, 7777.0, 123.024, 141, 163.1, 15.1 });
		//	Assert.False(isValid.Valid);
		//}

		//[Fact]
		//public void TestValidateTargetCyclesWithCorrectValue_ExpectSuccess()
		//{
		//	var isValid = _validationHelper.validatePayload(new List<double> { 9999.0, 8888.0, 7777.0, 123.0, 141, 163.1, 15.1 });
		//	Assert.True(isValid.Valid);
		//}

		//[Fact]
		//public void TestValidateAssetUIDParametersWithNull_ExpectFailure()
		//{
		//	var isValid = _validationHelper.ValidateAssetUIDParameters(null);
		//	Assert.False(isValid.Valid);
		//}

		//[Fact]
		//public void TestValidateAssetUIDParametersWithWrongValeus_ExpectFailure()
		//{
		//	var isValid = _validationHelper.ValidateAssetUIDParameters(new string[] { "123" });
		//	Assert.False(isValid.Valid);
		//}

		//[Fact]
		//public void TestValidateAssetUIDParametersWithEmptyGuid_ExpectFailure()
		//{
		//	var isValid = _validationHelper.ValidateAssetUIDParameters(new string[] { Guid.Empty.ToString() });
		//	Assert.False(isValid.Valid);
		//	Assert.True(isValid.ErrorCode == 400102);
		//}

		//[Fact]
		//public void TestValidateAssetTargetHoursWithInvalidTargetCount_ExpectFailure()
		//{
		//	var isValid = _validationHelper.ValidateAssetTargetHours(new List<double> { 8.0 }, "target");
		//	Assert.False(isValid.Valid);
		//	Assert.True(isValid.ErrorCode == 400103);
		//}

		//[Fact]
		//public void TestValidateAssetTargetHoursWithTargetHoursGreaterThan24_ExpectFailure()
		//{
		//	var isValid = _validationHelper.ValidateAssetTargetHours(new List<double> { 8.0, 9.0, 10.0, 11.0, 12.0, 13.0, 26.0 }, "target");
		//	Assert.False(isValid.Valid);
		//	Assert.True(isValid.ErrorCode == 400104);
		//}

		//[Fact]
		//public void TestValidateAssetTargetHoursWithCorrectValues_ExpectSuccess()
		//{
		//	var isValid = _validationHelper.ValidateAssetTargetHours(new List<double> { 8.0, 9.0, 10.0, 11.0, 12.0, 13.0, 14.0 }, "target");
		//	Assert.True(isValid.Valid);
		//}

		//[Fact]
		//public void TestValidateTargetRuntimeWithIdleHoursWithTargetIdleGreaterThanTargetRuntime_ExpectFailure()
		//{
		//	var isValid = _validationHelper.ValidateTargetRuntimeWithIdleHours(new List<double> { 8.0, 9.0, 10.0, 11.0, 12.0, 13.0, 14.0 }, new List<double> { 8.0, 19.0, 10.0, 11.0, 12.0, 13.0, 14.0 });
		//	Assert.False(isValid.Valid);
		//	Assert.True(isValid.ErrorCode == 400105);
		//}

		//[Fact]
		//public void TestValidateTargetRuntimeWithIdleHoursWithCorrectValues_ExpectSuccess()
		//{
		//	var isValid = _validationHelper.ValidateTargetRuntimeWithIdleHours(new List<double> { 8.0, 9.0, 10.0, 11.0, 12.0, 13.0, 14.0 }, new List<double> { 8.0, 9.0, 10.0, 11.0, 12.0, 13.0, 14.0 });
		//	Assert.True(isValid.Valid);
		//}

		//[Fact]
		//public void TestValidateStartDateAndEndDateWithEndDateGreaterThanStartDate_ExpectFailure()
		//{
		//	var isValid = _validationHelper.validateStartDateAndEndDate(DateTime.Now, DateTime.Now.AddDays(-4));
		//	Assert.False(isValid.Valid);
		//	Assert.True(isValid.ErrorCode == 400106);
		//}
	}
}
