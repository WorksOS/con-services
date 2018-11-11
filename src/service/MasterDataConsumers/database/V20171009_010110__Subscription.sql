            
SET @s = (SELECT IF(
    (SELECT COUNT(*)
			FROM  INFORMATION_SCHEMA.STATISTICS
			WHERE TABLE_SCHEMA = DATABASE()
			AND   TABLE_NAME   = 'Subscription'
            AND INDEX_NAME = 'IX_Subscription_CustomerUID_Dates'
		) > 0,
    "SELECT 1",
    "ALTER TABLE `Subscription` ADD KEY IX_Subscription_CustomerUID_Dates (fk_CustomerUID, StartDate, EndDate)"
));  

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;