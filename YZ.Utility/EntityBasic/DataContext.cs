using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YZ.Utility.EntityBasic
{
    public class DataContext
    {
        private const string ContextIDKey = "main-task-id";
        private static ConcurrentDictionary<string, Dictionary<string, object>> Contexts { get; set; }
        private static object safeContextLocker = new object();

        public static Dictionary<string, object> SafeContext()
        {
            lock (safeContextLocker)
            {
                string contextId = GetCurrentContextId();
                if (Contexts == null)
                {
                    Contexts = new ConcurrentDictionary<string, Dictionary<string, object>>();
                }

                Dictionary<string, object> context;
                if (!Contexts.ContainsKey(contextId) || Contexts[contextId] == null)
                    context = Contexts[contextId] = new Dictionary<string, object>();
                else
                    context = Contexts[contextId];

                return context;
            }
        }

        public static void SetContextItem(string key, object val)
        {
            //string contextId = GetCurrentContextId();
            //if (Contexts == null)
            //{
            //    Contexts = new ConcurrentDictionary<string, Dictionary<string, object>>();
            //}

            //Dictionary<string, object> context;
            //if (!Contexts.ContainsKey(contextId) || Contexts[contextId] == null)
            //    context = Contexts[contextId] = new Dictionary<string, object>();
            //else
            //    context = Contexts[contextId];

            var context = SafeContext();
            context[key] = val;
        }

        private static string GetCurrentContextId()
        {
            object contextId = CallContext.LogicalGetData(ContextIDKey);
            if (contextId == null)
            {
                var newTaskId = Task.CurrentId.HasValue ? Task.CurrentId.ToString() : Guid.NewGuid().ToString();
                CallContext.LogicalSetData(ContextIDKey, newTaskId);
                return newTaskId;
            }
            else
            {
                return contextId as string;
            }
        }

        public static void RemoveContext()
        {
            string contextId = GetCurrentContextId();
            Dictionary<string, object> currentContext;
            Contexts.TryRemove(contextId, out currentContext);
            CallContext.FreeNamedDataSlot(ContextIDKey);
        }

        /// <summary>
        /// 获取上下文中的键值内容
        /// </summary>
        /// <param name="key">键名</param>
        /// <returns>键值内容</returns>
        public static object GetContextItem(string key)
        {
            string contextId = GetCurrentContextId();
            if (Contexts != null && Contexts.ContainsKey(contextId)
                && Contexts[contextId] != null && Contexts[contextId].ContainsKey(key))
                return Contexts[contextId][key];
            else
                return null;
        }

        public static int GetContextItemInt(string key, int defaultValue)
        {
            object orgValue = GetContextItem(key);
            if (orgValue == null)
                return defaultValue;

            string stringValue = GetContextItem(key) as string;
            int ret;
            if (int.TryParse(stringValue, out ret))
            {
                return ret;
            }
            else
            {
                return defaultValue;
            }
        }


        public static string GetContextItemString(string key)
        {
            object orgValue = GetContextItem(key);
            if (orgValue == null)
                return null;
            return GetContextItem(key).ToString();
        }

    }
}
