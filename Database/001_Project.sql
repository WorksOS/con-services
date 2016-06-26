
CREATE TABLE IF NOT EXISTS `Project` (
  `ID` BIGINT(20) NOT NULL AUTO_INCREMENT,
  `ProjectID` INT(10) UNSIGNED NOT NULL COMMENT '',
  `Name` VARCHAR(255) NOT NULL COMMENT '',
  `ProjectTimeZone` VARCHAR(255) NOT NULL COMMENT '',
  `LandfillTimeZone` VARCHAR(255) NOT NULL COMMENT '',
  `RetrievalStartedAt` DATETIME NOT NULL COMMENT '',
  `ProjectUID` varchar(36) DEFAULT NULL,
  `CustomerUID` varchar(36) DEFAULT NULL,
  `LegacyCustomerID` INT(10) UNSIGNED NULL COMMENT '', 
  `SubscriptionUID` varchar(36) DEFAULT NULL,
  `IsDeleted` tinyint(4) DEFAULT 0,
  `LastActionedUTC` datetime(6) DEFAULT NULL,
  `StartDate` datetime DEFAULT NULL,
  `EndDate` datetime DEFAULT NULL,
  `fk_ProjectTypeID` INT(10) UNSIGNED DEFAULT NULL COMMENT '',
  `InsertUTC` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdateUTC` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`ID`)  COMMENT '',
  UNIQUE INDEX `UIX_Project_ProjectID` (`ProjectID` ASC)  COMMENT '')
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8;

/* Add CustomerID if not there */
SET @s = (SELECT IF(
    (SELECT COUNT(*)
       FROM INFORMATION_SCHEMA.COLUMNS
        WHERE table_name = 'Project'
        AND table_schema = DATABASE()
        AND column_name = 'LegacyCustomerID'
    ) > 0,
    "SELECT 1",
    "ALTER TABLE `Project` ADD COLUMN `LegacyCustomerID` INT(10) UNSIGNED NULL AFTER `CustomerUID`"
));
PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;



