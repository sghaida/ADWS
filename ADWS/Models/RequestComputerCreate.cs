using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ADWS.Models
{
    [DataContract]
    public class RequestComputerCreate : ADUserRequest
    {
        [DataMember]
        public string DisplayName { get; set; }

        [DataMember]
        public string Description { get; set; }
    }
}