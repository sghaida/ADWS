using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ADWS.Models
{
    [DataContract]
    public class UserGroupRequest
    {
        [DataMember]
        public string SamAccountName { get; set; }

        [DataMember]
        public string GroupName { get; set; }

        [DataMember]
        public DomainRequest DomainInfo { get; set; }
    }
}