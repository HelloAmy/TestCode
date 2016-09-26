using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace _01CommonProject._02AOP
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class AOPnterceptorAttribute : ContextAttribute, IContributeObjectSink
    {
        public AOPnterceptorAttribute()
            : base("AOPnterceptorAttribute")
        {

        }

        //实现IContributeObjectSink接口当中的消息接收器接口
        public IMessageSink GetObjectSink(MarshalByRefObject obj, IMessageSink next)
        {
            return new MyAopHandler(next);
        }
    }
}
