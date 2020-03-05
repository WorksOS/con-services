Feature: AssetDeviceSearchService

	References : A. Contract Document - None
	User Story 31219 : Support APP: AssetAPI to support search of Assets for a Support User
#___________________________________________________________________________________________

@Automated @Sanity @Positive
@AssetSearchService @US31219
Scenario: AssetDeviceSearchService_AssetSNHappyPath
	Given AssetDeviceSearchService Is Ready To Verify 'AssetDeviceSearchService_AssetSNHappyPath'
	And AssetServiceCreate Request Is Setup With Default Values
	When I Post Valid AssetServiceCreate Request
	And I Get the AssetDeviceSearchService For 'AssetSN'
	Then AssetDeviceSearchService Response With Created 'Asset' Should Be Returned

@Automated @Sanity @Positive
@AssetSearchService @US31219
Scenario: AssetDeviceSearchService_AssetNameHappyPath
	Given AssetDeviceSearchService Is Ready To Verify 'AssetDeviceSearchService_AssetNameHappyPath'
	And AssetServiceCreate Request Is Setup With Default Values
	When I Post Valid AssetServiceCreate Request
	And I Get the AssetDeviceSearchService For 'AssetName'
	Then AssetDeviceSearchService Response With Created 'Asset' Should Be Returned

@Automated @Sanity @Positive
@AssetSearchService @US31219
Scenario: AssetDeviceSearchService_DeviceSNHappyPath
	Given AssetDeviceSearchService Is Ready To Verify 'AssetDeviceSearchService_DeviceSNHappyPath'
	And DeviceServiceCreate Request Is Setup With Default Values
	When I Post Valid DeviceServiceCreate Request
	And I Get the AssetDeviceSearchService For 'DeviceSN'
	Then AssetDeviceSearchService Response With Created 'Device' Should Be Returned

@Automated @Sanity @Positive
@AssetSearchService @US31219
Scenario: AssetDeviceSearchService_DeviceAsset
	Given AssetDeviceSearchService Is Ready To Verify 'AssetDeviceSearchService_AssetDevice'
	And AssetDeviceAssociation Request Is Setup With Default Values
	When I Post Valid DeviceAssetAssociation Request
	And I Get the AssetDeviceSearchService For 'DeviceSN'
	Then AssetDeviceSearchService Response With Created 'DeviceAsset' Should Be Returned

@Automated @Sanity @Positive
@AssetSearchService @US31219
Scenario: AssetDeviceSearchService_AssetSNSorting
	Given AssetDeviceSearchService Is Ready To Verify 'AssetDeviceSearchService_AssetSNSorting'
	When I Get the AssetDeviceSearchService For 'AssetSNSorting'
	Then AssetDeviceSearchService Response Sorted By AssetSN Should Be Returned

@Automated @Sanity @Positive
@AssetSearchService @US31219
Scenario Outline: AssetDeviceSearchService_Valid
	Given AssetDeviceSearchService Is Ready To Verify 'AssetDeviceSearchService_<Description>'
	When I Get the AssetDeviceSearchService With '<SearchString>', '<PageNo>' And '<PageSize>'
	Then AssetDeviceSearchService Response With Valid '<Description>' Should Be Returned
Examples: 
| Description                | SearchString | PageNo | PageSize |
| PageNo                     | EMPTY        | Valid  | EMPTY    |
| PageSize                   | EMPTY        | EMPTY  | Valid    |
| SearchStringPageNo         | Valid        | Valid  | EMPTY    |
| SearchStringPageSize       | Valid        | EMPTY  | Valid    |
| PageNoPageSize             | EMPTY        | Valid  | Valid    |
| SearchStringPageNoPageSize | Valid        | Valid  | Valid    |

@Automated @Sanity @Negative
@AssetSearchService @US31219
Scenario Outline: AssetDeviceSearchService_Wrong
	Given AssetDeviceSearchService Is Ready To Verify 'AssetDeviceSearchService_<Description>'
	When I Get the AssetDeviceSearchService With '<SearchString>', '<PageNo>' And '<PageSize>'
	Then AssetDeviceSearchService Response With No AssetDevices Should Be Returned
Examples: 
| Description                     | SearchString | PageNo | PageSize |
| SearchString                    | Wrong        | EMPTY  | EMPTY    |
| PageNo                          | EMPTY        | Wrong  | EMPTY    |
| SearchStringPageNo              | Valid        | Wrong  | EMPTY    |
| PageNoPageSize                  | EMPTY        | Wrong  | Wrong    |
| SearchStringPageNoPageSize      | Valid        | Wrong  | Wrong    |
| PageNoValidSearchString         | Valid        | Wrong  | EMPTY    |
| PageNoPageSizeValidSearchString | Valid        | Wrong  | Wrong    |

@Automated @Sanity @Negative
@AssetSearchService @US31219
Scenario Outline: AssetDeviceSearchService_Invalid
	Given AssetDeviceSearchService Is Ready To Verify 'AssetDeviceSearchService_<Description>'
	When I Get the AssetDeviceSearchService With '<SearchString>', '<PageNo>' And '<PageSize>'
	Then AssetDeviceSearchService Response With ErrorMessage Should Be Returned
Examples: 
| Description                     | SearchString | PageNo  | PageSize |
| PageNoZero                      | EMPTY        | 0       | EMPTY    |
| PageNo                          | EMPTY        | -1      | EMPTY    |
| PageSizeZero                    | EMPTY        | EMPTY   | 0        |
| PageSize                        | EMPTY        | EMPTY   | -1       |
| SearchStringPageNo              | Invalid      | Invalid | EMPTY    |
| SearchStringPageSize            | Invalid      | EMPTY   | Invalid  |
| PageNoPageSize                  | EMPTY        | Invalid | Invalid  |
| SearchStringPageNoPageSize      | Invalid      | Invalid | Invalid  |
| PageNoValidSearchString         | Valid        | Invalid | EMPTY    |
| PageSizeValidSearchString       | Valid        | EMPTY   | Invalid  |
| PageNoPageSizeValidSearchString | Valid        | Invalid | Invalid  |

@Automated @Sanity @Negative
@AssetSearchService @US31219
Scenario: AssetDeviceSearchService_WrongPageSize
	Given AssetDeviceSearchService Is Ready To Verify 'AssetDeviceSearchService_<Description>'
	When I Get the AssetDeviceSearchService With 'Valid' SearchString And 'Wrong' PageSize
	Then AssetDeviceSearchService Response With Wrong PageSize Should Be Returned