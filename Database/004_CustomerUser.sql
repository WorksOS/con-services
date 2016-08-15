
CREATE TABLE IF NOT EXISTS `CustomerUser` ( 
  `fk_CustomerUID` varchar(64) NOT NULL,
  `fk_UserUID` varchar(64) NOT NULL,
  `LastActionedUTC` datetime(6) DEFAULT NULL,
  `InsertUTC` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdateUTC` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`fk_CustomerUID`, `fk_UserUID`),
  KEY (`fk_UserUID`, `fk_CustomerUID`)
) ENGINE=InnoDB CHARSET = DEFAULT COLLATE = DEFAULT;
