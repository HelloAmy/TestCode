using _01CommonProject._01AOP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _01CommonProject
{
    class Program
    {
        static void Main(string[] args)
        {
            TestAttributeAOP();
            Console.Read();
        }

        public static void TestAop()
        {
            AOPService1 service = new AOPService1();
            AOPService1 proxy = DelayProxyUtil.GetTransparentProxy(typeof(AOPService1), service) as AOPService1;
            proxy.Test();
            proxy.Test1("Hello");
        }

        public static void TestAttributeAOP()
        {
            _01CommonProject._02AOP.AOPService2 service = new _02AOP.AOPService2();

            service.Print("Hello world");
        }
    }
}
