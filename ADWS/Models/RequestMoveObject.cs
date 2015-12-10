
using System;
>>>>>>> 1f7ed621163aec8807a6f7352dc4569d1ce68bd3
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ADWS.Models
{
    [DataContract]
    public class RequestMoveObject
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

