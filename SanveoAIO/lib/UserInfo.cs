using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SanveoAIO
{
    public class UserInfo
    {
        public int Admin_Id { get; set; }
        public int U_Id { get; set; }
        public int G_Id { get; set; }
        public string UserGroupName { get; set; }
        public int Comp_ID { get; set; }
        public int C_Id { get; set; }
        public int L_Id { get; set; }
        public string validity { get; set; }
        public string Comp_Type { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string EmailId { get; set; }
        public bool Active { get; set; }
        public string CompanyName { get; set; }
        public string LocationName { get; set; }
        public DateTime today = DateTime.Now;
        public string Designation { get; set; }
        public string Profile_ID { get; set; }
        public int Trade_ID { get; set; }

        public UserInfo()
        {
            this.U_Id = 0;
        }
    }
}
