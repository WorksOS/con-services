CREATE TABLE IF NOT EXISTS FilterTypeEnum (
  ID int(11) NOT NULL,
  Description varchar(20) NOT NULL,
  PRIMARY KEY (ID),
  UNIQUE KEY UIX_FilterTypeEnum_ID (ID)
) ENGINE=InnoDB CHARSET = DEFAULT COLLATE = DEFAULT;

INSERT IGNORE FilterTypeEnum
  (ID,Description) VALUES (0, 'Persistent');
INSERT IGNORE FilterTypeEnum
  (ID,Description) VALUES (1, 'Transient');
INSERT IGNORE FilterTypeEnum
  (ID,Description) VALUES (2, 'Report');  


