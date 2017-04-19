CREATE TABLE IF NOT EXISTS ImportedFileTypeEnum (  
  ID int(11) NOT NULL,
  Description varchar(50) NOT NULL,
  PRIMARY KEY (ID),
  UNIQUE KEY UIX_ImportedFileTypeEnum_ID (ID)
) ENGINE=InnoDB CHARSET = DEFAULT COLLATE = DEFAULT;

/* The Ids are offset by 1 from CGen e.g. Linework==0 in cg.
  Also not all CG types are included. */
INSERT IGNORE ImportedFileTypeEnum
  (ID,Description) VALUES (0, 'Unknown');
INSERT IGNORE ImportedFileTypeEnum
  (ID,Description) VALUES (1, 'Linework');
INSERT IGNORE ImportedFileTypeEnum
	(ID,Description) VALUES (2, 'Design Surface');
INSERT IGNORE ImportedFileTypeEnum
	(ID,Description) VALUES (3, 'Surveyed Surface');
INSERT IGNORE ImportedFileTypeEnum
	(ID,Description) VALUES (4, 'Alignment');

/* 
INSERT IGNORE ImportedFileTypeEnum
	(ID,Description) VALUES (5, 'Mobile Linework');
  INSERT IGNORE ImportedFileTypeEnum
	(ID,Description) VALUES (6, 'Site Boundary');
  INSERT IGNORE ImportedFileTypeEnum
	(ID,Description) VALUES (7, 'Reference Surface');
  INSERT IGNORE ImportedFileTypeEnum
	(ID,Description) VALUES (8, 'Mass Haul Plan');
*/
