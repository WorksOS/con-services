Feature: DeviceDetail


@mytag
Scenario: DeviceDetail_HappyPath
	Given There Is DeviceDetailConfigInfoEvent For existing PL321 Device
	When I Publish To 'DeviceDetailsConfigInfoEvent' Kafka Topic
	#Then The Device Details Should Be Updated In Mysql DB
	#And UpdateDeviceEvent Should Be Available In MasterData Device Kafka Topic
