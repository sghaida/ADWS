using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ADWS.Models
{
    [DataContract]
    public class RequestGroupCreate
    {
        [DataMember]
        public string GroupName { get; set; }

        [DataMember]
        public string DisplayName { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public bool IsSecurityGroup { get; set; }

        [DataMember]
        public GroupScope OGroupScope { get; set; }

        [DataMember]
        public DomainRequest DomainInfo { get; set; }
    }
}