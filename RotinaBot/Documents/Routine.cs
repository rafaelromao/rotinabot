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
        private const string LASTMORNINGREMINDER_KEY = "lastMorningReminder";
        private const string LASTAFTERNOONREMINDER_KEY = "lastAfternoonReminder";
        private const string LASTEVENINGREMINDER_KEY = "lastEveningReminder";
        private const string PHONENUMBERREGISTRATIONSTATUS_KEY = "phoneNumberRegistrationStatus";
        private const string PHONENUMBER_KEY = "phoneNumber";
        private const string AUTHENTICATIONCODE_KEY = "authenticationCode";

        [DataMember(Name = OWNER_KEY)]
        public Identity Owner { get; set; }
        [DataMember(Name = TASKS_KEY)]
        public RoutineTask[] Tasks { get; set; }

        [DataMember(Name = LASTMORNINGREMINDER_KEY)]
        public DateTime LastMorningReminder { get; set; }
        [DataMember(Name = LASTAFTERNOONREMINDER_KEY)]
        public DateTime LastAfternoonReminder { get; set; }
        [DataMember(Name = LASTEVENINGREMINDER_KEY)]
        public DateTime LastEveningReminder { get; set; }

        [DataMember(Name = PHONENUMBERREGISTRATIONSTATUS_KEY)]
        public PhoneNumberRegistrationStatus PhoneNumberRegistrationStatus { get; set; }
        [DataMember(Name = PHONENUMBER_KEY)]
        public string PhoneNumber { get; set; }
        [DataMember(Name = AUTHENTICATIONCODE_KEY)]
        public string AuthenticationCode { get; set; }

        public Routine() : base(MediaType.Parse("application/x-routine+json"))
        {
            Tasks = new RoutineTask[0];
        }
    }
}
