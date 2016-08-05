CREATE TABLE IF NOT EXISTS `Machine` (
  `ID` BIGINT(20) NOT NULL AUTO_INCREMENT,
  `AssetID` int(10) unsigned NOT NULL,
  `MachineName` varchar(100) NOT NULL,
  `IsJohnDoe` tinyint(1) unsigned NOT NULL,
  `ProjectUID` varchar(36) DEFAULT NULL, 
  `InsertUTC` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdateUTC` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`ID`)  COMMENT ''
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

/* Add ProjectUID column if not there */
SET @s = (SELECT IF(
    (SELECT COUNT(*)
       FROM INFORMATION_SCHEMA.COLUMNS
        WHERE table_name = 'Machine'
        AND table_schema = DATABASE()
        AND column_name = 'ProjectUID'
    ) > 0,
    "SELECT 1",
    "ALTER TABLE `Machine` ADD COLUMN `ProjectUID` varchar(36) AFTER `IsJohnDoe`"
));

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;