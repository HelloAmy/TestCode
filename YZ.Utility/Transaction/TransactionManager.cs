using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Transactions;

namespace YZ.Utility
{
    public static class TransactionManager
    { 
        /// <summary>
        /// 创建事务
        /// </summary>
        /// <returns></returns>
        public static ITransaction Create()
        {
            return new TransactionScopeWrapper();
        }

        /// <summary>
        /// 排除事务
        /// </summary>
        /// <returns></returns>
        public static ITransaction SuppressTransaction()
        {
            return new TransactionScopeWrapper(TransactionScopeOption.Suppress);
        }

        private class TransactionScopeWrapper : ITransaction
        {
            private readonly TransactionScope m_Scope;

            public TransactionScopeWrapper()
            {
                m_Scope = TransactionScopeFactory.CreateTransactionScope();
            }

            public TransactionScopeWrapper(TransactionScopeOption tso)
            {
                m_Scope = TransactionScopeFactory.CreateTransactionScope(tso);
            }

            public void Complete()
            {
                m_Scope.Complete();
            }

            public void Dispose()
            {
                m_Scope.Dispose();
            }
        }
         
    }
}
