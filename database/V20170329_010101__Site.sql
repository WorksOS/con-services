/*
  DB scripts should be idempotent i.e. able to be run > 1 time without failing or indicating an error.
    e.g. 1) using Create-if not exists
         2) alter schema of change is not made.
*/

CREATE TABLE IF NOT EXISTS Site (  
  SiteUID varchar(36) NOT NULL,
  fk_ProjectUID varchar(36) NOT NULL,
  fk_GeoidUID varchar(36) NOT NULL, 
  IsActive bit(1) DEFAULT 0,    
  LastActionedUTC datetime(6) NULL,
  InsertUTC datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  UpdateUTC datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (SiteUID),
  UNIQUE KEY UIX_Site_SiteUID (SiteUID)
) ENGINE=InnoDB CHARSET = DEFAULT COLLATE = DEFAULT;
