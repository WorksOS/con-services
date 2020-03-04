CREATE TABLE IF NOT EXISTS `md_subscription_AssetSubscription` (
  `AssetSubscriptionID` bigint(20) NOT NULL AUTO_INCREMENT,
  `AssetSubscriptionUID` binary(16) NOT NULL,
  `fk_AssetUID` binary(16) NOT NULL,
  `fk_DeviceUID` binary(16) NOT NULL,
  `StartDate` datetime(6) NOT NULL,
  `EndDate` datetime(6) DEFAULT NULL,
  `InsertUTC` datetime(6) NOT NULL,
  `UpdateUTC` datetime(6) DEFAULT NULL,
  `fk_CustomerUID` binary(16) NOT NULL,
  `fk_ServiceTypeID` bigint(20) DEFAULT NULL,
  `LastProcessStatus` int(1) NOT NULL,
  `fk_SubscriptionSourceID` int(11) DEFAULT '1',
  PRIMARY KEY (`AssetSubscriptionID`),
  UNIQUE KEY `md_subscription_AssetSubscription_AssetSubscriptionUID_UK` (`AssetSubscriptionUID`),
  KEY `AssetSubs_CustomerUID_IDX` (`fk_CustomerUID`,`StartDate`,`fk_ServiceTypeID`,`EndDate`),
  KEY `AssetSubscription_IDX` (`fk_AssetUID`,`StartDate`,`fk_DeviceUID`,`EndDate`),
  KEY `AS_AssetSubscription_StartDT_EndDT_IDX` (`StartDate`,`EndDate`),
  KEY `AS_fkcolumn_LastProcessStatus_StartDt_EndDt_IDX` (`fk_SubscriptionSourceID`,`LastProcessStatus`,`StartDate`,`EndDate`),
  KEY `AS_AssetSubscription_UpdateUTC_EndDate_StartDate_IDX` (`UpdateUTC`,`EndDate`,`StartDate`)
) ENGINE=InnoDB AUTO_INCREMENT=0 DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `md_subscription_CustomerSubscription` (
  `CustomerSubscriptionID` bigint(20) NOT NULL AUTO_INCREMENT,
  `CustomerSubscriptionUID` binary(16) NOT NULL,
  `fk_CustomerUID` binary(16) NOT NULL,
  `fk_ServiceTypeID` bigint(20) NOT NULL,
  `StartDate` datetime(6) NOT NULL,
  `EndDate` datetime(6) DEFAULT NULL,
  `InsertUTC` datetime(6) NOT NULL,
  `UpdateUTC` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`CustomerSubscriptionID`),
  UNIQUE KEY `md_subscription_CustomerSubscription_UK` (`CustomerSubscriptionUID`),
  KEY `md_subscription_CustomerSubscription_ServiceType_FK` (`fk_ServiceTypeID`),
  KEY `md_subscription_CustomerSubscription_IDX` (`fk_CustomerUID`,`fk_ServiceTypeID`,`StartDate`,`EndDate`),
  CONSTRAINT `md_subscription_CustomerSubscription_ServiceType_FK` FOREIGN KEY (`fk_ServiceTypeID`) REFERENCES `md_subscription_ServiceType` (`ServiceTypeID`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB AUTO_INCREMENT=0 DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `md_subscription_ProjectSubscription` (
  `ProjectSubscriptionID` bigint(20) NOT NULL AUTO_INCREMENT,
  `ProjectSubscriptionUID` binary(16) NOT NULL,
  `fk_ProjectUid` binary(16) DEFAULT NULL,
  `StartDate` datetime(6) NOT NULL,
  `EndDate` datetime(6) DEFAULT NULL,
  `InsertUTC` datetime(6) NOT NULL,
  `UpdateUTC` datetime(6) DEFAULT NULL,
  `fk_CustomerUID` binary(16) NOT NULL,
  `fk_ServiceTypeID` bigint(20) DEFAULT NULL,
  PRIMARY KEY (`ProjectSubscriptionID`),
  UNIQUE KEY `md_subscription_ProjectSubscription_ProjectSubscription_UID_UK` (`ProjectSubscriptionUID`),
  KEY `md_subscription_ProjectSubscription_IDX` (`fk_ProjectUid`,`StartDate`,`EndDate`)
) ENGINE=InnoDB AUTO_INCREMENT=0 DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `md_subscription_ServiceType` (
  `ServiceTypeID` bigint(20) NOT NULL AUTO_INCREMENT,
  `Name` varchar(50) NOT NULL,
  `fk_ServiceTypeFamilyID` bigint(20) NOT NULL,
  `InsertUTC` datetime(6) NOT NULL,
  `UpdateUTC` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`ServiceTypeID`),
  UNIQUE KEY `md_subscription_ServiceType_Name_UK` (`Name`),
  KEY `md_subscription_ServiceType_IDX` (`fk_ServiceTypeFamilyID`),
  CONSTRAINT `md_subscription_ServiceType_ServiceTypeFamily_FK` FOREIGN KEY (`fk_ServiceTypeFamilyID`) REFERENCES `md_subscription_ServiceTypeFamily` (`ServiceTypeFamilyID`)
) ENGINE=InnoDB AUTO_INCREMENT=0 DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `md_subscription_ServiceTypeFamily` (
  `ServiceTypeFamilyID` bigint(20) NOT NULL AUTO_INCREMENT,
  `FamilyName` varchar(50) NOT NULL,
  `InsertUTC` datetime(6) NOT NULL,
  `UpdateUTC` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`ServiceTypeFamilyID`),
  UNIQUE KEY `md_subscription_ServiceTypeFamily_name_UK` (`FamilyName`)
) ENGINE=InnoDB AUTO_INCREMENT=0 DEFAULT CHARSET=utf8;