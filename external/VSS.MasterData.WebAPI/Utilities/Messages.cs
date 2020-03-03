namespace VSS.MasterData.WebAPI.Utilities
{
	public static class Messages
	{
		#region Customer Controller

		public const string InvalidCustomerType = "Invalid Customer type";
		public const string CustomerAlreadyExists = "Customer already exists";
		public const string AccountAlreadyExists = "Account already exists";
		public const string CustomerUIDNotFound = "CustomerUID not Found";
		public const string KafkaPublishError = "Failed to publish message to Kafka";
		public const string UnableToSaveToDb = "Unable to save to db. Make sure request is not duplicated and all keys exist";
		public const string CustomerDoesntExist = "No such Customer exists";
		public const string NoDataToUpdate = "Update request should contain atleast one data to update";
		public const string InvalidRelationshipType = "Invalid RelationType";
		public const string DuplicateCARequested = "Duplicate CA association requested for Customer {0} Asset {1}";
		public const string AssociationAlreadyExists = "Association already exists";
		public const string CustomerAssetDoesntExist = "No such Customer Asset Association exists";
		public const string CustomerUserAlreadyExists = "Customer User Already exists";
		public const string PublishKafkaFailure = "Unable to publish message to Kafka";
		public const string CustomerAssetAssociationSuccess = "Customer Asset association created successfully";
		public const string CustomerUserDoesntExist = "Customer User doesn't exists,skipping UpdateUserCustomerRelationshipEvent";
		public const string DissociateCustomerUserSkipped = "DissociateCustomerUserEvent Skipped : {0}";
		public const string CustomerUserAssociationNotExists = "No such Customer User Association exists";
		public const string BulkDissociateCustomerUserSkipped = "Bulk DissociateCustomerUserEvent Skipped";
		public const string PublishDissociateCustomerUserFailed = "Publish to kafka failed for dissociate Customer {0} with a user";
		public const string CustomerUsersDoesntExist = "No such Customer User(s) Association exists";
		public const string BothParentChildCustomerEmpty = "Both parent and child customerUID cannot be empty";
		public const string InvalidCustomerUID = "Invalid CustomerUID";
		public const string InvalidDeleteType = "Invalid Delete Type";
		public const string UnableToDeleteCustomerAccountInDb = "Unable to delete the customerAccount relationship in db." +
			" Make sure request is not duplicated and all keys exist";
		public const string UnableToUpdateCustomerAccountInDb = "Unable to update the customerAccount relationship in db." +
			" Make sure request is not duplicated and all keys exist";
		public const string ExceptionOccured = "Exception has occured. Message : {0}, StackTrace : {1}";
		public const string JWTTokenEmpty = "JWT token is empty";
		public const string InvalidUserUid = "Invalid UserUID";
		public const string ChildNodeAlreadyExistsInHierarchy
			= "Can't make the child node that already exists in hierarchy as a root node again";
		public const string CustomerDealerRelationNotExists = "Customer Dealer relationship doesn't exist";
		#endregion
	}
}
