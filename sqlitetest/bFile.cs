using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace sqlitetest
{
    public static class bFile
    {
        public static void Write(List<Student> students)
        {
            using (var bw = new BinaryWriter(File.OpenWrite(Filename)))
            {
                students.ForEach(s =>
                {
                    bw.Write(s.ID);
                    bw.Write(s.FIO);
                    bw.Write(s.Group);
                    bw.Write(s.BirthDay.Ticks);
                    bw.Write(s.Address);
                    bw.Write(s.PhoneNumber);
                    bw.Write(s.Login);
                    bw.Write(s.Pass);
                });
            }
            
        }
        public static List<Student> Read()
        {
            var List = new List<Student>();
            using (var br = new BinaryReader(File.OpenRead(Filename)))
            {
                
                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    var stu = new Student
                    {
                        ID = br.ReadInt32(),
                        FIO = br.ReadString(),
                        Group = br.ReadString(),
                        BirthDay = new DateTime(br.ReadInt64()),
                        Address = br.ReadString(),
                        PhoneNumber = br.ReadString(),
                        Login = br.ReadString(),
                        Pass = br.ReadString(),
                    };

                    List.Add(stu);
                
                }
            }
            return List;
        }
        public const string Filename = "Students.bin";
    }
}
