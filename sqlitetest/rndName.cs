using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace sqlitetest
{
    public static class rndName
    {
        public const string url = "https://randus.org/";
        public static Student GetStudent()
        {
            var pageRaw = new WebClient().DownloadData(url);
            var page = Encoding.UTF8.GetString(pageRaw);
            var list = new Dictionary<string,string>();
            var pos = 1;
            while ((pos = page.IndexOf("<div class=\"form-group\">", pos + 24)) > 0) {
                
                var text = page.Substring(pos);
                var pos2 = text.IndexOf("</div>");
                text = text.Substring(0, pos2);

                var keys = text.IndexOf("<label>") + 7;
                var keyl = text.IndexOf("</label>", keys) - keys;
                var key = text.Substring(keys, keyl).Trim();

                var vals = text.IndexOf("value=") + 7;
                var vall = text.IndexOf("\">", vals) - vals;
                var val = text.Substring(vals, vall).Replace("\r","").Replace("\n","").Trim();

                list.Add(key,val);
            }

            var stu = new Student
            {
                ID = 0,
                Address = list["Адрес"],
                BirthDay = DateTime.Now,
                FIO = list["Фамилия Имя Отчество"],
                Group = "",
                PhoneNumber = list["Номер телефона"],
                Login = list["Логин"],
                Pass = list["Пароль"],
                Scores = new Dictionary<string, Dictionary<DateTime, byte>>()
            };
            return stu;
        }
    }
}
