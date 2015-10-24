using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ADWS.Models
{
    [DataContract]
    public class AuthDataRequest
    {
        [DataMember( Name = "username" )]
        public string username { get; set; }

        [DataMember( Name = "password" )]
        public string password { get; set; }
    }
}