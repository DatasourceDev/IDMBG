using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using IDMBG.DAL;
using IDMBG.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.IO;
using IDMBG.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Renci.SshNet;
using System.Threading;
using IDMBG.Models;
using System.Collections;
using System.Reflection;

namespace IDMBG
{
    class Program
    {
        /*
         * ขอที่วางไฟล์
         */

        static void Main(string[] args)
        {
            if (!System.IO.Directory.Exists("C:\\inetpub\\wwwroot\\bg\\new_user"))
                System.IO.Directory.CreateDirectory("C:\\inetpub\\wwwroot\\bg\\new_user");

            import_staff_from_file();
            create_script();

            Console.WriteLine("Finish");
            //lock_expire_account();
            Thread.Sleep(100000);
        }
        public static void import_staff_from_file()
        {
            var collection = new ServiceCollection();
            collection.AddScoped<IUserProvider, AdUserProvider>();
            collection.AddScoped<ILDAPUserProvider, LDAPUserProvider>();

            var serviceProvider = collection.BuildServiceProvider();

            var _provider = serviceProvider.GetService<IUserProvider>();
            var _providerldap = serviceProvider.GetService<ILDAPUserProvider>();

            var _conf = new SystemConf();
            _conf.Portal = ConfigurationManager.AppSettings["Portal"];
            _conf.DefaultValue_emailDomain = ConfigurationManager.AppSettings["DefaultValue_emailDomain"]; ;
            _conf.DefaultValue_emailDomainForStudent = ConfigurationManager.AppSettings["DefaultValue_emailDomainForStudent"];
            _conf.DefaultValue_userprincipalname = ConfigurationManager.AppSettings["DefaultValue_userprincipalname"];
            _conf.DefaultValue_mailhost = ConfigurationManager.AppSettings["DefaultValue_mailhost"];
            _conf.DefaultValue_mailhostForStudent = ConfigurationManager.AppSettings["DefaultValue_mailhostForStudent"];
            _conf.DefaultValue_mailRoutingAddress = ConfigurationManager.AppSettings["DefaultValue_mailRoutingAddress"];
            _conf.DefaultValue_mailRoutingAddressForStudent = ConfigurationManager.AppSettings["DefaultValue_mailRoutingAddressForStudent"];
            _conf.DefaultValue_maildrop = ConfigurationManager.AppSettings["DefaultValue_maildrop"];
            _conf.DefaultValue_maildropForStudent = ConfigurationManager.AppSettings["DefaultValue_maildropForStudent"];
            _conf.DefaultValue_homeDirectory = ConfigurationManager.AppSettings["DefaultValue_homeDirectory"];
            _conf.DefaultValue_loginShell = ConfigurationManager.AppSettings["DefaultValue_loginShell"];
            _conf.DefaultValue_nsaccountlock = ConfigurationManager.AppSettings["DefaultValue_nsaccountlock"];
            _conf.DefaultValue_nsaccountlockForTemporaryAccount = ConfigurationManager.AppSettings["DefaultValue_nsaccountlockForTemporaryAccount"];
            _conf.DefaultValue_nsaccountlockForOneDayAccount = ConfigurationManager.AppSettings["DefaultValue_nsaccountlockForOneDayAccount"];
            _conf.DefaultValue_miWmprefCharset = ConfigurationManager.AppSettings["DefaultValue_miWmprefCharset"];
            _conf.DefaultValue_miWmprefReplyOption = ConfigurationManager.AppSettings["DefaultValue_miWmprefReplyOption"];
            _conf.DefaultValue_miWmprefTimezone = ConfigurationManager.AppSettings["DefaultValue_miWmprefTimezone"];
            _conf.DefaultValue_inetCOS = ConfigurationManager.AppSettings["DefaultValue_inetCOS"];
            _conf.DefaultValue_inetCOSForStudent = ConfigurationManager.AppSettings["DefaultValue_inetCOSForStudent"];
            _conf.DefaultValue_inetCOSForTemporaryAccount = ConfigurationManager.AppSettings["DefaultValue_inetCOSForTemporaryAccount"];
            _conf.DefaultValue_SCE_Package = ConfigurationManager.AppSettings["DefaultValue_SCE_Package"];
            _conf.DefaultValue_OU_Filter = ConfigurationManager.AppSettings["DefaultValue_OU_Filter"];
            _conf.TableReceiveAccount_ServerName = ConfigurationManager.AppSettings["TableReceiveAccount_ServerName"];
            _conf.IPAddress = ConfigurationManager.AppSettings["IPAddress"];
            _conf.Username = ConfigurationManager.AppSettings["Username"];
            _conf.Password = ConfigurationManager.AppSettings["Password"];
            _conf.Port = NumUtil.ParseInteger(ConfigurationManager.AppSettings["Port"]);

            using (var _context = new SpuContext())
            {
                using (var client = new SftpClient(_conf.IPAddress, _conf.Port, _conf.Username, _conf.Password))
                {
                    Console.WriteLine("Start create Account from File :" + DateUtil.Now());
                    var date = DateUtil.ToInternalDate3(DateUtil.Now());
                    try
                    {
                        client.Connect();
                        var files = client.ListDirectory("\\CUEmployee");
                        foreach (var file in files)
                        {
                            if (file.Name.Contains(".txt"))
                            {
                                var remoteFilePath = "\\CUEmployee" + "\\" + file.Name;
                                //var s = File.OpenWrite(localFilePath);
                                //client.DownloadFile(remoteFilePath, s);

                                var row = 1;
                                var lines = client.ReadAllLines(remoteFilePath);
                                ArrayList exsits = new ArrayList();
                                foreach (var input in lines)
                                {
                                    var j = 0;
                                    var remark = new StringBuilder();
                                    var imp = new import();
                                    imp.ImportVerify = true;
                                    imp.ImportRow = row;
                                    imp.basic_uid = "";
                                    imp.basic_givenname = "";
                                    imp.basic_sn = "";
                                    imp.cu_pplid = "";
                                    imp.LockStaus = "";
                                    imp.import_Type = ImportType.create;
                                    try
                                    {
                                        var columnNameList = input.Split('\t');
                                        if (columnNameList.Length == 11)
                                        {
                                            imp.import_create_option = ImportCreateOption.staff_hr;
                                            imp.system_idm_user_types = IDMUserType.staff;
                                            imp.cu_jobcode = columnNameList[j]; j++;
                                            imp.cu_thcn = columnNameList[j]; j++;
                                            imp.cu_thsn = columnNameList[j]; j++;
                                            imp.basic_givenname = columnNameList[j]; j++;
                                            imp.basic_sn = columnNameList[j]; j++;
                                            imp.structure_1 = columnNameList[j]; j++;
                                            imp.structure_2 = columnNameList[j]; j++;
                                            imp.status = columnNameList[j]; j++;
                                            imp.cu_pplid = columnNameList[j]; j++;
                                            imp.basic_mobile = columnNameList[j]; j++;
                                            imp.basic_telephonenumber = columnNameList[j]; j++;

                                        }
                                        else if (columnNameList.Length == 12)
                                        {
                                            imp.import_create_option = ImportCreateOption.staff_other;
                                            imp.system_idm_user_types = IDMUserType.staff;
                                            imp.cu_jobcode = columnNameList[j]; j++;
                                            imp.cu_thcn = columnNameList[j]; j++;
                                            imp.cu_thsn = columnNameList[j]; j++;
                                            imp.basic_givenname = columnNameList[j]; j++;
                                            imp.basic_sn = columnNameList[j]; j++;
                                            imp.structure_1 = columnNameList[j]; j++;
                                            imp.structure_2 = columnNameList[j]; j++;
                                            imp.status = columnNameList[j]; j++;
                                            imp.cu_pplid = columnNameList[j]; j++;
                                            imp.cu_CUexpire = columnNameList[j]; j++;
                                            imp.basic_mobile = columnNameList[j]; j++;
                                            imp.basic_telephonenumber = columnNameList[j]; j++;
                                        }
                                        imp.ImportRemark = remark.ToString();
                                        if (string.IsNullOrEmpty(imp.basic_givenname))
                                        {
                                            imp.ImportVerify = false;
                                            remark.AppendLine("row : " + row + ". First Name cannot be null.");
                                            Console.WriteLine("row : " + row + ". First Name cannot be null.");
                                            continue;
                                        }
                                        if (string.IsNullOrEmpty(imp.basic_sn))
                                        {
                                            imp.ImportVerify = false;
                                            remark.AppendLine("row : " + row + ". Last Name cannot be null.");
                                            Console.WriteLine("row : " + row + ". Last Name cannot be null.");
                                            continue;
                                        }

                                        if (string.IsNullOrEmpty(imp.cu_pplid))
                                        {
                                            imp.ImportVerify = false;
                                            remark.AppendLine("row : " + row + ". Citizen ID cannot be null.");
                                            Console.WriteLine("row : " + row + ". Citizen ID cannot be null.");
                                            continue;
                                        }
                                        else
                                        {
                                            if (imp.import_create_option == ImportCreateOption.staff_other || imp.import_create_option == ImportCreateOption.staff_hr)
                                            {
                                                var fim_user = _context.table_visual_fim_user
                                                    .Where(w => w.cu_pplid.ToLower() == imp.cu_pplid.ToLower() & (w.system_idm_user_type == IDMUserType.staff | w.system_idm_user_type == IDMUserType.affiliate | w.system_idm_user_type == IDMUserType.outsider | w.system_idm_user_type == IDMUserType.temporary))
                                                    .FirstOrDefault();
                                                if (fim_user != null)
                                                {
                                                    imp.ImportVerify = false;
                                                    remark.AppendLine("row : " + row + ". Citizen ID already exists.");
                                                    Console.WriteLine("row : " + row + ". Citizen ID already exists.");
                                                    writelog(LogType.log_create_account_with_file, LogStatus.failed, IDMSource.VisualFim, imp.cu_pplid, "Citizen ID " + imp.cu_pplid + " ซ้ำในระบบ");
                                                    continue;
                                                }
                                            }

                                            //var fim_user = _context.table_visual_fim_user.Where(w => w.cu_pplid.ToLower() == imp.cu_pplid.ToLower()).FirstOrDefault();
                                            //if (fim_user != null)
                                            //{
                                            //    imp.ImportVerify = false;
                                            //    remark.AppendLine("row : " + row + ". Citizen ID already exists.");
                                            //    Console.WriteLine("row : " + row + ". Citizen ID already exists.");
                                            //    writelog(LogType.log_create_account_with_file, LogStatus.failed, IDMSource.VisualFim, imp.cu_pplid, "Citizen ID "+ imp.cu_pplid+" ซ้ำในระบบ");
                                            //    continue;
                                            //}

                                            imp.cu_pplid = imp.cu_pplid.Replace("-", "");
                                        }

                                        /* save to db*/
                                        var model = new visual_fim_user();
                                        model.cu_jobcode = imp.cu_jobcode;
                                        model.basic_givenname = imp.basic_givenname;
                                        model.basic_sn = imp.basic_sn;
                                        model.cu_thcn = imp.cu_thcn;
                                        model.cu_thsn = imp.cu_thsn;
                                        model.cu_pplid = imp.cu_pplid;
                                        model.cu_CUexpire = imp.cu_CUexpire;
                                        model.basic_telephonenumber = imp.basic_telephonenumber;
                                        model.basic_mobile = imp.basic_mobile;

                                        if (string.IsNullOrEmpty(imp.ImportRemark))
                                            imp.ImportRemark = "";
                                        faculty faculty = null;

                                        if (imp.import_create_option == ImportCreateOption.staff_hr | imp.import_create_option == ImportCreateOption.staff_other)
                                        {
                                            faculty = _context.table_cu_faculty
                                                .Where(w => w.faculty_name.ToLower() == imp.structure_1.ToLower() | w.faculty_name.ToLower() == imp.structure_2.ToLower()
                                                | w.faculty_shot_name.ToLower() == imp.structure_1.ToLower() | w.faculty_shot_name.ToLower() == imp.structure_2.ToLower()
                                                ).FirstOrDefault();
                                            if (faculty == null)
                                            {
                                                var subfaculty = _context.table_cu_faculty_level2
                                                    .Where(w => w.sub_office_name.ToLower() == imp.structure_1.ToLower() | w.sub_office_name.ToLower() == imp.structure_2.ToLower()
                                                    | w.sub_office_shot_name.ToLower() == imp.structure_1.ToLower() | w.sub_office_shot_name.ToLower() == imp.structure_2.ToLower()
                                                    ).FirstOrDefault();
                                                if (subfaculty != null)
                                                {
                                                    faculty = _context.table_cu_faculty.Where(w => w.faculty_id == subfaculty.faculty_id).FirstOrDefault();
                                                }
                                            }
                                        }
                                        if (faculty != null)
                                        {
                                            model.system_faculty_id = (int)faculty.faculty_id;
                                            var distinguish_name = "";
                                            if (imp.import_create_option == ImportCreateOption.student | imp.import_create_option == ImportCreateOption.student_sasin | imp.import_create_option == ImportCreateOption.student_ppc | imp.import_create_option == ImportCreateOption.student_other)
                                            {
                                                distinguish_name = faculty.faculty_distinguish_name_student;
                                            }
                                            else if (imp.import_create_option == ImportCreateOption.staff_hr | imp.import_create_option == ImportCreateOption.staff_other)
                                            {
                                                if (imp.status.ToLower().Trim() == "student".ToLower())
                                                    distinguish_name = faculty.faculty_distinguish_name_student;
                                                else if (imp.status.ToLower().Trim() == "staff".ToLower() || imp.status.ToLower().Trim() == "พนักงานปกติ".ToLower())
                                                    distinguish_name = faculty.faculty_distinguish_name_staff;
                                                else if (imp.status.ToLower().Trim() == "outsider".ToLower())
                                                    distinguish_name = faculty.faculty_distinguish_name_outsider;
                                                else if (imp.status.ToLower().Trim() == "affiliate".ToLower())
                                                    distinguish_name = faculty.faculty_distinguish_name_affiliate;
                                            }
                                            if (!string.IsNullOrEmpty(distinguish_name))
                                            {
                                                var ous = distinguish_name.Split(',');
                                                if (ous.Length > 3)
                                                {
                                                    for (var i = ous.Length - 1; i >= 0; i--)
                                                    {
                                                        var ou = ous[i];
                                                        if (i < 3)
                                                        {
                                                            if (ous.Length == 6)
                                                            {
                                                                if (i == 2)
                                                                    model.system_ou_lvl1 = ou;
                                                                else if (i == 1)
                                                                    model.system_ou_lvl2 = ou;
                                                                else if (i == 0)
                                                                    model.system_ou_lvl3 = ou;
                                                            }
                                                            else if (ous.Length == 5)
                                                            {
                                                                if (i == 1)
                                                                    model.system_ou_lvl1 = ou;
                                                                else if (i == 0)
                                                                    model.system_ou_lvl2 = ou;
                                                            }
                                                            else if (ous.Length == 4)
                                                            {
                                                                if (i == 0)
                                                                    model.system_ou_lvl1 = ou;
                                                            }
                                                        }
                                                    }
                                                }

                                                try
                                                {

                                                    genNewAccount(_context, _conf, _providerldap, model, exsits);
                                                    _context.SaveChanges();
                                                    //continue;
                                                    var result_ldap = _providerldap.CreateUser(model, _context);
                                                    model.ldap_created = result_ldap.result;
                                                    if (result_ldap.result == true)
                                                    {
                                                        writelog(LogType.log_create_account_with_file, LogStatus.successfully, IDMSource.LDAP, model.basic_uid);
                                                        Console.WriteLine("Account " + model.basic_uid + " has been created on LDAP successfully.");
                                                    }
                                                    else
                                                    {
                                                        imp.ImportVerify = false;
                                                        imp.ImportRemark += writelog(LogType.log_create_account_with_file, LogStatus.failed, IDMSource.LDAP, model.basic_uid, log_exception: result_ldap.Message) + Environment.NewLine;
                                                        Console.WriteLine("Account " + model.basic_uid + " has been failed on LDAP.");
                                                    }

                                                    var result_ad = _provider.CreateUser(model, _context);
                                                    model.ad_created = result_ad.result;
                                                    if (result_ad.result == true)
                                                    {
                                                        writelog(LogType.log_create_account_with_file, LogStatus.successfully, IDMSource.AD, model.basic_uid);
                                                        Console.WriteLine("Account " + model.basic_uid + " has been created on AD successfully." + result_ldap.Message);
                                                    }
                                                    else
                                                    {
                                                        imp.ImportVerify = false;
                                                        imp.ImportRemark += writelog(LogType.log_create_account_with_file, LogStatus.failed, IDMSource.AD, model.basic_uid, log_exception: result_ad.Message) + Environment.NewLine;
                                                        Console.WriteLine("Account " + model.basic_uid + " has been failed on AD." + result_ad.Message);
                                                    }

                                                    _context.SaveChanges();
                                                    writelog(LogType.log_create_account_with_file, LogStatus.successfully, IDMSource.VisualFim, model.basic_uid);
                                                    Console.WriteLine("Account " + model.basic_uid + " has been created on IDM successfully.");

                                                }
                                                catch (Exception ex)
                                                {
                                                    imp.ImportRemark += writelog(LogType.log_create_account_with_file, LogStatus.failed, IDMSource.VisualFim, model.basic_uid, log_exception: ex.Message) + Environment.NewLine;
                                                    Console.WriteLine("Account " + model.basic_uid + " cannot create :" + ex.Message);
                                                    writelog(LogType.log_create_account_with_file, LogStatus.successfully, IDMSource.VisualFim, model.basic_uid, ex.Message);
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        remark.AppendLine(ex.Message);
                                        imp.ImportVerify = false;
                                        Console.WriteLine("Error :" + ex.Message);
                                    }
                                    row++;
                                }
                                var backupFilePath = "\\CUEmployee" + "\\old\\" + file.Name;
                                if (!file.Name.Contains(date))
                                    backupFilePath = "\\CUEmployee" + "\\old\\" + date + "_" + file.Name;

                                file.MoveTo(backupFilePath);
                            }


                        }
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                    }
                    finally
                    {
                        client.Disconnect();
                    }
                    Console.WriteLine("End create Account from File :" + DateUtil.Now());
                }
            }
        }
        public static void create_script()
        {
            Console.WriteLine("Start create new user files :" + DateUtil.Now());
            var dfrom = DateUtil.Now();
            var dto = DateUtil.Now();
            var date = DateUtil.ToInternalDate3(dfrom);
            //dfrom = DateUtil.ToDate(31,5,2021).Value;
            //dto = DateUtil.ToDate(31,5,2021).Value;
            var script_temps = new List<script_temp>();
            using (var _context = new SpuContext())
            {
                var staffs = _context.table_receive_staff.Where(w => 1 == 1);
                staffs = staffs.Where(w => w.create_date.Value.Date >= dfrom.Date);
                staffs = staffs.Where(w => w.create_date.Value.Date <= dto.Date);

                foreach (var item in staffs)
                {
                    var fim_user = _context.table_visual_fim_user.Where(w => w.basic_uid.ToLower() == item.login_name.ToLower()).FirstOrDefault();
                    if (fim_user != null)
                    {
                        var script_temp = new script_temp();
                        script_temp.id = item.id;
                        script_temp.displayname = item.displayname;
                        script_temp.login_name = item.login_name;
                        script_temp.password_initial = item.password_initial;
                        script_temp.email_address = item.email_address;
                        script_temp.server_name = item.server_name;
                        script_temp.expire = item.expire;
                        script_temp.status_id = item.status_id;
                        script_temp.org = item.org;
                        script_temp.create_date = item.create_date;
                        script_temp.receive_date = item.receive_date;
                        script_temp.manage_by = item.manage_by;
                        script_temp.ticket = item.ticket;
                        script_temp.visual_fim_user = fim_user;
                        script_temp.account_type = "staff";
                        script_temps.Add(script_temp);

                        Console.WriteLine("New User: " + item.displayname);
                    }
                }

                //var students = _context.table_receive_student.Where(w => 1 == 1);
                //students = students.Where(w => w.create_date.Value.Date >= dfrom.Date);
                //students = students.Where(w => w.create_date.Value.Date <= dto.Date);

                //foreach (var item in students)
                //{
                //    var fim_user = _context.table_visual_fim_user.Where(w => w.basic_uid.ToLower() == item.login_name.ToLower()).FirstOrDefault();
                //    if (fim_user != null)
                //    {
                //        var script_temp = new script_temp();
                //        script_temp.id = item.id;
                //        script_temp.displayname = item.displayname;
                //        script_temp.login_name = item.login_name;
                //        script_temp.password_initial = item.password_initial;
                //        script_temp.email_address = item.email_address;
                //        script_temp.server_name = item.server_name;
                //        script_temp.expire = item.expire;
                //        script_temp.status_id = item.status_id;
                //        script_temp.org = item.org;
                //        script_temp.create_date = item.create_date;
                //        script_temp.receive_date = item.receive_date;
                //        script_temp.manage_by = item.manage_by;
                //        script_temp.ticket = item.ticket;
                //        script_temp.visual_fim_user = fim_user;
                //        script_temp.account_type = "student";
                //        script_temps.Add(script_temp);

                //        Console.WriteLine("New User: " + item.displayname);
                //    }
                //}

                var temps = _context.table_receive_temp.Where(w => 1 == 1);
                temps = temps.Where(w => w.create_date.Value.Date >= dfrom.Date);
                temps = temps.Where(w => w.create_date.Value.Date <= dto.Date);

                foreach (var item in temps)
                {
                    var fim_user = _context.table_visual_fim_user.Where(w => w.basic_uid.ToLower() == item.login_name.ToLower()).FirstOrDefault();
                    if (fim_user != null)
                    {
                        var script_temp = new script_temp();
                        script_temp.id = item.id;
                        script_temp.displayname = item.displayname;
                        script_temp.login_name = item.login_name;
                        script_temp.password_initial = item.password_initial;
                        script_temp.email_address = item.email_address;
                        script_temp.server_name = item.server_name;
                        script_temp.expire = item.expire;
                        script_temp.status_id = item.status_id;
                        script_temp.org = item.org;
                        script_temp.create_date = item.create_date;
                        script_temp.receive_date = item.receive_date;
                        script_temp.manage_by = item.manage_by;
                        script_temp.ticket = item.ticket;
                        script_temp.visual_fim_user = fim_user;
                        script_temp.account_type = "temp";
                        script_temps.Add(script_temp);

                        Console.WriteLine("New User: " + item.displayname);
                    }
                }

                script_temps = script_temps.OrderByDescending(o => o.create_date).ToList();

                if (script_temps.Count == 0)
                {
                    Console.WriteLine("End create new user files :" + DateUtil.Now());
                    return;
                }

                String[] sparams = null;
                var Indicator = ":";
                var script_param = ScriptFormatParam.UNIX1;
                sparams = script_param.Split(':');

                var filename = "C:\\inetpub\\wwwroot\\bg\\new_user\\" + date + "_unix1.csv";
                try
                {
                    using (System.IO.StreamWriter StreamWriter1 = new System.IO.StreamWriter(filename, false, System.Text.Encoding.GetEncoding("windows-874")))
                    {
                        var texts = new List<string>();
                        foreach (var script in script_temps)
                        {
                            StreamWriter1.WriteLine(get_script_text(script, sparams, Indicator));
                        }
                        StreamWriter1.Close();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error :" + ex.Message);
                }

                script_param = ScriptFormatParam.GW1;
                sparams = script_param.Split(':');
                filename = "C:\\inetpub\\wwwroot\\bg\\new_user\\" + date + "_gw1.csv";
                try
                {
                    using (System.IO.StreamWriter StreamWriter1 = new System.IO.StreamWriter(filename, false, System.Text.Encoding.GetEncoding("windows-874")))
                    {
                        var texts = new List<string>();
                        foreach (var script in script_temps)
                        {
                            StreamWriter1.WriteLine(get_script_text(script, sparams, Indicator));
                        }
                        StreamWriter1.Close();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error :" + ex.Message);
                }

                script_param = ScriptFormatParam.GW2;
                sparams = script_param.Split(':');
                filename = "C:\\inetpub\\wwwroot\\bg\\new_user\\" + date + "_gw2.csv";
                try
                {
                    using (System.IO.StreamWriter StreamWriter1 = new System.IO.StreamWriter(filename, false, System.Text.Encoding.GetEncoding("windows-874")))
                    {
                        var texts = new List<string>();
                        foreach (var script in script_temps)
                        {
                            StreamWriter1.WriteLine(get_script_text(script, sparams, Indicator));
                        }
                        StreamWriter1.Close();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error :" + ex.Message);
                }

                script_param = ScriptFormatParam.pommo;
                sparams = script_param.Split(':');
                filename = "C:\\inetpub\\wwwroot\\bg\\new_user\\" + date + "_pommo.csv";
                try
                {
                    using (System.IO.StreamWriter StreamWriter1 = new System.IO.StreamWriter(filename, false, System.Text.Encoding.GetEncoding("windows-874")))
                    {
                        var texts = new List<string>();
                        foreach (var script in script_temps)
                        {
                            StreamWriter1.WriteLine(get_script_text(script, sparams, Indicator));
                        }
                        StreamWriter1.Close();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error :" + ex.Message);
                }

                Indicator = ",";
                script_param = ScriptFormatParam.Znix;
                sparams = script_param.Split(',');
                filename = "C:\\inetpub\\wwwroot\\bg\\new_user\\" + date + "_NDID_new.csv";
                try
                {
                    using (System.IO.StreamWriter StreamWriter1 = new System.IO.StreamWriter(filename, false, System.Text.Encoding.GetEncoding("windows-874")))
                    {
                        var texts = new List<string>();
                        foreach (var script in script_temps.Where(w => w.account_type != "student"))
                        {
                            StreamWriter1.WriteLine(get_script_text(script, sparams, Indicator));
                        }
                        StreamWriter1.Close();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error :" + ex.Message);
                }
            }
            Console.WriteLine("End create new user files :" + DateUtil.Now());
            upload_ftp_file();
        }

        private static string get_script_text(script_temp script, String[] sparams, string Indicator)
        {
            Type t = script.GetType();
            Type t2 = script.visual_fim_user.GetType();

            var text = new StringBuilder();
            foreach (var sparam in sparams)
            {
                var name = sparam;
                var name2 = name;
                var startindex = name.IndexOf("[");
                var endindex = name.IndexOf("]");
                if (startindex >= 0 & endindex >= 0)
                {
                    name = name.Substring(startindex, (endindex - startindex) + 1);
                    name2 = name;
                    name2 = name2.Replace("[", "");
                    name2 = name2.Replace("]", "");
                }
                PropertyInfo pro1 = t.GetProperty(name2);
                PropertyInfo pro2 = t2.GetProperty(name2);
                if (pro1 != null)
                {
                    var val = pro1.GetValue(script);
                    if (name2 == "password_initial" | name2 == "ticket")
                        val = Cryptography.decrypt(val.ToString());

                    if (name != sparam)
                    {
                        var newval = sparam.Replace(name, val.ToString());
                        text.Append(newval);
                        text.Append(Indicator);
                    }
                    else
                    {
                        var newval = "";
                        if (val.GetType().Name == "DateTime")
                            newval = DateUtil.ToDisplayDate4((DateTime)val);
                        else
                        {
                            if (val != null)
                                newval = val.ToString();
                        }
                        text.Append(newval);
                        text.Append(Indicator);
                    }


                }
                else if (pro2 != null)
                {
                    var val = pro2.GetValue(script.visual_fim_user);
                    if (name2 == "password_initial" | name2 == "ticket")
                        val = Cryptography.decrypt(val.ToString());
                    if (name != sparam)
                    {
                        var newval = sparam.Replace(name, val.ToString());
                        text.Append(newval);
                        text.Append(Indicator);
                    }
                    else
                    {
                        text.Append(val);
                        text.Append(Indicator);
                    }

                }
                else if (string.IsNullOrEmpty(name2))
                {
                    text.Append(Indicator);
                }
                else
                {
                    text.Append(name2);
                    text.Append(Indicator);
                }
            }
            return text.ToString().Substring(0, text.ToString().Length - 1);
        }


        public static void upload_ftp_file()
        {
            Console.WriteLine("Start upload new user files :" + DateUtil.Now());
            var _conf = new SystemConf();

            _conf.IPAddress = ConfigurationManager.AppSettings["IPAddress"];
            _conf.Username = ConfigurationManager.AppSettings["Username"];
            _conf.Password = ConfigurationManager.AppSettings["Password"];
            _conf.Port = NumUtil.ParseInteger(ConfigurationManager.AppSettings["Port"]);
            using (var client = new SftpClient(_conf.IPAddress, _conf.Port, _conf.Username, _conf.Password))
            {
                var date = DateUtil.ToInternalDate3(DateUtil.Now());
                var files = Directory.GetFiles("C:\\inetpub\\wwwroot\\bg\\new_user\\", "*" + date + "*.csv");
                foreach (var file in files)
                {
                    try
                    {
                        var filename = file.Replace("C:\\inetpub\\wwwroot\\bg\\new_user\\", "");
                        var remoteFilePath = "\\CU_Output\\" + filename;
                        client.Connect();
                        var s = File.OpenRead(file);
                        client.UploadFile(s, remoteFilePath);

                        Console.WriteLine("Upload :" + filename);
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                    }
                    finally
                    {
                        client.Disconnect();
                    }
                }

            }

            Console.WriteLine("End upload new user files :" + DateUtil.Now());
        }

        public static void lock_expire_account()
        {
            var collection = new ServiceCollection();
            collection.AddScoped<IUserProvider, AdUserProvider>();
            collection.AddScoped<ILDAPUserProvider, LDAPUserProvider>();

            var serviceProvider = collection.BuildServiceProvider();

            var _provider = serviceProvider.GetService<IUserProvider>();
            var _providerldap = serviceProvider.GetService<ILDAPUserProvider>();

            using (var _context = new SpuContext())
            {
                Console.WriteLine("Start Lock Expired Account :" + DateUtil.Now());

                var date = DateUtil.ToInternalDate3(DateUtil.Now());
                var filename = Directory.GetCurrentDirectory() + "\\idm_lock_user_" + date + ".csv";
                Console.WriteLine(filename);

                try
                {
                    using (System.IO.StreamWriter StreamWriter1 = new System.IO.StreamWriter(filename, false, System.Text.Encoding.GetEncoding("windows-874")))
                    {
                        var fim_users = _context.table_visual_fim_user.Where(w => w.cu_nsaccountlock == LockStaus.Unlock & !string.IsNullOrEmpty(w.cu_CUexpire)).ToList();
                        foreach (var model in fim_users)
                        {
                            try
                            {
                                if (DateUtil.ToDate(model.cu_CUexpire, "-") < DateUtil.Now())
                                {
                                    Console.WriteLine("Locked Account " + model.basic_uid + " :" + DateUtil.Now());
                                    model.cu_nsaccountlock = LockStaus.Lock;

                                    //var result_ldap = _providerldap.NsLockUser(model, _context);
                                    //if (result_ldap.result == true)
                                    //    writelog(LogType.log_unlock_account, LogStatus.successfully, IDMSource.LDAP, model.basic_uid);
                                    //else
                                    //    writelog(LogType.log_unlock_account, LogStatus.failed, IDMSource.LDAP, model.basic_uid, log_exception: result_ldap.Message);

                                    //var result_ad = _provider.EnableUser(model, _context);
                                    //if (result_ad.result == true)
                                    //    writelog(LogType.log_unlock_account, LogStatus.successfully, IDMSource.AD, model.basic_uid);
                                    //else
                                    //    writelog(LogType.log_unlock_account, LogStatus.failed, IDMSource.AD, model.basic_uid, log_exception: result_ad.Message);

                                    //writelog(LogType.log_unlock_account, LogStatus.successfully, IDMSource.VisualFim, model.basic_uid);
                                    //_context.SaveChanges();
                                    StreamWriter1.WriteLine(model.basic_uid);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Error :" + model.basic_uid);
                                Console.WriteLine(ex.Message);
                            }
                        }
                        StreamWriter1.Close();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error :" + ex.Message);

                }

                Console.WriteLine("End Lock Expired Account :" + DateUtil.Now());

            }
        }

        public static void export_new_account()
        {
            using (var _context = new SpuContext())
            {
                Console.WriteLine("Start export file :" + DateUtil.Now());

                var fim_users = _context.table_visual_fim_user.Where(w => w.system_actived == false & w.system_create_date.Value.Date == DateUtil.Now().AddDays(-1).Date);
                Console.WriteLine("User Count :" + fim_users.Count() + DateUtil.Now());

                var date = DateUtil.ToInternalDate3(DateUtil.Now());
                var filename = Directory.GetCurrentDirectory() + "\\idm_inactivate_user_" + date + ".csv";
                try
                {
                    using (System.IO.StreamWriter StreamWriter1 = new System.IO.StreamWriter(filename, false, System.Text.Encoding.GetEncoding("windows-874")))
                    {
                        foreach (var item in fim_users)
                        {
                            StreamWriter1.WriteLine(item.basic_mobile + "," + item.basic_uid);
                        }
                        StreamWriter1.Close();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error :" + ex.Message);
                }
                Console.WriteLine("End export file :" + DateUtil.Now());
            }
        }

        public static string writelog(LogType log_type_id, string log_status, IDMSource source, string uid, string log_description = "", string logonname = "", string log_exception = "")
        {
            using (var _context = new SpuContext())
            {
                if (string.IsNullOrEmpty(log_description))
                {
                    if (log_type_id == LogType.log_create_account)
                        log_description = "สร้างบัญชีผู้ใช้";
                    else if (log_type_id == LogType.log_create_account_with_file)
                        log_description = "สร้างบัญชีผู้ใช้";
                    else if (log_type_id == LogType.log_edit_account)
                        log_description = "แก้ไขบัญชีผู้ใช้";
                    else if (log_type_id == LogType.log_delete_account)
                        log_description = "ลบบัญชีผู้ใช้";
                    else if (log_type_id == LogType.log_delete_account_with_file)
                        log_description = "ลบบัญชีผู้ใช้";
                    else if (log_type_id == LogType.log_change_password)
                        log_description = "เปลี่ยนรหัสผ่านบัญชีผู้ใช้";
                    else if (log_type_id == LogType.log_reset_password)
                        log_description = "เปลี่ยนรหัสผ่านบัญชีผู้ใช้";
                    else if (log_type_id == LogType.log_lock_account)
                        log_description = "ล็อกบัญชีผู้ใช้";
                    else if (log_type_id == LogType.log_unlock_account)
                        log_description = "ปลดล็อกบัญชีผู้ใช้";
                    else if (log_type_id == LogType.log_lock_account_with_file)
                        log_description = "ล็อกบัญชีผู้ใช้";
                    else if (log_type_id == LogType.log_unlock_account_with_file)
                        log_description = "ปลดล็อกบัญชีผู้ใช้";
                    else if (log_type_id == LogType.log_edit_internetaccess)
                        log_description = "แก้ไข Internet Access บัญชีผู้ใช้";
                    else if (log_type_id == LogType.log_approve_reset_password)
                        log_description = "ขอเปลี่ยนรหัสผ่านบัญชีผู้ใช้";
                    else if (log_type_id == LogType.log_move_account)
                        log_description = "ย้ายกลุ่มของบัญชีรายชื่อผู้ใช้";
                    else if (log_type_id == LogType.log_approved_reset_password)
                        log_description = "อนุมัติการขอเปลี่ยนรหัสผ่านบัญชีผู้ใช้";
                    else if (log_type_id == LogType.log_reset_password_api)
                        log_description = "เปลี่ยนรหัสผ่านบัญชีผู้ใช้จาก API";

                    log_description += " " + uid + " บน " + source.ToString();
                    if (log_status == LogStatus.successfully)
                        log_description += " สำเร็จ";
                    else
                        log_description += " ไม่สำเร็จ";
                }

                if (string.IsNullOrEmpty(logonname))
                    logonname = "backgroud system";

                var log_target = "backgroud system";
                var log_action = "";

                var datetime = DateUtil.Now();
                var curdate = datetime.Year + "_" + datetime.Month.ToString("00") + "_" + datetime.Day.ToString("00");
                var tablename = "table_system_log_" + curdate;
                if (logTableIsExist(tablename) == false)
                {
                    if (logTableCreate(tablename) == false)
                        return "ไม่สามารถสร้าง log table ได้";
                }
                try
                {
                    var sql = new StringBuilder();
                    sql.AppendLine("INSERT INTO [DSM].[dbo].[" + tablename + "](");
                    sql.AppendLine(" [log_username]");
                    sql.AppendLine(" ,[log_ip]");
                    sql.AppendLine(" ,[log_type_id]");
                    sql.AppendLine(" ,[log_type]");
                    sql.AppendLine(" ,[log_action]");
                    sql.AppendLine(" ,[log_status]");
                    sql.AppendLine(" ,[log_description]");
                    sql.AppendLine(" ,[log_target]");
                    sql.AppendLine(" ,[log_target_ip]");
                    sql.AppendLine(" ,[log_datetime]");
                    sql.AppendLine(" ,[log_exception])");
                    sql.AppendLine(" VALUES (");
                    sql.AppendLine(" 'backgroud system'");
                    sql.AppendLine(" ,'backgroud system'");
                    sql.AppendLine(" ," + (int)log_type_id);
                    sql.AppendLine(" ,'" + log_type_id.ToString() + "'");
                    sql.AppendLine(" ,'" + log_action + "'");
                    sql.AppendLine(" ,'" + log_status + "'");
                    sql.AppendLine(" ,'" + log_description + "'");
                    sql.AppendLine(" ,'" + log_target + "'");
                    sql.AppendLine(" ,'backgroud system'");
                    sql.AppendLine(" ,'" + DateUtil.ToDisplayDateTime(DateUtil.Now()) + "'");

                    sql.AppendLine(" ,'" + log_exception + "'");
                    sql.AppendLine(" )");
                    using (var command = _context.Database.GetDbConnection().CreateCommand())
                    {
                        command.CommandText = sql.ToString();
                        _context.Database.OpenConnection();
                        var result = command.ExecuteNonQuery();
                        _context.Database.CloseConnection();
                    }
                }
                catch (Exception ex)
                {

                }
            }

            return log_description;
        }

        public static bool logTableIsExist(string tablename)
        {
            using (var _context = new SpuContext())
            {
                try
                {
                    var object_id = "";
                    var sql = "select object_id from sys.tables where name = '" + tablename + "'";
                    using (var command = _context.Database.GetDbConnection().CreateCommand())
                    {
                        command.CommandText = sql;
                        _context.Database.OpenConnection();
                        using (var result = command.ExecuteReader())
                        {
                            // do something with result
                            while (result.Read())
                            {
                                object_id = result.GetValue(0).ToString();
                            }
                        }
                        _context.Database.CloseConnection();
                    }
                    var column_id = "";
                    sql = "select column_id from sys.columns where name = 'log_exception' and object_id = '" + object_id + "'";
                    using (var command = _context.Database.GetDbConnection().CreateCommand())
                    {
                        command.CommandText = sql;
                        _context.Database.OpenConnection();
                        using (var result = command.ExecuteReader())
                        {
                            // do something with result
                            while (result.Read())
                            {
                                column_id = result.GetValue(0).ToString();
                            }
                        }
                        _context.Database.CloseConnection();
                    }
                    if (string.IsNullOrEmpty(column_id))
                    {
                        using (var command = _context.Database.GetDbConnection().CreateCommand())
                        {
                            command.CommandText = "alter table " + tablename + " add [log_exception][nvarchar](max) NULL";
                            _context.Database.OpenConnection();
                            var result = command.ExecuteNonQuery();
                            _context.Database.CloseConnection();
                        }
                    }
                    if (!string.IsNullOrEmpty(object_id))
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            return false;
        }
        public static bool logTableCreate(string tablename)
        {
            using (var _context = new SpuContext())
            {
                try
                {
                    var datetime = DateUtil.Now();
                    var curdate = datetime.Year + "_" + datetime.Month.ToString("00") + "_" + datetime.Day.ToString("00");
                    var sql = new StringBuilder();
                    sql.AppendLine("CREATE TABLE[dbo].[" + tablename + "](");
                    sql.AppendLine(" [log_id] [bigint] IDENTITY(1,1) NOT NULL,");
                    sql.AppendLine(" [log_username][nvarchar](max) NULL,");
                    sql.AppendLine(" [log_ip][nvarchar](max) NULL,");
                    sql.AppendLine(" [log_type_id] [bigint] NULL,");
                    sql.AppendLine(" [log_type][nvarchar](max) NULL,");
                    sql.AppendLine(" [log_action][nvarchar](max) NULL,");
                    sql.AppendLine(" [log_status][nvarchar](max) NULL,");
                    sql.AppendLine(" [log_description][nvarchar](max) NULL,");
                    sql.AppendLine(" [log_target][nvarchar](max) NULL,");
                    sql.AppendLine(" [log_target_ip][nvarchar](max) NULL,");
                    sql.AppendLine(" [log_datetime][nvarchar](max) NULL,");
                    sql.AppendLine(" [log_exception][nvarchar](max) NULL");
                    sql.AppendLine(") ON[PRIMARY]");
                    using (var command = _context.Database.GetDbConnection().CreateCommand())
                    {
                        command.CommandText = sql.ToString();
                        _context.Database.OpenConnection();
                        var result = command.ExecuteNonQuery();
                        _context.Database.CloseConnection();
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        public static void genNewAccount(SpuContext _context, SystemConf _conf, ILDAPUserProvider _providerldap, visual_fim_user model, ArrayList exsits)
        {
            var password = AppUtil.getNewPassword();
            var system_ou_lvl1 = AppUtil.getOuName(model.system_ou_lvl1);
            var system_ou_lvl2 = AppUtil.getOuName(model.system_ou_lvl2);
            var system_ou_lvl3 = AppUtil.getOuName(model.system_ou_lvl3);

            var officeShotName = "";
            var firstValuePath = "";
            if (!string.IsNullOrEmpty(system_ou_lvl1))
            {
                firstValuePath += system_ou_lvl1;
            }
            if (system_ou_lvl1 == "internet")
            {
                officeShotName = system_ou_lvl1.ToUpper();
            }
            if (!string.IsNullOrEmpty(system_ou_lvl2))
            {
                officeShotName = AppUtil.getOuName(model.system_ou_lvl2, false).ToUpper();
                firstValuePath += "/" + system_ou_lvl2;
            }


            var unix_homeDirectory = "";
            if (!string.IsNullOrEmpty(firstValuePath))
            {
                unix_homeDirectory = _conf.DefaultValue_homeDirectory.Replace("[path]", firstValuePath);
            }

            model.basic_givenname = model.basic_givenname.Replace(" ", "");
            model.basic_sn = model.basic_sn.Replace(" ", "");
            model.basic_givenname = model.basic_givenname.Substring(0, 1).ToUpper() + model.basic_givenname.Substring(1);
            model.basic_sn = model.basic_sn.Substring(0, 1).ToUpper() + model.basic_sn.Substring(1);
            model.system_idm_user_type = getIdmUserType(system_ou_lvl1);
            model.basic_displayname = model.basic_cn;
            model.basic_dn = "uid=[uid]";
            if (!string.IsNullOrEmpty(model.system_ou_lvl3))
                model.basic_dn += "," + model.system_ou_lvl3.ToLower();
            if (!string.IsNullOrEmpty(model.system_ou_lvl2))
                model.basic_dn += "," + model.system_ou_lvl2.ToLower();
            if (!string.IsNullOrEmpty(model.system_ou_lvl1))
                model.basic_dn += "," + model.system_ou_lvl1.ToLower();

            model.basic_cn = model.basic_cn;
            model.basic_displayname = model.basic_displayname;
            model.basic_dn += ",dc=chula,dc=ac,dc=th";
            model.basic_givenname = model.basic_givenname;
            model.basic_sn = model.basic_sn;
            model.basic_uid = genUid(_context, _providerldap, model.basic_givenname, model.basic_sn, model.system_idm_user_type, model.basic_dn, model.cu_jobcode);
            model.basic_dn = model.basic_dn.Replace("[uid]", model.basic_uid);
            if (model.system_idm_user_type == IDMUserType.student)
                model.basic_mail = genNewEmailForStudent(_conf, model.cu_jobcode);
            else
                model.basic_mail = genNewEmail(_context, _conf, model.basic_givenname, model.basic_sn, model.system_idm_user_type);
            if (!string.IsNullOrEmpty(model.basic_mobile))
                model.basic_mobile = model.basic_mobile.Trim();
            if (!string.IsNullOrEmpty(model.basic_telephonenumber))
                model.basic_telephonenumber = model.basic_telephonenumber.Trim();
            model.basic_userPassword = Cryptography.encrypt(password);
            if (model.system_idm_user_type == IDMUserType.student)
                model.basic_userprincipalname = _conf.DefaultValue_userprincipalname.Replace("[uid]", model.basic_uid);
            else
                model.basic_userprincipalname = model.basic_mail;
            if (model.cu_CUexpire_select == false & model.cu_CUexpire_day.HasValue & model.cu_CUexpire_month.HasValue & model.cu_CUexpire_year.HasValue)
                model.cu_CUexpire = model.cu_CUexpire_day + "-" + DateUtil.GetShortMonth(model.cu_CUexpire_month) + "-" + model.cu_CUexpire_year.ToString().Substring(2);
            model.cu_gecos = "";
            if (string.IsNullOrEmpty(model.basic_cn) == false)
            {
                model.cu_gecos = model.basic_cn;
                if (string.IsNullOrEmpty(officeShotName) == false)
                {
                    model.cu_gecos += ", " + officeShotName;
                    if (string.IsNullOrEmpty(model.basic_telephonenumber) == false)
                    {
                        model.cu_gecos += ", " + model.basic_telephonenumber;
                    }
                }
            }
            if (!string.IsNullOrEmpty(model.cu_jobcode))
                model.cu_jobcode = model.cu_jobcode.Trim();
            model.cu_mailacceptinggeneralid = model.basic_mail;
            model.cu_maildrop = _conf.DefaultValue_maildrop.Replace("[uid]", model.basic_uid);
            model.cu_mailhost = _conf.DefaultValue_mailhost;
            model.cu_mailRoutingAddress = _conf.DefaultValue_mailRoutingAddress.Replace("[uid]", model.basic_uid);
            model.cu_nsaccountlock = _conf.DefaultValue_nsaccountlock;
            model.cu_pplid = model.cu_pplid.Trim();
            model.cu_pwdchangedby = "backgroud system";
            model.cu_pwdchangeddate = DateUtil.Now();
            model.cu_pwdchangedloc = "backgroud system";
            model.cu_sce_package = _conf.DefaultValue_SCE_Package;
            model.cu_thcn = model.cu_thcn.Trim();
            model.cu_thsn = model.cu_thsn.Trim();
            model.mail_miWmprefCharset = _conf.DefaultValue_miWmprefCharset;
            model.mail_miWmprefEmailAddress = model.basic_mail;
            model.mail_miWmprefFullName = model.basic_cn;
            model.mail_miWmprefReplyOption = _conf.DefaultValue_miWmprefReplyOption;
            model.mail_miWmprefTimezone = _conf.DefaultValue_miWmprefTimezone;
            model.system_answer1 = "";
            model.system_answer2 = "";
            model.system_answer3 = "";
            model.system_create_by_uid = "backgroud system";
            model.system_create_date = DateUtil.Now();
            model.system_last_accessed_by_uid = "backgroud system";
            model.system_last_accessed_date = DateUtil.Now();
            model.system_modify_by_uid = "backgroud system";
            model.system_modify_date = DateUtil.Now();
            if (model.system_sub_office_id.HasValue)
            {
                var org = _context.table_cu_faculty_level2.Where(w => w.sub_office_id == model.system_sub_office_id).FirstOrDefault();
                if (org != null)
                    model.system_org = org.sub_office_name;
            }
            model.system_question1 = "";
            model.system_question2 = "";
            model.system_question3 = "";
            model.system_temporary_user_expire_date_counter = 0;
            model.system_waiting_time_for_access = 0;
            model.unix_gidNumber = ((int)model.system_idm_user_type).ToString();
            model.unix_homeDirectory = unix_homeDirectory.Replace("[uid]", model.basic_uid);

            string[] inetCOS = _conf.DefaultValue_inetCOS.Split('|');
            model.unix_inetCOS = inetCOS[0];
            model.unix_loginShell = _conf.DefaultValue_loginShell;
            model.unix_uidNumber = getUnixNumber(_context, exsits);

            model.internetaccess = 1;
            model.netcastaccess = 1;
            if (model.system_idm_user_type == IDMUserType.staff)
            {
                model.cu_mailhost = "";
                model.unix_inetCOS = inetCOS[1];

            }
            else if (model.system_idm_user_type == IDMUserType.student)
            {
                model.cu_CUexpire = "";
                model.cu_maildrop = _conf.DefaultValue_maildropForStudent.Replace("[uid]", model.basic_uid);
                model.cu_mailhost = _conf.DefaultValue_mailhostForStudent.Replace("[uid]", model.basic_uid);
                model.cu_mailRoutingAddress = _conf.DefaultValue_mailRoutingAddressForStudent.Replace("[uid]", model.basic_uid);
                model.mail_miWmprefCharset = "";
                model.mail_miWmprefEmailAddress = "";
                model.mail_miWmprefFullName = "";
                model.mail_miWmprefReplyOption = "";
                model.mail_miWmprefTimezone = "";
                model.unix_inetCOS = _conf.DefaultValue_inetCOSForStudent;
                model.cu_nsaccountlock = _conf.DefaultValue_nsaccountlock;

            }
            else if (model.system_idm_user_type == IDMUserType.outsider)
            {
                model.cu_mailhost = "";
            }
            else if (model.system_idm_user_type == IDMUserType.affiliate)
            {

            }
            else if (model.system_idm_user_type == IDMUserType.temporary)
            {
                model.basic_cn = model.basic_uid;
                model.basic_displayname = model.basic_uid;
                model.basic_mail = "";
                model.basic_mobile = "";
                model.basic_sn = model.basic_uid;
                model.basic_telephonenumber = "";
                model.basic_userprincipalname = "";
                model.cu_gecos = "";
                model.cu_jobcode = "";
                model.cu_mailacceptinggeneralid = "";
                model.cu_maildrop = "";
                model.cu_mailhost = "";
                model.cu_mailRoutingAddress = "";
                model.cu_nsaccountlock = _conf.DefaultValue_nsaccountlockForTemporaryAccount;
                model.cu_pplid = "";
                model.cu_pwdchangedby = "";
                model.cu_pwdchangedloc = "";
                model.cu_sce_package = _conf.DefaultValue_SCE_Package;
                model.cu_thcn = "";
                model.cu_thsn = "";
                model.mail_miWmprefCharset = "";
                model.mail_miWmprefEmailAddress = "";
                model.mail_miWmprefFullName = "";
                model.mail_miWmprefReplyOption = "";
                model.mail_miWmprefTimezone = "";
                model.unix_homeDirectory = "";
                model.unix_inetCOS = "";
                model.unix_loginShell = "";
                model.basic_givenname = model.basic_uid;
            }
            if ((system_ou_lvl1 == "student" & system_ou_lvl2 == "cude" & system_ou_lvl3 == "people") | (system_ou_lvl1 == "student" & system_ou_lvl2 == "cuds" & system_ou_lvl3 == "people"))
            {
                model.basic_mail = "";
                model.cu_mailacceptinggeneralid = "";
                model.cu_maildrop = "";
                model.cu_mailhost = "";
                model.cu_mailRoutingAddress = "";
            }

            model.system_enable_password_forgot = 0;
            model.system_temporary_user_expire_date_counter = 0;
            model.system_waiting_time_for_access = 0;

            var unix = new cu_unix();
            unix.username = model.basic_uid;
            unix.userPassword = "*";
            unix.uid_number = NumUtil.ParseInteger(model.unix_uidNumber);
            unix.status_id = NumUtil.ParseInteger(model.unix_gidNumber);
            unix.full_name = model.basic_cn;
            unix.home_directory = model.unix_homeDirectory;
            unix.directory = model.unix_loginShell;
            unix.create_by_username = "backgroud system";
            unix.create_date = DateUtil.Now();
            unix.modify_by_username = "backgroud system";
            unix.create_date = DateUtil.Now();
            _context.table_cu_unix.Add(unix);

            if (model.system_idm_user_type == IDMUserType.student)
            {
                var email = new cu_email_student();
                email.name_eng = model.basic_givenname;
                email.surname_eng = model.basic_sn;
                email.email = model.basic_mail;
                email.create_date = DateUtil.Now();
                _context.table_cu_email_student.Add(email);
            }
            else
            {
                var email = new cu_email();
                email.name_eng = model.basic_givenname;
                email.surname_eng = model.basic_sn;
                email.email = model.basic_mail;
                email.create_date = DateUtil.Now();
                _context.table_cu_email.Add(email);
            }
            if (model.system_idm_user_type == IDMUserType.temporary)
                model.unix_gidNumber = "";

            _context.table_visual_fim_user.Add(model);

            if (model.unix_gidNumber == "302")
            {
                var receive = new receive_student();
                receive.displayname = model.basic_displayname;
                receive.login_name = model.basic_uid;
                receive.password_initial = model.basic_userPassword;
                receive.ticket = Cryptography.encrypt(getNewTicket());
                receive.email_address = model.basic_mail;
                receive.server_name = _conf.TableReceiveAccount_ServerName;
                receive.expire = model.cu_CUexpire;
                receive.status_id = model.unix_gidNumber;
                receive.org = model.system_org;
                receive.create_date = DateUtil.Now();
                _context.table_receive_student.Add(receive);

            }
            else if (model.unix_gidNumber == "305")
            {
                var receive = new receive_temp();
                receive.displayname = model.basic_displayname;
                receive.login_name = model.basic_uid;
                receive.password_initial = model.basic_userPassword;
                receive.ticket = Cryptography.encrypt(getNewTicket());
                receive.email_address = model.basic_mail;
                receive.server_name = _conf.TableReceiveAccount_ServerName;
                receive.expire = model.cu_CUexpire;
                receive.status_id = model.unix_gidNumber;
                receive.org = model.system_org;
                receive.create_date = DateUtil.Now();
                _context.table_receive_temp.Add(receive);

            }
            else
            {
                var receive = new receive_staff();
                receive.displayname = model.basic_displayname;
                receive.login_name = model.basic_uid;
                receive.password_initial = model.basic_userPassword;
                receive.ticket = Cryptography.encrypt(getNewTicket());
                receive.email_address = model.basic_mail;
                receive.server_name = _conf.TableReceiveAccount_ServerName;
                receive.expire = model.cu_CUexpire;
                receive.status_id = model.unix_gidNumber;
                receive.org = model.system_org;
                receive.create_date = DateUtil.Now();
                _context.table_receive_staff.Add(receive);
            }
            return;
        }

        public static IDMUserType getIdmUserType(string ou)
        {
            if (ou == IDMUserType.staff.ToString())
                return IDMUserType.staff;
            else if (ou == IDMUserType.student.ToString())
                return IDMUserType.student;
            else if (ou == IDMUserType.outsider.ToString())
                return IDMUserType.outsider;
            else if (ou == IDMUserType.affiliate.ToString())
                return IDMUserType.affiliate;
            else if (ou == IDMUserType.temporary.ToString())
                return IDMUserType.temporary;
            else if (ou == "internet")
                return IDMUserType.temporary;
            return IDMUserType.temporary;
        }

        public static string genUid(SpuContext _context, ILDAPUserProvider _providerldap, string name, string surname, IDMUserType idm_user_type, string basic_dn, string jobcode)
        {
            string basic_uid = "";
            name = name.Trim();
            surname = surname.Trim();


            if (idm_user_type == IDMUserType.temporary)
            {
                basic_uid = genUidForTemporaryAccount(_context, _providerldap);
            }
            else if (idm_user_type == IDMUserType.affiliate)
            {
                basic_uid = genUidForStaff(_context, _providerldap, name, surname);
            }
            else if (idm_user_type == IDMUserType.outsider)
            {
                basic_uid = genUidForStaff(_context, _providerldap, name, surname);
            }
            else if (idm_user_type == IDMUserType.student)
            {
                basic_uid = genUidForStudent(_context, _providerldap, basic_dn, jobcode);
            }
            else if (idm_user_type == IDMUserType.staff)
            {
                basic_uid = genUidForStaff(_context, _providerldap, name, surname);
            }
            else
            {
                basic_uid = genUidForStaff(_context, _providerldap, name.Trim(), surname.Trim());
            }

            return basic_uid.ToLower();
        }

        public static string genUidForStaff(SpuContext _context, ILDAPUserProvider _providerldap, string name, string surname)
        {
            //check ถ้ามีชื่อกลาง---------------------------------------------------------
            //[ชื่อ].[ชื่อกลาง]@student.chula.ac.th
            //[ชื่อ].[นามสกุล]@student.chula.ac.th
            string[] array_name = (name.Replace("  ", " ")).Split(' ');
            if (array_name.Length > 1)
            {
                name = array_name[0];
                surname = array_name[1];
            }
            //-----------------------------------------------------------------------

            string new_username = "";
            int usernameLength = 8;
            if (surname.Length > 0)
            {
                if (name.Length > (usernameLength - 1))
                    new_username = surname.Substring(0, 1) + name.Substring(0, usernameLength - 1);
                else
                    new_username = surname.Substring(0, 1) + name;
            }
            else
            {
                if (name.Length > (usernameLength))
                    new_username = name.Substring(0, usernameLength);
                else
                    new_username = name;
            }
            bool haveUsernameIn_TableCuUnix = false;
            bool haveUsernameIn_TableVisualFim = false;
            bool haveUsernameIn_LDAP = false;

            //haveUsernameIn_TableCuUnix = int.Parse(DB.ExecuteQueryReturnFirstCell("SELECT count([username]) AS [have_username] FROM [table_cu_unix] WHERE [username] = '" + new_username + "';"));
            if (haveUsernameIn_TableCuUnix == false)
            {
                var fim_user = _context.table_visual_fim_user.Where(w => w.basic_uid.ToLower() == new_username.ToLower()).FirstOrDefault();
                if (fim_user != null)
                    haveUsernameIn_TableVisualFim = true;
            }

            //string[] attributeName = { new Config().ldapAuthenticationWith }; string[] attributeValue = { new_username }; bool Is_LIKE_mode = false;
            if (haveUsernameIn_TableCuUnix == false && haveUsernameIn_TableVisualFim == false)
            {

                var ldap = _providerldap.GetUser(new_username, _context);
                if (ldap != null)
                    haveUsernameIn_LDAP = true;
            }
            int counter_username = 0;
            while (haveUsernameIn_TableCuUnix == true | haveUsernameIn_TableVisualFim == true | haveUsernameIn_LDAP == true)
            {
                counter_username = (counter_username + 1);
                if (surname.Length > 0)
                {
                    if ((name + counter_username.ToString()).Length > (usernameLength - 1))
                        new_username = surname.Substring(0, 1) + name.Substring(0, usernameLength - (1 + counter_username.ToString().Length)) + counter_username.ToString();

                    else
                        new_username = surname.Substring(0, 1) + name + counter_username.ToString();

                }
                else
                {
                    if ((name + counter_username.ToString()).Length > (usernameLength))
                        new_username = name.Substring(0, usernameLength - (counter_username.ToString().Length)) + counter_username.ToString();
                    else
                        new_username = name + counter_username.ToString();

                }
                haveUsernameIn_TableCuUnix = false;
                haveUsernameIn_TableVisualFim = false;
                haveUsernameIn_LDAP = false;

                //haveUsernameIn_TableCuUnix = int.Parse(DB.ExecuteQueryReturnFirstCell("SELECT count([username]) AS [have_username] FROM [table_cu_unix] WHERE [username] = '" + new_username + "';"));
                if (haveUsernameIn_TableCuUnix == false)
                {
                    var fim_user = _context.table_visual_fim_user.Where(w => w.basic_uid.ToLower() == new_username.ToLower()).FirstOrDefault();
                    if (fim_user != null)
                        haveUsernameIn_TableVisualFim = true;
                }

                //string[] attributeName = { new Config().ldapAuthenticationWith }; string[] attributeValue = { new_username }; bool Is_LIKE_mode = false;
                if (haveUsernameIn_TableCuUnix == false && haveUsernameIn_TableVisualFim == false)
                {

                    var ldap = _providerldap.GetUser(new_username, _context);
                    if (ldap != null)
                        haveUsernameIn_LDAP = true;
                }
            }
            return new_username.ToLower();
        }
        public static string genUidForStudent(SpuContext _context, ILDAPUserProvider _providerldap, string basic_dn, string jobcode)
        {
            string basic_uid = "";


            if (string.IsNullOrEmpty(basic_uid))
            {
                if ((jobcode.Length > 0) && (System.Text.RegularExpressions.Regex.Match(jobcode[0].ToString(), "^[a-zA-Z]*$").Success))
                    basic_uid = jobcode.Trim();
                else
                    basic_uid = jobcode.Trim();
            }
            return basic_uid.ToLower();
        }
        public static string genUidForTemporaryAccount(SpuContext _context, ILDAPUserProvider _providerldap)
        {
            var uid = "tmp" + RandomPassword(5).ToLower();
            bool dup = true;
            while (dup == true)
            {
                var fim_user = _context.table_visual_fim_user.Where(w => w.basic_uid.ToLower() == uid.ToLower()).FirstOrDefault();
                if (fim_user == null)
                {
                    var ldap = _providerldap.GetUser(uid, _context);
                    if (ldap == null)
                    {
                        dup = false;
                        return uid;
                    }
                    else
                        uid = "tmp" + RandomPassword(5);
                }
                else
                    uid = "tmp" + RandomPassword(5);
            }
            return uid;
        }

        public static string genNewEmailForStudent(SystemConf _conf, string cu_jobcode)
        {

            string email_domain = _conf.DefaultValue_emailDomainForStudent; //"@student.chula.ac.th" *using in basic_mail*;}
            string newEmail = cu_jobcode + email_domain;
            return newEmail;
        }
        public static string genNewEmail(SpuContext _context, SystemConf _conf, string basic_givenname, string basic_sn, IDMUserType idm_user_type)
        {
            //check ถ้ามีชื่อกลาง---------------------------------------------------------
            //[ชื่อ].[ชื่อกลาง]@student.chula.ac.th
            //[ชื่อ].[นามสกุล]@student.chula.ac.th
            if (string.IsNullOrEmpty(basic_givenname) && string.IsNullOrEmpty(basic_sn))
                return "";

            string[] array_name = (basic_givenname.Replace("  ", " ")).Split(' ');
            if (array_name.Length > 1)
            {
                basic_givenname = array_name[0];
                //basic_sn = array_name[1];
            }

            string email_domain = _conf.DefaultValue_emailDomain; /*"@Chula.ac.th" *using in basic_mail*;*/
            if (idm_user_type == IDMUserType.student)
                email_domain = _conf.DefaultValue_emailDomainForStudent;

            string newEmail = "";

            string name_eng = basic_givenname.Trim().ToLower().Replace(" ", "");
            string surname_eng = basic_sn.Trim().ToLower().Replace(" ", "");

            string[] name_eng_Split = name_eng.Split(' ');
            if (name_eng_Split.Length > 1)
                name_eng = name_eng_Split[0];

            name_eng = AppUtil.stringUpper(name_eng);
            surname_eng = AppUtil.stringUpper(surname_eng);

            if (!string.IsNullOrEmpty(basic_givenname) && !string.IsNullOrEmpty(basic_sn))
            {
                for (int i = 1; i <= surname_eng.Length; i++)
                {
                    var mail = name_eng + "." + surname_eng.Substring(0, i) + email_domain;
                    var fim_user = _context.table_visual_fim_user.Where(w => w.basic_mail.ToLower() == mail.ToLower()).FirstOrDefault();
                    if (fim_user == null)
                    {
                        /*check ldap?*/
                        newEmail = mail;
                        break;
                    }
                }
                if (string.IsNullOrEmpty(newEmail))
                {
                    var runingNumber = 1;
                    while (string.IsNullOrEmpty(newEmail))
                    {
                        var mail = name_eng + "." + surname_eng + runingNumber.ToString() + email_domain;
                        var fim_user = _context.table_visual_fim_user.Where(w => w.basic_mail.ToLower() == mail.ToLower()).FirstOrDefault();
                        if (fim_user == null)
                        {
                            /*check ldap?*/
                            newEmail = mail;
                            break;
                        }
                        runingNumber++;
                    }
                }
            }
            else if (!string.IsNullOrEmpty(surname_eng) && string.IsNullOrEmpty(surname_eng))
            {
                var runingNumber = 1;
                while (string.IsNullOrEmpty(newEmail))
                {
                    var mail = name_eng + "." + runingNumber.ToString() + email_domain;
                    var fim_user = _context.table_visual_fim_user.Where(w => w.basic_mail.ToLower() == mail.ToLower()).FirstOrDefault();
                    if (fim_user == null)
                    {
                        /*check ldap?*/
                        newEmail = mail;
                        break;
                    }
                    runingNumber++;
                }
            }

            return newEmail;
        }

        public static string getUnixNumber(SpuContext _context, ArrayList exists)
        {
            var maxcuunix = _context.table_cu_unix.Select(s => s.uid_number).Max();
            var maxfim = _context.table_visual_fim_user.Select(s => Convert.ToInt64(s.unix_uidNumber)).Max();

            long startNumber = maxfim;

            if (maxcuunix > maxfim)
                startNumber = maxcuunix;
            startNumber += 1;

            while (exists.Contains(startNumber.ToString()))
            {
                startNumber++;
            }

            exists.Add(startNumber.ToString());
            return startNumber.ToString();
        }
        public static string getNewTicket()
        {
            int TicketLength = 8;
            string NewTicket = getNewTicket(TicketLength);
            return NewTicket;
        }
        public static string getNewTicket(int passwordLength)
        {
            string newPassword = "";
            //int passwordLength = 8;
            string[] numberSet = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
            string[] passwordSet = { "0", "9", "8", "7", "6", "5", "4", "3", "2", "1", };
            Random random = new Random();
            while (newPassword.Length < passwordLength)
            {
                int randomSet = random.Next(0, 100);
                if ((randomSet % 2) == 0) { newPassword += numberSet[(random.Next(0, (numberSet.Length - 1)))]; }
                else if ((randomSet % 2) == 1) { newPassword += passwordSet[(random.Next(0, (passwordSet.Length - 1)))]; }
            }
            return newPassword;
        }

        public static string RandomPassword(int length = 8, bool includenumber = true, bool includelower = true, bool includeupper = false)
        {
            string valid = "";
            string number = "1234567890";
            string lower = "abcdefghijklmnopqrstuvwxyz";
            string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            if (includenumber)
                valid += number;
            if (includelower)
                valid += lower;
            if (includeupper)
                valid += upper;
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }
    }

}
