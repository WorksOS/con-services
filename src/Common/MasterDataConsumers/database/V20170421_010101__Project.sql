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
