CREATE TABLE IF NOT EXISTS AssetDevice
 (
  ID bigint(20) NOT NULL AUTO_INCREMENT,
  fk_DeviceUID varchar(36) NOT NULL,
  fk_AssetUID varchar(36) NOT NULL,
  LastActionedUTC datetime(6) NOT NULL,
  PRIMARY KEY(ID),
  UNIQUE KEY `UIX_AssetDevice_DeviceUID_AssetUID` (fk_DeviceUID, fk_AssetUID),
  KEY `IX_AssetDevice_DeviceUID_AssetUID` (fk_DeviceUID, fk_AssetUID)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;