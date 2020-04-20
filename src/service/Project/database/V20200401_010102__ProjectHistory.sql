CREATE TABLE IF NOT EXISTS ProjectHistory (
  ProjectUID varchar(100) DEFAULT NULL,
  CustomerUID varchar(100) DEFAULT NULL,       
  ShortRaptorProjectID int(20) unsigned NOT NULL,
  Name varchar(255) NOT NULL,  
  fk_ProjectTypeID int(10) unsigned DEFAULT NULL, 
  ProjectTimeZone varchar(255) NOT NULL,
  ProjectTimeZoneIana varchar(255) NOT NULL,
  Boundary polygon DEFAULT NULL,
  CoordinateSystemFileName varchar(256) DEFAULT NULL,
  CoordinateSystemLastActionedUTC datetime DEFAULT NULL,
  IsArchived tinyint(4) DEFAULT '0',  
  LastActionedUTC datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  InsertUTC datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  UpdateUTC datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)
) ENGINE=InnoDB CHARSET = DEFAULT COLLATE = DEFAULT;