using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YZ.Utility
{
    public interface ITransaction : IDisposable
    {
        void Complete();
    }
}
