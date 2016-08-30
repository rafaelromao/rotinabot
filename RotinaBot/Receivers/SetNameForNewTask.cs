using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;

namespace RotinaBot.Receivers
{
    public class SetNameForNewTask : BaseMessageReceiver
    {
        public SetNameForNewTask(RotinaBot bot) : base(bot)
        {
        }

        public override async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            try
            {
                if (Bot.StateManager.GetState(message.From) == Bot.Settings.States.WaitingPhoneNumber)
                    return;

                await Bot.SetNameForNewTaskAsync(message.From, message.Content, cancellationToken);
                await Bot.SendTaskDaysRequestAsync(message.From, cancellationToken);
                Bot.StateManager.SetState(message.From, Bot.Settings.States.WaitingTaskDays);
            }
            catch (Exception)
            {
                await Bot.InformAProblemHasOcurredAsync(message.From, cancellationToken);
            }
        }
    }
}