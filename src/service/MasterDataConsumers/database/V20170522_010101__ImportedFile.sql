-- drop old combo pk as it's needed on the new importedFileID (bigint(20) UN
SET @s = (SELECT IF(
    (SELECT COUNT(*) 			
			FROM information_schema.table_constraints t
			LEFT JOIN information_schema.key_column_usage k
			USING(constraint_name,table_schema,table_name)
			WHERE t.constraint_type='PRIMARY KEY'
					AND t.table_schema=DATABASE()
					AND t.table_name='ImportedFile'
                    AND COLUMN_NAME = 'ImportedFileUID'
		) > 0,
    "ALTER TABLE `ImportedFile` DROP PRIMARY KEY",
    "SELECT 1"
));  

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;  


SET @s = (SELECT IF(
    (SELECT COUNT(*)
       FROM INFORMATION_SCHEMA.COLUMNS
        WHERE table_name = 'ImportedFile'
        AND table_schema = DATABASE()
        AND column_name = 'ImportedFileID'
    ) > 0,
    "SELECT 1",
    "ALTER TABLE `ImportedFile` ADD COLUMN `ImportedFileID` BIGINT(20) UNSIGNED NOT NULL AUTO_INCREMENT AFTER ImportedFileUID,
    ADD PRIMARY KEY (`ImportedFileID`)"
));  

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt; 

SET @s = (SELECT IF(
    (SELECT `AUTO_INCREMENT`
			FROM  INFORMATION_SCHEMA.TABLES
			WHERE TABLE_SCHEMA = DATABASE()
			AND   TABLE_NAME   = 'ImportedFile'
		) >= 2000000,
    "SELECT 1",
    "ALTER TABLE `ImportedFile` AUTO_INCREMENT = 2000000"
));  

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;  
  

SET @s = (SELECT IF(
    (SELECT COUNT(*)
			FROM  INFORMATION_SCHEMA.TABLE_CONSTRAINTS
			WHERE TABLE_SCHEMA = DATABASE()
			AND   TABLE_NAME   = 'ImportedFile'
			AND   CONSTRAINT_NAME   = 'UIX_ImportedFile_ImportedFileID'
		) > 0,
    "SELECT 1",
    "ALTER TABLE `ImportedFile` ADD UNIQUE KEY UIX_ImportedFile_ImportedFileID (ImportedFileID)"
));  

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;    


SET @s = (SELECT IF(
    (SELECT COUNT(*)
			FROM  INFORMATION_SCHEMA.TABLE_CONSTRAINTS
			WHERE TABLE_SCHEMA = DATABASE()
			AND   TABLE_NAME   = 'ImportedFile'
			AND   CONSTRAINT_NAME   = 'UIX_ImportedFile_ProjectUID_Name_SurveyedUTC'
		) > 0,
    "SELECT 1",
    "ALTER TABLE `ImportedFile` DROP UNIQUE KEY UIX_ImportedFile_ProjectUID_Name_SurveyedUTC"
));  

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;  