-- When the new fk_FilterTypeID column is created for existing rows, it defaults to 0 (persistant).
-- We need to ensure that any existing rows set thus, which WERE actually transient,
--      are set to the correct type.

UPDATE Filter
  SET fk_FilterTypeID = 1 
  where fk_FilterTypeID = 0 
     and (Name IS NULL OR Name like "")
     and ID > 0;
