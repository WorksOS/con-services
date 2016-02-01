
SELECT  '*******************************************************';
SELECT  'Start 	     	  000_02_06_003_Subscription_ServiceType';
SELECT  '*******************************************************';

--  US 9136: Alert  Manager Service: Design the Database to accommodate the Alert data model

CREATE TABLE IF NOT EXISTS ServiceType (
  ServiceTypeID 		 BIGINT(20)  NOT NULL AUTO_INCREMENT,
  Name 					 VARCHAR(50) NOT NULL,
  fk_ServiceTypeFamilyID BIGINT(20)  NOT NULL,
  InsertUTC 			 DATETIME(6) NOT NULL,
  UpdateUTC 			 DATETIME(6) DEFAULT NULL,
  PRIMARY KEY (ServiceTypeID),
  UNIQUE KEY ServiceType_Name_UK (Name),
  KEY ServiceType_IDX (fk_ServiceTypeFamilyID),
  CONSTRAINT ServiceType_ServiceTypeFamily_FK FOREIGN KEY (fk_ServiceTypeFamilyID) REFERENCES ServiceTypeFamily (ServiceTypeFamilyID)
  
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

SELECT  '*******************************************************';
SELECT  'End 	     	  000_02_06_003_Subscription_ServiceType';
SELECT  '*******************************************************';
