SET @s = (SELECT IF(
    (SELECT COUNT(*)
       FROM INFORMATION_SCHEMA.COLUMNS
        WHERE table_name = 'ImportedFile'
        AND table_schema = DATABASE()
        AND column_name = 'IsActivated'
    ) > 0,
    "ALTER TABLE `ImportedFile` DROP COLUMN `IsActivated`;",
    "SELECT 1"
));  

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt; 
