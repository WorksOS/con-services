CREATE TABLE IF NOT EXISTS DXFUnitsTypeEnum (
  ID int(11) NOT NULL,
  Description varchar(20) NOT NULL,
  PRIMARY KEY (ID),
  UNIQUE KEY UIX_DXFUnitsTypeEnum_ID (ID)
) ENGINE=InnoDB CHARSET = DEFAULT COLLATE = DEFAULT;

INSERT IGNORE DXFUnitsTypeEnum
  (ID,Description) VALUES (0, 'Meters');
INSERT IGNORE DXFUnitsTypeEnum
  (ID,Description) VALUES (1, 'Imperial Ft');
INSERT IGNORE DXFUnitsTypeEnum
  (ID,Description) VALUES (2, 'US survey Ft');  

