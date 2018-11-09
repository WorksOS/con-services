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