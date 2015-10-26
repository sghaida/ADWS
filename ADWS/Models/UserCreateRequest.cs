using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ADWS.Models
{
    [DataContract]
    public class UserCreateRequest
    {
        [DataMember]
        public string FirstName { get; set; }

        [DataMember]
        public string LastName { get; set; }

        [DataMember]
        public string UserLogonName { get; set; }

        [DataMember]
        public string EmployeeID { get; set; }

        [DataMember]
        public string EmailAddress { get; set; }

        [DataMember]
        public string Telephone { get; set; }

        [DataMember]
        public string Address { get; set; }

        [DataMember]
        public string PostalCode { get; set; }


        [DataMember]
        public string PostOfficeBox { get; set; }

        [DataMember]
        public string PhysicalDeliveryOffice { get; set; }

        [DataMember]
        public string Country { get; set; }

        [DataMember]
        public string City { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string Department { get; set; }

        [DataMember]
        public string Company { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string PhoneExtention { get; set; }

        [DataMember]
        public string PhoneIpAccessCode { get; set; }

        [DataMember]
        public string Password { get; set; }

        [DataMember]
        public DomainRequest DomainInfo { get; set; }

    }

}