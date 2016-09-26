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
    public class DelayProxyUtil
    {
        /// <summary>
        /// 调用被代理对象中方法，返回 被代理对象的 方法返回值
        /// </summary>
        /// <param name="target">目标对象</param>
        /// <param name="callmessage">方法</param>
        /// <returns>结果</returns>
        public static IMessage InvokeBeProxy(object target, IMethodCallMessage callmessage)
        {
            // 参数
            var args = callmessage.Args;

            // 返回值
            object returnValue = callmessage.MethodBase.Invoke(target, args);
 
            return new ReturnMessage(returnValue, args, args.Length, callmessage.LogicalCallContext, callmessage);
        }

        /// <summary>
        /// 抛出异常的情况
        /// </summary>
        /// <param name="ex">异常</param>
        /// <param name="callMessage">方法</param>
        /// <returns>结果</returns>
        public static IMessage ReturnException(Exception ex, IMethodCallMessage callMessage)
        {
            return new ReturnMessage(ex, callMessage);
        }

        /// <summary>
        /// 获取对象代理
        /// </summary>
        /// <param name="type"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static object GetTransparentProxy(Type type, object instance)
        {
            Type tempType = typeof(DelayProxy<>);

            tempType = tempType.MakeGenericType(type);

            RealProxy proxy = Activator.CreateInstance(tempType, new object[] { instance } )as RealProxy;

            return proxy.GetTransparentProxy();
        }
    }

}
