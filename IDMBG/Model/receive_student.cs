﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace IDMBG.Models
{
    public class receive_student
    {
        [Key]
        public Int64 id { get; set; }
        public string displayname { get; set; }

        public string login_name { get; set; }
        public string password_initial { get; set; }
        public string email_address { get; set; }
        public string server_name { get; set; }
        public string expire { get; set; }
        public string status_id { get; set; }
        public string org { get; set; }
        public DateTime? create_date { get; set; }
        public DateTime? receive_date { get; set; }
        public string manage_by { get; set; }
        public string ticket { get; set; }

    }
}
