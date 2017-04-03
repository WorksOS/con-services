  
CREATE TABLE IF NOT EXISTS ServiceTypeEnum (
  ID int(11) NOT NULL,
  Description varchar(40) NOT NULL,
  fk_ServiceTypeFamilyID INT(11)  NOT NULL,
  PRIMARY KEY (ID),
  UNIQUE KEY UIX_ServiceTypeEnum (ID),
  KEY IX_ServiceTypeEnum_ServiceTypeFamilyID (fk_ServiceTypeFamilyID)
) ENGINE=InnoDB CHARSET = DEFAULT COLLATE = DEFAULT;

INSERT IGNORE ServiceTypeEnum
  (ID,Description,fk_ServiceTypeFamilyID) VALUES (1, 'Essentials', 1);
INSERT IGNORE ServiceTypeEnum
  (ID,Description,fk_ServiceTypeFamilyID) VALUES (2, 'Manual Maintenance Log', 1);
INSERT IGNORE ServiceTypeEnum
  (ID,Description,fk_ServiceTypeFamilyID) VALUES (3, 'CAT Health', 1); 
INSERT IGNORE ServiceTypeEnum
  (ID,Description,fk_ServiceTypeFamilyID) VALUES (4, 'Standard Health', 1);
INSERT IGNORE ServiceTypeEnum
  (ID,Description,fk_ServiceTypeFamilyID) VALUES (5, 'CAT Utilization', 1);
INSERT IGNORE ServiceTypeEnum
  (ID,Description,fk_ServiceTypeFamilyID) VALUES (6, 'Standard Utilization', 1);
INSERT IGNORE ServiceTypeEnum
  (ID,Description,fk_ServiceTypeFamilyID) VALUES (7, 'CATMAINT', 1);
INSERT IGNORE ServiceTypeEnum
  (ID,Description,fk_ServiceTypeFamilyID) VALUES (8, 'VLMAINT', 1);
INSERT IGNORE ServiceTypeEnum
  (ID,Description,fk_ServiceTypeFamilyID) VALUES (9, 'Real Time Digital Switch Alerts', 1);
INSERT IGNORE ServiceTypeEnum
  (ID,Description,fk_ServiceTypeFamilyID) VALUES (10, '1 minute Update Rate Upgrade', 1);
INSERT IGNORE ServiceTypeEnum
  (ID,Description,fk_ServiceTypeFamilyID) VALUES (11, 'Connected Site Gateway', 1);  
INSERT IGNORE ServiceTypeEnum
  (ID,Description,fk_ServiceTypeFamilyID) VALUES (12, 'Load & Cycle Monitoring', 1);
INSERT IGNORE ServiceTypeEnum
  (ID,Description,fk_ServiceTypeFamilyID) VALUES (13, '3D Project Monitoring', 1);
INSERT IGNORE ServiceTypeEnum
  (ID,Description,fk_ServiceTypeFamilyID) VALUES (14, 'VisionLink RFID', 1);
INSERT IGNORE ServiceTypeEnum
  (ID,Description,fk_ServiceTypeFamilyID) VALUES (15, 'Manual 3D Project Monitoring', 2);
INSERT IGNORE ServiceTypeEnum
  (ID,Description,fk_ServiceTypeFamilyID) VALUES (16, 'Vehicle Connect', 1);
INSERT IGNORE ServiceTypeEnum
  (ID,Description,fk_ServiceTypeFamilyID) VALUES (17, 'Unified Fleet', 1);
INSERT IGNORE ServiceTypeEnum
  (ID,Description,fk_ServiceTypeFamilyID) VALUES (18, 'Advanced Productivity', 1);
INSERT IGNORE ServiceTypeEnum
  (ID,Description,fk_ServiceTypeFamilyID) VALUES (19, 'Landfill', 3);
INSERT IGNORE ServiceTypeEnum
  (ID,Description,fk_ServiceTypeFamilyID) VALUES (20, 'Project Monitoring', 3);
INSERT IGNORE ServiceTypeEnum
  (ID,Description,fk_ServiceTypeFamilyID) VALUES (21, 'Operator Id / Manage Operators', 2);

