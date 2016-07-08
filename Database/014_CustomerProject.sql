
CREATE TABLE IF NOT EXISTS `CustomerProject` ( 
  `fk_CustomerUID` varchar(64) NOT NULL,
  `fk_ProjectUID` varchar(64) NOT NULL,
  `LegacyCustomerID` INT(10) UNSIGNED NULL,
  `LastActionedUTC` datetime(6) DEFAULT NULL,
  `InsertUTC` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdateUTC` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`fk_CustomerUID`, `fk_ProjectUID`),
  KEY (`fk_ProjectUID`, `fk_CustomerUID`)
) ENGINE=InnoDB CHARSET=DEFAULT;