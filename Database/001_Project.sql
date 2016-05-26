CREATE TABLE IF NOT EXISTS `Project` (
  `ID` BIGINT(20) NOT NULL AUTO_INCREMENT,
  `ProjectID` INT(10) UNSIGNED NOT NULL COMMENT '',
  `Name` VARCHAR(255) NOT NULL COMMENT '',
  `ProjectTimeZone` VARCHAR(255) NOT NULL COMMENT '',
  `LandfillTimeZone` VARCHAR(255) NOT NULL COMMENT '',
  `RetrievalStartedAt` DATETIME NOT NULL COMMENT '',
  `ProjectUID` varchar(36) DEFAULT NULL,
  `CustomerUID` varchar(36) DEFAULT NULL,
  `SubscriptionUID` varchar(36) DEFAULT NULL,
  `IsDeleted` tinyint(4) DEFAULT 0,
  `LastActionedUTC` datetime(6) DEFAULT NULL,
  `StartDate` datetime DEFAULT NULL,
  `EndDate` datetime DEFAULT NULL,
  `fk_ProjectTypeID` INT(10) UNSIGNED DEFAULT NULL COMMENT '',
  `InsertUTC` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdateUTC` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`ID`)  COMMENT '',
  UNIQUE INDEX `UIX_Project_ProjectUID` (`ProjectUID`)  COMMENT '')
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8;

ALTER TABLE Project
	DROP INDEX UIX_Project_ProjectID;
	
ALTER TABLE Project
	ADD UNIQUE INDEX UIX_Project_ProjectUID (ProjectUID);

