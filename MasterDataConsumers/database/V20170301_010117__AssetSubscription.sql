
CREATE TABLE IF NOT EXISTS AssetSubscription ( 
  fk_AssetUID varchar(36) NOT NULL,
  fk_SubscriptionUID varchar(36) NOT NULL,
  EffectiveDate date DEFAULT NULL,
  LastActionedUTC datetime(6) DEFAULT NULL,
  InsertUTC datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  UpdateUTC datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (fk_AssetUID, fk_SubscriptionUID),
  UNIQUE KEY (fk_AssetUID, fk_SubscriptionUID)
) ENGINE=InnoDB CHARSET = DEFAULT COLLATE = DEFAULT;