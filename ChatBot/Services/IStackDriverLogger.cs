using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatBot.Services
{
    public interface IStackDriverLogger
    {
        Task WriteLogEntryAsync(string logId, string message, IDictionary<string, string> entryLabels);
    }
}
