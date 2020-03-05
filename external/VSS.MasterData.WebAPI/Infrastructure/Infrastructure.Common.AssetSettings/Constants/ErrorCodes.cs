using System.ComponentModel;

namespace CommonModel.Enum
{
	public enum ErrorCodes
    {
        #region InternalServerError - Starts with 500

        #region Unexpected Error - Postfixed with 100

        [Description("An Unexpected Error has occurred")]
        UnexpectedError = 5001001,

		#endregion

		#endregion

		#region Bad Request - Starts with 400

		#region PayloadModel Validator - Postfixed with - 001

		[Description("Invalid Request")]
		InvalidRequest = 4000010,

		[Description("Invalid Request, {0} is missing")]
		InvalidRequestPropertyMissing = 4000011,

		#endregion

		#region AssetSettingsListValidator - Postfixed with 021

		[Description("Filter Value is not available")]
		FilterValueNull = 4000211,

        [Description("{0} asset settings filter is not available")]
        InvalidFilterName = 4000212,

        [Description("Page Number should be greater than 0")]
        PageNumberLessThanOne = 4000213,

        [Description("{0} asset settings sort column is not available")]
        InvalidSortColumn = 4000214,

        [Description("Filter Name is not available")]
        FilterNameNull = 4000215,

        [Description("Page Size should be greater than 0")]
        PageSizeLessThanOne = 4000216,

        #endregion

        #region AssetValidator - Postfixed with 031

        [Description("AssetUIDs is null")]
        AssetUIDListNull = 4000311,

        [Description("Invalid AssetUID")]
        InvalidAssetUID = 4000312,

        #endregion

        #region CommonParametersValidator - Postfixed with 041 - 100

        #region CustomerUIDValidator - Postfixed with 041
        
        [Description("CustomerUid is null")]
        CustomerUIDNull = 4000411,

		[Description("Invalid CustomerUid")]
		InvalidCustomerUID = 4000412,

		#endregion

		#region UserUIDValidator - Postfixed with 051

		[Description("UserUid is null")]
        UserUIDNull = 4000511,

        #endregion

        #region UserUIDValidator - Postfixed with 061

        [Description("Request is null")]
        RequestIsNull = 4000611,

        #endregion

        #endregion

        #region AssetSettingsTargetValue - Postfixed with 101

        [Description("Target value should not be negative value")]
        TargetValueIsNegative = 4001011,

        #endregion

        #region AssetBurnRateSettings - Postfixed with 110

        [Description("Idle value should be less than Work value")]
        WorkValueShouldBeLessThanIdleValue = 4001101,

        [Description("Both Idle and Work value should not be a negative value")]
        WorkAndIdleValueShouldNotBeNegative = 4001102,

        [Description("Work value should not be a negative value")]
        WorkValueShouldNotBeNegative = 4001103,

        [Description("Idle value should not be a negative value")]
        IdleValueShouldNotBeNegative = 4001104,

		#endregion

		#region Subscription - Postfixed with 150

		[Description("Asset Subscription doesn't allow configuring {0}")]
		AssetSubscriptionIsInvalid = 4001500,

		#endregion

		[Description("Work Or Idle values should not be null")]
        WorkOrIdleValuesShouldNotBeNull = 4001105,

        [Description("Both Idle and Work values should be null or should not be null")]
        BothWorkAndIdleValuesShouldBeNullORShouldNotBeNull = 4001106,

		#endregion
	}
}
