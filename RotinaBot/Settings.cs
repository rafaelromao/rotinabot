using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lime.Protocol;

namespace RotinaBot
{
    public class Settings
    {
        public Phraseologies Phraseology { get; set; }
        public Commands Commands { get; set; }
        public States States { get; set; }
    }

    public class Phraseologies
    {
        public string InitialMessage { get; set; }
        public string WhatIHaveForToday { get; set; }
        public string WhatIHaveForTheWeek { get; set; }
        public string IncludeATaskInMyRoutine { get; set; }
        public string ExcludeATaskFromMyRoutine { get; set; }
        public string NoTaskForToday { get; set; }
        public string WheneverYouNeed { get; set; }
        public string TheTaskWasRemoved { get; set; }
        public string TheTaskWasRegistered { get; set; }
        public string ConfirmDelete { get; set; }
        public string During { get; set; }
        public string Confirm { get; set; }
        public string Cancel { get; set; }
        public string TheTaskWasNotFound { get; set; }
        public string SorryYouNeedToChooseAnOption { get; set; }
        public string WhatIsTheTaskName { get; set; }
        public string HereAreYourNextTasks { get; set; }
        public string NoTask { get; set; }
        public string HereAreYourTasksForTheWeek { get; set; }
        public string ChooseATaskToBeDeleted { get; set; }
        public string HereAreYouTasksForToday { get; set; }
        public string KeepGoing { get; set; }
        public string WhichTimeShallThisTaskBePerformed { get; set; }
        public string WhichDaysShallThisTaskBePerformed { get; set; }
        public string SorryICannotHelpYouRightNow { get; set; }
    }

    public class Commands
    {
        public string Show { get; set; }
        public string New { get; set; }
        public string ConfirmNew { get; set; }
        public string Cancel { get; set; }
        public string ShowAll { get; set; }
        public string Delete { get; set; }
        public string ConfirmDelete { get; set; }
    }

    public class States
    {
        public string Default { get; set; }
        public string WaitingDeleteTaskConfirmation { get; set; }
        public string WaitingTaskSelection { get; set; }
        public string WaitingDeleteTaskSelection { get; set; }
        public string WaitingTaskTime { get; set; }
        public string WaitingTaskDays { get; set; }
        public string WaitingTaskConfirmation { get; set; }
    }
}