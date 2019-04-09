SET @s = (SELECT IF(
    (SELECT COUNT(*)
       FROM INFORMATION_SCHEMA.COLUMNS
        WHERE table_name = 'Project'
        AND table_schema = DATABASE()
        AND column_name = 'GeometryWKT'
    ) > 0,
    "ALTER TABLE `Project` DROP COLUMN `GeometryWKT`",
	"SELECT 1"
));  

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt; 

