

SELECT  '********************************************************************';
SELECT  'Start 	     	   	  		  	      000_02_06_002_Customer_Customer';
SELECT  '********************************************************************';

--  Task 11503:Database creation and the objects for AppLauncher Microservice

CREATE TABLE IF NOT EXISTS `Customer` (
  `CustomerID` bigint(20) NOT NULL AUTO_INCREMENT,
  `CustomerUID` varchar(64) NOT NULL,
  `CustomerName` varchar(200) NOT NULL,
  `fk_CustomerTypeID` bigint(20) NOT NULL,
  `LastCustomerUTC` datetime NOT NULL,
  PRIMARY KEY (`CustomerID`),
  UNIQUE KEY `CustomerUID_UK` (`CustomerUID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

SELECT  '********************************************************************';
SELECT  'End 	     	   	  		  	      000_02_06_002_Customer_Customer';
SELECT  '********************************************************************';
