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
        public string InformSMSCode { get; set; }
        public string IDoNotWant { get; set; }
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
        public string PhoneNumberRegistrationOffer { get; set; }
        public string InformRegisterPhoneCommand { get; set; }
        public string RegistrationOkay { get; set; }
        public string RegistrationFailed { get; set; }
        public string Yes { get; set; }
        public string No { get; set; }
        public string ThisIsNotAValidPhoneNumber { get; set; }
        public string WhatAreMyNextTasks { get; set; }
    }

    public class Commands
    {
        public string Next { get; set; }
        public string Day { get; set; }
        public string Week { get; set; }
        public string New { get; set; }
        public string Confirm { get; set; }
        public string Cancel { get; set; }
        public string Delete { get; set; }
        public string Ignore { get; set; }
        public string Register { get; set; }
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
        public string WaitingPhoneNumber { get; set; }
        public string WaitingSMSCode { get; set; }
        public string WaitingInitialMenuOption { get; set; }
    }
}