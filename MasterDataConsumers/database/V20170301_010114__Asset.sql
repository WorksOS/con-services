
CREATE TABLE IF NOT EXISTS Asset 
(
  ID bigint(20) NOT NULL AUTO_INCREMENT,
  AssetUID varchar(36) NOT NULL,
  LegacyAssetID bigint(20),
  `Name` varchar(128) NULL,
  MakeCode varchar(128) NULL,
  SerialNumber varchar(128) NULL,
  Model varchar(128) NULL,  
  ModelYear int(11) NULL,
  IconKey int NULL,
  AssetType varchar(128) NULL,
  IsDeleted bool DEFAULT 0,  
  OwningCustomerUID varchar(36) DEFAULT NULL,
  EquipmentVIN varchar(50) DEFAULT NULL,
  LastActionedUTC datetime(6) NULL,
  InsertUTC datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  UpdateUTC datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY(ID),
  UNIQUE KEY UIX_Asset_AssetUID (AssetUID),
  KEY `IX_Asset_AssetUID` (AssetUID)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


SET @s = (SELECT IF(
    (SELECT COUNT(*)
       FROM INFORMATION_SCHEMA.COLUMNS
        WHERE table_name = 'Asset'
        AND table_schema = DATABASE()
        AND column_name = 'EquipmentVIN'
    ) > 0,
    "SELECT 1",
    "ALTER TABLE `Asset` ADD COLUMN `EquipmentVIN` varchar(50) DEFAULT NULL AFTER `OwningCustomerUID`"
)); 

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;   

SET @s = (SELECT IF(
    (SELECT COUNT(*)
       FROM INFORMATION_SCHEMA.COLUMNS
        WHERE table_name = 'Asset'
        AND table_schema = DATABASE()
        AND column_name = 'ModelYear'
    ) > 0,
    "SELECT 1",
    "ALTER TABLE `Asset` ADD COLUMN `ModelYear` int(11) NULL AFTER `Model`"
));  

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt; 