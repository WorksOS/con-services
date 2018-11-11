CREATE TABLE IF NOT EXISTS ProjectHistory (
  ID bigint(20) unsigned NOT NULL AUTO_INCREMENT,
  ProjectUID varchar(36) DEFAULT NULL,
  LegacyProjectID bigint(20) UNSIGNED NOT NULL,
  Name varchar(255) NOT NULL,
  
  Description varchar(2000) DEFAULT NULL,
  fk_ProjectTypeID INT(10) UNSIGNED DEFAULT NULL,  
  IsDeleted tinyint(4) DEFAULT 0,  
  
  ProjectTimeZone varchar(255) NOT NULL,
  LandfillTimeZone varchar(255) NOT NULL,   
    
  StartDate date DEFAULT NULL,
  EndDate date DEFAULT NULL,
  
  GeometryWKT varchar(4000) DEFAULT NULL,
  PolygonST POLYGON NULL,
  
  CoordinateSystemFileName varchar(256)  DEFAULT NULL,  
  CoordinateSystemLastActionedUTC datetime DEFAULT NULL,
  
  LastActionedUTC datetime(6) DEFAULT NULL,
  InsertUTC datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  UpdateUTC datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (ID),
  KEY IX_ProjectHistory_ProjectUID (ProjectUID)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

SET @s = (SELECT IF(
    (SELECT COUNT(*) FROM ProjectHistory
    ) > 0,
    "SELECT 1",
    "INSERT INTO ProjectHistory
	   (ProjectUID, LegacyProjectID, Name, Description, fk_ProjectTypeID,
		IsDeleted, ProjectTimeZone, LandfillTimeZone, StartDate, EndDate,
		GeometryWKT, PolygonST,
		CoordinateSystemFileName, CoordinateSystemLastActionedUTC,
		LastActionedUTC)
	  SELECT ProjectUID, LegacyProjectID, Name, Description, fk_ProjectTypeID,
		  IsDeleted, ProjectTimeZone, LandfillTimeZone, StartDate, EndDate,
		  GeometryWKT, PolygonST,
		  CoordinateSystemFileName, CoordinateSystemLastActionedUTC,
		  LastActionedUTC
	  FROM Project;"
));  

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;  
