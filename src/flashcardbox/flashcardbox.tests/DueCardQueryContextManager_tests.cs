using System.Collections.Generic;
using flashcardbox.backend.events;
using flashcardbox.backend.pipelines.queries.duecard;
using flashcardbox.messages;
using flashcardbox.messages.queries;
using FluentAssertions;
using nsimpleeventstore;
using Xunit;

namespace flashcardbox.tests
{
    public class DueCardQueryContextManager_tests
    {
        [Fact]
        public void Load()
        {
            var es = new InMemoryEventstore();
            es.Record(new Event[] {
                new NewCardEncountered{Question = "q1", Answer = "a1", Tags = "t1", Id="1"}, 
                new CardMovedTo{CardId = "1", BinIndex = 1}, 
                new DueCardSelected{CardId = "1"},
                
                new NewCardEncountered{Question = "q2", Answer = "a2", Tags = "t2", Id="2"},
                new CardMovedTo{CardId = "2", BinIndex = 2}, 
                new DueCardSelected{CardId = "2"},
                
                new NewCardEncountered{Question = "q3", Answer = "a3", Tags = "t3", Id="3"},
                new CardMovedTo{CardId = "3", BinIndex = 3}, 
                new DueCardSelected{CardId = "3"},
                
                new CardFoundMissing{CardId = "3"},
                
                new DueCardSelected{CardId = "2"},
                
                new CardWasChanged{CardId = "2", Question = "q2v2", Answer = "a2v2", Tags = "t2v2"},
            });
            var sut = new DueCardContextManager(es);

            var result = sut.Load(new DueCardQuery()).Ctx as DueCardContextModel;

            result.DueCard.Should().BeEquivalentTo(new DueCardFoundQueryResult {
                CardId = "2",
                Question = "q2v2",
                Answer = "a2v2",
                Tags = "t2v2",
                BinIndex = 2
            });
        }
        
        
        [Fact]
        public void Load_with_no_due_card_selected()
        {
            var es = new InMemoryEventstore();
            es.Record(new Event[] {
                new NewCardEncountered{Question = "q1", Answer = "a1", Tags = "t1", Id="1"}, 
                new CardMovedTo{CardId = "1", BinIndex = 1}, 
            });
            var sut = new DueCardContextManager(es);

            var result = sut.Load(new DueCardQuery()).Ctx as DueCardContextModel;

            result.DueCard.Should().BeNull();
        }
        
        
        [Fact]
        public void Load_with_missing_due_card()
        {
            var es = new InMemoryEventstore();
            es.Record(new Event[] {
                new NewCardEncountered{Question = "q3", Answer = "a3", Tags = "t3", Id="3"},
                new CardMovedTo{CardId = "3", BinIndex = 3}, 
                new DueCardSelected{CardId = "3"},
                
                new CardFoundMissing{CardId = "3"},
            });
            var sut = new DueCardContextManager(es);

            var result = sut.Load(new DueCardQuery()).Ctx as DueCardContextModel;

            result.DueCard.Should().BeNull();
        }
        
        
        [Fact]
        public void Load_with_no_due_card_because_it_was_moved()
        {
            var es = new InMemoryEventstore();
            es.Record(new Event[] {
                new NewCardEncountered{Question = "q1", Answer = "a1", Tags = "t1", Id="1"}, 
                new CardMovedTo{CardId = "1", BinIndex = 1}, 
                new DueCardSelected{CardId = "1"},
                
                new CardMovedTo{CardId = "1", BinIndex = 2}, 
            });
            var sut = new DueCardContextManager(es);

            var result = sut.Load(new DueCardQuery()).Ctx as DueCardContextModel;

            result.DueCard.Should().BeNull();
        }
    }
}