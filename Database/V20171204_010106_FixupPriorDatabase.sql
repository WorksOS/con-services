/*
 in Dec 2017 the VSS kafka Que version upgrade is being upgraded.
 The existing Landfill custom kafka consumers don't support it.
 So we changed these custom masterData (customer/project/Goefence/Subscription)
     consuemrs to using the Merino sandbox Consumers.alter
     
  The old database tables needed to be upgraded a little to support the
     new consumers. This script achieves that, and can be used to upgrade an existing 
     database installation as on production.alter
  If you are creating a brand new database then use all scripts
      from MerinoSandbox\MasterDataConsumers\database\. 
      BTW You could install all tables but not necessary.
      
  Customer; CustomerType; CustomerUser
  Project; ProjectType; CustomerProject
  Subscription; ServiceTypeFamiily; ServiceType; ProjectSubscription; 
        AssetSubscription (required by consumers, not landfill)
  Geofence; GeofenceTypeEnum; ProjectGeofence
*/
 
/*


-- ********  Customer *************

SET @s = (SELECT IF(
    (SELECT COUNT(*)
       FROM INFORMATION_SCHEMA.COLUMNS
        WHERE table_name = 'Customer'
        AND table_schema = DATABASE()
        AND column_name = 'CustomerName'
    ) > 0,
    "ALTER TABLE `Customer` CHANGE COLUMN `CustomerName` `Name` VARCHAR(200) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_unicode_ci' NOT NULL",
    "SELECT 1"
));

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @s = (SELECT IF(
    (SELECT COUNT(*)
       FROM INFORMATION_SCHEMA.COLUMNS
        WHERE table_name = 'Customer'
        AND table_schema = DATABASE()
        AND column_name = 'IsDeleted'
    ) > 0,
    "SELECT 1",
    "ALTER TABLE `Customer` ADD COLUMN `IsDeleted` TINYINT(4) DEFAULT 0 AFTER fk_CustomerTypeID"
));  

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt; 



-- ********   CustomerUser *************

SET @s = (SELECT IF(
    (SELECT COUNT(*)
       FROM INFORMATION_SCHEMA.COLUMNS
        WHERE table_name = 'CustomerUser'
        AND table_schema = DATABASE()
        AND column_name = 'fk_UserUID'
    ) > 0,
    "ALTER TABLE `CustomerUser` CHANGE COLUMN `fk_UserUID` `UserUID` VARCHAR(64) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_unicode_ci' NOT NULL",
    "SELECT 1"
));

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;


-- ********   Project *************

SET @s = (SELECT IF(
    (SELECT COUNT(*)
       FROM INFORMATION_SCHEMA.COLUMNS
        WHERE table_name = 'Project'
        AND table_schema = DATABASE()
        AND column_name = 'ProjectID'
    ) > 0,
    "ALTER TABLE `Project` CHANGE COLUMN `ProjectID` `LegacyProjectID` INT(10) UNSIGNED NOT NULL",
    "SELECT 1"
));

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;


SET @s = (SELECT IF(
    (SELECT COUNT(*)
       FROM INFORMATION_SCHEMA.COLUMNS
        WHERE table_name = 'Project'
        AND table_schema = DATABASE()
        AND column_name = 'Description'
    ) > 0,
    "SELECT 1",
    "ALTER TABLE `Project` ADD COLUMN `Description` nvarchar(2000) DEFAULT NULL AFTER `Name`"
));  

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;  


SET @s = (SELECT IF(
    (SELECT COUNT(*)
       FROM INFORMATION_SCHEMA.COLUMNS
        WHERE table_name = 'Project'
        AND table_schema = DATABASE()
        AND column_name = 'GeometryWKT'
    ) > 0,
    "SELECT 1",
    "ALTER TABLE `Project` ADD COLUMN `GeometryWKT` VARCHAR(4000) NULL DEFAULT NULL AFTER `EndDate`"
));  

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;  

SET @s = (SELECT IF(
    (SELECT COUNT(*)
       FROM INFORMATION_SCHEMA.COLUMNS
        WHERE table_name = 'Project'
        AND table_schema = DATABASE()
        AND column_name = 'PolygonST'
    ) > 0,
    "SELECT 1",
    "ALTER TABLE `Project` ADD COLUMN `PolygonST` POLYGON NULL DEFAULT NULL AFTER `GeometryWKT`"
));  

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;  


SET @s = (SELECT IF(
    (SELECT COUNT(*)
       FROM INFORMATION_SCHEMA.COLUMNS
        WHERE table_name = 'Project'
        AND table_schema = DATABASE()
        AND column_name = 'CoordinateSystemFileName'
    ) > 0,
    "SELECT 1",
    "ALTER TABLE `Project` ADD COLUMN `CoordinateSystemFileName` VARCHAR(256) NULL DEFAULT NULL AFTER `PolygonST`"
));  

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;   

SET @s = (SELECT IF(
    (SELECT COUNT(*)
       FROM INFORMATION_SCHEMA.COLUMNS
        WHERE table_name = 'Project'
        AND table_schema = DATABASE()
        AND column_name = 'CoordinateSystemLastActionedUTC'
    ) > 0,
    "SELECT 1",
    "ALTER TABLE `Project` ADD COLUMN `CoordinateSystemLastActionedUTC` DATETIME NULL DEFAULT NULL AFTER `LastActionedUTC`"
)); 

select  @s; 

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;   

SET @s = (SELECT IF(
    (SELECT COUNT(*)
       FROM INFORMATION_SCHEMA.COLUMNS
        WHERE table_name = 'Project'
        AND table_schema = DATABASE()
        AND column_name = 'ID'
    ) > 0,    
    "ALTER TABLE `Project` DROP COLUMN `ID`",
    "SELECT 1"
));  

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;  
 
-- dropping ID above should have dropped the old PK on it
SET @s = (SELECT IF(
    (SELECT COUNT(*) 
			FROM information_schema.table_constraints t
			LEFT JOIN information_schema.key_column_usage k
			USING(constraint_name,table_schema,table_name)
			WHERE t.constraint_type='PRIMARY KEY'
					AND t.table_schema=DATABASE()
					AND t.table_name='Project'
		) > 0,
    "SELECT 1",
    "ALTER TABLE `Project` ADD PRIMARY KEY (`LegacyProjectID`)"
));  

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;  
          

SET @s = (SELECT IF(
    (SELECT COUNT(*)
       FROM INFORMATION_SCHEMA.COLUMNS
        WHERE table_name = 'Project'
        AND table_schema = DATABASE()
        AND column_name = 'LegacyProjectID'
        AND EXTRA = 'auto_increment'
) > 0,
    "SELECT 1",
    "ALTER TABLE `Project` CHANGE COLUMN `LegacyProjectID` `LegacyProjectID` BIGINT(20) UNSIGNED NOT NULL AUTO_INCREMENT"
));  

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;  

  
SET @s = (SELECT IF(
    (SELECT `AUTO_INCREMENT`
			FROM  INFORMATION_SCHEMA.TABLES
			WHERE TABLE_SCHEMA = DATABASE()
			AND   TABLE_NAME   = 'Project'
		) >= 2000000,
    "SELECT 1",
    "ALTER TABLE `Project` AUTO_INCREMENT = 2000000"
));  

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;  
  

SET @s = (SELECT IF(
    (SELECT COUNT(*)
			FROM  INFORMATION_SCHEMA.TABLE_CONSTRAINTS
			WHERE TABLE_SCHEMA = DATABASE()
			AND   TABLE_NAME   = 'Project'
      AND   CONSTRAINT_NAME   = 'UIX_Project_LegacyProjectID'
		) > 0,
    "SELECT 1",
    "ALTER TABLE `Project` ADD UNIQUE KEY UIX_Project_LegacyProjectID (LegacyProjectID)"
));  

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;    


SET SQL_SAFE_UPDATES=0;
SET @s = (SELECT IF(
    (SELECT COUNT(*)
		FROM Project p  
			JOIN ProjectGeofence pg on pg.fk_ProjectUID = p.ProjectUID
			JOIN Geofence g on g.GeofenceUID = pg.fk_GeofenceUID
		WHERE g.fk_GeofenceTypeID = 1 AND p.GeometryWKT IS NULL
    ) > 0,    
    "UPDATE Project
		JOIN ProjectGeofence pg on pg.fk_ProjectUID = Project.ProjectUID
        JOIN Geofence g on g.GeofenceUID = pg.fk_GeofenceUID    
    SET Project.GeometryWKT = g.GeometryWKT 
    WHERE g.fk_GeofenceTypeID = 1 
		AND Project.GeometryWKT IS NULL;",
    "SELECT 1"
));  

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt; 
SET SQL_SAFE_UPDATES=1; 


-- ********   Geofence *************


SET @s = (SELECT IF(
    (SELECT COUNT(*)
       FROM INFORMATION_SCHEMA.COLUMNS
        WHERE table_name = 'Geofence'
        AND table_schema = DATABASE()
        AND column_name = 'CustomerUID'
    ) > 0,
    "ALTER TABLE `Geofence` CHANGE COLUMN `CustomerUID` `fk_CustomerUID` VARCHAR(36) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_unicode_ci' NOT NULL",
    "SELECT 1"
));

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;


SET @s = (SELECT IF(
    (SELECT COUNT(*)
       FROM INFORMATION_SCHEMA.COLUMNS
        WHERE table_name = 'Geofence'
        AND table_schema = DATABASE()
        AND column_name = 'Description'
    ) > 0,  
    "SELECT 1",
    "ALTER TABLE `Geofence` ADD COLUMN `Description` nvarchar(2000) NULL DEFAULT NULL  AFTER `IsDeleted`"
));  

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;  


SET @s = (SELECT IF(
    (SELECT COUNT(*)
       FROM INFORMATION_SCHEMA.COLUMNS
        WHERE table_name = 'Geofence'
        AND table_schema = DATABASE()
        AND column_name = 'AreaSqMeters'
    ) > 0,
    "SELECT 1",
    "ALTER TABLE `Geofence` ADD COLUMN `AreaSqMeters` DECIMAL DEFAULT 0 AFTER `Description`"
));  

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt; 

SET @s = (SELECT IF(
    (SELECT COUNT(*)
       FROM INFORMATION_SCHEMA.COLUMNS
        WHERE table_name = 'Geofence'
        AND table_schema = DATABASE()
        AND column_name = 'UserUID'
    ) > 0,  
    "SELECT 1",
    "ALTER TABLE `Geofence` ADD COLUMN `UserUID` varchar(100) NOT NULL  AFTER `fk_CustomerUID`"
));  

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;  


-- ********  Subscription *************

SET @s = (SELECT IF(
    (SELECT COUNT(*)
       FROM INFORMATION_SCHEMA.COLUMNS
        WHERE table_name = 'Subscription'
        AND table_schema = DATABASE()
        AND column_name = 'CustomerUID'
    ) > 0,
    "ALTER TABLE `Subscription` CHANGE COLUMN `CustomerUID` `fk_CustomerUID` VARCHAR(36) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_unicode_ci' NOT NULL",
    "SELECT 1"
));

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;


-- ********  ProjectSubscription *************

SET @s = (SELECT IF(
    (SELECT COUNT(*)
       FROM INFORMATION_SCHEMA.COLUMNS
        WHERE table_name = 'ProjectSubscription'
        AND table_schema = DATABASE()
        AND column_name = 'EffectiveDate'
    ) > 0,  
    "SELECT 1",
    "ALTER TABLE `ProjectSubscription` ADD COLUMN `EffectiveDate` date DEFAULT NULL  AFTER `fk_SubscriptionUID`"
));  

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt; 

*/

 