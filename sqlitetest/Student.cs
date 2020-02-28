using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;

namespace sqlitetest
{
    public class Student
    {
        public string FIO { get; set; }
        public string Group { get; set; }
        public DateTime BirthDay { get; set; }
        public int Age => new DateTime((DateTime.Now - BirthDay).Ticks).Year;
        public string Address { get; set; }
        public int ID { get; set; }
        public string PhoneNumber { get; set; }
        public string Login { get; set; }
        public string Pass { get; set; }
        public Dictionary<string, Dictionary<DateTime,byte>> Scores { get; set; }

        public static Student Load(int id)
        {
            // Получаем данные из БД относительно этого ID. 
            // Максимум 1 запись (limit 1)
            var data = db.Instance.QueryNamed($"select * from `students` where `id`='{id}' limit 1;");

            // Получаем первую запись или же NULL если такой ID не существует в БД
            var info = data.FirstOrDefault();

            // Если информации нет, то и загружать нечего!
            if (info == null)
                return null;

            var stu = new Student
            {
                ID = Convert.ToInt32(info["id"]),
                FIO = info["fio"],
                Group = info["group"],
                BirthDay = new DateTime(Convert.ToInt64(info["birthday"])),
                Address = info["address"],
                PhoneNumber = info["phonenumber"],
                Login = info["login"],
                Pass = info["pass"]
            };

            // Получаем информацию о оценках студента
            var scores = db.Instance.QueryNamed($"select * from `scores` where `id`={id}");

            stu.Scores = new Dictionary<string, Dictionary<DateTime, byte>>();

            //и перебираем всё
            for (int i = 0; i < scores.Count; i++)
            {
                // Если этого предмета нет в списке, создаём его
                if(!stu.Scores.ContainsKey(scores[i]["lesson"]))
                    stu.Scores.Add(scores[i]["lesson"], new Dictionary<DateTime, byte>());

                var dat = new DateTime(Convert.ToInt64(scores[i]["date"]));

                // Добавляем запись о оценке в конкретный день для конкретного предмета
                if(!stu.Scores[scores[i]["lesson"]].ContainsKey(dat))
                    stu.Scores[scores[i]["lesson"]].Add(dat, Convert.ToByte(scores[i]["score"]));
            }

            return stu;
        }

        public void Save(SQLiteConnection con)
        {

        }
    }

}
