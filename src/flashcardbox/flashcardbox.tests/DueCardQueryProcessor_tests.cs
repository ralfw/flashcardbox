using flashcardbox.backend.pipelines.queries.duecard;
using flashcardbox.messages;
using flashcardbox.messages.queries;
using FluentAssertions;
using Xunit;

namespace flashcardbox.tests
{
    public class DueCardQueryProcessor_tests
    {
        [Fact]
        public void Process()
        {
            var sut = new DueCardProcessor();

            var ctx = new DueCardContextModel {
                DueCard = new DueCardFoundQueryResult(){CardId = "1", Question = "q1"}
            };
            
            var result = sut.Process(new DueCardQuery(), ctx);
            
            result.Should().BeEquivalentTo(new DueCardFoundQueryResult {
                CardId = "1",
                Question = "q1"
            });
        }
        
        
        [Fact]
        public void Process_with_no_due_card_found()
        {
            var sut = new DueCardProcessor();

            var ctx = new DueCardContextModel {
                DueCard = null
            };

            sut.Process(new DueCardQuery(), ctx).Should().BeOfType<DueCardNotFoundQueryResult>();
        }
    }
}