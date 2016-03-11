
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

INSERT IGNORE ServiceType
  (ServiceTypeID, Name, fk_ServiceTypeFamilyID) VALUES (1, 'Essentials', 1);
INSERT IGNORE ServiceType
  (ServiceTypeID, Name, fk_ServiceTypeFamilyID) VALUES (2, 'Manual Maintenance Log', 1);
INSERT IGNORE ServiceType
  (ServiceTypeID, Name, fk_ServiceTypeFamilyID) VALUES (3, 'CAT Health', 1);
INSERT IGNORE ServiceType
  (ServiceTypeID, Name, fk_ServiceTypeFamilyID) VALUES (4, 'Standard Health', 1);
INSERT IGNORE ServiceType
  (ServiceTypeID, Name, fk_ServiceTypeFamilyID) VALUES (5, 'CAT Utilization', 1);
INSERT IGNORE ServiceType
  (ServiceTypeID, Name, fk_ServiceTypeFamilyID) VALUES (6, 'Standard Utilization', 1);
INSERT IGNORE ServiceType
  (ServiceTypeID, Name, fk_ServiceTypeFamilyID) VALUES (7, 'CATMAINT', 1);
INSERT IGNORE ServiceType
  (ServiceTypeID, Name, fk_ServiceTypeFamilyID) VALUES (8, 'VLMAINT', 1);
INSERT IGNORE ServiceType
  (ServiceTypeID, Name, fk_ServiceTypeFamilyID) VALUES (9, 'Real Time Digital Switch Alerts', 1);
INSERT IGNORE ServiceType
  (ServiceTypeID, Name, fk_ServiceTypeFamilyID) VALUES (10, '1 minute Update Rate Upgrade', 1);
INSERT IGNORE ServiceType
  (ServiceTypeID, Name, fk_ServiceTypeFamilyID) VALUES (11, 'Connected Site Gateway', 1);
INSERT IGNORE ServiceType
  (ServiceTypeID, Name, fk_ServiceTypeFamilyID) VALUES (12, 'Load & Cycle Monitoring', 1);
INSERT IGNORE ServiceType
  (ServiceTypeID, Name, fk_ServiceTypeFamilyID) VALUES (13, '3D Project Monitoring', 1);
INSERT IGNORE ServiceType
  (ServiceTypeID, Name, fk_ServiceTypeFamilyID) VALUES (14, 'VisionLink RFID', 1);
INSERT IGNORE ServiceType
  (ServiceTypeID, Name, fk_ServiceTypeFamilyID) VALUES (15, 'Manual 3D Project Monitoring', 2);
INSERT IGNORE ServiceType
  (ServiceTypeID, Name, fk_ServiceTypeFamilyID) VALUES (16, 'Vehicle Connect', 1);
INSERT IGNORE ServiceType
  (ServiceTypeID, Name, fk_ServiceTypeFamilyID) VALUES (17, 'Unified Fleet', 1);
INSERT IGNORE ServiceType
  (ServiceTypeID, Name, fk_ServiceTypeFamilyID) VALUES (18, 'Advanced Productivity', 1);
INSERT IGNORE ServiceType
  (ServiceTypeID, Name, fk_ServiceTypeFamilyID) VALUES (19, 'Landfill', 3);
INSERT IGNORE ServiceType
  (ServiceTypeID, Name, fk_ServiceTypeFamilyID) VALUES (20, 'Project Monitoring', 3);
INSERT IGNORE ServiceType
  (ServiceTypeID, Name, fk_ServiceTypeFamilyID) VALUES (21, 'Operator Id/ Manage Operators', 2);

SELECT  '*******************************************************';
SELECT  'End 	     	  000_02_06_003_Subscription_ServiceType';
SELECT  '*******************************************************';
