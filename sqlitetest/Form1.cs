using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace sqlitetest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private List<Student> students;

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void загрузитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var IDs = db.Instance.Query("select `id` from `students`;");

            students = IDs                                                                                              // Обрабатываем все полученные ID                   
                .Select(id => Student.Load(Convert.ToInt32(id[0])))                                                     // загружаем студента по ID
                .OrderBy(g=> $"{g.Group.Split('-').First()}-{Convert.ToInt32(g.Group.Split('-').Last()):000} {g.FIO}")  // Сортируем список по группам и по фамилиям
                .ToList();                                                                                              // Преобразовываем массив в список

            // Получаем уникальный список предметов в БД
            var lessons = db.Instance.Query("select distinct(`lesson`) from `scores`");
            var lessonsList = lessons
                .Select(l => l[0].ToString())   // Выбираем первый элемент массива (Название предмета)
                .OrderBy(l => l)                // Сортируем названия по алфавиту для удобства
                .ToList();                      // Превращаем в список


            // Добавляем базовые колонки в лист
            listView1.Columns.Add("Группа");
            listView1.Columns.Add("ФИО Студента");

            listView2.Columns.Add("Дата");
            // Добавляем предметы в список
            lessonsList.ToList().ForEach(lesson => listView2.Columns.Add(lesson));

            // Устанавливаем кол-во виртуальных записей равным кол-ву студентов.
            listView1.Items.AddRange(students.Select(s => new ListViewItem( new [] {s.Group, s.FIO} )).ToArray());
            listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private void listView1_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            listView2.Items.Clear();
            if (listView1.SelectedIndices.Count != 1)
                return;
            
            var stu = students[listView1.SelectedIndices[0]];

            // Получаем количество уникальных дат в списке оценок
            var datesCount = db.Instance.QueryNamed($"select `date` from `scores` where `id`='{stu.ID}';");

            var list = datesCount.Select(v =>
            {
                // Создаем пустой элемент листа, с пустыми колонками
                var item = new ListViewItem(new string[listView2.Columns.Count]);

                // Получаем дату
                var date = new DateTime(Convert.ToInt64(v["date"]));

                // Получаем все предметы и оценки за этот день
                var scores = stu.Scores
                    .SelectMany(x => x.Value
                        .Select(d => d.Key == date ? new[] { x.Key, d.Value.ToString() } : null)
                        .Where(z => z != null)
                        .ToArray()
                    ).ToArray();

                item.SubItems[0].Text = date.ToShortDateString();
                for (var i = 0; i < scores.Length; i++)
                {
                    var lesson = scores[i][0];
                    var score = scores[i][1];
                    var index = listView2.Columns.Cast<ColumnHeader>().First(c => c.Text == lesson).Index;
                    item.SubItems[index].Text = score;
                    item.UseItemStyleForSubItems = false;
                    switch (score)
                    {
                        case "5": item.SubItems[index].BackColor = Color.LightSeaGreen; break;
                        case "4": item.SubItems[index].BackColor = Color.LightGreen; break;
                        case "3": item.SubItems[index].BackColor = Color.LightGoldenrodYellow;break;
                        case "2": item.SubItems[index].BackColor = Color.LightCoral;break;
                    }
                }

                return item;
            }).ToArray();

            // Устанавливаем кол-во виртуальных записей равным кол-ву уникальных дат.
            listView2.Items.AddRange(list);
            listView2.AutoResizeColumns( ColumnHeaderAutoResizeStyle.HeaderSize);
        }
    }
}
