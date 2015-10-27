using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ADWS.Models
{
    [DataContract]
    public class RequestObjectRename
    {
        [DataMember]
        public string ObjectName { get; set; }

        [DataMember]
        public string NewObjectName { get; set; }

        [DataMember]
        public ObjectType TypeOfObject { get; set; }

        [DataMember]
        public DomainRequest DomainInfo { get; set; }
    }
}