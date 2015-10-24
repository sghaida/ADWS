using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ADWS.Models
{
    [DataContract]
    public class UserLockRequest
    {
        [DataMember]
        public string SamAccountName { get; set; }

        [DataMember]
        public bool LockUser { get; set; }

        [DataMember]
        public DomainRequest DomainInfo { get; set; }
    }
}