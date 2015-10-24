using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ADWS.Models
{
    [DataContract]
    public class ResponseMessage
    {
        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public bool IsSuccessful { get; set; }
    }
}