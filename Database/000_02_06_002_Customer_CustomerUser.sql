

SELECT  '********************************************************************';
SELECT  'Start 	     	   	  		  	  000_02_06_002_Customer_CustomerUser';
SELECT  '********************************************************************';

--  Task 11503:Database creation and the objects for AppLauncher Microservice

CREATE TABLE IF NOT EXISTS `UserCustomer` (
  `UserCustomerID` bigint(20) NOT NULL AUTO_INCREMENT,
  `fk_UserUID` varchar(64) NOT NULL,
  `fk_CustomerUID` varchar(64) NOT NULL,
  `fk_CustomerID` BIGINT(20) NOT NULL,
  `LastUserUTC` datetime NOT NULL,
  PRIMARY KEY (`UserCustomerID`),
  UNIQUE KEY `UserCustomer_UK` (fk_UserUID , fk_CustomerUID),
  KEY UserCustomer_fk_CustomerID_IDX (fk_CustomerUID),
  CONSTRAINT `UserCustomer_Customer_FK` FOREIGN KEY (`fk_CustomerID`) REFERENCES `Customer` (`CustomerID`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

SELECT  '********************************************************************';
SELECT  'End 	     	   	  		  	  000_02_06_002_Customer_CustomerUser';
SELECT  '********************************************************************';
