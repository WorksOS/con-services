CREATE TABLE IF NOT EXISTS `PreferenceKey` (
  `PreferenceKeyID` bigint(20) NOT NULL AUTO_INCREMENT,
  `PreferenceKeyUID` varchar(36) DEFAULT NULL, -- this could be a Guid
  `KeyName` varchar(100) NOT NULL,
  `InsertUTC` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdateUTC` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`PreferenceKeyID`),
  UNIQUE KEY `PreferenceKey_Name_UK` (`Name`)
) ENGINE=InnoDB CHARSET = DEFAULT COLLATE = DEFAULT;