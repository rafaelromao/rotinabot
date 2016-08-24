using System.Runtime.Serialization;
using Lime.Protocol;

namespace RotinaBot.Documents
{
    [DataContract(Namespace = "http://limeprotocol.org/2014")]
    public class RoutineTaskTime : Document
    {
        private const string VALUE_KEY = "value";

        [DataMember(Name = VALUE_KEY)]
        public RoutineTaskTimeValue Value { get; set; }

        public RoutineTaskTime() : base(MediaType.Parse("application/x-routinetasktime+json")) { }
    }

    public enum RoutineTaskTimeValue
    {
        Morning,
        Afternoon,
        Evening
    }

    internal static class RoutineTaskTimeValueExtension
    {
        public static string Name(this RoutineTaskTimeValue value)
        {
            switch (value)
            {
                default:
                case RoutineTaskTimeValue.Morning:
                    return "Manhã";
                case RoutineTaskTimeValue.Afternoon:
                    return "Tarde";
                case RoutineTaskTimeValue.Evening:
                    return "Noite";
            }
        }
    }

    internal static class RoutineTaskTimeExtension
    {
        public static RoutineTaskTimeValue GetValueOrDefault(this RoutineTaskTime days)
        {
            return days?.Value ?? RoutineTaskTimeValue.Evening;
        }
    }

}