SELECT  '***********************************************************';
SELECT  'End 	     	000_02_06_002_Subscription_ServiceTypeFamily';
SELECT  '***********************************************************';

--  US 9136: Alert  Manager Service: Design the Database to accommodate the Alert data model
CREATE TABLE IF NOT EXISTS ServiceTypeFamily (
  ServiceTypeFamilyID BIGINT(20)  NOT NULL AUTO_INCREMENT,
  FamilyName 		  VARCHAR(50) NOT NULL,
  InsertUTC 		  DATETIME(6) NOT NULL,
  UpdateUTC 		  DATETIME(6) DEFAULT NULL,
  PRIMARY KEY (ServiceTypeFamilyID),
  UNIQUE KEY ServiceTypeFamily_UK (FamilyName)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

SELECT  '***********************************************************';
SELECT  'End 	     	000_02_06_002_Subscription_ServiceTypeFamily';
SELECT  '***********************************************************';
