using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ADWS.Models
{

    [DataContract( Name = "ObjectType" )]
    public enum ObjectType
    {
        [EnumMember( Value = "USER" )]
        USER = 1 ,
        [EnumMember( Value = "GROUP" )]
        GROUP = 2,
        [EnumMember( Value = "COMPUTER" )]
        COMPUTER = 3
    };
}
