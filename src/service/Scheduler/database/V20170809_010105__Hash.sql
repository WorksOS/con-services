
CREATE TABLE IF NOT EXISTS Hash (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Key` varchar(100) NOT NULL,
  `Field` varchar(40) NOT NULL,
  `Value` longtext,
  `ExpireAt` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Hash_Key_Field` (`Key`,`Field`)
) ENGINE=InnoDB CHARSET = DEFAULT COLLATE = DEFAULT;
