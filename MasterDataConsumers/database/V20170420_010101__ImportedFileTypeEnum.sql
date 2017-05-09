CREATE TABLE IF NOT EXISTS ImportedFileTypeEnum (  
  ID int(11) NOT NULL,
  Description varchar(50) NOT NULL,
  PRIMARY KEY (ID),
  UNIQUE KEY UIX_ImportedFileTypeEnum_ID (ID)
) ENGINE=InnoDB CHARSET = DEFAULT COLLATE = DEFAULT;

/* The Ids are offset by 1 from CGen e.g. Linework==0 in cg.
  Also not all CG types are included. */
INSERT IGNORE ImportedFileTypeEnum
  (ID,Description) VALUES (0, 'Linework');
INSERT IGNORE ImportedFileTypeEnum
  (ID,Description) VALUES (1, 'DesignSurface');
INSERT IGNORE ImportedFileTypeEnum
	(ID,Description) VALUES (2, 'SurveyedSurface');
INSERT IGNORE ImportedFileTypeEnum
	(ID,Description) VALUES (3, 'Alignment');

/* 
INSERT IGNORE ImportedFileTypeEnum
	(ID,Description) VALUES (4, 'MobileLinework');
INSERT IGNORE ImportedFileTypeEnum
	(ID,Description) VALUES (5, 'SiteBoundary');
  INSERT IGNORE ImportedFileTypeEnum
	(ID,Description) VALUES (6, 'ReferenceSurface');
  INSERT IGNORE ImportedFileTypeEnum
	(ID,Description) VALUES (7, 'MassHaulPlan');
*/
