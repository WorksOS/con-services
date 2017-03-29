
CREATE TABLE IF NOT EXISTS CustomerProject ( 
  fk_CustomerUID varchar(36) NOT NULL,
  fk_ProjectUID varchar(36) NOT NULL, 
  LegacyCustomerID bigint(20) UNSIGNED NULL,
  LastActionedUTC datetime(6) DEFAULT NULL,
  InsertUTC datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  UpdateUTC datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (fk_CustomerUID, fk_ProjectUID),
  KEY IX_Project_AssetUID_CustomerUID_ProjectUID (fk_CustomerUID, fk_ProjectUID)
) ENGINE=InnoDB CHARSET = DEFAULT COLLATE = DEFAULT;