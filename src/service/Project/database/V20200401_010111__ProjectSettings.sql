CREATE TABLE IF NOT EXISTS  ProjectSettings (
  fk_ProjectUID varchar(100) NOT NULL,
  fk_ProjectSettingsTypeID int(10) NOT NULL DEFAULT '0',
  Settings text NOT NULL,
  UserID varchar(100) NOT NULL,
  LastActionedUTC datetime(6) DEFAULT NULL,
  InsertUTC datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  UpdateUTC datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (fk_ProjectUID,UserID,fk_ProjectSettingsTypeID),
  KEY IX_ProjectUID_UserID (fk_ProjectUID,UserID)
) ENGINE=InnoDB CHARSET = DEFAULT COLLATE = DEFAULT;
