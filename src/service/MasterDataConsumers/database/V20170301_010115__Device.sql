

CREATE TABLE IF NOT EXISTS  Device
 (
  ID bigint(20) NOT NULL AUTO_INCREMENT,
  DeviceUID varchar(36) NOT NULL,
  DeviceSerialNumber varchar(128) NOT NULL,
  DeviceType varchar(50) NOT NULL,
  DeviceState varchar(128) NOT NULL,
  DeregisteredUTC datetime(6) DEFAULT NULL,
  ModuleType varchar(50) DEFAULT NULL,
  MainboardSoftwareVersion varchar(50) DEFAULT NULL,
  RadioFirmwarePartNumber varchar(50) DEFAULT NULL,
  GatewayFirmwarePartNumber varchar(50) DEFAULT NULL,
  DataLinkType varchar(30) DEFAULT NULL,
  OwningCustomerUID varchar(36) DEFAULT NULL,
  LastActionedUTC datetime(6) NOT NULL,
  PRIMARY KEY (ID),
  UNIQUE KEY `UIX_Device_DeviceUID` (DeviceUID),
  KEY `IX_Device_DeviceSerialNumber_DeviceType` (DeviceSerialNumber, DeviceType)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
