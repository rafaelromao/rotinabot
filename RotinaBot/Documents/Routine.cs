using System;
using System.Runtime.Serialization;
using Lime.Protocol;

namespace RotinaBot.Documents
{
    [DataContract(Namespace = "http://limeprotocol.org/2014")]
    public class Routine : Document
    {
        private const string OWNER_KEY = "owner";
        private const string TASKS_KEY = "tasks";
        private const string SCHEDULES_KEY = "schedules";

        [DataMember(Name = OWNER_KEY)]
        public Identity Owner { get; set; }
        [DataMember(Name = TASKS_KEY)]
        public RoutineTask[] Tasks { get; set; }
        [DataMember(Name = SCHEDULES_KEY)]
        public RoutineTaskTimeValue[] Schedules { get; set; }

        public Routine() : base(MediaType.Parse("application/x-routine+json"))
        {
            Tasks = new RoutineTask[0];
            Schedules = new RoutineTaskTimeValue[0];
        }
    }
}
