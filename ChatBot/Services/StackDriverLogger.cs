using Google.Api;
using Google.Api.Gax.Grpc;
using Google.Cloud.Logging.Type;
using Google.Cloud.Logging.V2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatBot.Services
{
    public class StackDriverLogger : IStackDriverLogger
    {
        private readonly string _projectId;
        private readonly CallSettings _retryAWhile;

        public StackDriverLogger(string projectId)
        {
            _projectId = projectId;
            _retryAWhile = CallSettings.FromCallTiming(CallTiming.FromRetry(new RetrySettings(
            new BackoffSettings(TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(12), 2.0),
            new BackoffSettings(TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(120)),
            Google.Api.Gax.Expiration.FromTimeout(TimeSpan.FromSeconds(180)),
            (Grpc.Core.RpcException e) => 
                new[] { Grpc.Core.StatusCode.Internal, Grpc.Core.StatusCode.DeadlineExceeded }
                .Contains(e.Status.StatusCode)
            )));
        }

        public Task WriteLogEntryAsync(string logId, string message, IDictionary<string, string> entryLabels)
        {
            return Execute(_projectId, logId, message, entryLabels);
        }

        public async Task Execute(string projectId, string logId, string message, IDictionary<string, string> entryLabels)
        {
            var client = LoggingServiceV2Client.Create();
            LogName logName = new LogName(projectId, logId);
            LogEntry logEntry = new LogEntry
            {
                LogName = logName.ToString(),
                Severity = LogSeverity.Info,
                TextPayload = $"ChatBot - {message}"
            };
            MonitoredResource resource = new MonitoredResource { Type = "global" };
            if(entryLabels == null)
            {
                entryLabels = new Dictionary<string, string>
                {
                    { "Timestamp", DateTime.Now.ToString() }
                };
            }
            else
            {
                entryLabels.Add(new KeyValuePair<string, string>("Timestamp", DateTime.Now.ToString()));
            }
            await Task.Run(() => client.WriteLogEntries(
                LogNameOneof.From(logName), resource, entryLabels, new[] { logEntry }, _retryAWhile));
        }
    }
}
