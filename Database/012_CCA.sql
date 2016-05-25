

CREATE TABLE IF NOT EXISTS `CCA` (
  `ID` BIGINT(20) NOT NULL AUTO_INCREMENT,
  `ProjectID` int(10) unsigned NOT NULL,
  `GeofenceUID` varchar(36),
  `Date` date NOT NULL,
  `MachineID` int(10) unsigned NOT NULL,
  `LiftID` int(10) NULL,
  `Incomplete` double DEFAULT NULL,
  `Complete` double DEFAULT NULL,
  `Overcomplete` double DEFAULT NULL,
  `CCANotRetrieved` tinyint(1) unsigned NOT NULL DEFAULT '0',
  `CCANotAvailable` tinyint(1) unsigned NOT NULL DEFAULT '0',
  `CCAUpdatedTimestampUTC` datetime DEFAULT NULL,
  `InsertUTC` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdateUTC` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  UNIQUE KEY `UIX_CCA_ProjectID_Date_GeofenceUID_MachineID_LiftID` (`ProjectID`,`Date`,`GeofenceUID`,`MachineID`,`LiftID`),
  PRIMARY KEY (`ID`)  COMMENT ''
) ENGINE=InnoDB DEFAULT CHARSET=utf8;



