Feature: DeviceFirmwarePartNumber

@Positive @Level1 @Regression 
@ECMInfo @HappyPathCellularRadioFirmwarePartNumber 
Scenario: DeviceFirmware_HappyPathCellularRadioFirmwarePartNumber
	Given There Is DeviceCellularRadioFirmwarePartNumber_HappyPath Request
	When I Publish Valid Device Firmware Info to Kafka Topic 
	Then The Device Firmware Info Value Should Be Available In VSSDB

@Positive @Level1 @Regression 
@ECMInfo @HappyPathNetworkManagerFirmwarePartNumber
Scenario: DeviceFirmware_HappyPathNetworkManagerFirmwarePartNumber
	Given There Is DeviceNetworkManagerFirmwarePartNumber_HappyPath Request
	When I Publish Valid Device Firmware Info to Kafka Topic 
	Then The Device Firmware Info Value Should Be Available In VSSDB

@Manual @Sanity @Positive @Level1 @SanityTest 
@ECMInfo @HappyPathSatelliteRadioFirmwarePartNumber 
Scenario: DeviceFirmware_HappyPathSatelliteRadioFirmwarePartNumber
	Given There Is DeviceSatelliteRadioFirmwarePartNumber_HappyPath Request
	When I Publish Valid Device Firmware Info to Kafka Topic 
	Then The Device Firmware Info Value Should Be Available In VSSDB

	@Manual @Sanity @Positive @Level1 @SanityTest 
@ECMInfo @HappyPathCellularRadioFirmwarePartNumber 
Scenario Outline: DeviceFirmware_HappyPathCellularRadioFirmwareValuePartNumber
	Given There Is DeviceCellularRadioFirmwarePartNumber_HappyPath Request With '<Values>'
	When I Publish Valid Device Firmware Info to Kafka Topic 
	Then The Device Firmware Info Value Should Be Available In VSSDB

	Examples: 
	| Description                               | Values    |
	| CellularRadioFirmwarePartNumber_MinLength | MINLENGTH |
	| CellularRadioFirmwarePartNumber_MaxLength | MAXLENGTH |


@Manual @Sanity @Positive @Level1 @SanityTest 
@ECMInfo @HappyPathNetworkManagerFirmwarePartNumber 
Scenario Outline: DeviceFirmware_HappyPathNetworkManagerValueFirmwarePartNumber
	Given There Is DeviceNetworkManagerFirmwarePartNumber_HappyPath Request With '<Values>'
	When I Publish Valid Device Firmware Info to Kafka Topic 
	Then The Device Firmware Info Value Should Be Available In VSSDB

	Examples: 
	| Description                                | Values    |
	| NetworkManagerFirmwarePartNumber_MinLength | MINLENGTH |
	| NetworkManagerFirmwarePartNumber_MaxLength | MAXLENGTH |
	
	
@Manual @Sanity @Positive @Level1 @SanityTest 
@ECMInfo @HappyPathSatelliteRadioFirmwarePartNumber 
Scenario Outline: DeviceFirmware_HappyPathSatelliteRadioValueFirmwarePartNumber
	Given There Is DeviceSatelliteRadioFirmwarePartNumber_HappyPath Request With '<Values>'
	When I Publish Valid Device Firmware Info to Kafka Topic 
	Then The Device Firmware Info Value Should Be Available In VSSDB

	Examples: 
	| Description                                | Values    |
	| SatelliteRadioFirmwarePartNumber_MinLength | MINLENGTH |
	| SatelliteRadioFirmwarePartNumber_MaxLength | MAXLENGTH |
	| SatelliteRadioFirmwarePartNumber_NULL      | NULL      |
	