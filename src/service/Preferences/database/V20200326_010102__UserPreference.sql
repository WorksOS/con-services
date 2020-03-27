CREATE TABLE IF NOT EXISTS `UserPreference` (
  `UserPreferenceID` bigint(20) NOT NULL AUTO_INCREMENT,
  `UserUID` varchar(70)  DEFAULT NULL, -- this should be the cws e.g. "trn::profilex:us-west-2:user:eaf7260e-946a-4019-a92d-fab11683149e" (can we get this?)
  `fk_PreferenceKeyID` bigint(20) NOT NULL,
  `Value` mediumtext NOT NULL,
  `SchemaVersion` varchar(10) NOT NULL,
  `InsertUTC` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdateUTC` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`UserPreferenceID`),
  UNIQUE KEY `UserPreference_CustomerUser_Key_UK` (`UserUID`,`fk_PreferenceKeyID`)
  ) ENGINE=InnoDB CHARSET = DEFAULT COLLATE = DEFAULT;
