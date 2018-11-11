
-- max Settings size in CG is 9809, and that includes colors. Using Text allows us not to define a max.
CREATE TABLE IF NOT EXISTS ProjectSettings ( 
  fk_ProjectUID varchar(36) NOT NULL,
  Settings text NOT NULL,   
  LastActionedUTC datetime(6) DEFAULT NULL,
  InsertUTC datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  UpdateUTC datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (fk_ProjectUID),
  UNIQUE KEY (fk_ProjectUID)
) ENGINE=InnoDB CHARSET = DEFAULT COLLATE = DEFAULT;