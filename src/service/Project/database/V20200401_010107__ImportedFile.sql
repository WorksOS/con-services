CREATE TABLE IF NOT EXISTS ImportedFile (
  fk_ProjectUID varchar(100) NOT NULL,
  ImportedFileUID varchar(100) NOT NULL,
  ImportedFileID bigint(20) unsigned NOT NULL AUTO_INCREMENT,
  LegacyImportedFileID bigint(20) DEFAULT NULL,
  fk_CustomerUID varchar(100) NOT NULL,
  fk_ImportedFileTypeID int(11) NOT NULL,
  Name varchar(256) CHARACTER SET utf8 NOT NULL,
  FileDescriptor varchar(1000) NOT NULL,
  FileCreatedUTC datetime(6) NOT NULL,
  FileUpdatedUTC datetime(6) NOT NULL,
  ImportedBy varchar(256) NOT NULL,
  SurveyedUTC datetime(6) DEFAULT NULL,
  Offset decimal(7,3) DEFAULT '0.000',
  fk_ReferenceImportedFileUID varchar(100) DEFAULT NULL,
  MinZoomLevel int(11) DEFAULT '0',
  MaxZoomLevel int(11) DEFAULT '0',
  fk_DXFUnitsTypeID int(11) DEFAULT '0',
  IsDeleted tinyint(4) DEFAULT '0',
  LastActionedUTC datetime(6) NOT NULL,
  InsertUTC datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  UpdateUTC datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (ImportedFileID),
  UNIQUE KEY UIX_ImportedFile_ImportedFileUID (ImportedFileUID),
  UNIQUE KEY UIX_ImportedFile_ImportedFileID (ImportedFileID),
  KEY IX_ImportedFile_ProjectUID_ImportedFileTypeID (fk_ProjectUID, fk_ImportedFileTypeID),
  KEY IX_ImportedFile_CustomerUID_ProjectUID_ImportedFileTypeID (fk_CustomerUID,fk_ProjectUID,fk_ImportedFileTypeID),
  KEY IX_ImportedFile_ReferenceImportedFileUID (fk_ReferenceImportedFileUID)
) ENGINE=InnoDB CHARSET = DEFAULT COLLATE = DEFAULT;
