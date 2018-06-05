using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CTCBAStorage
{
    public class Factory
    {
        public static IAuditStorage GetAuditStorage()
        {
            return Impl.AuditStorage.GetInterface();
        }
    }
}
