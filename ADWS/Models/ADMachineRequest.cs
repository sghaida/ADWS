using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ADWS.Models
{
    [DataContract]
    public class ADMachineRequest : ADUserRequest
    {
        [DataMember]
        public bool IsEnabled { get; set; }
    }
}