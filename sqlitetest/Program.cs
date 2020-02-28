using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace sqlitetest
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //db.Instance.Execute("");

            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());
            var grp = new Dictionary<string, int>();
            var grpMax = 25;
            var ageMin = 18;
            var ageMax = 49;
            var grpLst = "БП,ПР,ЮР,БУХ".Split(',');
            var random = new Random();

            string getGroup()
            {
                var grpNam = grpLst[random.Next(0, grpLst.Length)];
                var cnt = grp.Count(g => g.Key.StartsWith(grpNam) & g.Value == grpMax) + 1;
                grpNam = $"{grpNam}-{cnt}";
                if (grp.ContainsKey(grpNam))
                    grp[grpNam]++;
                else
                    grp.Add(grpNam, 1);
                return grpNam;
            }

            DateTime getBirthDay()
            {
                var dat = new DateTime(DateTime.Now.Year - random.Next(ageMin, ageMax + 1), 1, 1);
                dat = dat.AddMonths(random.Next(0, 12));
                dat = dat.AddDays(random.Next(0, dat.Month == 2 ? 28 : 31));
                return dat;
            }

            var Stud = new List<Student>();
            for (var i = 0; i < 10; i++)
            {
                Stud.Add(rndName.GetStudent());
                Stud[i].Group = getGroup();
                Stud[i].BirthDay = getBirthDay();
            }
            bFile.Write(Stud);

        }
    }
}
