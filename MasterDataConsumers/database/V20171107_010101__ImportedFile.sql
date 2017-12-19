SET @s = (SELECT IF(
    (SELECT COUNT(*)
       FROM INFORMATION_SCHEMA.COLUMNS
        WHERE table_name = 'ImportedFile'
        AND table_schema = DATABASE()
        AND column_name = 'LegacyImportedFileID'
    ) > 0,
    "SELECT 1",
    "ALTER TABLE `ImportedFile` ADD COLUMN `LegacyImportedFileID` BIGINT(20) NULL AFTER ImportedFileID"
));  

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt; 