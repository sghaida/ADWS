using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using ADWS.Models;

namespace ADWS
{
    [ServiceContract]
    public interface IAdws
    {

        [OperationContract]
        [WebInvoke( 
            UriTemplate = "auth/user" ,  
            RequestFormat= WebMessageFormat.Json,  
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            Method = "POST" )]
        Session AuthenticateUserUsingCredentials( [MessageParameter( Name = "authdata" )] AuthDataRequest authData );

        [OperationContract]
        [WebInvoke(
            UriTemplate = "auth/session" , 
            ResponseFormat = WebMessageFormat.Json ,
            RequestFormat = WebMessageFormat.Json ,
            BodyStyle = WebMessageBodyStyle.Bare,
            Method="POST")]
        Session AuthenticateUserUsingSession( [MessageParameter( Name = "sessionkey" )] string sessionKey );
        
        [OperationContract]
        [WebInvoke(
            UriTemplate = "session/validate" , 
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json ,
            BodyStyle = WebMessageBodyStyle.Bare ,
            Method="POST" )]
        Session ValidateSession( [MessageParameter( Name = "sessionkey" )]  string sessionKey );

        [OperationContract]
        [WebInvoke( 
            UriTemplate = "user/attributes" , 
            RequestFormat = WebMessageFormat.Json , 
            ResponseFormat = WebMessageFormat.Json ,
            BodyStyle = WebMessageBodyStyle.Bare, 
            Method = "POST" )]
        UserInfoResponse GetUserAttributes( [MessageParameter( Name = "request" )] RequestBody request  );

        [OperationContract]
        [WebInvoke(
           UriTemplate = "ad/getdcsinfo" ,
           ResponseFormat = WebMessageFormat.Json ,
           RequestFormat = WebMessageFormat.Json ,
           BodyStyle = WebMessageBodyStyle.Bare ,
           Method = "POST" )]
        List<DomainResponse> GetLocalDomainController( string sessionKey );

        [OperationContract]
        [WebInvoke(
            UriTemplate = "ad/getnbtfromhost" ,
            RequestFormat = WebMessageFormat.Json ,
            ResponseFormat = WebMessageFormat.Json ,
            BodyStyle = WebMessageBodyStyle.Bare ,
            Method = "POST" )]
        string GetNetbiosDomainName( [MessageParameter( Name = "request" )] RequestBody request );

        [OperationContract]
        [WebInvoke(
            UriTemplate = "ad/gethostfromnbt" ,
            RequestFormat = WebMessageFormat.Json ,
            ResponseFormat = WebMessageFormat.Json ,
            BodyStyle = WebMessageBodyStyle.Bare ,
            Method = "POST" )]
        string GetFqdnFromNetBiosName( [MessageParameter( Name = "request" )] RequestBody request );

        [OperationContract]
        [WebInvoke(
            UriTemplate = "ad/account/add" ,
            RequestFormat = WebMessageFormat.Json ,
            ResponseFormat = WebMessageFormat.Json ,
            BodyStyle = WebMessageBodyStyle.Bare ,
            Method = "POST" )]
        ResponseMessage AddADUser( [MessageParameter( Name = "userinfo" )] RequestUserCreate userinfo );

        [OperationContract]
        [WebInvoke(
            UriTemplate = "ad/account/delete" ,
            RequestFormat = WebMessageFormat.Json ,
            ResponseFormat = WebMessageFormat.Json ,
            BodyStyle = WebMessageBodyStyle.Bare ,
            Method = "POST" )]
        ResponseMessage RemoveADUser( [MessageParameter( Name = "userinfo" )] ADUserRequest userinfo );

        [OperationContract]
        [WebInvoke(
            UriTemplate = "ad/group/add" ,
            RequestFormat = WebMessageFormat.Json ,
            ResponseFormat = WebMessageFormat.Json ,
            BodyStyle = WebMessageBodyStyle.Bare ,
            Method = "POST" )]
        ResponseMessage AddGroup( [MessageParameter( Name = "groupinfo" )] RequestGroupCreate groupInfo );

        [OperationContract]
        [WebInvoke(
            UriTemplate = "ad/group/remove" ,
            RequestFormat = WebMessageFormat.Json ,
            ResponseFormat = WebMessageFormat.Json ,
            BodyStyle = WebMessageBodyStyle.Bare ,
            Method = "POST" )]
        ResponseMessage RemoveGroup( [MessageParameter( Name = "groupinfo" )] ADGroupRequest groupInfo );

        [OperationContract]
        [WebInvoke(
            UriTemplate = "ad/machine/add" ,
            RequestFormat = WebMessageFormat.Json ,
            ResponseFormat = WebMessageFormat.Json ,
            BodyStyle = WebMessageBodyStyle.Bare ,
            Method = "POST" )]
        ResponseMessage AddMachine([MessageParameter( Name = "machineinfo" )] RequestComputerCreate computerInfo );

        [OperationContract]
        [WebInvoke(
            UriTemplate = "ad/machine/remove" ,
            RequestFormat = WebMessageFormat.Json ,
            ResponseFormat = WebMessageFormat.Json ,
            BodyStyle = WebMessageBodyStyle.Bare ,
            Method = "POST" )]
        ResponseMessage RemoveMachine( [MessageParameter( Name = "machineinfo" )] ADMachineRequest computerInfo );

        [OperationContract]
        [WebInvoke(
            UriTemplate = "ad/machine/enable" ,
            RequestFormat = WebMessageFormat.Json ,
            ResponseFormat = WebMessageFormat.Json ,
            BodyStyle = WebMessageBodyStyle.Bare ,
            Method = "POST" )]
        ResponseMessage EnableComputer( [MessageParameter( Name = "machineinfo" )] ADMachineRequest computerInfo );

        [OperationContract]
        [WebInvoke(
            UriTemplate = "ad/object/rename" ,
            RequestFormat = WebMessageFormat.Json ,
            ResponseFormat = WebMessageFormat.Json ,
            BodyStyle = WebMessageBodyStyle.Bare ,
            Method = "POST" )]
        ResponseMessage RenameObject( [MessageParameter( Name = "objectinfo" )] RequestObjectRename objectInfo);

        [OperationContract]
        [WebInvoke(
            UriTemplate = "ad/object/move" ,
            RequestFormat = WebMessageFormat.Json ,
            ResponseFormat = WebMessageFormat.Json ,
            BodyStyle = WebMessageBodyStyle.Bare ,
            Method = "POST" )]
        ResponseMessage MoveObject( [MessageParameter( Name = "objectinfo" )] RequestMoveObject objectInfo );

        [OperationContract]
        [WebInvoke(
            UriTemplate = "ad/account/join/group" ,
            RequestFormat = WebMessageFormat.Json ,
            ResponseFormat = WebMessageFormat.Json ,
            BodyStyle = WebMessageBodyStyle.Bare ,
            Method = "POST" )]
        ResponseMessage AddADUserToGroup( [MessageParameter( Name = "userinfo" )] UserGroupRequest userInfo );

        [OperationContract]
        [WebInvoke(
            UriTemplate = "ad/account/disjoin/group" ,
            RequestFormat = WebMessageFormat.Json ,
            ResponseFormat = WebMessageFormat.Json ,
            BodyStyle = WebMessageBodyStyle.Bare ,
            Method = "POST" )]
        ResponseMessage RemoveADUserFromGroup( [MessageParameter( Name = "userinfo" )] UserGroupRequest userinfo ); 

        [OperationContract]
        [WebInvoke(
            UriTemplate = "ad/account/password/reset" ,
            RequestFormat = WebMessageFormat.Json ,
            ResponseFormat = WebMessageFormat.Json ,
            BodyStyle = WebMessageBodyStyle.Bare ,
            Method = "POST" )]
        ResponseMessage ResetADUserPassword( UserPasswordRequest userinfo );

        [OperationContract]
        [WebInvoke(
            UriTemplate = "ad/account/lock" ,
            RequestFormat = WebMessageFormat.Json ,
            ResponseFormat = WebMessageFormat.Json ,
            BodyStyle = WebMessageBodyStyle.Bare ,
            Method = "POST" )]
        ResponseMessage UnlockADAccount( UserLockRequest userinfo );

    }

}
