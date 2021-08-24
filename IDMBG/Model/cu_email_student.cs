﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace IDMBG.Models
{
    public class cu_email_student
    {
        [Key]
        public Int64 email_id { get; set; }

        public string name_eng { get; set; }
        public string surname_eng { get; set; }
        public string email { get; set; }
        public DateTime? create_date { get; set; }
    }
}
