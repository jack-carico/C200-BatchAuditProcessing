using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CTCBAParser
{
    public interface IBatchParser
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="batchText">the batch output separated into text lines.</param>
        /// <param name="endOfAuditMarker">single line of text that is used to separate individual audits within the batch.</param>
        /// <returns></returns>
        List<IBatchAudit> ParseAuditBatch(string batchText, string endOfAuditMarker);

        List<IBatchAudit> ParseAuditBatch(System.IO.Stream textStream, string endOfAuditMarker);
    }
}
