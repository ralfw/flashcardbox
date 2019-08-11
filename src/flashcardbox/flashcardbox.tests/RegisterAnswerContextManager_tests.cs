using System.Collections.Generic;
using flashcardbox.events;
using flashcardbox.messages.commands.registeranswer;
using FluentAssertions;
using nsimpleeventstore;
using Xunit;

namespace flashcardbox.tests
{
    public class RegisterAnswerContextManager_tests
    {
        [Fact]
        public void Load()
        {
            var es = new InMemoryEventstore();
            es.Record(new Event[] {
                new CardMovedTo{CardId = "1", BinIndex = 0},
                new CardMovedTo{CardId = "x", BinIndex = 1},
                new CardMovedTo{CardId = "2", BinIndex = 2},
                new CardMovedTo{CardId = "1", BinIndex = 1},
                new CardFoundMissing{CardId = "x"} 
            });
            
            var sut = new RegisterAnswerContextManager(es);

            var result = sut.Load(new RegisterAnswerCommand()).Ctx as RegisterAnswerContextModel;
            
            result.CardsInBins.Should().BeEquivalentTo(new Dictionary<string,int> {
                {"1", 1},
                {"2", 2}
            });
        }
    }
}