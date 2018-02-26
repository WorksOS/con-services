-- USE `VSS-MasterData-Project-Alpha`;

CREATE TABLE IF NOT EXISTS ImportedFileHistory (    
  ID bigint(20) NOT NULL AUTO_INCREMENT,
  fk_ImportedFileUID varchar(36) NOT NULL,
  FileCreatedUTC datetime(6) NOT NULL,
  FileUpdatedUTC datetime(6) NOT NULL,
  ImportedBy varchar(256) NOT NULL,  
  InsertUTC datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  UpdateUTC datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (ID),  
  KEY IX_ImportedFileHistory_ImportedFileUID (fk_ImportedFileUID)
) ENGINE=InnoDB CHARSET = DEFAULT COLLATE = DEFAULT;


-- the following  backfills the ImportedFileHistory table. Can be run > once
/*
-- USE `VSS-MasterData-Project-Alpha`;

INSERT INTO ImportedFileHistory
(fk_ImportedFileUID, FileCreatedUTC, FileUpdatedUTC, ImportedBy)
	SELECT iff.ImportedFileUID, iff.FileCreatedUTC, iff.FileUpdatedUTC, iff.ImportedBy
		FROM ImportedFile iff
		LEFT OUTER JOIN ImportedFileHistory ifh on ifh.fk_ImportedFileUID = iff.ImportedFileUID
                                     AND ifh.FileCreatedUTC = iff.FileCreatedUTC
                                     AND ifh.FileUpdatedUTC = iff.FileUpdatedUTC
		WHERE ifh.fk_ImportedFileUID IS NULL;
*/