 
SET @s = (SELECT IF(
    (SELECT COUNT(*)
			FROM  INFORMATION_SCHEMA.STATISTICS
			WHERE TABLE_SCHEMA = DATABASE()
			AND   TABLE_NAME   = 'Project'
            AND INDEX_NAME = 'IX_Project_Deleted_ProjectType'
		) > 0,
    "SELECT 1",
    "ALTER TABLE `Project` ADD KEY IX_Project_Deleted_ProjectType (IsDeleted, fk_ProjectTypeID)"
));  

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;