CREATE TABLE IF NOT EXISTS `md_preference_PreferenceKey` (
  `PreferenceKeyID` bigint(20) NOT NULL AUTO_INCREMENT,
  `PreferenceKeyUID` binary(16) NOT NULL,
  `PreferenceKeyName` varchar(100) NOT NULL,
  `InsertUTC` datetime(6) NOT NULL,
  `UpdateUTC` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`PreferenceKeyID`),
  UNIQUE KEY `PreferenceKey_Name_UK` (`PreferenceKeyName`),
  UNIQUE KEY `PreferenceKey_UID_UK` (`PreferenceKeyUID`)
) ENGINE=InnoDB AUTO_INCREMENT=0 DEFAULT CHARSET=utf8;
CREATE TABLE IF NOT EXISTS `md_preference_PreferenceUser` (
  `PreferenceUserID` bigint(20) NOT NULL AUTO_INCREMENT,
  `fk_UserUID` binary(16) DEFAULT NULL,
  `fk_PreferenceKeyID` bigint(20) NOT NULL,
  `PreferenceValue` mediumtext NOT NULL,
  `SchemaVersion` varchar(10) NOT NULL,
  `InsertUTC` datetime(6) NOT NULL,
  `UpdateUTC` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`PreferenceUserID`),
  UNIQUE KEY `PreferenceUser_CustomerUserKey_UK` (`fk_UserUID`,`fk_PreferenceKeyID`),
  KEY `PreferenceUser_PreferenceKey_FK` (`fk_PreferenceKeyID`),
  CONSTRAINT `PreferenceUser_PreferenceKey_FK` FOREIGN KEY (`fk_PreferenceKeyID`) REFERENCES `md_preference_PreferenceKey` (`PreferenceKeyID`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB AUTO_INCREMENT=0 DEFAULT CHARSET=utf8;