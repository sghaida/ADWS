using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ADWS.Models
{
    [DataContract]
    public class ADMoveObjectRequest
    {
        [DataMember]
        public string ObjectName { get; set; }

        [DataMember]
        public string NewParentObjectPath { get; set; }

        [DataMember]
        public ObjectType TypeOfObject { get; set; }

        [DataMember]
        public DomainRequest DomainInfo { get; set; }
    }
}