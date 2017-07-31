
CREATE TABLE IF NOT EXISTS Filter (
  ID bigint(20) NOT NULL AUTO_INCREMENT,
  CustomerUID varchar(36) NOT NULL,  
  UserUID varchar(36) NOT NULL,  
  ProjectUID varchar(36) NOT NULL,  
  FilterUID varchar(36) NOT NULL,  
  Name varchar(200) DEFAULT NULL,
  FilterJson text NOT NULL,
  IsDeleted tinyint(4) DEFAULT 0,
  LastActionedUTC datetime(6) NOT NULL,
  InsertUTC datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  UpdateUTC datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (ID),
  UNIQUE KEY UIX_Filter_CustomerUID (CustomerUID, UserUID, ProjectUID, FilterUID),
  KEY IX_Filter_ProjectUID (ProjectUID),
  KEY IX_Filter_FilterUID (FilterUID)
) ENGINE=InnoDB CHARSET = DEFAULT COLLATE = DEFAULT;
