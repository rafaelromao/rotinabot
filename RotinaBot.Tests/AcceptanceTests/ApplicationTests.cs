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
        private ApplicationTester GetTester(bool useSecondaryAccount)
        {
            if (!useSecondaryAccount)
                return Tester;

            var secondaryOptions = Options<TServiceProvider>().Clone();
            secondaryOptions.TesterAccountIndex = 1;
            return new ApplicationTester(secondaryOptions);
        }

        protected async Task<Select> SendHiAsync()
        {
            // Send hi to the bot
            await Tester.SendMessageAsync("Oi");

            // Wait for the answer from the bot
            var response = await Tester.ReceiveMessageAsync();
            response.ShouldNotBeNull();

            var select = response.Content as Select;
            var actual = select?.Text;

            string expected;

            if (actual != Settings.Phraseology.InitialMessage)
            {
                // Receive phone number registration offer
                expected = Settings.Phraseology.PhoneNumberRegistrationOffer;
                expected.ShouldNotBeNull();

                actual.ShouldBe(expected);

                // Ignore phone number registration

                await Tester.SendMessageAsync(Settings.Commands.Ignore);
                response = await Tester.ReceiveMessageAsync();
                response.ShouldNotBeNull();

                var document = response.Content as PlainText;
                actual = document?.Text;

                expected = Settings.Phraseology.InformRegisterPhoneCommand;
                expected.ShouldNotBeNull();

                actual.ShouldBe(expected);

                await Tester.SendMessageAsync("Oi");

                response = await Tester.ReceiveMessageAsync();
                response.ShouldNotBeNull();
            }

            // Receive initial menu
            select = response.Content as Select;
            actual = select?.Text;

            // Get the expected response
            expected = Settings.Phraseology.InitialMessage;
            expected.ShouldNotBeNull();

            // Assert that the answer from the bot is the expected one
            actual.ShouldBe(expected);

            return select;
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

            await Tester.SendMessageAsync(cancel ? Settings.Commands.Cancel : Settings.Commands.Confirm);

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

            var option = select.Options.Single(o => o.Value.ToString() == Settings.Commands.Week);
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

        protected async Task RegisterPhoneNumberAsync(string phoneNumber, bool isValidPhoneNumber, bool useValidAuthenticationCode, bool useSecondaryAccount = false)
        {
            var tester = GetTester(useSecondaryAccount);

            // Send hi to the bot
            await tester.SendMessageAsync(Settings.Commands.Register);

            // Wait for the answer from the bot
            var response = await tester.ReceiveMessageAsync();
            response.ShouldNotBeNull();

            var select = response.Content as Select;
            var actual = select?.Text;


            // Receive phone number registration offer

            var expected = Settings.Phraseology.PhoneNumberRegistrationOffer;
            expected.ShouldNotBeNull();

            actual.ShouldBe(expected);

            // Send phone number

            await tester.SendMessageAsync(phoneNumber);
            response = await tester.ReceiveMessageAsync();
            response.ShouldNotBeNull();

            var document = response.Content as PlainText;
            actual = document?.Text;

            if (isValidPhoneNumber)
            {
                expected = Settings.Phraseology.InformSMSCode;
                expected.ShouldNotBeNull();

                actual.ShouldBe(expected);

                // Send SMS code
                if (useValidAuthenticationCode)
                {
                    await tester.SendMessageAsync(tester.GetService<ISMSAuthenticator>().GenerateAuthenticationCode());

                    // Assert registration was okay

                    response = await tester.ReceiveMessageAsync();
                    response.ShouldNotBeNull();

                    document = response.Content as PlainText;
                    actual = document?.Text;

                    expected = Settings.Phraseology.RegistrationOkay;
                    expected.ShouldNotBeNull();

                    actual.ShouldBe(expected);
                }
                else
                {
                    // Send Wrong SMS code
                    await tester.SendMessageAsync(tester.GetService<ISMSAuthenticator>().GenerateAuthenticationCode() + "9");

                    // Assert registration was failed

                    response = await tester.ReceiveMessageAsync();
                    response.ShouldNotBeNull();

                    select = response.Content as Select;
                    actual = select?.Text;

                    expected = Settings.Phraseology.RegistrationFailed;
                    expected.ShouldNotBeNull();

                    actual.ShouldBe(expected);
                }
            }
            else
            {
                expected = Settings.Phraseology.ThisIsNotAValidPhoneNumber;
                expected.ShouldNotBeNull();

                actual.ShouldBe(expected);
            }
        }

        protected async Task CheckASingleTaskIsListedAsync(string taskName, bool useSecondaryAccount)
        {
            var tester = GetTester(useSecondaryAccount);

            // Request the bot to show the routine for the day

            await tester.SendMessageAsync(Settings.Commands.Day);

            var response = await tester.ReceiveMessageAsync();

            var select = response.Content as Select;
            select.ShouldNotBeNull();

            select?.Options.Any(o => o.Text.StartsWith(taskName)).ShouldBeTrue();

            // Request the bot to show the routine for the week

            await tester.SendMessageAsync(Settings.Commands.Week);

            response = await tester.ReceiveMessageAsync();

            var document = response.Content as PlainText;
            document.ShouldNotBeNull();

            document?.Text.Contains(taskName).ShouldBeTrue();
        }
    }

    [TestFixture]
    public class TestApplicationWithFakeBucket : BaseTestFixture<FakeServiceProviderWithFakeBucketAndNoScheduler>
    {
        [Test]
        public async Task SendHi()
        {
            await SendHiAsync();
        }

        [Test]
        public async Task SendInvalidCommandAndCheckForFallBack()
        {
            // Send message to the bot
            await Tester.SendMessageAsync("/delete:1");

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
        }


        [Test]
        public async Task ShowThereIsNothingForToday()
        {
            // Send messages to the bot
            var select = await SendHiAsync();

            var option = select.Options.Single(o => o.Value.ToString() == Settings.Commands.Day);
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

            await CheckASingleTaskIsListedAsync(taskName, false);
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

            await Tester.SendMessageAsync(Settings.Commands.Day);

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

            await Tester.SendMessageAsync(Settings.Commands.Day);

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

            await Tester.SendMessageAsync(Settings.Commands.Day);

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

            await Tester.SendMessageAsync(Settings.Commands.Day);

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

            await Tester.SendMessageAsync(Settings.Commands.Day);

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

    [TestFixture]
    public class TestApplicationWithFakeBucketNoSchedulerAndFakeSMSSender :
        BaseTestFixture<FakeServiceProviderWithFakeBucketAndNoScheduler>
    {
        [Test]

        [TestCase("31955557777")]
        [TestCase("31-955557777")]
        [TestCase("31-95555-7777")]
        [TestCase("3195555-7777")]
        [TestCase("(31)95555-7777")]
        [TestCase("(31)955557777")]
        [TestCase("(31)-95555-7777")]
        [TestCase("3155557777")]
        [TestCase("31-55557777")]
        [TestCase("31-5555-7777")]
        [TestCase("315555-7777")]
        [TestCase("(31)5555-7777")]
        [TestCase("(31)55557777")]
        [TestCase("(31)-5555-7777")]
        public async Task RegisterPhoneNumberWithSuccess(string phoneNumber)
        {
            await RegisterPhoneNumberAsync(phoneNumber, true, true);
        }

        [Test]
        public async Task RegisterPhoneNumberWithWrongCode()
        {
            await RegisterPhoneNumberAsync("31955557777", true, false);
        }

        [Test]
        public async Task RegisterPhoneNumberWithWrongNumber()
        {
            await RegisterPhoneNumberAsync("5", false, true);
        }

        [Test]
        public async Task RegisterPhoneNumberWithSuccessAndCheckItIsNotOfferedAnymore()
        {
            await RegisterPhoneNumberAsync("31955557777", true, true);

            // Send hi to the bot
            await Tester.SendMessageAsync("Oi");

            // Wait for the answer from the bot
            var response = await Tester.ReceiveMessageAsync();
            response.ShouldNotBeNull();

            var select = response.Content as Select;
            var actual = select?.Text;

            // Receive initial menu

            var expected = Settings.Phraseology.InitialMessage;
            expected.ShouldNotBeNull();

            actual.ShouldBe(expected);
        }

        [Test]
        public async Task RegisterPhoneNumberWithSuccessUsingTwoAccounts()
        {
            await ShowThereIsNothingForTheWeekAsync();

            await RegisterPhoneNumberAsync("31955557777", true, true);

            await CreateANewTaskFromTaskNameAsync("Nova tarefa");

            await RegisterPhoneNumberAsync("31955557777", true, true, true);

            await CheckASingleTaskIsListedAsync("Nova tarefa", true);
        }
    }
}
