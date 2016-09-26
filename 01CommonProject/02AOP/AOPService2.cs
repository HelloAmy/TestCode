using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _01CommonProject._02AOP
{
    /// <summary>
    /// 必须继承ContextBoundObject
    /// </summary>
    [AOPnterceptor]
    public class AOPService2 : ContextBoundObject
    {
        [AOPMethod]
        public void Print(string str)
        {
            Console.WriteLine(str);
        }
    }
}
