using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ADWS.Models
{
	[DataContract]
    public class RequestContactCreate
	{
        [DataMember]
        public string ContactName { get; set; }

        [DataMember]
        public string FirstName { get; set; }

        [DataMember]
        public string LastName { get; set; }

        [DataMember]
        public string TelephoneNumber { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string EmailAddress { get; set; }

        [DataMember]
        public DomainRequest DomainInfo { get; set; }

	}
}