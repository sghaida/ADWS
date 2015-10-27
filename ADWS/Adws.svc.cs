using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Web.Security;
using ADWS.Models;

namespace ADWS
{
    public class Adws : IAdws
    {

        //WEB.CONF AD RELATED FIELDS
        private static readonly int SessionTTL = int.Parse( ConfigurationManager.AppSettings[ "SessionTTL" ] );

        private static readonly string LocalGcUri = ConfigurationManager.AppSettings[ "LocalDomainURI" ];
        private static readonly string LocalGcUsername = ConfigurationManager.AppSettings[ "LocalDomainUser" ];
        private static readonly string LocalGcPassword = ConfigurationManager.AppSettings[ "LocalDomainPassword" ];
        private static readonly string AdSearchFilter = ConfigurationManager.AppSettings[ "ADSearchFilter" ];

        private static readonly DirectoryEntry directoryEntry = new DirectoryEntry( LocalGcUri , LocalGcUsername , LocalGcPassword );
        private readonly DirectorySearcher searcher = new DirectorySearcher( directoryEntry );

        /// <summary>
        /// Authenticate User from Active directory based on his email address and password 
        /// </summary>
        /// <param name="emailAddress">Email Address</param>
        /// <param name="password">Password</param>
        /// <returns>Session</returns>
        public Session AuthenticateUserUsingCredentials( AuthDataRequest authData )
        {
            UserInfoResponse userInfo = new UserInfoResponse();
            string emailAddress = authData.username;
            string password = authData.password;


            Session stat = new Session();

            string msg = string.Empty;

            if ( string.IsNullOrEmpty( emailAddress ) || string.IsNullOrEmpty( password ) )
            {
                stat.Message = "Email and/or password can't be empty!";
                stat.IsAuthenticated = false;

                return stat;
            }
            try
            {
                userInfo = GetUserAttributes( emailAddress );

                if ( userInfo == null )
                {
                    stat.Message = "Error: Couldn't fetch user information!";
                    stat.IsAuthenticated = false;

                    return stat;
                }

                var directoryEntry = new DirectoryEntry( LocalGcUri , userInfo.Upn , password );

                directoryEntry.AuthenticationType = AuthenticationTypes.None;

                var localFilter = string.Format( AdSearchFilter , emailAddress );

                var localSearcher = new DirectorySearcher( directoryEntry );

                localSearcher.PropertiesToLoad.Add( "mail" );
                localSearcher.Filter = localFilter;

                var result = localSearcher.FindOne();

                if ( result != null )
                {
                    stat.Message = "You have logged in successfully!";
                    stat.IsAuthenticated = true;

                    //Set the session Data
                    SessionData session = new SessionData();

                    session.Username = userInfo.EmailAddress;
                    session.Password = password;
                    session.SessionStart = DateTime.Now;

                    //Encrypt Session Data
                    stat.SessionKey = SessionHandler.EncryptSession( session );

                    return stat;
                }

                stat.Message = "Login failed, please try again.";
                stat.IsAuthenticated = false;

                return stat;
            }
            catch ( Exception ex )
            {
                stat.Message = "Wrong Email and/or Password " + ex;
                stat.IsAuthenticated = false;

                return stat;
            }
        }

        /// <summary>
        /// Authonticate user based on the previously provided session key
        /// </summary>
        /// <param name="sessionKey">session key</param>
        /// <returns>Session</returns>
        public Session AuthenticateUserUsingSession( string sessionKey )
        {
            return ValidateSession( sessionKey );
        }

        /// <summary>
        /// Validate Session Key if it is valid and if it is not expired
        /// </summary>
        /// <param name="sessionKey"></param>
        /// <returns></returns>
        public Session ValidateSession( string sessionKey )
        {
            Session stat = new Session();

            if ( string.IsNullOrWhiteSpace( sessionKey ) )
            {
                stat.Message = "No Session key has been provide";
                stat.IsAuthenticated = false;

                return stat;
            }
            else
            {
                try
                {
                    SessionData sessionData = SessionHandler.DecryptSession( sessionKey );

                    if ( sessionKey != null && ( ( DateTime.Now.Subtract( sessionData.SessionStart ) ).TotalHours < SessionTTL ) )
                    {
                        stat.Message = "You have logged in successfully!";
                        stat.IsAuthenticated = true;
                        stat.SessionKey = sessionKey;
                        return stat;
                    }
                    else
                    {
                        AuthDataRequest authData = new AuthDataRequest();
                        authData.username = sessionData.Username;
                        authData.password = sessionData.Password;

                        stat = AuthenticateUserUsingCredentials( authData );
                        stat.Message = "You have logged in successfully!, and Session key has been renewed";

                        return stat;
                    }
                }
                catch ( Exception ex )
                {
                    stat.Message = "Couldn't validate Session key, kinldy authenticate first " + ex;
                    stat.IsAuthenticated = false;

                    return stat;
                }
            }
        }

        /// <summary>
        /// Get user attrubutes from email address
        /// </summary>
        /// <param name="mailAddress">User Email Address</param>
        /// <returns>UserInfo Object</returns>
        public UserInfoResponse GetUserAttributes( RequestBody request )
        {
            var userInfo = new UserInfoResponse();

            string sessionKey = request.SessionKey;
            string mailAddress = request.Body;

            Session stat = ValidateSession( sessionKey );

            if ( stat.IsAuthenticated == true )
            {
                var searchFilter = string.Format( AdSearchFilter , mailAddress );

                searcher.Filter = searchFilter;

                try
                {
                    var searchResult = searcher.FindOne();

                    if ( searchResult != null )
                    {
                        if ( searchResult.Properties.Contains( "title" ) )
                            userInfo.Title = (string)searchResult.Properties[ "title" ][ 0 ];

                        if ( searchResult.Properties.Contains( "givenName" ) )
                            userInfo.FirstName = (string)searchResult.Properties[ "givenName" ][ 0 ];

                        if ( searchResult.Properties.Contains( "sn" ) )
                            userInfo.LastName = (string)searchResult.Properties[ "sn" ][ 0 ];

                        if ( searchResult.Properties.Contains( "cn" ) )
                            userInfo.DisplayName = (string)searchResult.Properties[ "cn" ][ 0 ];

                        if ( searchResult.Properties.Contains( "samaccountname" ) )
                            userInfo.SamAccountName = (string)searchResult.Properties[ "samaccountname" ][ 0 ];

                        if ( searchResult.Properties.Contains( "department" ) )
                            userInfo.Department = (string)searchResult.Properties[ "department" ][ 0 ];

                        if ( searchResult.Properties.Contains( "mail" ) )
                            userInfo.EmailAddress = (string)searchResult.Properties[ "mail" ][ 0 ];

                        if ( searchResult.Properties.Contains( "employeeid" ) )
                            userInfo.EmployeeId = (string)searchResult.Properties[ "employeeid" ][ 0 ];

                        if ( searchResult.Properties.Contains( "telephonenumber" ) )
                            userInfo.BusinessPhone = (string)searchResult.Properties[ "telephonenumber" ][ 0 ];

                        if ( searchResult.Properties.Contains( "physicalDeliveryOfficeName" ) )
                            userInfo.PhysicalDeliveryOfficeName =
                                (string)searchResult.Properties[ "physicalDeliveryOfficeName" ][ 0 ];

                        if ( searchResult.Properties.Contains( "msrtcsip-primaryuseraddress" ) )
                            userInfo.SipAccount = (string)searchResult.Properties[ "msrtcsip-primaryuseraddress" ][ 0 ];

                        if ( searchResult.Properties.Contains( "msrtcsip-line" ) )
                            userInfo.Telephone = (string)searchResult.Properties[ "msrtcsip-line" ][ 0 ];

                        if ( searchResult.Properties.Contains( "msrtcsip-primaryhomeserver" ) )
                            userInfo.PrimaryHomeServerDn =
                                ( (string)searchResult.Properties[ "msrtcsip-primaryhomeserver" ][ 0 ] ).Replace(
                                    "CN=Lc Services,CN=Microsoft," , "" );

                        if ( searchResult.Properties.Contains( "userprincipalname" ) )
                            userInfo.Upn = (string)searchResult.Properties[ "userprincipalname" ][ 0 ];

                        //Get the IP Dialing Code and extensionfor projects
                        if ( searchResult.Properties.Contains( "extensionAttribute1" ) &&
                            searchResult.Properties.Contains( "extensionAttribute2" ) )
                        {
                            userInfo.OtherTelphone = (string)searchResult.Properties[ "extensionAttribute2" ][ 0 ] +
                                                     searchResult.Properties[ "extensionAttribute1" ][ 0 ];
                        }
                    }

                }
                catch ( Exception ex )
                {
                    userInfo.Message = ex.Message;
                    return userInfo;
                }

            }
            return userInfo;
        }

        /// <summary>
        /// Get user attrubutes from email address
        /// </summary>
        /// <param name="mailAddress">User Email Address</param>
        /// <returns>UserInfo Object</returns>
        private UserInfoResponse GetUserAttributes( string mailAddress )
        {
            var userInfo = new UserInfoResponse();

            var searchFilter = string.Format( AdSearchFilter , mailAddress );

            searcher.Filter = searchFilter;

            try
            {
                var searchResult = searcher.FindOne();

                if ( searchResult != null )
                {
                    if ( searchResult.Properties.Contains( "title" ) )
                        userInfo.Title = (string)searchResult.Properties[ "title" ][ 0 ];

                    if ( searchResult.Properties.Contains( "givenName" ) )
                        userInfo.FirstName = (string)searchResult.Properties[ "givenName" ][ 0 ];

                    if ( searchResult.Properties.Contains( "sn" ) )
                        userInfo.LastName = (string)searchResult.Properties[ "sn" ][ 0 ];

                    if ( searchResult.Properties.Contains( "cn" ) )
                        userInfo.DisplayName = (string)searchResult.Properties[ "cn" ][ 0 ];

                    if ( searchResult.Properties.Contains( "samaccountname" ) )
                        userInfo.SamAccountName = (string)searchResult.Properties[ "samaccountname" ][ 0 ];

                    if ( searchResult.Properties.Contains( "department" ) )
                        userInfo.Department = (string)searchResult.Properties[ "department" ][ 0 ];

                    if ( searchResult.Properties.Contains( "mail" ) )
                        userInfo.EmailAddress = (string)searchResult.Properties[ "mail" ][ 0 ];

                    if ( searchResult.Properties.Contains( "employeeid" ) )
                        userInfo.EmployeeId = (string)searchResult.Properties[ "employeeid" ][ 0 ];

                    if ( searchResult.Properties.Contains( "telephonenumber" ) )
                        userInfo.BusinessPhone = (string)searchResult.Properties[ "telephonenumber" ][ 0 ];

                    if ( searchResult.Properties.Contains( "physicalDeliveryOfficeName" ) )
                        userInfo.PhysicalDeliveryOfficeName =
                            (string)searchResult.Properties[ "physicalDeliveryOfficeName" ][ 0 ];

                    if ( searchResult.Properties.Contains( "msrtcsip-primaryuseraddress" ) )
                        userInfo.SipAccount = (string)searchResult.Properties[ "msrtcsip-primaryuseraddress" ][ 0 ];

                    if ( searchResult.Properties.Contains( "msrtcsip-line" ) )
                        userInfo.Telephone = (string)searchResult.Properties[ "msrtcsip-line" ][ 0 ];

                    if ( searchResult.Properties.Contains( "msrtcsip-primaryhomeserver" ) )
                        userInfo.PrimaryHomeServerDn =
                            ( (string)searchResult.Properties[ "msrtcsip-primaryhomeserver" ][ 0 ] ).Replace(
                                "CN=Lc Services,CN=Microsoft," , "" );

                    if ( searchResult.Properties.Contains( "userprincipalname" ) )
                        userInfo.Upn = (string)searchResult.Properties[ "userprincipalname" ][ 0 ];

                    //Get the IP Dialing Code and extensionfor projects
                    if ( searchResult.Properties.Contains( "extensionAttribute1" ) &&
                        searchResult.Properties.Contains( "extensionAttribute2" ) )
                    {
                        userInfo.OtherTelphone = (string)searchResult.Properties[ "extensionAttribute2" ][ 0 ] +
                                                    searchResult.Properties[ "extensionAttribute1" ][ 0 ];
                    }

                    return userInfo;
                }

                return null;
            }
            catch ( Exception ex )
            {
                var argEx = new ArgumentException( "Exception" , "ex" , ex );
                throw argEx;
            }
        }

        /// <summary>
        /// Get All Active Directory Domain Controllers
        /// </summary>
        /// <returns></returns>
        public List<DomainResponse> GetLocalDomainController( string sessionKey )
        {
            Session stat = ValidateSession( sessionKey );
            List<DomainResponse> domainControllersInfo = new List<DomainResponse>();

            if ( stat.IsAuthenticated == true )
            {
                try
                {
                    var obj = Forest.GetCurrentForest();
                    var collection = obj.Domains;
                    foreach ( Domain domain in collection )
                    {
                        var domainControllers = domain.FindAllDiscoverableDomainControllers();
                        foreach ( DomainController domainController in domainControllers )
                        {
                            if ( !domainControllersInfo.Any( item => item.DomainName == domain.Name ) )

                                domainControllersInfo.Add( new DomainResponse
                                {
                                    DomainName = domain.Name ,
                                    Forest = domain.Forest.Name ,
                                    DomainMode = domain.DomainMode.ToString() ,
                                    InfrastructureRoleOwnerHostName = domain.InfrastructureRoleOwner.Name ,
                                    InfrastructureRoleOwnerIPAddress = domain.InfrastructureRoleOwner.IPAddress ,

                                    DcIpAddress = domainController.IPAddress ,
                                    FQDN = domainController.Name ,
                                    OS = domainController.OSVersion ,
                                    SiteName = domainController.SiteName ,


                                } );
                        }
                    }
                }
                catch ( Exception ex )
                {
                    var argEx = new ArgumentException( "Exception" , "ex" , ex );
                    throw argEx;
                }
            }
            return domainControllersInfo;
        }

        /// <summary>
        ///     Get AD Domain NetBios Name
        /// </summary>
        /// <param name="dnsDomainName">DNS Suffix Name</param>
        /// <returns>Domain NetBios Name</returns>
        public string GetNetbiosDomainName( RequestBody request )
        {
            var netbiosDomainName = string.Empty;

            string sessionKey = request.SessionKey;
            string dnsDomainName = request.Body;

            Session stat = ValidateSession( sessionKey );

            if ( stat.IsAuthenticated == true )
            {
                try
                {
                    var rootDse = new DirectoryEntry( string.Format( "LDAP://{0}/RootDSE" , dnsDomainName ) );

                    var configurationNamingContext = rootDse.Properties[ "configurationNamingContext" ][ 0 ].ToString();

                    var searchRoot = new DirectoryEntry( "LDAP://cn=Partitions," + configurationNamingContext );

                    var searcher = new DirectorySearcher( searchRoot );
                    searcher.SearchScope = SearchScope.OneLevel;
                    // searcher.PropertiesToLoad.Add("netbiosname");
                    searcher.Filter = string.Format( "(&(objectcategory=Crossref)(dnsRoot={0})(netBIOSName=*))" ,
                        dnsDomainName );

                    var result = searcher.FindOne();

                    if ( result != null )
                    {
                        netbiosDomainName = result.Properties[ "netbiosname" ][ 0 ].ToString();
                    }

                    return netbiosDomainName;
                }
                catch ( Exception ex )
                {
                    var argEx = new ArgumentException( "Exception" , "ex" , ex );
                    throw argEx;
                }
            }
            return netbiosDomainName;
        }

        /// <summary>
        ///     Get DNS Name from AD Netbios Name
        /// </summary>
        /// <param name="netBiosName">AD Netbios Name</param>
        /// <returns>Domain FQDN</returns>
        public string GetFqdnFromNetBiosName( RequestBody request )
        {
            var fqdn = string.Empty;
            string netBiosName = request.Body;
            string sessionKey = request.SessionKey;

            Session stat = ValidateSession( sessionKey );

            if ( stat.IsAuthenticated == true )
            {
                try
                {
                    var rootDse = new DirectoryEntry( string.Format( "LDAP://{0}/RootDSE" , netBiosName ) );

                    var configurationNamingContext = rootDse.Properties[ "configurationNamingContext" ][ 0 ].ToString();

                    var searchRoot = new DirectoryEntry( "LDAP://cn=Partitions," + configurationNamingContext );

                    var searcher = new DirectorySearcher( searchRoot );
                    searcher.SearchScope = SearchScope.OneLevel;
                    //searcher.PropertiesToLoad.Add("dnsroot");
                    searcher.Filter = string.Format( "(&(objectcategory=Crossref)(netbiosname={0}))" , netBiosName );

                    var result = searcher.FindOne();
                    if ( result != null )
                        fqdn = result.Properties[ "dnsroot" ][ 0 ].ToString();


                }
                catch ( Exception ex )
                {
                    var argEx = new ArgumentException( "Exception" , "ex" , ex );
                    throw argEx;
                }
            }
            return fqdn;
        }

        /// <summary>
        /// Create AD User in a container
        /// </summary>
        /// <param name="userinfo">ADUser object</param>
        /// <returns></returns>
        public ResponseMessage AddADUser( RequestUserCreate userinfo )
        {
            ResponseMessage status = new ResponseMessage();

            status.IsSuccessful = false;
            status.Message = string.Empty;

            Session stat = ValidateSession( userinfo.DomainInfo.SessionKey );

            if ( stat.IsAuthenticated == true )
            {

                PrincipalContext principalContext = null;

                string uri = FixADURI( userinfo.DomainInfo.ADHost , userinfo.DomainInfo.ContainerPath );

                if ( string.IsNullOrWhiteSpace( uri ) )
                {
                    status.Message = status.Message = "AD Host is not allowed to be empty, kindly provide the AD Host";
                    return status;
                }

                bool isAllowWite = CheckWriteOermission( uri , userinfo.DomainInfo.BindingUserName , userinfo.DomainInfo.BindingUserPassword );

                try
                {
                    UserPrincipal usr = FindADUser( userinfo.UserLogonName , userinfo.DomainInfo );
                    if ( usr != null )
                    {
                        status.Message = " user already exists. Please use a different User Logon Name";
                        return status;
                    }
                    else
                    {
                        principalContext = new PrincipalContext( ContextType.Domain , userinfo.DomainInfo.DomainName , userinfo.DomainInfo.ContainerPath , userinfo.DomainInfo.BindingUserName , userinfo.DomainInfo.BindingUserPassword );
                    }
                }
                catch ( Exception ex )
                {
                    status.Message = @"Failed to create PrincipalContext: " + ex;
                    return status;
                }

                // Create the new UserPrincipal object
                UserPrincipal userPrincipal = new UserPrincipal( principalContext );

                if ( !string.IsNullOrWhiteSpace( userinfo.LastName ) )
                    userPrincipal.Surname = userinfo.LastName;

                if ( !string.IsNullOrWhiteSpace( userinfo.FirstName ) )
                    userPrincipal.GivenName = userinfo.FirstName;

                if ( !string.IsNullOrWhiteSpace( userinfo.LastName ) && !string.IsNullOrWhiteSpace( userinfo.FirstName ) )
                    userPrincipal.DisplayName = userinfo.FirstName + " " + userinfo.LastName;

                if ( !string.IsNullOrWhiteSpace( userinfo.Description ) )
                    userPrincipal.Description = userinfo.Description;

                if ( !string.IsNullOrWhiteSpace( userinfo.EmployeeID ) )
                    userPrincipal.EmployeeId = userinfo.EmployeeID;

                if ( !string.IsNullOrWhiteSpace( userinfo.EmailAddress ) )
                    userPrincipal.EmailAddress = userinfo.EmailAddress;

                if ( !string.IsNullOrWhiteSpace( userinfo.Telephone ) )
                    userPrincipal.VoiceTelephoneNumber = userinfo.Telephone;

                if ( !string.IsNullOrWhiteSpace( userinfo.UserLogonName ) )
                    userPrincipal.SamAccountName = userinfo.UserLogonName;

                if ( !string.IsNullOrWhiteSpace( userinfo.Password ) )
                    userPrincipal.SetPassword( userinfo.Password );

                userPrincipal.Enabled = true;
                userPrincipal.ExpirePasswordNow();

                try
                {
                    userPrincipal.Save();

                    DirectoryEntry de = (DirectoryEntry)userPrincipal.GetUnderlyingObject();

                    FillUserExtraAttributes( ref de , userinfo );

                    de.CommitChanges();
                    status.Message = "Account has been created successfuly";
                    status.IsSuccessful = true;
                }
                catch ( Exception ex )
                {
                    status.Message = "Exception creating user object. " + ex;
                    status.IsSuccessful = false;
                    return status;
                }

                return status;
            }
            else
            {
                status.Message = "Kindly authenticate first";
                return status;
            }
        }

        /// <summary>
        /// Delete AD User Account
        /// </summary>
        /// <param name="userinfo">ADUserRequest object</param>
        /// <returns>ResponseMessage</returns>
        public ResponseMessage RemoveADUser( ADUserRequest userinfo ) 
        {
            ResponseMessage status = new ResponseMessage();

            status.IsSuccessful = false;
            status.Message = string.Empty;

            Session stat = ValidateSession( userinfo.DomainInfo.SessionKey );

            if ( stat.IsAuthenticated == true )
            {

                string uri = FixADURI( userinfo.DomainInfo.ADHost , userinfo.DomainInfo.ContainerPath );

                if ( string.IsNullOrWhiteSpace( uri ) )
                {
                    status.Message = status.Message = "AD Host is not allowed to be empty, kindly provide the AD Host";
                    return status;
                }

                bool isAllowWite = CheckWriteOermission( uri , userinfo.DomainInfo.BindingUserName , userinfo.DomainInfo.BindingUserPassword );

                try
                {
                    UserPrincipal usr = FindADUser( userinfo.SamAccountName , userinfo.DomainInfo );
                    if ( usr != null )
                    {
                        usr.Delete();

                        status.Message = " user Account has been Deleted";
                        status.IsSuccessful = true;

                        return status;
                    }
                    else
                    {
                        status.Message = " Can't delete user account. please check with administrator";

                        return status;
                    }
                }
                catch ( Exception ex )
                {
                    status.Message = @"Failed to create PrincipalContext: " + ex;
                    return status;
                }
            }
            else
            {
                status.Message = "Kindly authenticate first";
                return status;
            }
        }

        /// <summary>
        /// Add AD Group
        /// </summary>
        /// <param name="groupInfo">CreateGroupRequest</param>
        /// <returns>ResponseMessage</returns>
        public ResponseMessage AddGroup( RequestGroupCreate groupInfo ) 
        {
            ResponseMessage status = new ResponseMessage();

            status.IsSuccessful = false;
            status.Message = string.Empty;

            Session stat = ValidateSession( groupInfo.DomainInfo.SessionKey );

            if ( stat.IsAuthenticated == true )
            {

                PrincipalContext principalContext = null;

                string uri = FixADURI( groupInfo.DomainInfo.ADHost , groupInfo.DomainInfo.ContainerPath );

                if ( string.IsNullOrWhiteSpace( uri ) )
                {
                    status.Message = status.Message = "AD Host is not allowed to be empty, kindly provide the AD Host";
                    return status;
                }

                bool isAllowWite = CheckWriteOermission( uri , groupInfo.DomainInfo.BindingUserName , groupInfo.DomainInfo.BindingUserPassword );

                try
                {
                    GroupPrincipal group = FindADGroup( groupInfo.GroupName , groupInfo.DomainInfo );

                    if ( group != null )
                    {

                        status.Message = @"There is a existing group with the provided name, kindly choose another name";

                        return status;
                    }
                    else
                    {
                        principalContext = new PrincipalContext( ContextType.Domain , groupInfo.DomainInfo.DomainName , groupInfo.DomainInfo.ContainerPath , groupInfo.DomainInfo.BindingUserName , groupInfo.DomainInfo.BindingUserPassword );

                        group = new GroupPrincipal( principalContext , groupInfo.GroupName );
                        group.DisplayName = groupInfo.DisplayName;
                        group.Description = groupInfo.Description;
                        group.GroupScope = groupInfo.OGroupScope;
                        group.IsSecurityGroup = groupInfo.IsSecurityGroup;
                        group.Save();

                        status.Message = @"Group has been added successfully ";
                        status.IsSuccessful = true;
                        return status;
                    }
                }
                catch ( Exception ex )
                {
                    status.Message = status.Message = "error has accured while adding the desgnated group" + ex;
                    return status;
                }
            }
            else
            {
                status.Message = "Kindly authenticate first";
                return status;
            }
        }

        /// <summary>
        /// Remove Group
        /// </summary>
        /// <param name="groupInfo">ADGroupRequest</param>
        /// <returns>ResponseMessage</returns>
        public ResponseMessage RemoveGroup(ADGroupRequest groupInfo) 
        {
            ResponseMessage status = new ResponseMessage();

            status.Message = string.Empty;
            status.IsSuccessful = false;

            Session stat = ValidateSession( groupInfo.DomainInfo.SessionKey );

            if ( stat.IsAuthenticated == true )
            {

                string uri = FixADURI( groupInfo.DomainInfo.ADHost , groupInfo.DomainInfo.ContainerPath );

                if ( string.IsNullOrWhiteSpace( uri ) )
                {
                    status.Message = status.Message = "AD Host is not allowed to be empty, kindly provide the AD Host";
                    return status;
                }

                bool isAllowWite = CheckWriteOermission( uri , groupInfo.DomainInfo.BindingUserName , groupInfo.DomainInfo.BindingUserPassword );

                try
                {
                    GroupPrincipal group = FindADGroup( groupInfo.GroupName , groupInfo.DomainInfo );

                    if ( group != null )
                    {
                        group.Delete();

                        status.Message = @"The group has been removed successfully";
                        status.IsSuccessful = true;

                        return status;
                    }
                    else
                    {
                        status.Message = @"The given Group doesn't exist ";
                        return status;
                    }
                }
                catch ( Exception ex )
                {
                    status.Message = status.Message = "error has accured while adding the desgnated group" + ex;
                    return status;
                }
            }
            else
            {
                status.Message = "Kindly authenticate first";
                return status;
            }

            
        }

        /// <summary>
        /// ADD Active Directory Computer Obbject
        /// </summary>
        /// <param name="computerInfo">ADComputerRequest</param>
        /// <returns>ResponseMessage</returns>
        public ResponseMessage AddMachine( RequestComputerCreate computerInfo ) 
        {
            ResponseMessage status = new ResponseMessage();

            status.IsSuccessful = false;
            status.Message = string.Empty;

            Session stat = ValidateSession( computerInfo.DomainInfo.SessionKey );

            if ( stat.IsAuthenticated == true )
            {

                string uri = FixADURI( computerInfo.DomainInfo.ADHost , computerInfo.DomainInfo.ContainerPath );

                if ( string.IsNullOrWhiteSpace( uri ) )
                {
                    status.Message = status.Message = "AD Host is not allowed to be empty, kindly provide the AD Host";
                    return status;
                }

                bool isAllowWite = CheckWriteOermission( uri , computerInfo.DomainInfo.BindingUserName , computerInfo.DomainInfo.BindingUserPassword );

                try
                {
                    ComputerPrincipal computer = FindADComputer( computerInfo.SamAccountName , computerInfo.DomainInfo );

                    if ( computer != null )
                    {

                        status.Message = @"There is a existing computer with the provided name, kindly choose another name";

                        return status;
                    }
                    else
                    {
                        var principalContext = new PrincipalContext( ContextType.Domain , computerInfo.DomainInfo.DomainName , computerInfo.DomainInfo.ContainerPath , computerInfo.DomainInfo.BindingUserName , computerInfo.DomainInfo.BindingUserPassword );

                        computer = new ComputerPrincipal( principalContext );
                        computer.DisplayName = computerInfo.DisplayName;
                        computer.Description = computerInfo.Description;
                        computer.SamAccountName = computerInfo.SamAccountName;
                        computer.Enabled = true;
                        computer.SetPassword( GenerateADPassword( uri , computerInfo.DomainInfo.BindingUserName , computerInfo.DomainInfo.BindingUserPassword ) );
                        computer.Save();

                        status.Message = @"Computer has been added successfully ";
                        status.IsSuccessful = true;
                        return status;
                    }
                }
                catch ( Exception ex )
                {
                    status.Message = status.Message = "error has accured while adding the desgnated group" + ex;
                    return status;
                }
            }
            else
            {
                status.Message = "Kindly authenticate first";
                return status;
            }
        }

        /// <summary>
        /// Remove Machine Object from Active Directory
        /// </summary>
        /// <param name="computerInfo">ADMachineRequest</param>
        /// <returns>ResponseMessage</returns>
        public ResponseMessage RemoveMachine( ADMachineRequest computerInfo ) 
        {
            ResponseMessage status = new ResponseMessage();

            status.IsSuccessful = false;
            status.Message = string.Empty;

            Session stat = ValidateSession( computerInfo.DomainInfo.SessionKey );

            if ( stat.IsAuthenticated == true )
            {

                string uri = FixADURI( computerInfo.DomainInfo.ADHost , computerInfo.DomainInfo.ContainerPath );

                if ( string.IsNullOrWhiteSpace( uri ) )
                {
                    status.Message = status.Message = "AD Host is not allowed to be empty, kindly provide the AD Host";
                    return status;
                }

                bool isAllowWite = CheckWriteOermission( uri , computerInfo.DomainInfo.BindingUserName , computerInfo.DomainInfo.BindingUserPassword );

                try
                {
                    ComputerPrincipal computer = FindADComputer( computerInfo.SamAccountName , computerInfo.DomainInfo );

                    if ( computer != null )
                    {
                        computer.Delete();

                        status.Message = @"There is a existing computer with the provided name, kindly choose another name";
                        status.IsSuccessful = true;
                        return status;
                    }
                    else
                    {
                        status.Message = @"Computer Object doesn't exist with the correspondent name ";

                        return status;
                    }
                }
                catch ( Exception ex )
                {
                    status.Message = status.Message = "error has accured while adding the desgnated group" + ex;
                    return status;
                }
            }
            else
            {
                status.Message = "Kindly authenticate first";
                return status;
            }
        }

        /// <summary>
        /// enable or Disable Machine Object in Active Directory
        /// </summary>
        /// <param name="computerInfo">ADMachineRequest</param>
        /// <returns>ResponseMessage</returns>
        public ResponseMessage EnableComputer( ADMachineRequest computerInfo ) 
        {
            ResponseMessage status = new ResponseMessage();

            status.IsSuccessful = false;
            status.Message = string.Empty;

            Session stat = ValidateSession( computerInfo.DomainInfo.SessionKey );

            if ( stat.IsAuthenticated == true )
            {

                string uri = FixADURI( computerInfo.DomainInfo.ADHost , computerInfo.DomainInfo.ContainerPath );

                if ( string.IsNullOrWhiteSpace( uri ) )
                {
                    status.Message = status.Message = "AD Host is not allowed to be empty, kindly provide the AD Host";
                    return status;
                }

                bool isAllowWite = CheckWriteOermission( uri , computerInfo.DomainInfo.BindingUserName , computerInfo.DomainInfo.BindingUserPassword );

                try
                {
                    ComputerPrincipal computer = FindADComputer( computerInfo.SamAccountName , computerInfo.DomainInfo );

                    if ( computer != null )
                    {
                        computer.Enabled = computerInfo.IsEnabled;

                        status.Message = @"Operation was done successfully";
                        status.IsSuccessful = true;
                        return status;
                    }
                    else
                    {
                        status.Message = @"Computer Object doesn't exist with the correspondent name ";

                        return status;
                    }
                }
                catch ( Exception ex )
                {
                    status.Message = status.Message = "error has accured while adding the desgnated group" + ex;
                    return status;
                }
            }
            else
            {
                status.Message = "Kindly authenticate first";
                return status;
            }
        }

        /// <summary>
        /// Add user to a certain group
        /// </summary>
        /// <param name="userinfo">UserGroupRequest</param>
        /// <returns>ResponseMessage</returns>
        public ResponseMessage AddADUserToGroup( UserGroupRequest userinfo )
        {
            ResponseMessage status = new ResponseMessage();

            status.IsSuccessful = false;
            status.Message = string.Empty;

            Session stat = ValidateSession( userinfo.DomainInfo.SessionKey );

            if ( stat.IsAuthenticated == true )
            {

                PrincipalContext principalContext = null;

                string uri = FixADURI( userinfo.DomainInfo.ADHost , userinfo.DomainInfo.ContainerPath );

                if ( string.IsNullOrWhiteSpace( uri ) )
                {
                    status.Message = status.Message = "AD Host is not allowed to be empty, kindly provide the AD Host";
                    return status;
                }

                bool isAllowWite = CheckWriteOermission( uri , userinfo.DomainInfo.BindingUserName , userinfo.DomainInfo.BindingUserPassword );

                try
                {
                    GroupPrincipal group = FindADGroup( userinfo.GroupName , userinfo.DomainInfo );

                    if ( group != null )
                    {
                        group.Members.Add( principalContext , IdentityType.SamAccountName , userinfo.SamAccountName );

                        group.Save();

                        status.Message = "User added successfuly to the designated Group";
                        status.IsSuccessful = true;

                        return status;
                    }
                    else
                    {
                        status.Message = @"Group Doesn't Exist: ";
                        return status;
                    }
                }
                catch ( Exception ex )
                {
                    status.Message = status.Message = "error has accured while adding the user to the designated group" + ex;
                    return status;
                }
            }
            else
            {
                status.Message = "Kindly authenticate first";
                return status;
            }

        }

        /// <summary>
        /// Remove user from a certain group
        /// </summary>
        /// <param name="userinfo">UserGroupRequest</param>
        /// <returns>ResponseMessage</returns>
        public ResponseMessage RemoveADUserFromGroup( UserGroupRequest userinfo ) 
        {
            ResponseMessage status = new ResponseMessage();

            status.IsSuccessful = false;
            status.Message = string.Empty;

            Session stat = ValidateSession( userinfo.DomainInfo.SessionKey );

            if ( stat.IsAuthenticated == true )
            {

                PrincipalContext principalContext = null;

                string uri = FixADURI( userinfo.DomainInfo.ADHost , userinfo.DomainInfo.ContainerPath );

                if ( string.IsNullOrWhiteSpace( uri ) )
                {
                    status.Message = status.Message = "AD Host is not allowed to be empty, kindly provide the AD Host";
                    return status;
                }

                bool isAllowWite = CheckWriteOermission( uri , userinfo.DomainInfo.BindingUserName , userinfo.DomainInfo.BindingUserPassword );

                try
                {
                    GroupPrincipal group = FindADGroup( userinfo.GroupName , userinfo.DomainInfo );

                    if ( group != null )
                    {
                        group.Members.Remove( principalContext , IdentityType.SamAccountName , userinfo.SamAccountName );

                        group.Save();

                        status.Message = "User has been removed successfuly from the designated Group";
                        status.IsSuccessful = true;

                        return status;
                    }
                    else
                    {
                        status.Message = @"Group Doesn't Exist: ";
                        return status;
                    }
                }
                catch ( Exception ex )
                {
                    status.Message = status.Message = "error has accured while removing the user to the designated group" + ex;
                    return status;
                }
            }
            else
            {
                status.Message = "Kindly authenticate first";
                return status;
            }


        }

        /// <summary>
        /// Reset User Password
        /// </summary>
        /// <param name="userinfo">ADUserInfo</param>
        /// <returns></returns>
        public ResponseMessage ResetADUserPassword( UserPasswordRequest userinfo )
        {
            ResponseMessage status = new ResponseMessage();

            status.IsSuccessful = false;
            status.Message = string.Empty;

            Session stat = ValidateSession( userinfo.DomainInfo.SessionKey );

            if ( stat.IsAuthenticated == true )
            {

                string uri = FixADURI( userinfo.DomainInfo.ADHost , userinfo.DomainInfo.ContainerPath );

                if ( string.IsNullOrWhiteSpace( uri ) )
                {
                    status.Message = status.Message = "AD Host is not allowed to be empty, kindly provide the AD Host";
                    return status;
                }

                bool isAllowWite = CheckWriteOermission( uri , userinfo.DomainInfo.BindingUserName , userinfo.DomainInfo.BindingUserPassword );

                try
                {
                    UserPrincipal usr = FindADUser( userinfo.SamAccountName , userinfo.DomainInfo );

                    if ( usr != null )
                    {
                        if ( userinfo.IsAutoGeneratedPassword == true )
                            userinfo.NewPassword = GenerateADPassword( uri , userinfo.DomainInfo.BindingUserName , userinfo.DomainInfo.BindingUserPassword );

                        if ( userinfo.IsOldPasswordProvided == true )
                            usr.ChangePassword( userinfo.OldPassword , userinfo.NewPassword );
                        else
                            usr.SetPassword( userinfo.NewPassword );

                        if ( userinfo.WithPasswordExpiration == true )
                            usr.ExpirePasswordNow();

                        usr.Enabled = true;
                        usr.Save();

                        status.Message = " Password has been reseted Successfully";
                        status.IsSuccessful = true;

                        return status;
                    }
                    else
                    {
                        status.Message = "User doesn't exist";
                        return status;
                    }

                }
                catch ( Exception ex )
                {
                    status.Message = @"Failed to create PrincipalContext: " + ex;
                    return status;
                }
            }
            else
            {
                status.Message = "Kindly authenticate first";
                return status;
            }
        }

        /// <summary>
        /// Disable or unlock AD User Account
        /// </summary>
        /// <param name="userinfo"></param>
        /// <returns>ResponseMessage</returns>
        public ResponseMessage UnlockADAccount( UserLockRequest userinfo ) 
        {
            ResponseMessage status = new ResponseMessage();

            status.IsSuccessful = false;
            status.Message = string.Empty;

            Session stat = ValidateSession( userinfo.DomainInfo.SessionKey );

            if ( stat.IsAuthenticated == true )
            {

                string uri = FixADURI( userinfo.DomainInfo.ADHost , userinfo.DomainInfo.ContainerPath );

                if ( string.IsNullOrWhiteSpace( uri ) )
                {
                    status.Message = status.Message = "AD Host is not allowed to be empty, kindly provide the AD Host";
                    return status;
                }

                bool isAllowWite = CheckWriteOermission( uri , userinfo.DomainInfo.BindingUserName , userinfo.DomainInfo.BindingUserPassword );

                try
                {
                    UserPrincipal usr = FindADUser( userinfo.SamAccountName , userinfo.DomainInfo );

                    if ( usr != null )
                    {
                        if ( userinfo.LockUser == true )
                        {
                            if ( usr.IsAccountLockedOut() )
                                usr.UnlockAccount();
                        }
                        else
                            usr.Enabled = false;

                        usr.Save();

                        status.Message = " Transaction has been executed successfully.";
                        status.IsSuccessful = true;

                        return status;
                    }
                    else
                    {
                        status.Message = "User doesn't exist";
                        return status;
                    }
                }
                catch ( Exception ex )
                {
                    status.Message = @"Failed to create PrincipalContext: " + ex;
                    return status;
                }
            }
            else
            {
                status.Message = "Kindly authenticate first";
                return status;
            }
        }

        /// <summary>
        /// Rename Active Directory Object {User or Group}
        /// </summary>
        /// <param name="objectInfo"></param>
        /// <returns></returns>
        public ResponseMessage RenameObject(RequestObjectRename objectInfo) 
        {
            ResponseMessage status = new ResponseMessage();

            status.IsSuccessful = false;
            status.Message = string.Empty;

            Session stat = ValidateSession( objectInfo.DomainInfo.SessionKey );

            if ( stat.IsAuthenticated == true )
            {

                string uri = FixADURI( objectInfo.DomainInfo.ADHost , objectInfo.DomainInfo.ContainerPath );

                if ( string.IsNullOrWhiteSpace( uri ) )
                {
                    status.Message = status.Message = "AD Host is not allowed to be empty, kindly provide the AD Host";
                    return status;
                }

                bool isAllowWite = CheckWriteOermission( uri , objectInfo.DomainInfo.BindingUserName , objectInfo.DomainInfo.BindingUserPassword );

                try
                {
                    switch ( objectInfo.TypeOfObject )
                    {
                        case ObjectType.USER:

                        UserPrincipal usr = FindADUser( objectInfo.ObjectName , objectInfo.DomainInfo );

                        if ( usr != null )
                        {
                            var userObj = (DirectoryEntry)usr.GetUnderlyingObject();
                            userObj.Rename( "CN=" + objectInfo.NewObjectName );
                            userObj.CommitChanges();

                            status.Message = " User has been renamed successfuly";
                            status.IsSuccessful = true;
                        }
                        else
                        {
                            status.Message = " No such user exists";
                        }

                        break;

                        case ObjectType.GROUP:
                        GroupPrincipal group = FindADGroup( objectInfo.ObjectName , objectInfo.DomainInfo );

                        if ( group != null )
                        {
                            var groupObj = (DirectoryEntry)group.GetUnderlyingObject();
                            groupObj.Rename( "CN=" + objectInfo.NewObjectName );
                            groupObj.CommitChanges();

                            status.Message = " Group Has been renamed successfuly";
                            status.IsSuccessful = true;

                        }
                        else
                        {
                            status.Message = " No such Group exists";

                        }

                        break;

                        case ObjectType.COMPUTER:
                        ComputerPrincipal computer = FindADComputer( objectInfo.ObjectName , objectInfo.DomainInfo );

                        if ( computer != null )
                        {
                            var computerObj = (DirectoryEntry)computer.GetUnderlyingObject();
                            computerObj.Rename( "CN=" + objectInfo.NewObjectName );
                            computerObj.CommitChanges();

                            status.Message = " Group Has been renamed successfuly";
                            status.IsSuccessful = true;

                        }
                        else
                        {
                            status.Message = " No such Group exists";

                        }

                        break;
                        default:
                        status.Message = " Transaction type is not applicable";
                        status.IsSuccessful = false;

                        break;

                    }
                    return status;
                }
                catch ( Exception ex )
                {
                    status.Message = "An error occurred please check with system admin " + ex;
                    return status;
                }
            }
            else
            {
                status.Message = "Kindly authenticate first";
                return status;
            }
        }

        /// <summary>
        /// Move AD Object from one place to another
        /// </summary>
        /// <param name="objectInfo">ADMoveObjectRequest</param>
        /// <returns>ResponseMessage</returns>
        public ResponseMessage MoveObject( RequestMoveObject objectInfo ) 
        {
            ResponseMessage status = new ResponseMessage();

            status.IsSuccessful = false;
            status.Message = string.Empty;

            Session stat = ValidateSession( objectInfo.DomainInfo.SessionKey );

            if ( stat.IsAuthenticated == true )
            {

                string uriFrom = FixADURI( objectInfo.DomainInfo.ADHost , objectInfo.DomainInfo.ContainerPath );
                string uriTo = FixADURI( objectInfo.DomainInfo.ADHost , objectInfo.NewParentObjectPath );

                if ( string.IsNullOrWhiteSpace( uriFrom ) )
                {
                    status.Message = status.Message = "AD Host is not allowed to be empty, kindly provide the AD Host";
                    return status;
                }

                bool isAllowWiteFrom = CheckWriteOermission( uriFrom , objectInfo.DomainInfo.BindingUserName , objectInfo.DomainInfo.BindingUserPassword );
                bool isAllowWiteTo = CheckWriteOermission( uriTo , objectInfo.DomainInfo.BindingUserName , objectInfo.DomainInfo.BindingUserPassword );

                try
                {

                    DirectoryEntry objTo = new DirectoryEntry( uriTo.Replace( @"GC://" , @"LDAP://" ) , objectInfo.DomainInfo.BindingUserName , objectInfo.DomainInfo.BindingUserPassword );

                    switch ( objectInfo.TypeOfObject )
                    {
                        case ObjectType.USER:

                        UserPrincipal usrFrom = FindADUser( objectInfo.ObjectName , objectInfo.DomainInfo );

                        if ( usrFrom != null )
                        {
                            var userObj = (DirectoryEntry)usrFrom.GetUnderlyingObject();

                            userObj.MoveTo( objTo );
                            userObj.CommitChanges();

                            userObj.Close();
                            objTo.Close();

                            status.Message = " User has been Moved successfuly";
                            status.IsSuccessful = true;
                        }
                        else
                        {
                            status.Message = " No such user exists";
                        }

                        break;

                        case ObjectType.GROUP:

                        GroupPrincipal groupFrom = FindADGroup( objectInfo.ObjectName , objectInfo.DomainInfo );

                        if ( groupFrom != null )
                        {
                            var groupObj = (DirectoryEntry)groupFrom.GetUnderlyingObject();
                            groupObj.MoveTo( objTo );
                            groupObj.CommitChanges();

                            groupObj.Close();
                            objTo.Close();

                            status.Message = " Group Has been Moved successfuly";
                            status.IsSuccessful = true;
                        }
                        else
                        {
                            status.Message = " No such Group exists";
                        }

                        break;

                        case ObjectType.COMPUTER:

                        ComputerPrincipal computerFrom = FindADComputer( objectInfo.ObjectName , objectInfo.DomainInfo );

                        if ( computerFrom != null )
                        {
                            var computerObj = (DirectoryEntry)computerFrom.GetUnderlyingObject();
                            computerObj.MoveTo( objTo );
                            computerObj.CommitChanges();

                            computerObj.Close();
                            objTo.Close();

                            status.Message = " Computer Has been Moved successfuly";
                            status.IsSuccessful = true;
                        }
                        else
                        {
                            status.Message = " No such Group exists";
                        }

                        break;

                        default:
                        status.Message = " Transaction type is not applicable";
                        status.IsSuccessful = false;
                        break;
                    }
                    return status;

                }
                catch ( Exception ex )
                {
                    status.Message = "An error occurred please check with system admin " + ex;
                    return status;
                }
            }
            else 
            {
                status.Message = "Kindly authenticate first";
                return status;
            }
        }

        /// <summary>
        /// Validate if user exists in Active directory
        /// </summary>
        /// <param name="username">Identity to match</param>
        /// <param name="domainInfo">Acive Directory Domain Info</param>
        /// <returns></returns>
        private UserPrincipal FindADUser( string username , DomainRequest domainInfo ) 
        {
           
            Session stat = ValidateSession( domainInfo.SessionKey );

            if ( stat.IsAuthenticated == true )
            {
                PrincipalContext principalContext = null;

                try
                {
                    principalContext = new PrincipalContext( ContextType.Domain , domainInfo.DomainName ,  domainInfo.BindingUserName , domainInfo.BindingUserPassword );
                }
                catch ( Exception ex )
                {
                    throw ex;
                }

                return UserPrincipal.FindByIdentity( principalContext , username );
            }
            else 
            {
                throw new Exception( "Kindly authenticate first" );
            }
            
        }

        /// <summary>
        /// Find if Group exists in Active Directory
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="domainInfo"></param>
        /// <returns></returns>
        private GroupPrincipal FindADGroup( string groupName , DomainRequest domainInfo ) 
        {
            Session stat = ValidateSession( domainInfo.SessionKey );

            PrincipalContext principalContext = null;

            if ( stat.IsAuthenticated == true ) 
            {
                try
                {
                    principalContext = new PrincipalContext( ContextType.Domain , domainInfo.DomainName,  domainInfo.BindingUserName , domainInfo.BindingUserPassword );
                }
                 catch ( Exception ex )
                 {
                     throw ex;
                 }

                return GroupPrincipal.FindByIdentity( principalContext , groupName );
            }
            else 
            {
                throw new Exception( "Kindly authenticate first" );
            }
        }

        /// <summary>
        /// Find Computer Object
        /// </summary>
        /// <param name="computerName">computerName</param>
        /// <param name="domainInfo">DomainRequest</param>
        /// <returns></returns>
        private ComputerPrincipal FindADComputer(string computerName, DomainRequest domainInfo)
        {
            Session stat = ValidateSession( domainInfo.SessionKey );

            PrincipalContext principalContext = null;

            if ( stat.IsAuthenticated == true ) 
            {
                try
                {
                    principalContext = new PrincipalContext( ContextType.Domain , domainInfo.DomainName,  domainInfo.BindingUserName , domainInfo.BindingUserPassword );
                }
                 catch ( Exception ex )
                 {
                     throw ex;
                 }

                return ComputerPrincipal.FindByIdentity( principalContext , computerName );
            }
            else 
            {
                throw new Exception( "Kindly authenticate first" );
            }
        }

        /// <summary>
        /// Check if the user has a permission on a specific container based on his credintials 
        /// </summary>
        /// <param name="path">Conianter path</param>
        /// <param name="username">AD user</param>
        /// <param name="password">AD Password</param>
        /// <returns></returns>
        private bool CheckWriteOermission( string path , string username , string password )
        {
            using ( DirectoryEntry de = new DirectoryEntry( path , username , password ) )
            {
                de.RefreshCache( new string[] { "allowedAttributesEffective" } );
                return de.Properties[ "allowedAttributesEffective" ].Value != null;
            }
        }

        /// <summary>
        /// Generate Password based on Group Policy Password Complexity
        /// </summary>
        /// <param name="domainURI">LDAP Context eg. "LDAP://X.X.X.X/DC=X,DC=Y"</param>
        /// <param name="bindingUserName">Domain username</param>
        /// <param name="bindingUserPassword">Domain user password</param>
        /// <returns>Generated Password</returns>
        private string GenerateADPassword( string domainURI , string bindingUserName , string bindingUserPassword )
        {
            DirectoryEntry child = new DirectoryEntry( domainURI.Replace("GC:","LDAP:") , bindingUserName , bindingUserPassword );
            
            int minPwdLength = (int)child.Properties[ "minPwdLength" ].Value;
            int pwdProperties = (int)child.Properties[ "pwdProperties" ].Value;

            return Membership.GeneratePassword( minPwdLength , pwdProperties );
        }

        private void FillUserExtraAttributes( ref DirectoryEntry de , RequestUserCreate userinfo ) 
        {
            try 
            {
                if ( !string.IsNullOrWhiteSpace( userinfo.Title ) )
                    de.Properties[ "title" ].Value = userinfo.Title;

                if ( !string.IsNullOrWhiteSpace( userinfo.City ) )
                    de.Properties[ "l" ].Value = userinfo.City;

                if ( !string.IsNullOrWhiteSpace( userinfo.Country ) )
                    de.Properties[ "c" ].Value = userinfo.Country;

                if ( !string.IsNullOrWhiteSpace( userinfo.PostalCode ) )
                    de.Properties[ "postalCode" ].Value = userinfo.PostalCode;

                if ( !string.IsNullOrWhiteSpace( userinfo.PostOfficeBox ) )
                    de.Properties[ "postOfficeBox" ].Value = userinfo.PostOfficeBox;

                if ( !string.IsNullOrWhiteSpace( userinfo.Address ) )
                    de.Properties[ "streetAddress" ].Value = userinfo.Address;

                if ( !string.IsNullOrWhiteSpace( userinfo.Department ) )
                    de.Properties[ "department" ].Value = userinfo.Department;

                if ( !string.IsNullOrWhiteSpace( userinfo.PhysicalDeliveryOffice ) )
                    de.Properties[ "physicalDeliveryOfficeName" ].Value = userinfo.PhysicalDeliveryOffice;

                if ( !string.IsNullOrWhiteSpace( userinfo.Company ) )
                    de.Properties[ "company" ].Value = userinfo.Company;

                if ( !string.IsNullOrWhiteSpace( userinfo.PhoneExtention ) )
                    de.Properties[ "extensionAttribute1" ].Value = userinfo.PhoneExtention;

                if ( !string.IsNullOrWhiteSpace( userinfo.PhoneIpAccessCode ) )
                    de.Properties[ "extensionAttribute2" ].Value = userinfo.PhoneIpAccessCode;
            }
            catch ( Exception ex ) 
            {
                throw ex;
            }
        }

        private string FixADURI( string adHost , string containerPath ) 
        {
            string uri = string.Empty;

            if ( !string.IsNullOrWhiteSpace( adHost ) )
            {
                uri += adHost;
            }
            else
            {
                return null;
            }

            if ( !string.IsNullOrWhiteSpace( containerPath ) )
            {
                uri += @"/" + containerPath;
            }

            return uri;
        }
    
    }

}
