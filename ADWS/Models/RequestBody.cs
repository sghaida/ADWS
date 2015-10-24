using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ADWS.Models
{
    [DataContract]
    public class RequestBody
    {
        [DataMember( Name = "request" )]
        public string Body { get; set; }

        [DataMember( Name = "sessionkey" )]
        public string SessionKey { get; set; }
    }
}