using ConsoleRedisTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Weiz.Redis.RedisTest
{
    class Program
    {
        static void Main(string[] args)
        {
            

            new RedisTool().Add<StudentModel>("a", new StudentModel() { id = 1, name = "程庆" },-1);



            List<StudentModel> listStudent = new List<StudentModel>();
            for (int i = 0; i < 5; i++)
            {
                StudentModel student = new StudentModel();
                student.id = i;
                student.name = "程庆" + i;
                listStudent.Add(student);
            }
            new RedisTool().AddList<StudentModel>("list", listStudent, -1);


            StudentModel student5 = new StudentModel();
            student5.id = 5;
            student5.name = "程庆" +5;

            new RedisTool().AddEntityToList<StudentModel>("list", student5, -1);



            new RedisTool().Set<StudentModel>("set", student5);


            Console.ReadKey();
        }
    }
}
