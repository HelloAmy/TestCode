using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Text;
using System.Threading.Tasks;

namespace _01CommonProject._01AOP
{
    public class DelayProxy<T> : RealProxy
    {
        private static object objLock = new object();

        private T target;

        public DelayProxy(T target)
            : base(typeof(T))
        {
            this.target = target;
        }

        public override System.Runtime.Remoting.Messaging.IMessage Invoke(System.Runtime.Remoting.Messaging.IMessage msg)
        {
            IMethodCallMessage callMessage = (IMethodCallMessage)msg;
           
            IMessage message = DelayProxyUtil.InvokeBeProxy(this.target, callMessage);

            Action<IMessage> action = this.WriteAcc;
            action.BeginInvoke(message, null, null);
            return message;
        }

        public void WriteAcc(IMessage message)
        {
            ReturnMessage returnMessage = (ReturnMessage)message;
            string inputjson = JsonConvert.SerializeObject(returnMessage.Args);

            Console.WriteLine("类型名:" + returnMessage.TypeName);

            Console.WriteLine("方法名:" + returnMessage.MethodName);

            Console.WriteLine("入参:" + inputjson);

            Console.WriteLine("返回值:" + JsonConvert.SerializeObject(returnMessage.ReturnValue));

            Console.WriteLine();
            Console.WriteLine();
        }
    }
}
