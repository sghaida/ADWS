using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ADWS.Models
{
    [DataContract]
    public class ADUserRequest
    {
        [DataMember]
        public string SamAccountName { get; set; }

        [DataMember]
        public DomainRequest DomainInfo { get; set; }
    }
}