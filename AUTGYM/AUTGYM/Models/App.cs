using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AUTGYM.Models
{
    public class App
    {

        public class AppUser
        {
            public string UserID { get; set; }

            public string username { get; set; }
            public string Ticket { get; set; }

            public string Telephone { get; set; }

           
        }

        public class TimeTable
        {
            public string CourseName { get; set; }
            public string Date { get; set; }
            public string MemberJoint { get; set; }
            public string Status { get; set; }
          
        }
    }
}