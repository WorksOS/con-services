
CREATE TABLE IF NOT EXISTS JobQueue (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `JobId` int(11) NOT NULL,
  `Queue` varchar(50) NOT NULL,
  `FetchedAt` datetime(6) DEFAULT NULL,
  `FetchToken` varchar(36) DEFAULT NULL,
  
  PRIMARY KEY (`Id`),
  INDEX `IX_JobQueue_QueueAndFetchedAt` (`Queue`,`FetchedAt`)
) ENGINE=InnoDB CHARSET = DEFAULT COLLATE = DEFAULT;
