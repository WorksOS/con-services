SELECT  '***********************************************************';
SELECT  'End 	      000_02_06_006_Subscription_ProjectSubscription';
SELECT  '***********************************************************';

--  US 9136: Alert  Manager Service: Design the Database to accommodate the Alert data model
CREATE TABLE IF NOT EXISTS ProjectSubscription (
  ProjectSubscriptionID 	BIGINT(20)  NOT NULL AUTO_INCREMENT,
  ProjectSubscriptionUID 	VARCHAR(64) NOT NULL,
  fk_ProjectUID 			VARCHAR(64) NOT NULL,
  fk_CustomerSubscriptionID BIGINT(20)  NOT NULL,
  StartDate 				DATETIME(6) NOT NULL,
  EndDate 					DATETIME(6)     NULL,
  InsertUTC 				DATETIME(6) NOT NULL,
  UpdateUTC 				DATETIME(6) DEFAULT NULL,
  PRIMARY KEY (ProjectSubscriptionID),
  UNIQUE KEY ProjectSubscriptionID_IDX (fk_ProjectUID, StartDate, EndDate),
  CONSTRAINT ProjectSubs_CustomerSubs_FK FOREIGN KEY (fk_CustomerSubscriptionID) REFERENCES CustomerSubscription (CustomerSubscriptionID) ON DELETE NO ACTION ON UPDATE NO ACTION

) ENGINE=InnoDB DEFAULT CHARSET=latin1;

SELECT  '***********************************************************';
SELECT  'End 	      000_02_06_006_Subscription_ProjectSubscription';
SELECT  '***********************************************************';
