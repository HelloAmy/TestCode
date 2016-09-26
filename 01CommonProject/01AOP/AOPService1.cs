using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _01CommonProject._01AOP
{
    public class AOPService1:MarshalByRefObject
    {
       public void Test()
       {
           Console.WriteLine("调用Test方法");
       }

       public string Test1(string input)
       {
           return string.IsNullOrEmpty(input) ? string.Empty : input.ToLower();
       }

    }
}
