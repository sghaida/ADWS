<<<<<<< HEAD
﻿using System;
=======
using System;
>>>>>>> 1f7ed621163aec8807a6f7352dc4569d1ce68bd3
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ADWS.Models
{
    [DataContract]
    public class ADGroupRequest
    {
        [DataMember]
        public string GroupName { get; set; }

        [DataMember]
        public DomainRequest DomainInfo { get; set; }
    }
<<<<<<< HEAD
}
=======
}
>>>>>>> 1f7ed621163aec8807a6f7352dc4569d1ce68bd3
