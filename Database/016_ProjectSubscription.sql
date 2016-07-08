

CREATE TABLE IF NOT EXISTS `ProjectSubscription` ( 
  `fk_ProjectUID` varchar(64) NOT NULL,
  `fk_SubscriptionUID` varchar(64) NOT NULL,
  `LastActionedUTC` datetime(6) DEFAULT NULL,
  `InsertUTC` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdateUTC` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`fk_ProjectUID`, `fk_SubscriptionUID`),
  KEY (`fk_ProjectUID`, `fk_SubscriptionUID`)
) ENGINE=InnoDB CHARSET=DEFAULT;