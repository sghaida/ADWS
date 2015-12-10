using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ADWS.Models
{
    [DataContract]
    public class ADGroupRequest
    {
        [DataMember]
        public string GroupName { get; set; }

        [DataMember]
        public DomainRequest DomainInfo { get; set; }
    }
}