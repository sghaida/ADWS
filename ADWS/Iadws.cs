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
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
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
            UriTemplate = "ad/user/add" ,
            RequestFormat = WebMessageFormat.Json ,
            ResponseFormat = WebMessageFormat.Json ,
            BodyStyle = WebMessageBodyStyle.Bare ,
            Method = "POST" )]
        ResponseMessage AddADUser( [MessageParameter( Name = "userinfo" )] UserCreateRequest userinfo );

        [OperationContract]
        [WebInvoke(
            UriTemplate = "ad/user/add/group" ,
            RequestFormat = WebMessageFormat.Json ,
            ResponseFormat = WebMessageFormat.Json ,
            BodyStyle = WebMessageBodyStyle.Bare ,
            Method = "POST" )]
        ResponseMessage AddADUserToGroup( [MessageParameter( Name = "userinfo" )] UserGroupRequest userInfo );

        [OperationContract]
        [WebInvoke(
            UriTemplate = "ad/user/password/reset" ,
            RequestFormat = WebMessageFormat.Json ,
            ResponseFormat = WebMessageFormat.Json ,
            BodyStyle = WebMessageBodyStyle.Bare ,
            Method = "POST" )]
        ResponseMessage ResetADUserPassword( UserPasswordRequest userinfo );

        [OperationContract]
        [WebInvoke(
            UriTemplate = "ad/user/lock" ,
            RequestFormat = WebMessageFormat.Json ,
            ResponseFormat = WebMessageFormat.Json ,
            BodyStyle = WebMessageBodyStyle.Bare ,
            Method = "POST" )]
        ResponseMessage UnlockADAccount( UserLockRequest userinfo );

    }

}
