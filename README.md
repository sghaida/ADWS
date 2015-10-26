# ADWS
Active Directory WCF Web Services to handle Different AD Routines along with AD Authentication and SSO through encrypted sessionkey.
the Webservices has rest and SOAP Endpoints. all Input and output for Restful API is JSON Serialized

The AD Routines which have been implemented up to day are listed as below 

###Authentication
* AuthenticateUserUsingCredentials
* AuthenticateUserUsingSession
* ValidateSession

###Querying
* GetUserAttributes
* GetLocalDomainController
* GetNetbiosDomainName
* GetFqdnFromNetBiosName

###AD User Related
* AddADUser
* RemoveADUser
* AddADUserToGroup
* RemoveADUserFromGroup
* ResetADUserPassword
* UnlockADAccount
* RenameObject
* MoveObject

###AD Group Related
* AddGroup
* RemoveGroup
* RenameObject
* MoveObject
