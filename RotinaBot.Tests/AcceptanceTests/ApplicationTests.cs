using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol.Serialization.Newtonsoft;
using Newtonsoft.Json;
using Shouldly;
using Takenet.MessagingHub.Client.Host;
using RotinaBot.Tests.AcceptanceTests.Base;
using RotinaBot.Tests.AcceptanceTests.Mocks;

namespace RotinaBot.Tests.AcceptanceTests
{
    //[TestFixture]
    public class ApplicationTests : TestClass<FakeServiceProvider>
    {
        private string Serialize<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj, JsonNetSerializer.Settings);
        }
        private static Func<MessageApplicationReceiver, bool> ContentMatches(string command)
        {
            return r => r.Content != null && Regex.IsMatch(command, r.Content, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        //[TestCase("Hi")]
        //[Test]
        public async Task NavigationStarted(string command)
        {
            // Get the welcome receiver
            var welcomeReceiver = Tester.Application.MessageReceivers.FirstOrDefault(r => r.Type == null && ContentMatches(command)(r));

            // Get the expected response
            var expected = Serialize(welcomeReceiver?.Response?.JsonContent);
            expected.ShouldNotBeNull();

            // Send message to the bot
            await Tester.SendMessageAsync(command);

            // Wait for the answer from the bot
            var response = await Tester.ReceiveMessageAsync();
            response.ShouldNotBeNull();

            var document = response.Content as PlainText;
            var actual = Serialize(document);

            // Assert that the answer from the bot is the expected one
            actual.ShouldBe(expected);
        }
    }
}
