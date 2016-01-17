CREATE TABLE IF NOT EXISTS projects (
  projectId INT(10) UNSIGNED NOT NULL COMMENT '',
  name VARCHAR(255) NOT NULL COMMENT '',
  timeZone VARCHAR(255) NOT NULL COMMENT '',
  retrievalStartedAt DATETIME NOT NULL COMMENT '',
  daysToSubscriptionExpiry int(11) DEFAULT NULL,
  projectUid varchar(36) DEFAULT NULL,
  customerUid varchar(36) DEFAULT NULL,
  subscriptionUid varchar(36) DEFAULT NULL,
  lastActionedUTC datetime DEFAULT NULL,
  InsertUTC datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  UpdateUTC datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (projectId)  COMMENT '',
  UNIQUE INDEX projectId_UNIQUE (projectId ASC)  COMMENT '')
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8;