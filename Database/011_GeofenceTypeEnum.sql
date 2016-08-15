
CREATE TABLE IF NOT EXISTS GeofenceTypeEnum (
  ID int(11) NOT NULL,
  Description varchar(20) NOT NULL,
  PRIMARY KEY (ID),
  UNIQUE KEY UIX_GeofenceTypeEnum_ID (ID)
) ENGINE=InnoDB CHARSET = DEFAULT COLLATE = DEFAULT;

INSERT IGNORE GeofenceTypeEnum
  (ID,Description) VALUES (0, 'Generic');
INSERT IGNORE GeofenceTypeEnum
  (ID,Description) VALUES (1, 'Project');
INSERT IGNORE GeofenceTypeEnum
  (ID,Description) VALUES (2, 'Borrow');
INSERT IGNORE GeofenceTypeEnum
  (ID,Description) VALUES (3, 'Waste');
INSERT IGNORE GeofenceTypeEnum
  (ID,Description) VALUES (4, 'AvoidanceZone');
INSERT IGNORE GeofenceTypeEnum
  (ID,Description) VALUES (5, 'Stockpile');
INSERT IGNORE GeofenceTypeEnum
  (ID,Description) VALUES (6, 'CutZone');
INSERT IGNORE GeofenceTypeEnum
  (ID,Description) VALUES (7, 'FillZone');
INSERT IGNORE GeofenceTypeEnum
  (ID,Description) VALUES (8, 'Import');
INSERT IGNORE GeofenceTypeEnum
  (ID,Description) VALUES (9, 'Export');
INSERT IGNORE GeofenceTypeEnum
  (ID,Description) VALUES (10, 'Landfill');

  


