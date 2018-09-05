
CREATE TABLE IF NOT EXISTS DistributedLock (
  `Resource` varchar(100) NOT NULL,
  `CreatedAt` datetime(6) NOT NULL
) ENGINE=InnoDB CHARSET = DEFAULT COLLATE = DEFAULT;
