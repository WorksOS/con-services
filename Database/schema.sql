
--
-- Table structure for table `Bookmark`
--

DROP TABLE IF EXISTS `Bookmark`;

CREATE TABLE `Bookmark` (
  `fk_BookmarkTypeID` int(11) NOT NULL,
  `Value` bigint(20) NOT NULL,
  `UpdateUTC` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`fk_BookmarkTypeID`),
  UNIQUE KEY `UIX_Bookmark_fk_BookmarkTypeID` (`fk_BookmarkTypeID`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
--
-- Table structure for table `BookmarkTypeEnum`
--

DROP TABLE IF EXISTS `BookmarkTypeEnum`;
CREATE TABLE `BookmarkTypeEnum` (
  `ID` int(11) NOT NULL,
  `Description` varchar(20) NOT NULL,
  PRIMARY KEY (`ID`),
  UNIQUE KEY `UIX_BookmarkEnum_ID` (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;


--
-- Table structure for table `projects`
--

DROP TABLE IF EXISTS `projects`;
CREATE TABLE `projects` (
  `projectId` int(10) unsigned NOT NULL,
  `name` varchar(255) NOT NULL,
  `timeZone` varchar(255) NOT NULL,
  `retrievalStartedAt` datetime NOT NULL,
  `daysToSubscriptionExpiry` int(11) DEFAULT NULL,
  `projectUid` varchar(36) DEFAULT NULL,
  `customerUid` varchar(36) DEFAULT NULL,
  `subscriptionUid` varchar(36) DEFAULT NULL,
  `lastActionedUTC` datetime DEFAULT NULL,
  `InsertUTC` datetime DEFAULT NULL,
  `UpdateUTC` datetime DEFAULT NULL,
  `IsDeleted` tinyint(4) DEFAULT '0',
  PRIMARY KEY (`projectId`),
  UNIQUE KEY `projectId_UNIQUE` (`projectId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;


--
-- Table structure for table `entries`
--

DROP TABLE IF EXISTS `entries`;
CREATE TABLE `entries` (
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

DROP TABLE IF EXISTS `subscriptions`;
CREATE TABLE `subscriptions` (
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

DROP TABLE IF EXISTS `users`;
CREATE TABLE `users` (
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

DROP TABLE IF EXISTS `sessions`;
CREATE TABLE `sessions` (
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

DROP TABLE IF EXISTS `usersprojects`;
CREATE TABLE `usersprojects` (
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
INSERT INTO `BookmarkTypeEnum` VALUES (0,'None'),(16,'ProjectEventOffset');
UNLOCK TABLES;