using System.Linq;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Shouldly;
using RotinaBot.Tests.AcceptanceTests.Base;
using RotinaBot.Tests.AcceptanceTests.Mocks;
using NUnit.Framework;

namespace RotinaBot.Tests.AcceptanceTests
{
    [TestFixture]
    public class ApplicationTests : TestClass<FakeServiceProvider>
    {
        [Test]
        public async Task SendHi()
        {
            await SendHiAsync();
        }

        private async Task<Select> SendHiAsync()
        {
            // Get the expected response
            var expected = Settings.Phraseology.InitialMessage;
            expected.ShouldNotBeNull();

            // Send message to the bot
            await Tester.SendMessageAsync("Oi");

            // Wait for the answer from the bot
            var response = await Tester.ReceiveMessageAsync();
            response.ShouldNotBeNull();

            var document = response.Content as Select;
            var actual = document?.Text;

            // Assert that the answer from the bot is the expected one
            actual.ShouldBe(expected);

            return document;
        }

        [Test]
        public async Task ShowThereIsNothingForToday()
        {
            var select = await SendHiAsync();

            // Get the expected response
            var expected = Settings.Phraseology.NoTaskForToday;
            expected.ShouldNotBeNull();

            // Send message to the bot
            var option = select.Options.Single(o => o.Value.ToString() == Settings.Commands.ShowMyRoutine);
            await Tester.SendMessageAsync(option.Value.ToString());

            // Wait for the answer from the bot
            var response = await Tester.ReceiveMessageAsync();
            response.ShouldNotBeNull();

            var document = response.Content as PlainText;
            var actual = document?.Text;

            // Assert that the answer from the bot is the expected one
            actual.ShouldBe(expected);
        }
    }
}
