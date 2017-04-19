CREATE TABLE IF NOT EXISTS ImportedFile (  
  fk_ProjectUID varchar(36) NOT NULL,
  ImportedFileUID varchar(36) NOT NULL,
  fk_CustomerUID varchar(36) NOT NULL,  
  fk_ImportedFileTypeID int(11) DEFAULT 0, 
  Name nvarchar(256) NOT NULL,
  SurveyedUTC datetime(6) DEFAULT NULL,
  -- todo TCCPath nvarchar(256) NOT NULL, -- name/size cg=SourcePath(nvchar 400) and SourceFilespaceID(nvchar 50) ? 
    
  LastActionedUTC datetime(6) DEFAULT NULL,
  InsertUTC datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  UpdateUTC datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (ImportedFileUID),
  UNIQUE KEY UIX_ImportedFile_ImportedFileUID (ImportedFileUID),
  UNIQUE KEY UIX_ImportedFile_ProjectUID_Name_SurveyedUTC (fk_ProjectUID,Name, SurveyedUTC),
  KEY IX_ImportedFile_ProjectUID_ImportedFileTypeID (fk_ProjectUID,fk_ImportedFileTypeID),
  KEY IX_ImportedFile_CustomerUID_ProjectUID_ImportedFileTypeID (fk_CustomerUID,fk_ProjectUID,fk_ImportedFileTypeID)
) ENGINE=InnoDB CHARSET = DEFAULT COLLATE = DEFAULT;
