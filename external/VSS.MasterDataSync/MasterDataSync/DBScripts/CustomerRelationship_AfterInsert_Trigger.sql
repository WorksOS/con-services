USE [VSS-Store]
GO

/****** Object:  Trigger [dbo].[tr_CustomerRelationship_AfterInsert]    Script Date: 20-10-2017 13:56:41 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER OFF
GO

CREATE TRIGGER [dbo].[tr_CustomerRelationship_AfterInsert] ON [dbo].[CustomerRelationship]
AFTER INSERT 
AS
BEGIN
	DECLARE @ParentCustomerID BIGINT = NULL;
	DECLARE @AssociatedCustomerID BIGINT = NULL ;
	DECLARE @Operation NVARCHAR(64);
	DECLARE @LastCustomerRelationshipExportUTC  DATETIME;
	DECLARE @ClientCustomerID BIGINT;
	DECLARE @fk_CustomerRelationshipTypeID INT;
	DECLARE @CustomerTypeID  INT = 0 ;

	select @ParentCustomerID			  = i.fk_ParentCustomerID , 
		   @ClientCustomerID			  =	i.fk_ClientCustomerID ,
		   @fk_CustomerRelationshipTypeID = i.fk_CustomerRelationshipTypeID  from inserted i;		

	IF @fk_CustomerRelationshipTypeID = 1 -- TCS Dealer
	BEGIN
		Select @CustomerTypeID = fk_CustomerTypeID  From NH_OP..Customer WHERE ID = @ClientCustomerID			  
		IF @CustomerTypeID = 0 -- Dealer ( This is Delaer to Dealer Relation)
		BEGIN
			SET @ParentCustomerID = @ParentCustomerID
			SET @AssociatedCustomerID = @ClientCustomerID
		END
		ELSE
		BEGIN
			SET @ParentCustomerID = @ParentCustomerID
			SELECT @AssociatedCustomerID = fk_ParentCustomerID FROM NH_OP..CustomerRelationShip cr INNER JOIN NH_OP..CustomerRelationShipType crt
			    ON cr.fk_CustomerRelationshipTypeID = crt.id
			 WHERE crt.name ='TCS Customer' AND fk_ClientCustomerID = @ClientCustomerID
		END
	END
	ELSE  -- TCS Customer
	BEGIN
		SET @AssociatedCustomerID = @ParentCustomerID
		SELECT @ParentCustomerID = fk_ParentCustomerID FROM NH_OP..CustomerRelationShip cr INNER JOIN NH_OP..CustomerRelationShipType crt
		ON cr.fk_CustomerRelationshipTypeID = crt.id
		WHERE crt.name ='TCS Dealer' AND fk_ClientCustomerID = @ClientCustomerID
	END
    
	SET @Operation= 'Add';
	SET @LastCustomerRelationshipExportUTC=GetUTCDate()	;
	IF @ParentCustomerID IS NOT NULL AND @AssociatedCustomerID IS NOT NULL 
	BEGIN
		INSERT INTO CustomerRelationshipExport 
			    (  ParentCustomerID ,  AssociatedCustomerID ,   Operation ,   LastCustomerRelationshipExportUTC  ) 
		 VALUES ( @ParentCustomerID , @AssociatedCustomerID  , @Operation  , @LastCustomerRelationshipExportUTC  );

	END
		 				
END


GO


