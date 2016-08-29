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
        private const string PHONENUMBERREGISTRATIONSTATUS_KEY = "phoneNumberRegistrationStatus";
        private const string PHONENUMBER_KEY = "phoneNumber";
        private const string AUTHENTICATIONCODE_KEY = "authenticationCode";

        [DataMember(Name = OWNER_KEY)]
        public Identity Owner { get; set; }
        [DataMember(Name = TASKS_KEY)]
        public RoutineTask[] Tasks { get; set; }
        [DataMember(Name = SCHEDULES_KEY)]
        public RoutineTaskTimeValue[] Schedules { get; set; }
        [DataMember(Name = PHONENUMBERREGISTRATIONSTATUS_KEY)]
        public PhoneNumberRegistrationStatus PhoneNumberRegistrationStatus { get; set; }
        [DataMember(Name = PHONENUMBER_KEY)]
        public string PhoneNumber { get; set; }
        [DataMember(Name = AUTHENTICATIONCODE_KEY)]
        public string AuthenticationCode { get; set; }

        public Routine() : base(MediaType.Parse("application/x-routine+json"))
        {
            Tasks = new RoutineTask[0];
            Schedules = new RoutineTaskTimeValue[0];
        }
    }
}
