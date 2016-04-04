

CREATE TABLE IF NOT EXISTS `Entries` (
  `ID` BIGINT(20) NOT NULL AUTO_INCREMENT,
  `ProjectID` int(10) unsigned NOT NULL,
  `Date` date NOT NULL,
  `Weight` double NOT NULL,
  `Volume` double DEFAULT NULL,
  `VolumeNotRetrieved` tinyint(1) unsigned NOT NULL DEFAULT '0',
  `VolumeNotAvailable` tinyint(1) unsigned NOT NULL DEFAULT '0',
  `VolumesUpdatedTimestampUTC` datetime DEFAULT NULL,
  `InsertUTC` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdateUTC` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  UNIQUE KEY `UIX_Entries_ProjectID_Date` (`ProjectID`,`Date`),
  PRIMARY KEY (`ID`)  COMMENT ''
) ENGINE=InnoDB DEFAULT CHARSET=utf8;


