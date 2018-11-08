
CREATE TABLE IF NOT EXISTS AggregatedCounter (
	Id int(11) NOT NULL AUTO_INCREMENT,
	`Key` varchar(100) NOT NULL,
	`Value` int(11) NOT NULL,
	ExpireAt datetime DEFAULT NULL,
	PRIMARY KEY (`Id`),
	UNIQUE KEY `IX_CounterAggregated_Key` (`Key`)
) ENGINE=InnoDB CHARSET = DEFAULT COLLATE = DEFAULT;
