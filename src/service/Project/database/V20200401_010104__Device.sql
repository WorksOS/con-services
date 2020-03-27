USE `Alpha-Project-c2s2`;

CREATE TABLE IF NOT EXISTS  Device
 (
  ID bigint(20) NOT NULL AUTO_INCREMENT,
  DeviceUID varchar(80) NOT NULL,
  `ShortRaptorProjectID` int(20) unsigned NOT NULL AUTO_INCREMENT,  
  `InsertUTC` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdateUTC` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (ID),
  UNIQUE KEY `UIX_Device_DeviceUID` (DeviceUID),
  KEY `IX_Device_DeviceSerialNumber_DeviceType` (DeviceSerialNumber, DeviceType)
) ENGINE=InnoDB CHARSET = DEFAULT COLLATE = DEFAULT;
