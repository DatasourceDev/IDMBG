﻿using IDMBG.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IDMBG
{
    public class BaseDTO
    {

    }

    public class User
    {
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Password { get; set; }
        public string UserRole { get; set; }
    }

    public class Token
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
    public class Result
    {
        public bool result { get; set; }
        public string Message { get; set; }

    }
    public class Organization
    {
        public string ouname { get; set; }
        public string ou { get; set; }
        public string schemaname { get; set; }
        public string path { get; set; }

    }
    public class SearchDTO
    {
        public string text_search { get; set; }
        public string logstatus_search { get; set; }

        public IDMUserType? usertype_search { get; set; }
        public string log_type_search { get; set; }

        public Status? status_search { get; set; }
        public string dfrom { get; set; }
        public string dto { get; set; }
        public string import_option { get; set; }

        private int _pageno; // field
        public int pageno
        {
            get
            {
                if (_pageno == 0)
                    return 1;
                return _pageno;
            }
            set { _pageno = value; }
        }
        private int _pagelen; // field
        public int pagelen
        {
            get
            {
                if (_pagelen == 0)
                    return 1;
                return _pagelen;
            }
            set { _pagelen = value; }
        }

        public int itemcnt { get; set; }

        public ReturnCode? code { get; set; }
        public string msg { get; set; }

        private IQueryable<object> _lists;

        public IQueryable<object> lists
        {
            get
            {
                return _lists;
            }
            set
            {
                _lists = value;
            }
        }
    }
    public class SystemConf
    {
        public string Portal { get; set; }
        public string DefaultValue_emailDomain { get; set; }
        public string DefaultValue_emailDomainForStudent { get; set; }
        public string DefaultValue_userprincipalname { get; set; }
        public string DefaultValue_mailhost { get; set; }
        public string DefaultValue_mailhostForStudent { get; set; }
        public string DefaultValue_mailRoutingAddress { get; set; }
        public string DefaultValue_mailRoutingAddressForStudent { get; set; }
        public string DefaultValue_maildrop { get; set; }
        public string DefaultValue_maildropForStudent { get; set; }
        public string DefaultValue_homeDirectory { get; set; }
        public string DefaultValue_loginShell { get; set; }
        public string DefaultValue_nsaccountlock { get; set; }
        public string DefaultValue_nsaccountlockForTemporaryAccount { get; set; }
        public string DefaultValue_nsaccountlockForOneDayAccount { get; set; }
        public string DefaultValue_miWmprefCharset { get; set; }
        public string DefaultValue_miWmprefReplyOption { get; set; }
        public string DefaultValue_miWmprefTimezone { get; set; }
        public string DefaultValue_inetCOS { get; set; }
        public string DefaultValue_inetCOSForStudent { get; set; }
        public string DefaultValue_inetCOSForTemporaryAccount { get; set; }
        public string DefaultValue_SCE_Package { get; set; }
        public string DefaultValue_OU_Filter { get; set; }

        public string TableReceiveAccount_ServerName { get; set; }
        public string IPAddress { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int Port { get; set; }

    }

    public class script_temp
    {
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
        public string account_type { get; set; }
        public visual_fim_user visual_fim_user { get; set; }
    }
}
