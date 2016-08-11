

CREATE TABLE IF NOT EXISTS `ProjectGeofence` ( 
  `fk_ProjectUID` varchar(64) NOT NULL,
  `fk_GeofenceUID` varchar(64) NOT NULL,
  `LastActionedUTC` datetime(6) DEFAULT NULL,
  `InsertUTC` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdateUTC` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`fk_ProjectUID`, `fk_GeofenceUID`),
  KEY (`fk_ProjectUID`, `fk_GeofenceUID`)
) ENGINE=InnoDB CHARSET=DEFAULT;