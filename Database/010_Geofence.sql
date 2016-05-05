CREATE TABLE IF NOT EXISTS Geofence (
  GeofenceUID varchar(36) NOT NULL,
  Name varchar(128) NOT NULL,
  fk_GeofenceTypeID int(11) NOT NULL,
  GeometryWKT varchar(4000) NOT NULL,
  FillColor int(11) NOT NULL,
  IsTransparent bit(1) NOT NULL,
  IsDeleted bit(1) DEFAULT 0,
  CustomerUID varchar(36) NOT NULL,
  ProjectUID varchar(36) NULL,
  LastActionedUTC datetime(6) NULL,
  InsertUTC datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  UpdateUTC datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (GeofenceUID),
  UNIQUE KEY UIX_Geofence_GeofenceUID (GeofenceUID)
) ENGINE=InnoDB DEFAULT CHARACTER SET = utf8;
