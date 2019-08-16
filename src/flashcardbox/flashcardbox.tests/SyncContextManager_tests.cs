using System.Collections.Generic;
using flashcardbox.backend.adapters;
using flashcardbox.backend.events;
using flashcardbox.backend.pipelines.commands.sync;
using flashcardbox.messages;
using flashcardbox.messages.commands;
using FluentAssertions;
using nsimpleeventstore;
using Xunit;

namespace flashcardbox.tests
{
    public class SyncContextManager_tests
    {
        [Fact]
        public void Load_message_context()
        {
            var es = new InMemoryEventstore();
            es.Record(new Event[]
            {
                new BoxConfigured{Bins = new[]{new BoxConfigured.Bin{LowerDueThreshold = 1,UpperDueThreshold = 2} }}, 
                
                new NewCardEncountered{Question = "q1", Answer = "a1", Tags = "t1", Id = "1"}, 
                new CardMovedTo{CardId = "1", BinIndex = 0}, 
                new NewCardEncountered{Question = "q2", Answer = "a2", Tags = "t2,t3", Id = "2"},
                new CardMovedTo{CardId = "2", BinIndex = 3},
                new NewCardEncountered{Question = "q3", Answer = "a3", Tags = "", Id = "3"},
                new CardWasChanged{Question = "q1v2", Answer = "a1v2", Tags = "t1", CardId = "1"},
                new CardMovedTo{CardId = "1", BinIndex = 2},
                new CardFoundMissing{CardId = "3"}, 
                
                new BoxConfigured{Bins = new[]{new BoxConfigured.Bin{LowerDueThreshold = 10,UpperDueThreshold = 20} }}
            });
            var sut = new SyncContextManagement(es);
            
            
            var result = sut.Load(new SyncCommand());

            
            var ctxModel = result.Ctx as SyncContextModel;
            
            ctxModel.Config.Should().BeEquivalentTo(new FlashcardboxConfig{Bins = new[]{new FlashcardboxConfig.Bin {
                LowerDueThreshold = 10,
                UpperDueThreshold = 20
            }}});            
            
            ctxModel.Flashcards.Should().BeEquivalentTo(new Dictionary<string,(string binIndex, string hash)> {
                {"1", ("2", FlashcardHash.Calculate("q1v2","a1v2","t1"))},
                {"2", ("3", FlashcardHash.Calculate("q2", "a2", "t2,t3"))}
            });
        }
    }
}