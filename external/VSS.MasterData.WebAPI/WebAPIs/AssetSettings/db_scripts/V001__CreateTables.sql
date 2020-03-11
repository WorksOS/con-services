CREATE TABLE  IF NOT EXISTS `md_asset_AssetConfigType` (
  `AssetConfigTypeID` bigint(20) NOT NULL AUTO_INCREMENT,
  `ConfigTypeName` varchar(50) NOT NULL,
  `ConfigTypeDescr` varchar(100) DEFAULT NULL,
  `InsertUTC` datetime(6) NOT NULL,
  `UpdateUTC` datetime(6) NOT NULL,
  PRIMARY KEY (`AssetConfigTypeID`)
) ENGINE=InnoDB AUTO_INCREMENT=0 DEFAULT CHARSET=utf8;



INSERT INTO md_asset_AssetConfigType(AssetConfigTypeID,ConfigTypeName,ConfigTypeDescr,InsertUTC,UpdateUTC) VALUES(0, 'IdletimeHours','IdletimeHours','2017-04-20 00:00:00','2017-04-20 00:00:00') ON DUPLICATE KEY UPDATE ConfigTypeName=VALUES(ConfigTypeName), ConfigTypeDescr=VALUES(ConfigTypeDescr), InsertUTC=VALUES(InsertUTC), UpdateUTC=VALUES(UpdateUTC);
INSERT INTO md_asset_AssetConfigType(AssetConfigTypeID,ConfigTypeName,ConfigTypeDescr,InsertUTC,UpdateUTC) VALUES(1, 'RuntimeHours','RuntimeHours','2017-04-19 00:00:00','2017-04-19 00:00:00') ON DUPLICATE KEY UPDATE ConfigTypeName=VALUES(ConfigTypeName), ConfigTypeDescr=VALUES(ConfigTypeDescr), InsertUTC=VALUES(InsertUTC), UpdateUTC=VALUES(UpdateUTC);
INSERT INTO md_asset_AssetConfigType(AssetConfigTypeID,ConfigTypeName,ConfigTypeDescr,InsertUTC,UpdateUTC) VALUES(2, 'OdometerinKmsPerWeek','OdometerinKmsPerWeek','2017-04-19 00:00:00','2017-04-19 00:00:00') ON DUPLICATE KEY UPDATE ConfigTypeName=VALUES(ConfigTypeName), ConfigTypeDescr=VALUES(ConfigTypeDescr), InsertUTC=VALUES(InsertUTC), UpdateUTC=VALUES(UpdateUTC);
INSERT INTO md_asset_AssetConfigType(AssetConfigTypeID,ConfigTypeName,ConfigTypeDescr,InsertUTC,UpdateUTC) VALUES(3, 'BucketVolumeinCuMeter','BucketVolumeinCuMeter','2017-04-20 00:00:00','2017-04-20 00:00:00') ON DUPLICATE KEY UPDATE ConfigTypeName=VALUES(ConfigTypeName), ConfigTypeDescr=VALUES(ConfigTypeDescr), InsertUTC=VALUES(InsertUTC), UpdateUTC=VALUES(UpdateUTC);
INSERT INTO md_asset_AssetConfigType(AssetConfigTypeID,ConfigTypeName,ConfigTypeDescr,InsertUTC,UpdateUTC) VALUES(4, 'PayloadinTonnes','PayloadinTonnes','2017-04-20 00:00:00','2017-04-20 00:00:00') ON DUPLICATE KEY UPDATE ConfigTypeName=VALUES(ConfigTypeName), ConfigTypeDescr=VALUES(ConfigTypeDescr), InsertUTC=VALUES(InsertUTC), UpdateUTC=VALUES(UpdateUTC);
INSERT INTO md_asset_AssetConfigType(AssetConfigTypeID,ConfigTypeName,ConfigTypeDescr,InsertUTC,UpdateUTC) VALUES(5, 'CycleCount','CycleCount','2017-04-20 00:00:00','2017-04-20 00:00:00') ON DUPLICATE KEY UPDATE ConfigTypeName=VALUES(ConfigTypeName), ConfigTypeDescr=VALUES(ConfigTypeDescr), InsertUTC=VALUES(InsertUTC), UpdateUTC=VALUES(UpdateUTC);
INSERT INTO md_asset_AssetConfigType(AssetConfigTypeID,ConfigTypeName,ConfigTypeDescr,InsertUTC,UpdateUTC) VALUES(6, 'VolumeinCuMeter','VolumeinCuMeter','2017-04-20 00:00:00','2017-04-20 00:00:00') ON DUPLICATE KEY UPDATE ConfigTypeName=VALUES(ConfigTypeName), ConfigTypeDescr=VALUES(ConfigTypeDescr), InsertUTC=VALUES(InsertUTC), UpdateUTC=VALUES(UpdateUTC);
INSERT INTO md_asset_AssetConfigType(AssetConfigTypeID,ConfigTypeName,ConfigTypeDescr,InsertUTC,UpdateUTC) VALUES(9, 'IdlingBurnRateinLiPerHour','IdlingBurnRateinLiPerHour','2017-04-27 00:00:00','2017-04-27 00:00:00') ON DUPLICATE KEY UPDATE ConfigTypeName=VALUES(ConfigTypeName), ConfigTypeDescr=VALUES(ConfigTypeDescr), InsertUTC=VALUES(InsertUTC), UpdateUTC=VALUES(UpdateUTC);
INSERT INTO md_asset_AssetConfigType(AssetConfigTypeID,ConfigTypeName,ConfigTypeDescr,InsertUTC,UpdateUTC) VALUES(10, 'WorkingBurnRateinLiPerHour','WorkingBurnRateinLiPerHour','2017-04-27 00:00:00','2017-04-27 00:00:00') ON DUPLICATE KEY UPDATE ConfigTypeName=VALUES(ConfigTypeName), ConfigTypeDescr=VALUES(ConfigTypeDescr), InsertUTC=VALUES(InsertUTC), UpdateUTC=VALUES(UpdateUTC);
INSERT INTO md_asset_AssetConfigType(AssetConfigTypeID,ConfigTypeName,ConfigTypeDescr,InsertUTC,UpdateUTC) VALUES(11, 'PayloadPerCycleInTonnes','Used to save Estimated Payload that an asset will carry per cycle','2017-09-06 00:00:00','2017-09-06 00:00:00') ON DUPLICATE KEY UPDATE ConfigTypeName=VALUES(ConfigTypeName), ConfigTypeDescr=VALUES(ConfigTypeDescr), InsertUTC=VALUES(InsertUTC), UpdateUTC=VALUES(UpdateUTC);




CREATE TABLE  IF NOT EXISTS  `md_asset_AssetConfig` (
  `AssetConfigID` bigint(20) NOT NULL AUTO_INCREMENT,
  `AssetConfigUID` binary(16) NOT NULL,
  `fk_AssetUID` binary(16) NOT NULL,
  `fk_AssetConfigTypeID` bigint(20) NOT NULL,
  `StartDate` datetime(6) NOT NULL,
  `EndDate` datetime(6) DEFAULT NULL,
  `ConfigValue` varchar(30) NOT NULL,
  `InsertUTC` datetime(6) NOT NULL,
  `UpdateUTC` datetime(6) NOT NULL,
  `StatusInd` bit(1) NOT NULL,
  PRIMARY KEY (`AssetConfigID`),
  UNIQUE KEY `AssetConfig_AssetConfigUID_UK` (`AssetConfigUID`),
  KEY `AssetConfig_AssetUID_IDX` (`fk_AssetUID`,`fk_AssetConfigTypeID`,`StartDate`,`EndDate`),
  KEY `AssetConfig_AssetConfigType_FK` (`fk_AssetConfigTypeID`),
  CONSTRAINT `AssetConfig_AssetConfigType_FK` FOREIGN KEY (`fk_AssetConfigTypeID`) REFERENCES `md_asset_AssetConfigType` (`AssetConfigTypeID`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB AUTO_INCREMENT=0 DEFAULT CHARSET=utf8;




CREATE TABLE IF NOT EXISTS  `md_asset_AssetWeeklyConfig` (
  `AssetWeeklyConfigID` bigint(20) NOT NULL AUTO_INCREMENT,
  `AssetWeeklyConfigUID` binary(16) NOT NULL,
  `fk_AssetUID` binary(16) NOT NULL,
  `fk_AssetConfigTypeID` bigint(20) NOT NULL,
  `StartDate` datetime(6) NOT NULL,
  `EndDate` datetime(6) DEFAULT NULL,
  `SundayConfigValue` varchar(30) NOT NULL,
  `MondayConfigValue` varchar(30) NOT NULL,
  `TuesdayConfigValue` varchar(30) NOT NULL,
  `WednesdayConfigValue` varchar(30) NOT NULL,
  `ThursdayConfigValue` varchar(30) NOT NULL,
  `FridayConfigValue` varchar(30) NOT NULL,
  `SaturdayConfigValue` varchar(30) NOT NULL,
  `InsertUTC` datetime(6) NOT NULL,
  `UpdateUTC` datetime(6) DEFAULT NULL,
  `StatusInd` bit(1) NOT NULL,
  PRIMARY KEY (`AssetWeeklyConfigID`),
  UNIQUE KEY `AssetWeeklyConfig_AssetWeeklyConfigUID_IDX` (`AssetWeeklyConfigUID`),
  KEY `AssetWeeklyConfig_AssetUID_IDX` (`fk_AssetUID`,`fk_AssetConfigTypeID`,`StartDate`,`EndDate`),
  KEY `AssetWeeklyConfig_AssetConfigType_FK` (`fk_AssetConfigTypeID`),
  CONSTRAINT `AssetWeeklyConfig_AssetConfigType_FK` FOREIGN KEY (`fk_AssetConfigTypeID`) REFERENCES `md_asset_AssetConfigType` (`AssetConfigTypeID`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB AUTO_INCREMENT=0 DEFAULT CHARSET=utf8;



CREATE TABLE IF NOT EXISTS `md_asset_WorkDefinitionType` (
  `WorkDefinitionTypeID` tinyint(4) NOT NULL AUTO_INCREMENT,
  `Description` varchar(30) NOT NULL,
  `InsertUTC` datetime(6) NOT NULL,
  `UpdateUTC` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`WorkDefinitionTypeID`)
) ENGINE=InnoDB AUTO_INCREMENT=0 DEFAULT CHARSET=utf8;



INSERT INTO md_asset_WorkDefinitionType(WorkDefinitionTypeID,Description,InsertUTC,UpdateUTC) VALUES(0, 'Unknown', '2017-06-21 07:13:12','2017-06-21 07:13:12') ON DUPLICATE KEY UPDATE Description=VALUES(Description), InsertUTC=VALUES(InsertUTC), UpdateUTC=VALUES(UpdateUTC);
INSERT INTO md_asset_WorkDefinitionType(WorkDefinitionTypeID,Description,InsertUTC,UpdateUTC) VALUES(1, 'Movement Events', '2017-06-21 07:13:12','2017-06-21 07:13:12') ON DUPLICATE KEY UPDATE Description=VALUES(Description), InsertUTC=VALUES(InsertUTC), UpdateUTC=VALUES(UpdateUTC);
INSERT INTO md_asset_WorkDefinitionType(WorkDefinitionTypeID,Description,InsertUTC,UpdateUTC) VALUES(2, 'Sensor Events', '2017-06-21 07:13:12','2017-06-21 07:13:12') ON DUPLICATE KEY UPDATE Description=VALUES(Description), InsertUTC=VALUES(InsertUTC), UpdateUTC=VALUES(UpdateUTC);
INSERT INTO md_asset_WorkDefinitionType(WorkDefinitionTypeID,Description,InsertUTC,UpdateUTC) VALUES(3, 'Movement and Sensor Events', '2017-06-21 07:13:12','2017-06-21 07:13:12') ON DUPLICATE KEY UPDATE Description=VALUES(Description), InsertUTC=VALUES(InsertUTC), UpdateUTC=VALUES(UpdateUTC);
INSERT INTO md_asset_WorkDefinitionType(WorkDefinitionTypeID,Description,InsertUTC,UpdateUTC) VALUES(4, 'Meter Delta', '2017-06-21 07:13:12','2017-06-21 07:13:12') ON DUPLICATE KEY UPDATE Description=VALUES(Description), InsertUTC=VALUES(InsertUTC), UpdateUTC=VALUES(UpdateUTC);



CREATE TABLE  IF NOT EXISTS `md_asset_AssetWorkDefinition` (
  `AssetWorkDefinitionID` bigint(20) NOT NULL AUTO_INCREMENT,
  `fk_AssetUID` binary(16) NOT NULL,
  `StartDate` datetime(6) NOT NULL,
  `EndDate` datetime(6) DEFAULT NULL,
  `fk_WorkDefinitionTypeID` tinyint(4) NOT NULL,
  `SwitchNumber` int(11) DEFAULT NULL,
  `SwitchWorkStartState` int(11) DEFAULT NULL,
  `InsertUTC` datetime(6) NOT NULL,
  `UpdateUTC` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`AssetWorkDefinitionID`),
  KEY `AssetWorkDefinition_WorkDefinitionType_FK` (`fk_WorkDefinitionTypeID`),
  UNIQUE KEY `AssetWorkDefinition_AssetUID_StartDate_IDX` (`fk_AssetUID`,`StartDate`), -- modified index to unique index  
  CONSTRAINT `AssetWorkDefinition_WorkDefinitionType_FK` FOREIGN KEY (`fk_WorkDefinitionTypeID`) REFERENCES `md_asset_WorkDefinitionType` (`WorkDefinitionTypeID`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB AUTO_INCREMENT=0 DEFAULT CHARSET=utf8;









CREATE TABLE  IF NOT EXISTS `msg_md_assetsmu_Latestsmu` (
  `ID` bigint(20) NOT NULL AUTO_INCREMENT,
  `fk_AssetUID` binary(16) NOT NULL,
  `RunTimeHours` decimal(16,4) DEFAULT NULL,
  `LastRuntimeHoursUTC` datetime(6) DEFAULT NULL,
  `OdometerKilometers` decimal(16,4) DEFAULT NULL,
  `LastOdometerKilometersUTC` datetime(6) DEFAULT NULL,
  `RowUpdatedUTC` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`ID`),
  UNIQUE KEY `msg_md_assetsmu_Latestsmu_AssetUID_UK` (`fk_AssetUID`),
  KEY `msg_md_assetsmu_Latestsmu__RuntimeHours_IDX` (`RunTimeHours`),
  KEY `msg_md_assetsmu_Latestsmu__OdometerKilometers_IDX` (`OdometerKilometers`)
) ENGINE=InnoDB AUTO_INCREMENT=0 DEFAULT CHARSET=utf8;
