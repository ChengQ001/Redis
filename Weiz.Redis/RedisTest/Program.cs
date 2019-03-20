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
            Console.WriteLine("Redis写入缓存：zhong");

            RedisCacheHelper.Add<StudentModel>("zhong", new StudentModel (){id=1,name="程庆"}, DateTime.Now.AddSeconds(10));

            

            for (int i = 0; i < 1000; i++)
            {
                Thread.Sleep(1000);
                var  str3 = RedisCacheHelper.Get<StudentModel>("zhong");
                Console.WriteLine(RedisCacheHelper.Exists("zhong")+"   "+ str3?.id +"   "+str3?.name);
            }

            Console.ReadKey();
        }
    }
}
