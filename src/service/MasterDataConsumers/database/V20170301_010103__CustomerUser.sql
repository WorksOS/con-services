
CREATE TABLE IF NOT EXISTS CustomerUser ( 
  fk_CustomerUID varchar(36) NOT NULL,
  UserUID varchar(36) NOT NULL,
  LastActionedUTC datetime(6) DEFAULT NULL,
  InsertUTC datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  UpdateUTC datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (fk_CustomerUID, UserUID),
  UNIQUE KEY UIX_CustomerUser_CustomerUID_UserUID (fk_CustomerUID, UserUID),
  KEY IX_CustomerUser_UserUID_CustomerUID (UserUID, fk_CustomerUID)
) ENGINE=InnoDB CHARSET = DEFAULT COLLATE = DEFAULT;
