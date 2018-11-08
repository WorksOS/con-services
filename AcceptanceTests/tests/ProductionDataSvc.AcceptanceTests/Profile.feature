Feature: Profile
  I should be able to request Profile data.

Scenario Outline: Profile - Good Request
  Given the service route "/api/v1/profiles/productiondata" request repo "ProfileRequest.json" and result repo "ProfileResponse.json"
  When I POST with parameter "<ParameterName>" I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | ParameterName                    | ResultName                       | HttpCode |
  | AllByAlignment                   | AllByAlignment                   | 200      |
  | AllByLine                        | AllByLine                        | 200      |
  | CcvOverrideTarget                | CcvOverrideTarget                | 200      |
  | CcvPercentExcludeSupersededLifts | CcvPercentExcludeSupersededLifts | 200      |
  | CcvPercentIncludeSupersededLifts | CcvPercentIncludeSupersededLifts | 200      |
  | HeightByLine                     | HeightByLine                     | 200      |
  | CompositeHeightByLine            | CompositeHeightByLine            | 200      |
  | PassCountByLine                  | PassCountByLine                  | 200      |
  | TemperatureByAlignment           | TemperatureByAlignment           | 200      |
  | MdpUsingMachineMdp               | MdpUsingMachineMdp               | 200      |
  | MdpUsingOverrideMdp              | MdpUsingOverrideMdp              | 200      |
  | MdpPercentUsingMachineMdp        | MdpPercentUsingMachineMdp        | 200      |
  | CcvChangeAllLayerExclSuperseded  | CcvChangeAllLayerExclSuperseded  | 200      |
  | CcvChangeAllLayerInclSuperseded  | CcvChangeAllLayerInclSuperseded  | 200      |
  | CcvChangeTopLayerInclSuperseded  | CcvChangeTopLayerInclSuperseded  | 200      |

Scenario Outline: Profile - Bad Request
  Given the service route "/api/v1/profiles/productiondata" request repo "ProfileRequest.json" and result repo "ProfileResponse.json"
  When I POST with parameter "<ParameterName>" I expect response code <HttpCode>
  Then the response should contain code "<ErrorCode>"
  Examples: 
  | ParameterName                         | HttpCode | ErrorCode |
  | NullProjectId                         | 400      | -1        |
  | AlignmentProfileNotSpecifyingStations | 400      | -1        |