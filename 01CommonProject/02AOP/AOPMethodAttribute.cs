using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _01CommonProject._02AOP
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class AOPMethodAttribute : Attribute
    {

    }
}
