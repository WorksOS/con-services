

CREATE TABLE IF NOT EXISTS `Subscription` (
  `ID` BIGINT(20) NOT NULL AUTO_INCREMENT,
  `SubscriptionUID` varchar(36) NOT NULL,
  `CustomerUID` varchar(36) NOT NULL,
  `fk_ServiceTypeID` INT(11)  NOT NULL,
  `StartDate` datetime(6) DEFAULT NULL,
  `EndDate` datetime DEFAULT NULL,
  `EffectiveUTC` datetime(6) DEFAULT NULL,
  `LastActionedUTC` datetime(6) DEFAULT NULL,
  `InsertUTC` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdateUTC` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),  
  UNIQUE KEY `UIX_Subscription_SubscriptionUID` (`SubscriptionUID`),
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

