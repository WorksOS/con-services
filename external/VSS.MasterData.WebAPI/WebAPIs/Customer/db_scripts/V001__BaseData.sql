-------------- md_customer_Customer---------------------------
CREATE TABLE IF NOT EXISTS `md_customer_Customer` (
  `CustomerID` bigint(20) NOT NULL AUTO_INCREMENT,
  `CustomerUID` binary(16) NOT NULL,
  `CustomerName` varchar(200) NOT NULL,
  `fk_CustomerTypeID` bigint(20) NOT NULL,
  `LastCustomerUTC` datetime(6) NOT NULL,
  `PrimaryContactEmail` varchar(300) DEFAULT NULL,
  `FirstName` varchar(100) DEFAULT NULL,
  `LastName` varchar(100) DEFAULT NULL,
  `NetworkDealerCode` varchar(45) DEFAULT NULL,
  `IsActive` tinyint(1) DEFAULT NULL,
  PRIMARY KEY (`CustomerID`),
  UNIQUE KEY `md_customer_Customer_CustomerUID_UK` (`CustomerUID`)
) ENGINE=InnoDB AUTO_INCREMENT=0 DEFAULT CHARSET=utf8;

ALTER TABLE `md_customer_Customer` 
ADD COLUMN `BSSID` VARCHAR(50) NOT NULL AFTER `NetworkDealerCode`,
ADD COLUMN `DealerNetwork` VARCHAR(50) NOT NULL AFTER `BSSID`,
ADD COLUMN `NetworkCustomerCode` VARCHAR(50) DEFAULT NULL AFTER `DealerNetwork`,
ADD COLUMN `DealerAccountCode` VARCHAR(50) DEFAULT NULL AFTER `NetworkCustomerCode`;

-------------- md_customer_CustomerAccount ---------------------------
CREATE TABLE IF NOT EXISTS `md_customer_CustomerAccount` (
  `CustomerAccountID` bigint(20) NOT NULL AUTO_INCREMENT,
  `CustomerAccountUID` binary(16) NOT NULL,
  `BSSID` varchar(50) NOT NULL,
  `AccountName` varchar(200) NOT NULL,
  `NetworkCustomerCode` varchar(50) DEFAULT NULL,
  `DealerAccountCode` varchar(50) DEFAULT NULL,
  `fk_ParentCustomerUID` binary(16) DEFAULT NULL,
  `fk_ChildCustomerUID` binary(16) DEFAULT NULL,
  `RowUpdatedUTC` datetime NOT NULL,
  PRIMARY KEY (`CustomerAccountID`),
  UNIQUE KEY `md_customer_CustomerAccount_CustomerAccountUID_UK` (`CustomerAccountUID`),
  KEY `CustomerAccount_ParentCustomerUID_ChildCustomerUID_IDX` (`fk_ParentCustomerUID`,`fk_ChildCustomerUID`),
  KEY `CustomerAccount_ChildCustomerUID_IDX` (`fk_ChildCustomerUID`)
) ENGINE=InnoDB AUTO_INCREMENT=0 DEFAULT CHARSET=utf8;

----------------- md_customer_CustomerUser -------------------------------
CREATE TABLE IF NOT EXISTS `md_customer_CustomerUser` (
  `UserCustomerID` bigint(20) NOT NULL AUTO_INCREMENT,
  `fk_UserUID` binary(16) NOT NULL,
  `fk_CustomerUID` binary(16) NOT NULL,
  `fk_CustomerID` bigint(20) NOT NULL,
  `LastUserUTC` datetime(6) NOT NULL,
  PRIMARY KEY (`UserCustomerID`),
  UNIQUE KEY `md_customer_CustomerUser_UK` (`fk_UserUID`,`fk_CustomerUID`),
  KEY `md_customer_CustomerUser_fk_CustomerID_IDX` (`fk_CustomerUID`),
  KEY `md_customer_CustomerUser_Customer_FK` (`fk_CustomerID`),
  CONSTRAINT `md_customer_CustomerUser_Customer_FK` FOREIGN KEY (`fk_CustomerID`) REFERENCES `md_customer_Customer` (`CustomerID`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB AUTO_INCREMENT=0 DEFAULT CHARSET=utf8;

----------------- md_customer_CustomerAssetRelationType -------------------------------
CREATE TABLE IF NOT EXISTS `md_customer_CustomerAssetRelationType` (
  `AssetRelationTypeID` int(11) NOT NULL,
  `RelationTypeName` varchar(50) NOT NULL,
  `InsertUTC` datetime(6) NOT NULL,
  `UpdateUTC` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`AssetRelationTypeID`),
  UNIQUE KEY `md_customer_CustomerAssetRelationType_Name_UK` (`RelationTypeName`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

----------------- md_customer_CustomerAsset -------------------------------
CREATE TABLE IF NOT EXISTS `md_customer_CustomerAsset` (
  `AssetCustomerID` bigint(20) NOT NULL AUTO_INCREMENT,
  `Fk_CustomerUID` binary(16) NOT NULL,
  `Fk_AssetUID` binary(16) NOT NULL,
  `fk_AssetRelationTypeID` int(11) NOT NULL,
  `LastCustomerUTC` datetime(6) NOT NULL,
  PRIMARY KEY (`AssetCustomerID`),
  UNIQUE KEY `md_customer_CustomerAsset_CustomerUID_AssetUID_UK` (`Fk_CustomerUID`,`Fk_AssetUID`),
  KEY `md_customer_CustomerAsset_AssetRelationType_FK` (`fk_AssetRelationTypeID`),
  CONSTRAINT `md_customer_CustomerAsset_AssetRelationType_FK` FOREIGN KEY (`fk_AssetRelationTypeID`) REFERENCES `md_customer_CustomerAssetRelationType` (`AssetRelationTypeID`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `md_customer_CustomerAsset_Customer_FK` FOREIGN KEY (`Fk_CustomerUID`) REFERENCES `md_customer_Customer` (`CustomerUID`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB AUTO_INCREMENT=0 DEFAULT CHARSET=utf8;

----------------- md_customer_CustomerRelationshipNode -------------------------------
CREATE TABLE  IF NOT EXISTS `md_customer_CustomerRelationshipNode` (
  `CustomerRelationshipNodeID` bigint(20) NOT NULL AUTO_INCREMENT,
  `fk_RootCustomerUID` binary(16) NOT NULL,
  `Fk_CustomerUID` binary(16) NOT NULL,
  `LeftNodePosition` int(11) NOT NULL,
  `RightNodePosition` int(11) NOT NULL,  
  `fk_ParentCustomerUID` binary(16) NOT NULL,
  `LastCustomerRelationshipNodeUTC` datetime(6) NOT NULL,
  PRIMARY KEY (`CustomerRelationshipNodeID`),
  KEY `IX_RCID_LNP_RNP` (`fk_RootCustomerUID`,`LeftNodePosition`,`RightNodePosition`),
  KEY `CRN_LftNdPos_RgtNdPos_IDX` (`LeftNodePosition`,`RightNodePosition`),
  KEY `CRN_CustomerRelationshipNode_fk_CustomerUID_IDX` (`Fk_CustomerUID`)
) ENGINE=InnoDB AUTO_INCREMENT=0 DEFAULT CHARSET=utf8;