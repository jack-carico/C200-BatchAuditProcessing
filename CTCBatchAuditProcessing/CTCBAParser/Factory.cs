using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CTCBAParser
{
    public class Factory
    {
        public static IBatchParser GetBatchParser()
        {
            return Impl.BatchParser.GetInterface();
        }
    }
}
