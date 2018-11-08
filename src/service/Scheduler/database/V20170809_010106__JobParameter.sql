
CREATE TABLE IF NOT EXISTS JobParameter (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `JobId` int(11) NOT NULL,
  `Name` varchar(40) NOT NULL,
  `Value` longtext,

  PRIMARY KEY (`Id`),
  CONSTRAINT `IX_JobParameter_JobId_Name` UNIQUE (`JobId`,`Name`),
  KEY `FK_JobParameter_Job` (`JobId`),
  CONSTRAINT `FK_JobParameter_Job` FOREIGN KEY (`JobId`) REFERENCES `Job` (`Id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB CHARSET = DEFAULT COLLATE = DEFAULT;
