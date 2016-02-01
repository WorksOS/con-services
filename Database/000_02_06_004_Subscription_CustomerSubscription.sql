SELECT  '***********************************************************';
SELECT  'End 	     000_02_06_004_Subscription_CustomerSubscription';
SELECT  '***********************************************************';

--  US 9136: Alert  Manager Service: Design the Database to accommodate the Alert data model
CREATE TABLE IF NOT EXISTS CustomerSubscription (
  CustomerSubscriptionID bigint(20) NOT NULL AUTO_INCREMENT,
  fk_CustomerUID   VARCHAR(64) NOT NULL,
  fk_ServiceTypeID BIGINT(20)  NOT NULL,
  StartDate 	   DATETIME(6) NOT NULL,
  EndDate   	   DATETIME(6)     NULL,
  InsertUTC 	   DATETIME(6) NOT NULL,
  UpdateUTC 	   DATETIME(6) DEFAULT NULL,
  PRIMARY KEY (CustomerSubscriptionID),
  UNIQUE KEY CustomerSubscription_IDX (fk_CustomerUID, fk_ServiceTypeID , StartDate, EndDate),
  CONSTRAINT CustomerSubscription_ServiceType_FK FOREIGN KEY (fk_ServiceTypeID) REFERENCES ServiceType (ServiceTypeID) ON DELETE NO ACTION ON UPDATE NO ACTION

) ENGINE=InnoDB DEFAULT CHARSET=latin1;

SELECT  '***********************************************************';
SELECT  'End 	     000_02_06_004_Subscription_CustomerSubscription';
SELECT  '***********************************************************';
