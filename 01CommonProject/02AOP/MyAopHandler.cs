using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace _01CommonProject._02AOP
{
    public sealed class MyAopHandler : IMessageSink
    {
        private IMessageSink nextSink;

        public IMessageSink NextSink
        {
            get { return this.nextSink; }
            set { this.nextSink = value; }
        }

        public MyAopHandler(IMessageSink  nextSink)
        {
            this.nextSink = nextSink;
        }

        public IMessageCtrl AsyncProcessMessage(IMessage msg, IMessageSink replySink)
        {
            throw new NotImplementedException();
        }

        public IMessage SyncProcessMessage(IMessage msg)
        {
            IMessage retMsg = null;

            IMethodCallMessage callMsg = msg as IMethodCallMessage;

            if (callMsg != null && (Attribute.GetCustomAttribute(callMsg.MethodBase,typeof(AOPMethodAttribute))) == null)
            {
                retMsg = nextSink.SyncProcessMessage(msg);
            }
            else
            {
                Console.WriteLine("执行之前");
                retMsg = nextSink.SyncProcessMessage(msg);
                Console.WriteLine("执行之后");
            }

            return retMsg;
        }
    }
}
