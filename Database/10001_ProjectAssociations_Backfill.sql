/**** Landfill Customer Project, Project Geofence and Project Subscription Association Backfill ***/
 
INSERT IGNORE INTO `CustomerProject` 
(`fk_CustomerUID`, `fk_ProjectUID`)
SELECT `CustomerUID`, `ProjectUID` FROM `Project`;

INSERT IGNORE INTO `ProjectGeofence` 
(`fk_ProjectUID`, `fk_GeofenceUID`)
SELECT `ProjectUID`, `GeofenceUID` FROM `Geofence`;

INSERT IGNORE INTO `ProjectSubscription` 
(`fk_ProjectUID`, `fk_SubscriptionUID`)
SELECT `ProjectUID`, `SubscriptionUID` FROM `Project`;