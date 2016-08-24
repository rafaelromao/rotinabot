using System.Runtime.Serialization;
using Lime.Protocol;

namespace RotinaBot.Documents
{
    [DataContract(Namespace = "http://limeprotocol.org/2014")]
    public class RoutineTaskDays : Document
    {
        private const string VALUE_KEY = "value";

        [DataMember(Name = VALUE_KEY)]
        public RoutineTaskDaysValue Value { get; set; }

        public RoutineTaskDays() : base(MediaType.Parse("application/x-routinetaskdays+json")) { }
    }

    public enum RoutineTaskDaysValue
    {
        EveryDay,
        WorkDays,
        WeekEnds
    }

    internal static class RoutineTaskDaysValueExtension
    {
        public static string Name(this RoutineTaskDaysValue value)
        {
            switch (value)
            {
                default:
                case RoutineTaskDaysValue.EveryDay:
                    return "Todos os dias";
                case RoutineTaskDaysValue.WorkDays:
                    return "Nos dias de semana";
                case RoutineTaskDaysValue.WeekEnds:
                    return "Nos finais de semana";
            }
        }
    }

    internal static class RoutineTaskDaysExtension
    {
        public static RoutineTaskDaysValue GetValueOrDefault(this RoutineTaskDays days)
        {
            return days?.Value ?? RoutineTaskDaysValue.EveryDay;
        }
    }
}