CREATE TABLE IF NOT EXISTS GeofenceTypeEnum (
  ID int(11) NOT NULL,
  Description varchar(20) COLLATE utf8mb4_unicode_ci NOT NULL,
  PRIMARY KEY (ID),
  UNIQUE KEY UIX_GeofenceTypeEnum_ID (ID)
) ENGINE=InnoDB CHARSET = DEFAULT COLLATE = DEFAULT;

INSERT IGNORE GeofenceTypeEnum
  (ID,Description) VALUES (0, 'Generic');
INSERT IGNORE GeofenceTypeEnum
  (ID,Description) VALUES (1, 'Project');
INSERT IGNORE GeofenceTypeEnum
  (ID,Description) VALUES (11, 'Filter');  


