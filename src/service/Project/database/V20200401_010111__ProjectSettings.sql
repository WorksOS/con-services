USE `Alpha-Project-ccss`;

CREATE TABLE IF NOT EXISTS  `ProjectSettings` (
  `fk_ProjectUID` varchar(80) COLLATE utf8mb4_unicode_ci NOT NULL,
  `fk_ProjectSettingsTypeID` int(10) NOT NULL DEFAULT '0',
  `Settings` text COLLATE utf8mb4_unicode_ci NOT NULL,
  `UserID` varchar(80) COLLATE utf8mb4_unicode_ci NOT NULL,
  `LastActionedUTC` datetime(6) DEFAULT NULL,
  `InsertUTC` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdateUTC` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`fk_ProjectUID`,`UserID`,`fk_ProjectSettingsTypeID`),
  KEY `IX_ProjectUID_UserID` (`fk_ProjectUID`,`UserID`)
) ENGINE=InnoDB CHARSET = DEFAULT COLLATE = DEFAULT;
