
--
-- Table structure for table `Bookmark`
--

CREATE TABLE IF NOT EXISTS `Bookmark` (
  `fk_BookmarkTypeID` int(11) NOT NULL,
  `Value` bigint(20) NOT NULL,
  `UpdateUTC` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`fk_BookmarkTypeID`),
  UNIQUE KEY `UIX_Bookmark_fk_BookmarkTypeID` (`fk_BookmarkTypeID`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
--
-- Table structure for table `BookmarkTypeEnum`
--

CREATE TABLE IF NOT EXISTS `BookmarkTypeEnum` (
  `ID` int(11) NOT NULL,
  `Description` varchar(20) NOT NULL,
  PRIMARY KEY (`ID`),
  UNIQUE KEY `UIX_BookmarkEnum_ID` (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;


--
-- Table structure for table `entries`
--

CREATE TABLE IF NOT EXISTS `entries` (
  `projectId` int(10) unsigned NOT NULL,
  `date` date NOT NULL,
  `weight` double NOT NULL,
  `volume` double DEFAULT NULL,
  `volumeNotRetrieved` tinyint(1) unsigned NOT NULL DEFAULT '0',
  `volumeNotAvailable` tinyint(1) unsigned NOT NULL DEFAULT '0',
  `volumesUpdatedTimestamp` datetime DEFAULT NULL,
  UNIQUE KEY `projectId_date_unique` (`projectId`,`date`),
  KEY `entriesProjectId` (`projectId`),
  CONSTRAINT `projectId` FOREIGN KEY (`projectId`) REFERENCES `projects` (`projectId`) ON DELETE NO ACTION ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;


--
-- Table structure for table `subscriptions`
--

CREATE TABLE IF NOT EXISTS `subscriptions` (
  `subscriptionUid` varchar(36) NOT NULL,
  `customerUid` varchar(36) NOT NULL,
  `startDate` datetime(6) DEFAULT NULL,
  `lastActionedUTC` datetime(6) DEFAULT NULL,
  `InsertUTC` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdateUTC` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  `endDate` datetime DEFAULT NULL,
  PRIMARY KEY (`subscriptionUid`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Table structure for table `users`
--

CREATE TABLE IF NOT EXISTS `users` (
  `userId` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `name` varchar(255) NOT NULL,
  `projectsRetrievedAt` datetime NOT NULL,
  `unitsId` int(10) unsigned DEFAULT NULL,
  PRIMARY KEY (`userId`),
  UNIQUE KEY `name_UNIQUE` (`name`)
) ENGINE=InnoDB AUTO_INCREMENT=3448 DEFAULT CHARSET=utf8;

--
-- Table structure for table `sessions`
--

CREATE TABLE IF NOT EXISTS `sessions` (
  `sessionId` varchar(32) NOT NULL,
  `userId` int(10) unsigned NOT NULL,
  `createdAt` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`sessionId`),
  UNIQUE KEY `sessionId_UNIQUE` (`sessionId`),
  KEY `fkUserId_idx` (`userId`),
  CONSTRAINT `fkUserId` FOREIGN KEY (`userId`) REFERENCES `users` (`userId`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Table structure for table `usersprojects`
--

CREATE TABLE IF NOT EXISTS `usersprojects` (
  `userId` int(10) unsigned NOT NULL,
  `projectId` int(10) unsigned NOT NULL,
  PRIMARY KEY (`userId`,`projectId`),
  UNIQUE KEY `userProjectId` (`userId`,`projectId`),
  KEY `fkProjectId_idx` (`projectId`),
  KEY `usersprojectsUserId` (`userId`),
  CONSTRAINT `upProjectId` FOREIGN KEY (`projectId`) REFERENCES `projects` (`projectId`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `upUserId` FOREIGN KEY (`userId`) REFERENCES `users` (`userId`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;


LOCK TABLES `BookmarkTypeEnum` WRITE;
INSERT IGNORE INTO `BookmarkTypeEnum` VALUES (0,'None'),(16,'ProjectEventOffset');
UNLOCK TABLES;