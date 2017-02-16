
CREATE TABLE IF NOT EXISTS Project (
  ID bigint(20) NOT NULL AUTO_INCREMENT,
  ProjectUID varchar(36) DEFAULT NULL,
  LegacyProjectID bigint(20) UNSIGNED NOT NULL,
  Name varchar(255) NOT NULL,
  fk_ProjectTypeID INT(10) UNSIGNED DEFAULT NULL,
  IsDeleted tinyint(4) DEFAULT 0,
  
  ProjectTimeZone varchar(255) NOT NULL,
  LandfillTimeZone varchar(255) NOT NULL,   
    
  StartDate date DEFAULT NULL,
  EndDate date DEFAULT NULL,
  
  GeometryWKT varchar(4000) DEFAULT NULL,
  
  LastActionedUTC datetime(6) DEFAULT NULL,
  InsertUTC datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  UpdateUTC datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (ID),
  UNIQUE KEY UIX_Project_ProjectUID (ProjectUID ASC)
) ENGINE=InnoDB CHARSET = DEFAULT COLLATE = DEFAULT;


/* currently defaults to null. This could be changed later when existing tables are backfilled */
SET @s = (SELECT IF(
    (SELECT COUNT(*)
       FROM INFORMATION_SCHEMA.COLUMNS
        WHERE table_name = 'Project'
        AND table_schema = DATABASE()
        AND column_name = 'GeofenceUID'
    ) > 0,
    "SELECT 1",
    "ALTER TABLE `Project` ADD COLUMN `GeometryWKT` varchar(4000) DEFAULT NULL, AFTER `EndDate`"
));