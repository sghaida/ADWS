using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ADWS.Models
{
    [DataContract]
    public class UserInfoResponse
    {
        [DataMember]
        public string Title { set; get; }

        [DataMember]
        public string DisplayName { set; get; }

        [DataMember]
        public string FirstName { set; get; }

        [DataMember]
        public string LastName { set; get; }

        [DataMember]
        public string SamAccountName { set; get; }

        [DataMember]
        public string Upn { set; get; }

        [DataMember]
        public string EmailAddress { set; get; }

        [DataMember]
        public string EmployeeId { set; get; }

        [DataMember]
        public string Department { set; get; }

        [DataMember]
        public string BusinessPhone { get; set; }

        [DataMember]
        public string Telephone { get; set; }

        [DataMember]
        public string SipAccount { set; get; }

        [DataMember]
        public string PrimaryHomeServerDn { get; set; }

        [DataMember]
        public string PoolName { set; get; }

        [DataMember]
        public string PhysicalDeliveryOfficeName { set; get; }

        [DataMember]
        public string OtherTelphone { get; set; }

    }

}