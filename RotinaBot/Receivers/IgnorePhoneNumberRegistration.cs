using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;

namespace RotinaBot.Receivers
{
    public class IgnorePhoneNumberRegistration : BaseMessageReceiver
    {
        public IgnorePhoneNumberRegistration(RotinaBot bot) : base(bot)
        {
        }

        public override async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            await Bot.IgnorePhoneNumberRegistrationAsync(message.From, cancellationToken);
            await Bot.InformPhoneNumberRegistrationCommandAsync(message.From, cancellationToken);
            await Task.Delay(TimeSpan.FromMilliseconds(300), cancellationToken);
            await Bot.SendInitialMenuAsync(message.From, cancellationToken);
            Bot.StateManager.SetState(message.From, Bot.Settings.States.Default);
        }
    }
}