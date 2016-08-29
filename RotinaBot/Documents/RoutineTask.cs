using System;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Lime.Messaging.Contents;
using Lime.Protocol;

namespace RotinaBot.Documents
{
    [DataContract(Namespace = "http://limeprotocol.org/2014")]
    public class RoutineTask : Document
    {
        private const string ID_KEY = "id";
        private const string OWNER_KEY = "name";
        private const string DAYS_KEY = "days";
        private const string TIME_KEY = "time";
        private const string ISACTIVE_KEY = "isactive";
        private const string LASTTIME_KEY = "lasttime";

        [DataMember(Name = ID_KEY)]
        public long Id { get; set; }
        [DataMember(Name = OWNER_KEY)]
        public string Name { get; set; }
        [DataMember(Name = DAYS_KEY)]
        public RoutineTaskDays Days { get; set; }
        [DataMember(Name = TIME_KEY)]
        public RoutineTaskTime Time { get; set; }
        [DataMember(Name = ISACTIVE_KEY)]
        public bool IsActive { get; set; }
        [DataMember(Name = LASTTIME_KEY)]
        public DateTimeOffset LastTime { get; set; }

        public TimeSpan Delay => DateTime.Today - LastTime;

        public RoutineTask() : base(MediaType.Parse("application/x-routinetask+json")) {}

        public static string ExtractTaskIdFromCompleteCommand(string externalTaskId)
        {
            var pattern = new Regex(@"^(\/complete:(?<taskId>.*))$");
            var match = pattern.Match(externalTaskId);
            var taskId = match.Groups["taskId"].Value;
            return taskId;
        }

        public static string ExtractTaskIdFromDeleteCommand(string externalTaskId)
        {
            var pattern = new Regex(@"^(\/delete:(?<taskId>.*))$");
            var match = pattern.Match(externalTaskId);
            var taskId = match.Groups["taskId"].Value;
            return taskId;
        }

        public static string CreateCompleteCommand(string taskId)
        {
            return $"/complete:{taskId}";
        }

        public static string CreateDeleteCommand(string taskId)
        {
            return $"/delete:{taskId}";
        }
    }
}