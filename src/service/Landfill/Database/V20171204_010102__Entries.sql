

CREATE TABLE IF NOT EXISTS `Entries` (
  `ID` BIGINT(20) NOT NULL AUTO_INCREMENT,
  `ProjectID` int(10) unsigned NOT NULL,
  `ProjectUID` varchar(36) DEFAULT NULL,
  `GeofenceUID` varchar(36),
  `Date` date NOT NULL,
  `Weight` double NOT NULL,
  `Volume` double DEFAULT NULL,
  `VolumeNotRetrieved` tinyint(1) unsigned NOT NULL DEFAULT '0',
  `VolumeNotAvailable` tinyint(1) unsigned NOT NULL DEFAULT '0',
  `VolumesUpdatedTimestampUTC` datetime DEFAULT NULL,
  `InsertUTC` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdateUTC` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  UNIQUE KEY `UIX_Entries_ProjectID_GeofenceUID_Date` (`ProjectID`,`GeofenceUID`, `Date`),
  PRIMARY KEY (`ID`)  COMMENT ''
) ENGINE=InnoDB CHARSET = DEFAULT COLLATE = DEFAULT;

/* Add GeofenceUID column if not there */
SET @s = (SELECT IF(
    (SELECT COUNT(*)
       FROM INFORMATION_SCHEMA.COLUMNS
        WHERE table_name = 'Entries'
        AND table_schema = DATABASE()
        AND column_name = 'GeofenceUID'
    ) > 0,
    "SELECT 1",
    "ALTER TABLE `Entries` ADD COLUMN `GeofenceUID` varchar(36) AFTER `ProjectID`"
));

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

/* Add ProjectUID column if not there */
SET @s = (SELECT IF(
    (SELECT COUNT(*)
       FROM INFORMATION_SCHEMA.COLUMNS
        WHERE table_name = 'Entries'
        AND table_schema = DATABASE()
        AND column_name = 'ProjectUID'
    ) > 0,
    "SELECT 1",
    "ALTER TABLE `Entries` ADD COLUMN `ProjectUID` varchar(36) AFTER `ProjectID`"
));

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

/* Drop old index if there */
SET @s = (SELECT IF(
    (SELECT COUNT(*)
       FROM INFORMATION_SCHEMA.STATISTICS
        WHERE table_name = 'Entries'
        AND table_schema = DATABASE()
        AND index_name = 'UIX_Entries_ProjectID_Date'
    ) = 0,
    "SELECT 1",
    "ALTER TABLE `Entries` DROP KEY UIX_Entries_ProjectID_Date"
));

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

/* Add ProjectUID unique key if not there */
SET @s = (SELECT IF(
    (SELECT COUNT(*)
       FROM INFORMATION_SCHEMA.STATISTICS
        WHERE table_name = 'Entries'
        AND table_schema = DATABASE()
        AND index_name = 'UIX_Entries_ProjectUID_GeofenceUID_Date'
    ) > 0,
    "SELECT 1",
    "ALTER TABLE `Entries` ADD UNIQUE KEY UIX_Entries_ProjectUID_GeofenceUID_Date (ProjectUID, GeofenceUID, Date);"
));

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

/* Add ProjectID unique key if not there */
SET @s = (SELECT IF(
    (SELECT COUNT(*)
       FROM INFORMATION_SCHEMA.STATISTICS
        WHERE table_name = 'Entries'
        AND table_schema = DATABASE()
        AND index_name = 'UIX_Entries_ProjectID_GeofenceUID_Date'
    ) > 0,
    "SELECT 1",
    "ALTER TABLE `Entries` ADD UNIQUE KEY UIX_Entries_ProjectID_GeofenceUID_Date (ProjectID, GeofenceUID, Date);"
));

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
