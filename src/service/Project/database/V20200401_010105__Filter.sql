USE `Alpha-Project-c2s2`;

CREATE TABLE IF NOT EXISTS `Filter` (
  `ID` bigint(20) NOT NULL AUTO_INCREMENT,
  `FilterUID` varchar(36) COLLATE utf8mb4_unicode_ci NOT NULL,
  `fk_CustomerUID` varchar(80) COLLATE utf8mb4_unicode_ci NOT NULL,
  `fk_ProjectUID` varchar(80) COLLATE utf8mb4_unicode_ci NOT NULL,
  `UserID` varchar(80) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Name` varchar(200) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `fk_FilterTypeID` int(10) NOT NULL DEFAULT '0',
  `FilterJson` text COLLATE utf8mb4_unicode_ci NOT NULL,
  `IsDeleted` tinyint(4) DEFAULT '0',
  `LastActionedUTC` datetime(6) NOT NULL,
  `InsertUTC` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdateUTC` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`ID`),
  UNIQUE KEY `UIX_Filter_FilterUID` (`FilterUID`),
  KEY `UIX_Filter_CustomerUID` (`fk_CustomerUID`,`fk_ProjectUID`,`UserID`),
  KEY `IX_Filter_ProjectUID` (`fk_ProjectUID`),
  KEY `IX_Filter_FilterUID` (`FilterUID`)
) ENGINE=InnoDB CHARSET = DEFAULT COLLATE = DEFAULT;
