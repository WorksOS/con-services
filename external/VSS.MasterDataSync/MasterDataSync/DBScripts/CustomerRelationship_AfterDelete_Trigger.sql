USE [VSS-Store]
GO

/****** Object:  Trigger [dbo].[tr_CustomerRelationship_AfterDelete]    Script Date: 20-10-2017 13:56:20 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER OFF
GO


CREATE TRIGGER [dbo].[tr_CustomerRelationship_AfterDelete] ON [dbo].[CustomerRelationship]
AFTER DELETE
AS
BEGIN
    SET NOCOUNT ON;
  
	DECLARE @ParentCustomerID BIGINT = NULL;
	DECLARE @AssociatedCustomerID BIGINT = NULL ;
	DECLARE @Operation NVARCHAR(64);
	DECLARE @LastCustomerRelationshipExportUTC  DATETIME;
	DECLARE @ClientCustomerID BIGINT;
	DECLARE @fk_CustomerRelationshipTypeID INT;
	DECLARE @CustomerTypeID  INT = 0 ;
	DECLARE @ParentCustomerTypeID INT =0;
	DECLARE @RemoveRelation INT  = 1 ;

	select @ParentCustomerID			  = i.fk_ParentCustomerID , 
		   @ClientCustomerID			  =	i.fk_ClientCustomerID ,
		   @fk_CustomerRelationshipTypeID = i.fk_CustomerRelationshipTypeID  from DELETED i;		

	SET @RemoveRelation = 1

	Select @ParentCustomerTypeID = fk_CustomerTypeID  From NH_OP..Customer WHERE ID = @ParentCustomerID			  
	Select @CustomerTypeID	     = fk_CustomerTypeID  From NH_OP..Customer WHERE ID = @ClientCustomerID			  
	
	IF @CustomerTypeID = 2 -- Account
	BEGIN
		SET @fk_CustomerRelationshipTypeID = 0 /* setting it back to Customer if the customertype is account  */ 
	END

	IF @fk_CustomerRelationshipTypeID = 1 -- TCS Dealer
	BEGIN
		
		
		IF @CustomerTypeID = 0 -- Dealer ( This is Delaer to Dealer Relation)
		BEGIN
			SET @ParentCustomerID = @ParentCustomerID
			SET @AssociatedCustomerID = @ClientCustomerID
			SET @Operation= 'Remove';
		END
		ELSE
		BEGIN
			SET @ParentCustomerID = @ParentCustomerID
			SELECT @AssociatedCustomerID = fk_ParentCustomerID FROM NH_OP..CustomerRelationShip cr INNER JOIN NH_OP..CustomerRelationShipType crt
			    ON cr.fk_CustomerRelationshipTypeID = crt.id
			 WHERE crt.name ='TCS Customer' AND fk_ClientCustomerID = @ClientCustomerID
			 SET @Operation= 'Remove';
		END
		
	END
	ELSE  -- TCS Customer
	BEGIN
		IF   @ParentCustomerTypeID = 0  AND  @CustomerTypeID = 2  --- ( Parent is Dealer and Client is Account) 
			BEGIN
				SET @ParentCustomerID = @ParentCustomerID
				SELECT @AssociatedCustomerID = fk_ParentCustomerID FROM NH_OP..CustomerRelationShip cr INNER JOIN NH_OP..CustomerRelationShipType crt
				ON cr.fk_CustomerRelationshipTypeID = crt.id
				WHERE crt.name ='TCS Customer' AND fk_ClientCustomerID = @ClientCustomerID
				SET @Operation= 'Remove';
			END
		ELSE IF   @ParentCustomerTypeID = 1  AND @CustomerTypeID = 2 --( Parent is Customer and Client is Account )
			BEGIN
				SET @AssociatedCustomerID = @ParentCustomerID
				SELECT @ParentCustomerID = fk_ParentCustomerID FROM NH_OP..CustomerRelationShip cr INNER JOIN NH_OP..CustomerRelationShipType crt
				ON cr.fk_CustomerRelationshipTypeID = crt.id
				WHERE crt.name ='TCS Dealer' AND fk_ClientCustomerID = @ClientCustomerID
				SET @Operation= 'Remove';
			END					
		
		
		 /* If Dealer or Customer has another Account then we should not remove the relation*/
		IF EXISTS (Select fk_ClientCUstomerID , Count(1) From CustomerRelationship WHERE fk_ParentCustomerID IN (@ParentCustomerID , @AssociatedCustomerID )
		GROUP BY fk_ClientCUstomerID 
		HAVING Count(1) >=2 )
		BEGIN
			SET @RemoveRelation = 0
		END

	END
    	
	
	SET @LastCustomerRelationshipExportUTC=GetUTCDate()	;
	IF (@ParentCustomerID IS NOT NULL AND @AssociatedCustomerID IS NOT NULL  ) AND @RemoveRelation = 1 
	BEGIN
		INSERT INTO CustomerRelationshipExport 
				(  ParentCustomerID ,  AssociatedCustomerID ,  Operation ,  LastCustomerRelationshipExportUTC  ) 
		VALUES ( @ParentCustomerID , @AssociatedCustomerID , @Operation , @LastCustomerRelationshipExportUTC  );

	END
	 
END



GO


