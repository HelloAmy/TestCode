using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YZ.Utility
{
    /// <summary>
    /// 编码、名称 这类key-vale模式的简单数据类型
    /// </summary>
    public class CodeNamePair
    {
        public string Code { get; set; }

        public string Name { get; set; }

        public override string ToString()
        {
            return Code;
        }

        public static implicit operator CodeNamePair(string code)
        {
            return new CodeNamePair { Code = code };
        }

        public static implicit operator CodeNamePair(int code)
        {
            return new CodeNamePair { Code = code.ToString() };
        }
    }
}
