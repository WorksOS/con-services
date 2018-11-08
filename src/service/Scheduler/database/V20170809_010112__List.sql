
CREATE TABLE IF NOT EXISTS List
(	`Id` int(11) NOT NULL AUTO_INCREMENT,
	`Key` varchar(100) NOT NULL,
	`Value` longtext NULL,
	`ExpireAt` datetime(6) NULL,
	PRIMARY KEY (`Id`)
) ENGINE=InnoDB CHARSET = DEFAULT COLLATE = DEFAULT;
