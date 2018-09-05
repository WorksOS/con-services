
CREATE TABLE IF NOT EXISTS `State`
(	Id int(11) NOT NULL AUTO_INCREMENT,
	JobId int(11) NOT NULL,
	Name varchar(20) NOT NULL,
	Reason varchar(100) NULL,
	CreatedAt datetime(6) NOT NULL,
	Data longtext NULL,
	PRIMARY KEY (`Id`),
	KEY `FK_HangFire_State_Job` (`JobId`),
	CONSTRAINT `FK_HangFire_State_Job` FOREIGN KEY (`JobId`) REFERENCES `Job` (`Id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB CHARSET = DEFAULT COLLATE = DEFAULT;
