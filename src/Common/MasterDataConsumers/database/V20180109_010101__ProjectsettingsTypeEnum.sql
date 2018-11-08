
CREATE TABLE IF NOT EXISTS ProjectSettingsTypeEnum (
  ID int(11) NOT NULL,
  Description varchar(20) NOT NULL,
  PRIMARY KEY (ID),
  UNIQUE KEY UIX_ProjectSettingsTypeEnum_ID (ID)
) ENGINE=InnoDB CHARSET = DEFAULT COLLATE = DEFAULT;

INSERT IGNORE ProjectSettingsTypeEnum
  (ID,Description) VALUES (0, 'Unknown');
INSERT IGNORE ProjectSettingsTypeEnum
  (ID,Description) VALUES (1, 'Targets');
INSERT IGNORE ProjectSettingsTypeEnum
  (ID,Description) VALUES (2, 'ImportedFiles');  

