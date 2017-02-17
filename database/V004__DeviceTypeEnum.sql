

CREATE TABLE IF NOT EXISTS DeviceTypeEnum (
  ID int(11) NOT NULL,
  Description varchar(50) NOT NULL,
  PRIMARY KEY (ID),
  UNIQUE KEY UIX_DeviceTypeEnum_ID (ID)
) ENGINE=InnoDB CHARSET = DEFAULT COLLATE = DEFAULT;

INSERT IGNORE DeviceTypeEnum
  (ID,Description) VALUES (0, 'MANUALDEVICE');
INSERT IGNORE DeviceTypeEnum
  (ID,Description) VALUES (1, 'PL121');
INSERT IGNORE DeviceTypeEnum
  (ID,Description) VALUES (2, 'PL321');
INSERT IGNORE DeviceTypeEnum
  (ID,Description) VALUES (3, 'Series522');
INSERT IGNORE DeviceTypeEnum
  (ID,Description) VALUES (4, 'Series523');
INSERT IGNORE DeviceTypeEnum
  (ID,Description) VALUES (5, 'Series521');
INSERT IGNORE DeviceTypeEnum
  (ID,Description) VALUES (6, 'SNM940');
INSERT IGNORE DeviceTypeEnum
  (ID,Description) VALUES (7, 'CrossCheck');  
INSERT IGNORE DeviceTypeEnum
  (ID,Description) VALUES (8, 'TrimTrac');   
INSERT IGNORE DeviceTypeEnum
  (ID,Description) VALUES (9, 'PL420');   
INSERT IGNORE DeviceTypeEnum
  (ID,Description) VALUES (10, 'PL421'); 
INSERT IGNORE DeviceTypeEnum
  (ID,Description) VALUES (11, 'TM3000');   
INSERT IGNORE DeviceTypeEnum
  (ID,Description) VALUES (12, 'TAP66');  
INSERT IGNORE DeviceTypeEnum
  (ID,Description) VALUES (13, 'SNM451');  
INSERT IGNORE DeviceTypeEnum
  (ID,Description) VALUES (14, 'PL431');  
INSERT IGNORE DeviceTypeEnum
  (ID,Description) VALUES (15, 'DCM300');  
INSERT IGNORE DeviceTypeEnum
  (ID,Description) VALUES (16, 'PL641');  
INSERT IGNORE DeviceTypeEnum
  (ID,Description) VALUES (17, 'PLE641');  
INSERT IGNORE DeviceTypeEnum
  (ID,Description) VALUES (18, 'PLE641PLUSPL631');  
INSERT IGNORE DeviceTypeEnum
  (ID,Description) VALUES (19, 'PLE631');  
INSERT IGNORE DeviceTypeEnum
  (ID,Description) VALUES (20, 'PL631');  
INSERT IGNORE DeviceTypeEnum
  (ID,Description) VALUES (21, 'PL241');  
INSERT IGNORE DeviceTypeEnum
  (ID,Description) VALUES (22, 'PL231');  
INSERT IGNORE DeviceTypeEnum
  (ID,Description) VALUES (23, 'BasicVirtualDevice');  
INSERT IGNORE DeviceTypeEnum
  (ID,Description) VALUES (24, 'MTHYPHEN10');  
INSERT IGNORE DeviceTypeEnum
  (ID,Description) VALUES (25, 'XT5060');  
INSERT IGNORE DeviceTypeEnum
  (ID,Description) VALUES (26, 'XT4860');  
INSERT IGNORE DeviceTypeEnum
  (ID,Description) VALUES (27, 'TTUSeries');  
INSERT IGNORE DeviceTypeEnum
  (ID,Description) VALUES (28, 'XT2000');  
INSERT IGNORE DeviceTypeEnum
  (ID,Description) VALUES (29, 'MTGModularGatewayHYPHENMotorEngine');  
INSERT IGNORE DeviceTypeEnum
  (ID,Description) VALUES (30, 'MTGModularGatewayHYPHENElectricEngine');  
INSERT IGNORE DeviceTypeEnum
  (ID,Description) VALUES (31, 'MCHYPHEN3');  
INSERT IGNORE DeviceTypeEnum
  (ID,Description) VALUES (33, 'XT6540');  
INSERT IGNORE DeviceTypeEnum
  (ID,Description) VALUES (34, 'XT65401');  
INSERT IGNORE DeviceTypeEnum
  (ID,Description) VALUES (35, 'XT65402');  
INSERT IGNORE DeviceTypeEnum
  (ID,Description) VALUES (36, 'THREEPDATA');  
INSERT IGNORE DeviceTypeEnum
  (ID,Description) VALUES (37, 'PL131');  
INSERT IGNORE DeviceTypeEnum
  (ID,Description) VALUES (38, 'PL141');  
INSERT IGNORE DeviceTypeEnum
  (ID,Description) VALUES (39, 'PL440');  
INSERT IGNORE DeviceTypeEnum
  (ID,Description) VALUES (40, 'PLE601');  
INSERT IGNORE DeviceTypeEnum
  (ID,Description) VALUES (41, 'PL161');  
INSERT IGNORE DeviceTypeEnum
  (ID,Description) VALUES (42, 'PL240');  
INSERT IGNORE DeviceTypeEnum
  (ID,Description) VALUES (43, 'PL542');  
INSERT IGNORE DeviceTypeEnum
  (ID,Description) VALUES (44, 'PLE642');  
INSERT IGNORE DeviceTypeEnum
  (ID,Description) VALUES (45, 'PLE742');  
INSERT IGNORE DeviceTypeEnum
  (ID,Description) VALUES (46, 'SNM941');  

