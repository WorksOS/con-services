Feature: DeviceService

@Automated @Sanity @Positive
@DeviceService
Scenario Outline: DeviceService_CreateHappyPath
	Given DeviceService Is Ready To Verify 'DeviceService_CreateHappyPath'
	And DeviceServiceCreate Request Is Setup With Default Values
	Then I Update '<RadioFirmwarePartNumber>' And '<FirmwarePartNumber>' 
	When I Post Valid DeviceServiceCreate Request  
	Then The DeviceCreated Details must be stored in MySql DB
	#And The Processed DeviceServiceCreate Message must be available in Kafka topic
Examples:
| Description             | RadioFirmwarePartNumber | FirmwarePartNumber |
| RadioFirmwarePartNumber | NOTEMPTY                | NULL               |
| FirmwarePartNumber      | NULL                    | NOTEMPTY           |
| Both                    | NOTEMPTY                | NOTEMPTY           |


@Automated @Sanity @Positive
@DeviceService
Scenario Outline: DeviceService_DevicePartNumberHappyPath
	Given DeviceService Is Ready To Verify 'DeviceService_DevicePartNumberHappyPath'
	And DeviceServiceCreate Request Is Setup With Default Values
	Then I Set '<CellModemIMEI>' And '<DevicePartNumber>' And '<CellularFirmwarePartnumber>' And '<NetworkFirmwarePartnumber>' And '<SatelliteFirmwarePartnumber>'
	When I Post Valid DeviceServiceCreate Request  
	Then The DeviceCreated Details must be stored in MySql DB
	#And The Processed DeviceServiceCreate Message must be available in Kafka topic
Examples:
| Description                    | CellModemIMEI      | DevicePartNumber | CellularFirmwarePartnumber      | NetworkFirmwarePartnumber      | SatelliteFirmwarePartnumber      |
| CellModemIMEI                  | Test_CellModemIMEI | NULL             | NULL                            | NULL                           | NULL                             |
| CellularFirmwarePartnumber     | NULL               | NULL             | Test_CellularFirmwarePartnumber | NULL                           | NULL                             |
| NetworkFirmwarePartnumber      | NULL               | NULL             | NULL                            | Test_NetworkFirmwarePartnumber | NULL                             |
| SatelliteFirmwarePartnumber    | NULL               | NULL             | NULL                            | NULL                           | Test_SatelliteFirmwarePartnumber |
| CellNetworkSatellitePartnumber | NULL               | NULL             | Test_CellularFirmwarePartnumber | Test_NetworkFirmwarePartnumber | Test_SatelliteFirmwarePartnumber |



@Automated @Sanity @Positive
@DeviceService
Scenario Outline: DeviceService_UpdateHappyPath
	Given DeviceService Is Ready To Verify 'DeviceService_UpdateHappyPath'
	And DeviceServiceCreate Request Is Setup With Default Values
	Then I Update CreateEventRequest With '<CreateEventElement>'
	When I Post Valid DeviceServiceCreate Request  
	Then The DeviceCreated Details must be stored in MySql DB
	#And The Processed DeviceServiceCreate Message must be available in Kafka topic
	Then DeviceServiceUpdate Request Is Setup With Default Values
	And I Update UpdateEventRequest With '<UpdateEventElement>'
	When I Post Valid DeviceServiceupdate Request  
	Then The DeviceUpdated Details must be stored in MySql DB
	#And The Processed DeviceServiceUpdate Message must be available in Kafka topic
Examples:
| Description             | CreateEventElement | UpdateEventElement |
| FirmwarePartNumber      | Radio              | Firmware           |
| RadioFirmwarePartNumber | Firmware           | Radio              |
| Both                    | NOTEMPTY           | NOTEMPTY           |
