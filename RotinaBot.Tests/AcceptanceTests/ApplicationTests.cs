using System.Linq;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Shouldly;
using RotinaBot.Tests.AcceptanceTests.Base;
using RotinaBot.Tests.AcceptanceTests.Mocks;
using NUnit.Framework;
using RotinaBot.Documents;

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

        private async Task CreateANewTaskFromTaskNameAsync(
            string taskName, 
            RoutineTaskDaysValue days = RoutineTaskDaysValue.EveryDay, 
            RoutineTaskTimeValue time = RoutineTaskTimeValue.Morning, 
            bool cancel = false)
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

            await Tester.SendMessageAsync(new PlainText { Text = ((int)days).ToString() });

            response = await Tester.ReceiveMessageAsync();
            response.ShouldNotBeNull();

            select = response.Content as Select;
            actual = select?.Text;

            expected = Settings.Phraseology.WhichTimeShallThisTaskBePerformed;
            expected.ShouldNotBeNull();

            actual.ShouldBe(expected);

            // Inform task time

            await Tester.SendMessageAsync(new PlainText { Text = ((int)time).ToString() });

            response = await Tester.ReceiveMessageAsync();
            response.ShouldNotBeNull();

            select = response.Content as Select;
            actual = select?.Text;

            actual.ShouldStartWith(taskName);

            // Confirm the new task

            await Tester.SendMessageAsync(cancel ? Settings.Commands.Cancel : Settings.Commands.ConfirmNew );

            response = await Tester.ReceiveMessageAsync();
            response.ShouldNotBeNull();

            document = response.Content as PlainText;
            actual = document?.Text;

            expected = cancel ? Settings.Phraseology.WheneverYouNeed : Settings.Phraseology.TheTaskWasRegistered;

            expected.ShouldNotBeNull();

            actual.ShouldBe(expected);
        }

        private async Task ShowThereIsNothingForTheWeekAsync()
        {
            // Send messages to the bot
            var select = await SendHiAsync();

            var option = @select.Options.Single(o => o.Value.ToString() == Settings.Commands.ShowAll);
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
        public async Task SendHi()
        {
            await SendHiAsync();
        }

        [Test]
        public async Task ShowThereIsNothingForToday()
        {
            // Send messages to the bot
            var select = await SendHiAsync();

            var option = select.Options.Single(o => o.Value.ToString() == Settings.Commands.Show);
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
            await ShowThereIsNothingForTheWeekAsync();
        }

        [Test]
        public async Task ShowThereIsNothingForDeletion()
        {
            // Send messages to the bot
            var select = await SendHiAsync();

            var option = select.Options.Single(o => o.Value.ToString() == Settings.Commands.Delete);
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

            var option = select.Options.Single(o => o.Value.ToString() == Settings.Commands.New);
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

        [Test]
        public async Task StartCreatingANewTaskAndThenCancel()
        {
            await CreateANewTaskFromTaskNameAsync("Nova tarefa", cancel: true);
        }

        [Test]
        public async Task CheckANewTaskIsListed()
        {
            // Ensure there is no task already registered

            await ShowThereIsNothingForTheWeekAsync();

            // Create a new task

            const string taskName = "Nova tarefa";

            await CreateANewTaskFromTaskNameAsync(taskName);

            // Request the bot to show the routine for the day

            await Tester.SendMessageAsync(Settings.Commands.Show);

            var response = await Tester.ReceiveMessageAsync();

            var select = response.Content as Select;
            select.ShouldNotBeNull();

            select?.Options.Any(o => o.Text.StartsWith(taskName)).ShouldBeTrue();

            // Request the bot to show the routine for the week

            await Tester.SendMessageAsync(Settings.Commands.ShowAll);

            response = await Tester.ReceiveMessageAsync();

            var document = response.Content as PlainText;
            document.ShouldNotBeNull();

            document?.Text.Contains(taskName).ShouldBeTrue();
        }

        [Test]
        public async Task CheckTwoNewTasksWithSameNameAreListed()
        {
            // Ensure there is no task already registered

            await ShowThereIsNothingForTheWeekAsync();

            // Create a new task

            const string taskName = "Nova tarefa";

            await CreateANewTaskFromTaskNameAsync(taskName);

            // Create another new task

            await CreateANewTaskFromTaskNameAsync(taskName);

            // Request the bot to show the routine for the day

            await Tester.SendMessageAsync(Settings.Commands.Show);

            var response = await Tester.ReceiveMessageAsync();

            var select = response.Content as Select;
            select.ShouldNotBeNull();

            select?.Options.Length.ShouldBe(3);

            // Cancel the task selection

            await Tester.SendMessageAsync(Settings.Commands.Cancel);

            await Tester.IgnoreMessageAsync();
        }

        [Test]
        public async Task InsertATaskNotForTodayAndCheckItIsListedOkay()
        {
            // Ensure there is no task already registered

            await ShowThereIsNothingForTheWeekAsync();

            // Create a new task

            const string taskName = "Nova tarefa";

            await CreateANewTaskFromTaskNameAsync(taskName, RoutineTaskDaysValue.WeekEnds);

            // Create another new task

            await CreateANewTaskFromTaskNameAsync(taskName, RoutineTaskDaysValue.WorkDays);

            // Create a third new task

            await CreateANewTaskFromTaskNameAsync(taskName, RoutineTaskDaysValue.EveryDay);

            // Request the bot to show the routine for the day

            await Tester.SendMessageAsync(Settings.Commands.Show);

            var response = await Tester.ReceiveMessageAsync();

            var select = response.Content as Select;
            select.ShouldNotBeNull();

            select?.Options.Length.ShouldBe(3);

            // Cancel the task selection

            await Tester.SendMessageAsync(Settings.Commands.Cancel);

            await Tester.IgnoreMessageAsync();
        }

    }
}
