ALTER TABLE `Asset` ADD INDEX `IX_SerialNumber` (`SerialNumber` ASC);
ALTER TABLE `Subscription` ADD INDEX `IX_EndDate_ServiceType` (`EndDate` ASC, `fk_ServiceTypeID` ASC);

