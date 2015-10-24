using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ADWS.Models
{
    [DataContract]
    public class DomainResponse
    {
        [DataMember( Name = "domain" )]
        public string DomainName { get; set; }

        [DataMember( Name = "dchost" )]
        public string DcIpAddress { get; set; }

        [DataMember( Name = "forest" )]
        public string Forest { get; set; }

        [DataMember( Name = "fqdn" )]
        public string FQDN { get; set; }

        [DataMember( Name = "adsitename" )]
        public string SiteName { get; set; }

        [DataMember( Name = "osversion" )]
        public string OS { get; set; }

        [DataMember( Name = "domianmode" )]
        public string DomainMode { get; set; }

        [DataMember( Name = "infrastructureroleownerhost" )]
        public string InfrastructureRoleOwnerHostName { get; set; }

        [DataMember( Name = "infrastructureroleownerip" )]
        public string InfrastructureRoleOwnerIPAddress { get; set; }
    }

}