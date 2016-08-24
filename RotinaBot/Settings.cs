using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RotinaBot
{
    public class Settings
    {
        public Phraseologies Phraseology { get; set; }
        public Commands Commands { get; set; }
    }

    public class Phraseologies
    {
        public string InitialMessage { get; set; }
        public string WhatIHaveForToday { get; set; }
        public string WhatIHaveForTheWeek { get; set; }
        public string IncludeATaskInMyRoutine { get; set; }
        public string ExcludeATaskFromMyRoutine { get; set; }
        public string NoTaskForToday { get; set; }
    }

    public class Commands
    {
        public string ShowMyRoutine { get; set; }
        public string NewTask { get; set; }
        public string ConfirmNewTask { get; set; }
        public string Cancel { get; set; }
        public string ShowAllMyRoutine { get; set; }
        public string DeleteTask { get; set; }
        public string ConfirmDeleteTask { get; set; }
    }
}
