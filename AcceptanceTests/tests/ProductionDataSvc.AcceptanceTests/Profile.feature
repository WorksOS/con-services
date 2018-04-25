Feature: Profile
	I should be able to request Profile data.

Background: 
	Given the Profile service URI "/api/v1/profiles/productiondata", request repo "ProfileRequest.json" and result repo "ProfileResponse.json"

Scenario Outline: Profile - Good Request
	When I request Profile supplying "<ParameterName>" paramters from the repository
	Then the Profile response should match "<ResultName>" result from the repository
	Examples: 
	| ParameterName                    | ResultName                       |
	| AllByAlignment                   | AllByAlignment                   |
	| AllByLine                        | AllByLine                        |
	| CcvOverrideTarget                | CcvOverrideTarget                |
	| CcvPercentExcludeSupersededLifts | CcvPercentExcludeSupersededLifts |
	| CcvPercentIncludeSupersededLifts | CcvPercentIncludeSupersededLifts |
	| HeightByLine                     | HeightByLine                     |
	| CompositeHeightByLine            | CompositeHeightByLine            |
	| PassCountByLine                  | PassCountByLine                  |
	| TemperatureByAlignment           | TemperatureByAlignment           |
	| MdpUsingMachineMdp               | MdpUsingMachineMdp               |
	| MdpUsingOverrideMdp              | MdpUsingOverrideMdp              |
	| MdpPercentUsingMachineMdp        | MdpPercentUsingMachineMdp        |
	| CcvChangeAllLayerExclSuperseded  | CcvChangeAllLayerExclSuperseded  |
	| CcvChangeAllLayerInclSuperseded  | CcvChangeAllLayerInclSuperseded  |
	| CcvChangeTopLayerInclSuperseded  | CcvChangeTopLayerInclSuperseded  |

Scenario Outline: Profile - Bad Request
	When I request Profile supplying "<ParameterName>" paramters from the repository expecting http error code <httpCode>
	Then the response should contain error code <errorCode>
	Examples: 
	| ParameterName							| httpCode | errorCode |
	| NullProjectId							| 400      | -1        |
	| AlignmentProfileNotSpecifyingStations	| 400      | -1        |