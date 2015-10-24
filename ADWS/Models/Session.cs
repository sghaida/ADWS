using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ADWS.Models
{
    [DataContract]
    public class Session
    {
        [DataMember]
        public bool IsAuthenticated { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public string SessionKey { get; set; }
        
    }
}