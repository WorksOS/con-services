SET @s = (SELECT IF(
    (SELECT COUNT(*)
       FROM INFORMATION_SCHEMA.COLUMNS
        WHERE table_name = 'Filter'
        AND table_schema = DATABASE()
        AND column_name = 'fk_FilterTypeID'
    ) > 0,
    "SELECT 1",
    "ALTER TABLE `Filter` 
            ADD COLUMN `fk_FilterTypeID` INT(10) NOT NULL DEFAULT 0 AFTER `Name`"
)); 

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;  

