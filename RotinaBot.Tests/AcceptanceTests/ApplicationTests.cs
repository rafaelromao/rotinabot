using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Shouldly;
using RotinaBot.Tests.AcceptanceTests.Base;
using RotinaBot.Tests.AcceptanceTests.Mocks;
using NUnit.Framework;

namespace RotinaBot.Tests.AcceptanceTests
{
    [TestFixture]
    public class ApplicationTests : TestClass<FakeServiceProvider>
    {
        private async Task<Select> SendHiAsync()
        {
            // Send message to the bot
            await Tester.SendMessageAsync("Oi");

            // Wait for the answer from the bot
            var response = await Tester.ReceiveMessageAsync();
            response.ShouldNotBeNull();

            var document = response.Content as Select;
            var actual = document?.Text;

            // Get the expected response
            var expected = Settings.Phraseology.InitialMessage;
            expected.ShouldNotBeNull();

            // Assert that the answer from the bot is the expected one
            actual.ShouldBe(expected);

            return document;
        }

        private async Task CreateANewTaskFromTaskNameAsync(string taskName)
        {
            Message response;
            Select select;
            string actual;
            string expected;
            PlainText document;

            // Inform task name

            await Tester.SendMessageAsync(taskName);

            response = await Tester.ReceiveMessageAsync();
            response.ShouldNotBeNull();

            select = response.Content as Select;
            actual = select?.Text;

            expected = Settings.Phraseology.WhichDaysShallThisTaskBePerformed;
            expected.ShouldNotBeNull();

            actual.ShouldBe(expected);

            // Inform task day

            await Tester.SendMessageAsync(new PlainText { Text = select?.Options.First().Order.ToString() });

            response = await Tester.ReceiveMessageAsync();
            response.ShouldNotBeNull();

            select = response.Content as Select;
            actual = select?.Text;

            expected = Settings.Phraseology.WhichTimeShallThisTaskBePerformed;
            expected.ShouldNotBeNull();

            actual.ShouldBe(expected);

            // Inform task time

            await Tester.SendMessageAsync(new PlainText { Text = select?.Options.First().Order.ToString() });

            response = await Tester.ReceiveMessageAsync();
            response.ShouldNotBeNull();

            select = response.Content as Select;
            actual = select?.Text;

            actual.ShouldStartWith(taskName);

            // Confirm the new task

            await Tester.SendMessageAsync(select?.Options.First().Value);

            response = await Tester.ReceiveMessageAsync();
            response.ShouldNotBeNull();

            document = response.Content as PlainText;
            actual = document?.Text;

            expected = Settings.Phraseology.TheTaskWasRegistered;
            expected.ShouldNotBeNull();

            actual.ShouldBe(expected);
        }



        [Test]
        public async Task SendHi()
        {
            await SendHiAsync();
        }

        [Test]
        public async Task ShowThereIsNothingForToday()
        {
            // Send messages to the bot
            var select = await SendHiAsync();

            var option = select.Options.Single(o => o.Value.ToString() == Settings.Commands.ShowMyRoutine);
            await Tester.SendMessageAsync(option.Value.ToString());

            // Wait for the answer from the bot
            var response = await Tester.ReceiveMessageAsync();
            response.ShouldNotBeNull();

            var document = response.Content as PlainText;
            var actual = document?.Text;

            // Get the expected response
            var expected = Settings.Phraseology.NoTaskForToday;
            expected.ShouldNotBeNull();

            // Assert that the answer from the bot is the expected one
            actual.ShouldBe(expected);
        }

        [Test]
        public async Task ShowThereIsNothingForTheWeek()
        {
            // Send messages to the bot
            var select = await SendHiAsync();

            var option = select.Options.Single(o => o.Value.ToString() == Settings.Commands.ShowAllMyRoutine);
            await Tester.SendMessageAsync(option.Value.ToString());

            // Wait for the answer from the bot
            var response = await Tester.ReceiveMessageAsync();
            response.ShouldNotBeNull();

            var document = response.Content as PlainText;
            var actual = document?.Text;

            // Get the expected response
            var expected = Settings.Phraseology.NoTask;
            expected.ShouldNotBeNull();

            // Assert that the answer from the bot is the expected one
            actual.ShouldBe(expected);
        }

        [Test]
        public async Task ShowThereIsNothingForDeletion()
        {
            // Send messages to the bot
            var select = await SendHiAsync();

            var option = select.Options.Single(o => o.Value.ToString() == Settings.Commands.DeleteTask);
            await Tester.SendMessageAsync(option.Value.ToString());

            // Wait for the answer from the bot
            var response = await Tester.ReceiveMessageAsync();
            response.ShouldNotBeNull();

            var document = response.Content as PlainText;
            var actual = document?.Text;

            // Get the expected response
            var expected = Settings.Phraseology.NoTask;
            expected.ShouldNotBeNull();

            // Assert that the answer from the bot is the expected one
            actual.ShouldBe(expected);
        }

        [Test]
        public async Task CreateANewTaskFromMenu()
        {
            // Say Hi

            var select = await SendHiAsync();

            var option = select.Options.Single(o => o.Value.ToString() == Settings.Commands.NewTask);
            await Tester.SendMessageAsync(option.Value.ToString());

            var response = await Tester.ReceiveMessageAsync();
            response.ShouldNotBeNull();

            var document = response.Content as PlainText;
            var actual = document?.Text;

            // Choose new task

            var expected = Settings.Phraseology.WhatIsTheTaskName;
            expected.ShouldNotBeNull();

            actual.ShouldBe(expected);

            await CreateANewTaskFromTaskNameAsync("Nova tarefa");
        }

        [Test]
        public async Task CreateANewTaskFromTaskName()
        {
            await CreateANewTaskFromTaskNameAsync("Nova tarefa");
        }
    }
}
