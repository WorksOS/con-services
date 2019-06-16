SET @s = (SELECT IF(
    (SELECT COUNT(*)
       FROM INFORMATION_SCHEMA.COLUMNS
        WHERE table_name = 'Machine'
        AND table_schema = DATABASE()
        AND column_name = 'AssetUID'
    ) > 0,
    "SELECT 1",
    "ALTER TABLE `Machine` ADD COLUMN `AssetUID` varchar(36) AFTER `AssetID`"
));

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

