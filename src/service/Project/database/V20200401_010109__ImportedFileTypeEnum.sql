CREATE TABLE IF NOT EXISTS ImportedFileTypeEnum (
  ID int(11) NOT NULL,
  Description varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  PRIMARY KEY (ID),
  UNIQUE KEY UIX_ImportedFileTypeEnum_ID (ID)
) ENGINE=InnoDB CHARSET = DEFAULT COLLATE = DEFAULT;

INSERT IGNORE ImportedFileTypeEnum
  (ID,Description) VALUES (0, 'Linework');
INSERT IGNORE ImportedFileTypeEnum
  (ID,Description) VALUES (1, 'DesignSurface');
INSERT IGNORE ImportedFileTypeEnum
  (ID,Description) VALUES (2, 'SurveyedSurface');  
INSERT IGNORE ImportedFileTypeEnum
  (ID,Description) VALUES (3, 'Alignment');  
INSERT IGNORE ImportedFileTypeEnum
  (ID,Description) VALUES (6, 'ReferenceSurface');  
INSERT IGNORE ImportedFileTypeEnum
  (ID,Description) VALUES (7, 'MassHaulPlan');  
INSERT IGNORE ImportedFileTypeEnum
  (ID,Description) VALUES (8, 'GeoTiff');   