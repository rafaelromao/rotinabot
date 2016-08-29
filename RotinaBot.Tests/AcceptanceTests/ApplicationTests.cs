using System;
using System.Linq;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Shouldly;
using RotinaBot.Tests.AcceptanceTests.Mocks;
using NUnit.Framework;
using RotinaBot.Documents;
using Takenet.MessagingHub.Client.Tester;

namespace RotinaBot.Tests.AcceptanceTests
{
    public class BaseTestFixture<TServiceProvider> : Base.TestClass<TServiceProvider>
        where TServiceProvider : ApplicationTesterServiceProvider
    {
        protected async Task<Select> SendHiAsync()
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

        protected async Task CreateANewTaskFromTaskNameAsync(
            string taskName,
            RoutineTaskDaysValue days = RoutineTaskDaysValue.EveryDay,
            RoutineTaskTimeValue time = RoutineTaskTimeValue.Morning,
            bool cancel = false)
        {
            // Inform task name

            await Tester.SendMessageAsync(taskName);

            var response = await Tester.ReceiveMessageAsync();
            response.ShouldNotBeNull();

            var select = response.Content as Select;
            var actual = select?.Text;

            var expected = Settings.Phraseology.WhichDaysShallThisTaskBePerformed;
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

            await Tester.SendMessageAsync(cancel ? Settings.Commands.Cancel : Settings.Commands.ConfirmNew);

            response = await Tester.ReceiveMessageAsync();
            response.ShouldNotBeNull();

            var document = response.Content as PlainText;
            actual = document?.Text;

            expected = cancel ? Settings.Phraseology.WheneverYouNeed : Settings.Phraseology.TheTaskWasRegistered;

            expected.ShouldNotBeNull();

            actual.ShouldBe(expected);
        }

        protected async Task ShowThereIsNothingForTheWeekAsync()
        {
            // Send messages to the bot
            var select = await SendHiAsync();

            var option = select.Options.Single(o => o.Value.ToString() == Settings.Commands.ShowAll);
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
    }

    [TestFixture]
    public class TestApplicationWithFakeBucket : BaseTestFixture<FakeServiceProviderWithFakeBucket>
    {
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
        public async Task InsertATaskMarkItAsCompletedAndCheckItIsListedOkay()
        {
            // Ensure there is no task already registered

            await ShowThereIsNothingForTheWeekAsync();

            // Create a new task

            const string taskName = "Nova tarefa";

            await CreateANewTaskFromTaskNameAsync(taskName, RoutineTaskDaysValue.WeekEnds);

            // Create another new task

            await CreateANewTaskFromTaskNameAsync(taskName, RoutineTaskDaysValue.WorkDays);

            // Create a third new task

            await CreateANewTaskFromTaskNameAsync(taskName);

            // Request the bot to show the routine for the day

            await Tester.SendMessageAsync(Settings.Commands.Show);

            var response = await Tester.ReceiveMessageAsync();

            var select = response.Content as Select;
            select.ShouldNotBeNull();

            select?.Options.Length.ShouldBe(3);

            // Mark the first task as completed

            await Tester.SendMessageAsync(select?.Options.First().Value);

            response = await Tester.ReceiveMessageAsync();

            var document = response.Content as PlainText;
            var actual = document?.Text;

            var expected = Settings.Phraseology.KeepGoing;
            expected.ShouldNotBeNull();

            actual.ShouldBe(expected);

            // Request the bot to show the routine for the day again

            await Tester.SendMessageAsync(Settings.Commands.Show);

            response = await Tester.ReceiveMessageAsync();

            select = response.Content as Select;
            select.ShouldNotBeNull();

            select?.Options.Length.ShouldBe(2);

            // Cancel the task selection

            await Tester.SendMessageAsync(Settings.Commands.Cancel);

            await Tester.IgnoreMessageAsync();
        }

        [Test]
        public async Task InsertTwoNewTasksAndMarkBothAsCompletedInSequence()
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

            // Mark the first task as completed

            await Tester.SendMessageAsync(select?.Options[0].Value);

            response = await Tester.ReceiveMessageAsync();

            var document = response.Content as PlainText;
            var actual = document?.Text;

            var expected = Settings.Phraseology.KeepGoing;
            expected.ShouldNotBeNull();

            actual.ShouldBe(expected);

            // Mark the second task as completed

            await Tester.SendMessageAsync(select?.Options[1].Value);

            response = await Tester.ReceiveMessageAsync();

            document = response.Content as PlainText;
            actual = document?.Text;

            expected = Settings.Phraseology.KeepGoing;
            expected.ShouldNotBeNull();

            actual.ShouldBe(expected);

            await Tester.IgnoreMessageAsync();
        }

        [Test]
        public async Task InsertATaskRemoveItAndCheckItIsNotListed()
        {
            // Ensure there is no task already registered

            await ShowThereIsNothingForTheWeekAsync();

            // Create a new task

            const string taskName = "Nova tarefa";

            await CreateANewTaskFromTaskNameAsync(taskName, RoutineTaskDaysValue.WeekEnds);

            // Request the bot to delete a task

            await Tester.SendMessageAsync(Settings.Commands.Delete);

            var response = await Tester.ReceiveMessageAsync();

            var select = response.Content as Select;
            select.ShouldNotBeNull();

            var expected = Settings.Phraseology.ChooseATaskToBeDeleted;
            expected.ShouldNotBeNull();

            var actual = select?.Text;
            actual.ShouldBe(expected);

            select?.Options.Length.ShouldBe(2);

            // Select the first task to be deleted

            await Tester.SendMessageAsync(select?.Options.First().Value);

            response = await Tester.ReceiveMessageAsync();

            select = response.Content as Select;
            select.ShouldNotBeNull();

            expected = Settings.Phraseology.Confirm;
            expected.ShouldNotBeNull();

            actual = select?.Options.First().Text;
            actual.ShouldBe(expected);

            select?.Options.Length.ShouldBe(2);

            // Confirm the deletion

            await Tester.SendMessageAsync(select?.Options.First().Value);

            response = await Tester.ReceiveMessageAsync();

            var document = response.Content as PlainText;
            actual = document?.Text;

            expected = Settings.Phraseology.TheTaskWasRemoved;
            expected.ShouldNotBeNull();

            actual.ShouldBe(expected);

            // Request the bot to show all the tasks

            await ShowThereIsNothingForTheWeekAsync();
        }
    }

    [TestFixture]
    public class TestApplicationWithFakeBucketAndScheduler : BaseTestFixture<FakeServiceProviderWithFakeBucketAndScheduler>
    {
        [Test]
        public async Task InsertATaskAndCheckIfTheScheduledRemiderIsSent()
        {
            // Ensure there is no task already registered

            await ShowThereIsNothingForTheWeekAsync();

            // Create a new task

            const string taskName = "Nova tarefa";

            var hour = DateTime.Now.Hour;
            var time = hour > 18
                ? RoutineTaskTimeValue.Evening
                : (hour > 12 ? RoutineTaskTimeValue.Afternoon : RoutineTaskTimeValue.Morning);
            await CreateANewTaskFromTaskNameAsync(taskName, time: time);

            // The bot should show the next tasks

            var response = await Tester.ReceiveMessageAsync();

            var select = response.Content as Select;
            select.ShouldNotBeNull();

            select.Text.ShouldBe(Settings.Phraseology.HereAreYourNextTasks);
        }
    }
}
