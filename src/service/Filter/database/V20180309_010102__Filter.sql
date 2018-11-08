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

-- When the new fk_FilterTypeID column is created for existing rows, it defaults to 0 (persistant).
-- We need to ensure that any existing rows set thus, which WERE actually transient,
--      are set to the correct type.

UPDATE Filter
  SET fk_FilterTypeID = 1 
  where fk_FilterTypeID = 0 
     and (Name IS NULL OR Name like "")
     and ID > 0;

