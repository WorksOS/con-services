
CREATE TABLE IF NOT EXISTS `CCA` (
  `ID` BIGINT(20) NOT NULL AUTO_INCREMENT,
  `ProjectUID` varchar(36),
  `GeofenceUID` varchar(36),
  `Date` date NOT NULL,
  `MachineID` int(10) unsigned NOT NULL,
  `LiftID` int(10) NOT NULL,
  `Incomplete` double DEFAULT NULL,
  `Complete` double DEFAULT NULL,
  `Overcomplete` double DEFAULT NULL,
  `CCANotRetrieved` tinyint(1) unsigned NOT NULL DEFAULT '0',
  `CCANotAvailable` tinyint(1) unsigned NOT NULL DEFAULT '0',
  `CCAUpdatedTimestampUTC` datetime DEFAULT NULL,
  `InsertUTC` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdateUTC` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  UNIQUE KEY `UIX_CCA_ProjectUID_Date_GeofenceUID_MachineID_LiftID` (`ProjectUID`,`Date`,`GeofenceUID`,`MachineID`,`LiftID`),
  PRIMARY KEY (`ID`)  COMMENT ''
) ENGINE=InnoDB CHARSET = DEFAULT COLLATE = DEFAULT;

SET @s = (SELECT IF(
    (SELECT COUNT(*)
       FROM INFORMATION_SCHEMA.COLUMNS
        WHERE table_name = 'CCA'        
        AND COLUMN_NAME = 'LiftID'
        AND IS_NULLABLE = 'YES'
    ) > 0,
    "ALTER TABLE `CCA` MODIFY COLUMN LiftID int(10) NOT NULL",
    "SELECT 1"    
));

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;




