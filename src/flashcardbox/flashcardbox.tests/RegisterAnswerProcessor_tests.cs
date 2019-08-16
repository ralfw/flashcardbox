using System.Collections.Generic;
using flashcardbox.backend.events;
using flashcardbox.backend.pipelines.commands.registeranswer;
using flashcardbox.messages;
using FluentAssertions;
using nsimplemessagepump.contract;
using Xunit;

namespace flashcardbox.tests
{
    public class RegisterAnswerProcessor_tests
    {
        [Fact]
        public void Register_success_by_moving_card_to_next_bin()
        {
            var ctx = new RegisterAnswerContextModel
            {
                CardsInBins = new Dictionary<string, int> {
                    {"1", 1},
                    {"2", 2}
                }
            };
            var sut = new RegisterAnswerProcessor();

            var (status, events, version, notifications) = sut.Process(new RegisterAnswerCommand {CardId = "1", CorrectlyAnswered = true}, ctx, "");

            status.Should().BeOfType<Success>();
            events.Should().BeEquivalentTo(
                new QuestionAnsweredCorrectly{CardId = "1", Id = events[0].Id},
                new CardMovedTo{CardId = "1", BinIndex = 2, Id = events[1].Id});
            notifications.Should().BeEmpty();
        }
        
        
        [Fact]
        public void Register_failure_by_moving_card_back_to_bin_1()
        {
            var ctx = new RegisterAnswerContextModel
            {
                CardsInBins = new Dictionary<string, int> {
                    {"1", 1},
                    {"2", 2}
                }
            };
            var sut = new RegisterAnswerProcessor();

            var (status, events, version, notifications) = sut.Process(new RegisterAnswerCommand {CardId = "1", CorrectlyAnswered = false}, ctx, "");

            status.Should().BeOfType<Success>();
            events.Should().BeEquivalentTo(
                new QuestionAnsweredIncorrectly{CardId = "1", Id = events[0].Id},
                new CardMovedTo{CardId = "1", BinIndex = 1, Id = events[1].Id});
            notifications.Should().BeEmpty();
        }
        
        
        [Fact]
        public void Refuse_regiatration_for_non_existent_card()
        {
            var ctx = new RegisterAnswerContextModel
            {
                CardsInBins = new Dictionary<string, int> {
                    {"1", 1},
                    {"2", 2}
                }
            };
            var sut = new RegisterAnswerProcessor();

            var (status, events, version, notifications) = sut.Process(new RegisterAnswerCommand {CardId = "x", CorrectlyAnswered = true}, ctx, "");

            status.Should().BeOfType<Failure>();
            events.Should().BeEmpty();
            notifications.Should().BeEmpty();
        }
    }
}