using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.SQLite;
using System.Linq;
using System.Text;

namespace sqlitetest
{
    public class db
    {
        private static SQLiteConnection con = null;
        private static db instance;
        public static db Instance => instance = instance ?? new db();
        public SQLiteConnection Con => con;

        public db()
        {
            con = new SQLiteConnection(@"Data Source=students.sqlite;Version=3;DateTimeFormat=Ticks;Journal Mode=Off;Synchronous=Full;");
            con.Open();
            if (con.State == ConnectionState.Open)
            {
                instance = this;
                CreateDB();
            }
            else 
                throw new NullReferenceException("Can`t open DB!");
        }

        public void CreateDB()
        {
            Execute("create table if not exists \"students\" (\"id\" integer not null default 1 primary key autoincrement, \"fio\" text not null, \"group\" text not null, \"birthday\" numeric not null, \"address\" text, \"phonenumber\" integer, \"login\" text, \"pass\" text);");
            Execute("create table if not exists \"scores\" ( \"id\" integer not null, \"lesson\" text not null, \"date\" numeric not null, \"score\" integer not null);");

            if (Scalar("select count(*) from `students`;").ToString() == "0")
            {
                GenerateStudents(out var cnt, out var stu);
                GenerateScores(cnt, stu);
            }
        }

        private void GenerateStudents(out int count, out List<Student> stu)
        {
            stu = new List<Student>();
            var grp = new Dictionary<string,int>();
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

            count = random.Next(500, 999);
            for (int i = 0; i < count; i++)
            {
                var stud = rndName.GetStudent();
                stud.BirthDay = getBirthDay();
                stud.Group = getGroup();
                stu.Add(stud);
            }

            stu = stu.Select((s, i) =>{s.ID = i + 1;return s;}).ToList();

            var request = "insert into `students` values\n";
            request += string.Join(",\n", stu.Select(s => $"({s.ID},'{s.FIO}','{s.Group}','{s.BirthDay.Ticks}','{s.Address}','{s.PhoneNumber}','{s.Login}','{s.Pass}')"));
            request += ";";

            Execute(request);
        }

        private void GenerateScores(int count, List<Student> stu)
        {
            var lessons = new Dictionary<string, string[]>
            {
                {"БП", "Высшая математика,Основы безопастности,Защита данных,Базы данных,Компьютерные сети,Программирование".Split(',')},
                {"ПР", "Высшая математика,ОСАЛП,Базы данных,Компьютерные сети,Программирование,Офисное программирование".Split(',')},
                {"ЮР", "Высшая математика,Право,Аналитика,Юриспруденция".Split(',')},
                {"БУХ", "Высшая математика,Аналитика,Офисные программы,Статистика".Split(',')}
            };

            var random = new Random();                              // Рандом. Ну куда уж без него!
            var request = "insert into `scores` values\n";          // Заготовка запроса для вставки в БД
            var requestData = new List<string>();                   // Список добавляемых оценок в БД
            var date = new DateTime(DateTime.Now.Year - 1, 9, 1);   // Дата начала учебного года
            var EndDate = date.AddMonths(10);                       // Дата окончания учебного года
            var scoredStu = new List<int>();                        // Студенты, которые получили оценку
            while (date < EndDate)
            {
                date = date.AddDays(1);
                if (date.DayOfWeek == DayOfWeek.Saturday | date.DayOfWeek == DayOfWeek.Sunday)
                    continue;

                requestData.Clear();
                scoredStu.Clear();                                                  // Очищаем список оценённых студентов для этого дня
                var randomCount = random.Next(10, 20);                              // Генерируем кол-во студентов, получивших оценку в этот день

                for (int i = 0; i < randomCount; i++)                               // Раздаём оценки случайным студентам
                {
                    var randomID = 0;                                               // ID студента
                    while (scoredStu.Contains((randomID = random.Next(0, count))))  // Генерируем рандомный ИД студента, и сразу проверяем, получал ли он сегодня оценку
                    { /* Пустой цикл */ }

                    var stud = stu[randomID];                                       // Получаем студента из коллекции
                    randomID = stud.ID;                                             // Получаем ID студента
                    var lesson = lessons[stud.Group.Split('-').First()];            // Получаем список предметов относительно группы студента
                    var lessonRnd = lesson[random.Next(0, lesson.Length)];          // Выбираем случайный предмет
                    var score = random.Next(2, 6);                                  // генерируем случайную оценку для студента ( от 2 до 5 )
                    scoredStu.Add(randomID);                                        // Записываем в список, что этот студент уже получил оценку
                    requestData.Add($"('{randomID}','{lessonRnd}','{date.Ticks}','{score}')");  // Составляем часть запроса для этого студента
                }
                
                var requestFormated = request + string.Join(",\n", requestData) + ";";    // Формируем запрос с оценками за текущий день

                Execute(requestFormated);                                           // Отправляем данные в базу
            }
        }

        public void Execute(string cmd)
        {
            if (string.IsNullOrEmpty(cmd))
                return;

            using (var command = new SQLiteCommand(cmd, con))
            {
                command.ExecuteNonQuery();
            }
        }

        public object Scalar(string cmd)
        {
            if (string.IsNullOrEmpty(cmd))
                return null;

            using (var command = new SQLiteCommand(cmd, con))
            {
                return command.ExecuteScalar();
            }
        }

        public object[][] Query(string cmd)
        {
            if (string.IsNullOrEmpty(cmd))
                return null;

            using (var command = new SQLiteCommand(cmd, con))
            {
                var reader =  command.ExecuteReader();
                var values = new object[reader.FieldCount];
                var rows = new List<object[]>();
                
                while (reader.Read())
                {
                    values= new object[reader.FieldCount];
                    reader.GetValues(values);
                    rows.Add(values);
                }

                return rows.ToArray();
            }
        }

        public List<NameValueCollection> QueryNamed(string cmd)
        {
            if (string.IsNullOrEmpty(cmd))
                return null;

            using (var command = new SQLiteCommand(cmd, con))
            {
                var reader = command.ExecuteReader();
                var rows = new List<NameValueCollection>();

                while (reader.Read())
                {
                    rows.Add(reader.GetValues());
                }

                return rows;
            }
        }

    }
}
