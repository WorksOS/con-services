

CREATE TABLE IF NOT EXISTS ProjectTypeEnum (
  ID int(11) NOT NULL,
  Description varchar(20) NOT NULL,
  PRIMARY KEY (ID),
  UNIQUE KEY UIX_ProjectTypeEnum (ID)
) ENGINE=InnoDB CHARSET=DEFAULT;

INSERT IGNORE ProjectTypeEnum
  (ID,Description) VALUES (0, 'Standard');
INSERT IGNORE ProjectTypeEnum
  (ID,Description) VALUES (1, 'Landfill');
INSERT IGNORE ProjectTypeEnum
  (ID,Description) VALUES (2, 'Project Monitoring');  


