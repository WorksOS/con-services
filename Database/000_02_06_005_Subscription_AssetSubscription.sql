SELECT  '***********************************************************';
SELECT  'End 	     	000_02_06_005_Subscription_AssetSubscription';
SELECT  '***********************************************************';

--  US 9136: Alert  Manager Service: Design the Database to accommodate the Alert data model
CREATE TABLE IF NOT EXISTS AssetSubscription (
  AssetSubscriptionID 		BIGINT(20)  NOT NULL AUTO_INCREMENT,
  AssetSubscriptionUID 	    VARCHAR(64) NOT NULL,
  fk_AssetUID 				VARCHAR(64) NOT NULL,
  fk_DeviceUID 				VARCHAR(64) NOT NULL,
  fk_CustomerSubscriptionID BIGINT(20)  NOT NULL,
  StartDate 				DATETIME(6) NOT NULL,
  EndDate   				DATETIME(6)     NULL,
  InsertUTC 				DATETIME(6) NOT NULL,
  UpdateUTC 				DATETIME(6) DEFAULT NULL,
  PRIMARY KEY (AssetSubscriptionID),
  KEY AssetSubscription_IDX (fk_AssetUID, StartDate, fk_DeviceUID, EndDate),
  CONSTRAINT AssetSubs_CustomerSubs_FK FOREIGN KEY (fk_CustomerSubscriptionID) REFERENCES CustomerSubscription (CustomerSubscriptionID) ON DELETE NO ACTION ON UPDATE NO ACTION

) ENGINE=InnoDB DEFAULT CHARSET=latin1;

SELECT  '***********************************************************';
SELECT  'End 	     	000_02_06_005_Subscription_AssetSubscription';
SELECT  '***********************************************************';
