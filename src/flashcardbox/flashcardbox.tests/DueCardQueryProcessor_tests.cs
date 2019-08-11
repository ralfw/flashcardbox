using flashcardbox.messages.queries;
using flashcardbox.messages.queries.duecard;
using FluentAssertions;
using Xunit;

namespace flashcardbox.tests
{
    public class DueCardQueryProcessor_tests
    {
        [Fact]
        public void Process()
        {
            var sut = new DueCardQueryProcessor();

            var ctx = new DueCardQueryContextModel {
                DueCard = new DueCardQueryResult{CardId = "1", Question = "q1"}
            };
            
            var result = sut.Process(new DueCardQuery(), ctx);
            
            result.Should().BeEquivalentTo(new DueCardQueryResult {
                CardId = "1",
                Question = "q1"
            });
        }
        
        
        [Fact]
        public void Process_with_no_due_card_found()
        {
            var sut = new DueCardQueryProcessor();

            var ctx = new DueCardQueryContextModel {
                DueCard = null
            };

            var result = sut.Process(new DueCardQuery(), ctx) as DueCardQueryResult;

            result.CardId.Should().BeEmpty();
        }
    }
}