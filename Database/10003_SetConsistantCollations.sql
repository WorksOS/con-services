/** notes:
  1) a USE statement is not sufficient for these ddl commands and the schema name must be included in each query
  2) running this schema change may require the reader be restarted before/during
  **/

/***
USE `VSS-Landfill`;

SELECT schema_name, default_character_set_name, default_collation_name 
	FROM information_schema.SCHEMATA 
	WHERE schema_name = "VSS-Landfill"
		AND ( default_character_set_name <> 'utf8mb4'
					OR default_collation_name <> 'utf8mb4_unicode_ci'
        );


SELECT table_schema, table_name, CCSA.character_set_name, table_collation 
FROM information_schema.`TABLES` T,
       information_schema.`COLLATION_CHARACTER_SET_APPLICABILITY` CCSA
WHERE CCSA.collation_name = T.table_collation
  AND table_schema = "VSS-Landfill"
  -- AND table_name = "AssetTargetHoursTemplate"
  AND ( character_set_name <> 'utf8mb4'
				OR table_collation <> 'utf8mb4_unicode_ci'
			)
ORDER BY table_name;

-- this command isn't supported in PREPARE statement so have to check it independantly 
ALTER SCHEMA `VSS-Landfill` DEFAULT CHARACTER SET utf8mb4 DEFAULT COLLATE utf8mb4_unicode_ci;

SET @s = (SELECT IF(
    (SELECT COUNT(*)
				FROM information_schema.`TABLES` T,
					information_schema.`COLLATION_CHARACTER_SET_APPLICABILITY` CCSA
				WHERE CCSA.collation_name = T.table_collation
					AND table_schema = "VSS-Landfill"
					AND table_name = "CCA"
					AND ( character_set_name <> 'utf8mb4'
								OR table_collation <> 'utf8mb4_unicode_ci'
							)
    ) > 0,    
    "ALTER TABLE `CCA` CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci",
    "SELECT 1"
));

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @s = (SELECT IF(
    (SELECT COUNT(*)
				FROM information_schema.`TABLES` T,
					information_schema.`COLLATION_CHARACTER_SET_APPLICABILITY` CCSA
				WHERE CCSA.collation_name = T.table_collation
					AND table_schema = "VSS-Landfill"
					AND table_name = "Customer"
					AND ( character_set_name <> 'utf8mb4'
								OR table_collation <> 'utf8mb4_unicode_ci'
							)
    ) > 0,    
    "ALTER TABLE `Customer` CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci",
    "SELECT 1"
));

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @s = (SELECT IF(
    (SELECT COUNT(*)
				FROM information_schema.`TABLES` T,
					information_schema.`COLLATION_CHARACTER_SET_APPLICABILITY` CCSA
				WHERE CCSA.collation_name = T.table_collation
					AND table_schema = "VSS-Landfill"
					AND table_name = "CustomerProject"
					AND ( character_set_name <> 'utf8mb4'
								OR table_collation <> 'utf8mb4_unicode_ci'
							)
    ) > 0,    
    "ALTER TABLE `CustomerProject` CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci",
    "SELECT 1"
));

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @s = (SELECT IF(
    (SELECT COUNT(*)
				FROM information_schema.`TABLES` T,
					information_schema.`COLLATION_CHARACTER_SET_APPLICABILITY` CCSA
				WHERE CCSA.collation_name = T.table_collation
					AND table_schema = "VSS-Landfill"
					AND table_name = "CustomerTypeEnum"
					AND ( character_set_name <> 'utf8mb4'
								OR table_collation <> 'utf8mb4_unicode_ci'
							)
    ) > 0,    
    "ALTER TABLE `CustomerTypeEnum` CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci",
    "SELECT 1"
));

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;


SET @s = (SELECT IF(
    (SELECT COUNT(*)
				FROM information_schema.`TABLES` T,
					information_schema.`COLLATION_CHARACTER_SET_APPLICABILITY` CCSA
				WHERE CCSA.collation_name = T.table_collation
					AND table_schema = "VSS-Landfill"
					AND table_name = "CustomerUser"
					AND ( character_set_name <> 'utf8mb4'
								OR table_collation <> 'utf8mb4_unicode_ci'
							)
    ) > 0,    
    "ALTER TABLE `CustomerUser` CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci",
    "SELECT 1"
));

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @s = (SELECT IF(
    (SELECT COUNT(*)
				FROM information_schema.`TABLES` T,
					information_schema.`COLLATION_CHARACTER_SET_APPLICABILITY` CCSA
				WHERE CCSA.collation_name = T.table_collation
					AND table_schema = "VSS-Landfill"
					AND table_name = "Entries"
					AND ( character_set_name <> 'utf8mb4'
								OR table_collation <> 'utf8mb4_unicode_ci'
							)
    ) > 0,    
    "ALTER TABLE `Entries` CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci",
    "SELECT 1"
));

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @s = (SELECT IF(
    (SELECT COUNT(*)
				FROM information_schema.`TABLES` T,
					information_schema.`COLLATION_CHARACTER_SET_APPLICABILITY` CCSA
				WHERE CCSA.collation_name = T.table_collation
					AND table_schema = "VSS-Landfill"
					AND table_name = "Geofence"
					AND ( character_set_name <> 'utf8mb4'
								OR table_collation <> 'utf8mb4_unicode_ci'
							)
    ) > 0,    
    "ALTER TABLE `Geofence` CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci",
    "SELECT 1"
));

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @s = (SELECT IF(
    (SELECT COUNT(*)
				FROM information_schema.`TABLES` T,
					information_schema.`COLLATION_CHARACTER_SET_APPLICABILITY` CCSA
				WHERE CCSA.collation_name = T.table_collation
					AND table_schema = "VSS-Landfill"
					AND table_name = "GeofenceTypeEnum"
					AND ( character_set_name <> 'utf8mb4'
								OR table_collation <> 'utf8mb4_unicode_ci'
							)
    ) > 0,    
    "ALTER TABLE `GeofenceTypeEnum` CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci",
    "SELECT 1"
));

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @s = (SELECT IF(
    (SELECT COUNT(*)
				FROM information_schema.`TABLES` T,
					information_schema.`COLLATION_CHARACTER_SET_APPLICABILITY` CCSA
				WHERE CCSA.collation_name = T.table_collation
					AND table_schema = "VSS-Landfill"
					AND table_name = "Machine"
					AND ( character_set_name <> 'utf8mb4'
								OR table_collation <> 'utf8mb4_unicode_ci'
							)
    ) > 0,    
    "ALTER TABLE `Machine` CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci",
    "SELECT 1"
));

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @s = (SELECT IF(
    (SELECT COUNT(*)
				FROM information_schema.`TABLES` T,
					information_schema.`COLLATION_CHARACTER_SET_APPLICABILITY` CCSA
				WHERE CCSA.collation_name = T.table_collation
					AND table_schema = "VSS-Landfill"
					AND table_name = "Project"
					AND ( character_set_name <> 'utf8mb4'
								OR table_collation <> 'utf8mb4_unicode_ci'
							)
    ) > 0,    
    "ALTER TABLE `Project` CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci",
    "SELECT 1"
));

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;


SET @s = (SELECT IF(
    (SELECT COUNT(*)
				FROM information_schema.`TABLES` T,
					information_schema.`COLLATION_CHARACTER_SET_APPLICABILITY` CCSA
				WHERE CCSA.collation_name = T.table_collation
					AND table_schema = "VSS-Landfill"
					AND table_name = "ProjectGeofence"
					AND ( character_set_name <> 'utf8mb4'
								OR table_collation <> 'utf8mb4_unicode_ci'
							)
    ) > 0,    
    "ALTER TABLE `ProjectGeofence` CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci",
    "SELECT 1"
));

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @s = (SELECT IF(
    (SELECT COUNT(*)
				FROM information_schema.`TABLES` T,
					information_schema.`COLLATION_CHARACTER_SET_APPLICABILITY` CCSA
				WHERE CCSA.collation_name = T.table_collation
					AND table_schema = "VSS-Landfill"
					AND table_name = "ProjectSubscription"
					AND ( character_set_name <> 'utf8mb4'
								OR table_collation <> 'utf8mb4_unicode_ci'
							)
    ) > 0,    
    "ALTER TABLE `ProjectSubscription` CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci",
    "SELECT 1"
));

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @s = (SELECT IF(
    (SELECT COUNT(*)
				FROM information_schema.`TABLES` T,
					information_schema.`COLLATION_CHARACTER_SET_APPLICABILITY` CCSA
				WHERE CCSA.collation_name = T.table_collation
					AND table_schema = "VSS-Landfill"
					AND table_name = "ProjectTypeEnum"
					AND ( character_set_name <> 'utf8mb4'
								OR table_collation <> 'utf8mb4_unicode_ci'
							)
    ) > 0,    
    "ALTER TABLE `ProjectTypeEnum` CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci",
    "SELECT 1"
));

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @s = (SELECT IF(
    (SELECT COUNT(*)
				FROM information_schema.`TABLES` T,
					information_schema.`COLLATION_CHARACTER_SET_APPLICABILITY` CCSA
				WHERE CCSA.collation_name = T.table_collation
					AND table_schema = "VSS-Landfill"
					AND table_name = "ServiceTypeEnum"
					AND ( character_set_name <> 'utf8mb4'
								OR table_collation <> 'utf8mb4_unicode_ci'
							)
    ) > 0,    
    "ALTER TABLE `ServiceTypeEnum` CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci",
    "SELECT 1"
));

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @s = (SELECT IF(
    (SELECT COUNT(*)
				FROM information_schema.`TABLES` T,
					information_schema.`COLLATION_CHARACTER_SET_APPLICABILITY` CCSA
				WHERE CCSA.collation_name = T.table_collation
					AND table_schema = "VSS-Landfill"
					AND table_name = "ServiceTypeFamilyEnum"
					AND ( character_set_name <> 'utf8mb4'
								OR table_collation <> 'utf8mb4_unicode_ci'
							)
    ) > 0,    
    "ALTER TABLE `ServiceTypeFamilyEnum` CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci",
    "SELECT 1"
));

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;


SET @s = (SELECT IF(
    (SELECT COUNT(*)
				FROM information_schema.`TABLES` T,
					information_schema.`COLLATION_CHARACTER_SET_APPLICABILITY` CCSA
				WHERE CCSA.collation_name = T.table_collation
					AND table_schema = "VSS-Landfill"
					AND table_name = "Subscription"
					AND ( character_set_name <> 'utf8mb4'
								OR table_collation <> 'utf8mb4_unicode_ci'
							)
    ) > 0,    
    "ALTER TABLE `Subscription` CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci",
    "SELECT 1"
));

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

***/