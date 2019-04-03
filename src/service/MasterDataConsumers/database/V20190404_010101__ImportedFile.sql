SET @s = (SELECT IF(
    (SELECT COUNT(*)
       FROM INFORMATION_SCHEMA.COLUMNS
        WHERE table_name = 'ImportedFile'
        AND table_schema = DATABASE()
        AND column_name = 'Offset'
    ) > 0,
    "SELECT 1",
    "ALTER TABLE `ImportedFile` ADD COLUMN `Offset` FLOAT(7,3) DEFAULT 0 AFTER SurveyedUTC, ADD COLUMN `fk_ReferenceImportedFileUID` varchar(36) DEFAULT NULL AFTER Offset"
));  

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt; 



SET @s = (SELECT IF(
    (SELECT COUNT(*)
			FROM  INFORMATION_SCHEMA.TABLE_CONSTRAINTS
			WHERE TABLE_SCHEMA = DATABASE()
			AND   TABLE_NAME   = 'ImportedFile'
			AND   CONSTRAINT_NAME   = 'UIX_ImportedFile_ImportedFileUID_ReferenceImportedFileUID'
		) > 0,
    "SELECT 1",
    "ALTER TABLE `ImportedFile` ADD UNIQUE KEY UIX_ImportedFile_ImportedFileUID_ReferenceImportedFileUID (ImportedFileUID, fk_ReferenceImportedFileUID)"
));  

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;    