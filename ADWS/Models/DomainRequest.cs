using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ADWS.Models
{
    /// <summary>
    /// This Model represents the domain Request object which will be used to query active directory 
    /// </summary>
    [DataContract]
    public class DomainRequest
    {
        [DataMember]
        public string ADHost { get; set; }

        [DataMember]
        public string DomainName { get; set; }

        [DataMember]
        public string ContainerPath { get; set; }

        [DataMember]
        public string BindingUserName { get; set; }

        [DataMember]
        public string BindingUserPassword { get; set; }

        [DataMember]
        public string SessionKey { get; set; }
    }
}