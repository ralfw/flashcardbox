using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using flashcardbox.events;
using flashcardbox.messages.commands;
using flashcardbox.messages.commands.sync;
using FluentAssertions;
using nsimpleeventstore;
using nsimplemessagepump.contract;
using Xunit;

namespace flashcardbox.tests
{
    public class SyncCommand_tests
    {
        //TODO: overwrite config because it has changed
        
        [Fact]
        public void Process_without_previous_config()
        {
            const string BOX_PATH = "sampledb_sync_processing";
            
            var ctx = new SyncCommandMessageContextModel {
                Flashcards = new Dictionary<string, (string binIndex, string hash)> {
                    {"2", ("2", FlashcardHash.Calculate("unchanged q2","a2","t2"))},
                    {"3", ("4", FlashcardHash.Calculate("q3","a3",""))}, // to be changed
                    {"99", ("3", "xyz")} // to be deleted
                },
                Config = new FlashcardboxConfig()
            };
            var db = new FlashcardboxDb(BOX_PATH);

            var sut = new SyncCommandProcessor(db);
            
            File.Copy(Path.Combine(BOX_PATH, "flashcards original.csv"),
                      Path.Combine(BOX_PATH, "flashcards.csv"), true);

            
            var (status, events, version, notifications) = sut.Process(new SyncCommand(), ctx, "");

            
            Assert.IsType<Success>(status);
            events.Should().BeEquivalentTo(new Event[]{
                new CardImported{Question = "new q1", Answer = "a1", Tags = "t1", Id = events[0].Id}, 
                new CardMovedTo{CardId = events[0].Id, BinIndex = 0, Id = events[1].Id}, 
                
                new CardChanged{CardId = "3", Question = "changed q3v2", Answer = "a3", Tags = "", Id = events[2].Id}, 
                
                new CardImported{Question = "new with bin q4", Answer = "a4", Tags = "t4", Id = events[3].Id}, 
                new CardMovedTo{CardId = events[3].Id, BinIndex = 9, Id = events[4].Id}, 
                
                new CardDeleted(){CardId = "99", Id = events[5].Id},
                
                new BoxConfigured{ Bins = new[]{new BoxConfigured.Bin{LowerDueThreshold = 10, UpperDueThreshold = 20}}, Id = events[6].Id} 
            });

            var flashcards = db.LoadFlashcards();
            flashcards.Should().BeEquivalentTo(new[]
            {
                new FlashcardRecord{Question = "new q1", Answer = "a1", Tags = "t1", BinIndex = "0", Id = events[0].Id},
                new FlashcardRecord{Question = "unchanged q2", Answer = "a2", Tags = "t2", BinIndex = "2", Id = "2"},
                new FlashcardRecord{Question = "changed q3v2", Answer = "a3", Tags = "", BinIndex = "4", Id = "3"},
                new FlashcardRecord{Question = "new with bin q4", Answer = "a4", Tags = "t4", BinIndex = "9", Id = events[3].Id}
            });
        }
        
        
        [Fact]
        public void Process_and_overwrite_previous_config()
        {
            const string BOX_PATH = "sampledb_sync_processing";
            
            var ctx = new SyncCommandMessageContextModel {
                Flashcards = new Dictionary<string, (string binIndex, string hash)> {
                    {"2", ("2", FlashcardHash.Calculate("unchanged q2","a2","t2"))},
                    {"3", ("4", FlashcardHash.Calculate("q3","a3",""))}, // to be changed
                    {"99", ("3", "xyz")} // to be deleted
                },
                Config = new FlashcardboxConfig{Bins = new[]{new FlashcardboxConfig.Bin{
                    LowerDueThreshold = 1,
                    UpperDueThreshold = 2
                }}}
            };
            var db = new FlashcardboxDb(BOX_PATH);

            var sut = new SyncCommandProcessor(db);
            
            File.Copy(Path.Combine(BOX_PATH, "flashcards original.csv"),
                      Path.Combine(BOX_PATH, "flashcards.csv"), true);

            
            var (status, events, version, notifications) = sut.Process(new SyncCommand(), ctx, "");

            
            Assert.IsType<Success>(status);
            events.Last().Should().BeEquivalentTo(
                new BoxConfigured{ Bins = new[]{new BoxConfigured.Bin{LowerDueThreshold = 10, UpperDueThreshold = 20}}, Id = events[6].Id} 
            );
        }
        
        
        
        [Fact]
        public void Load_message_context()
        {
            var es = new InMemoryEventstore();
            es.Record(new Event[]
            {
                new BoxConfigured{Bins = new[]{new BoxConfigured.Bin{LowerDueThreshold = 1,UpperDueThreshold = 2} }}, 
                
                new CardImported{Question = "q1", Answer = "a1", Tags = "t1", Id = "1"}, 
                new CardMovedTo{CardId = "1", BinIndex = 0}, 
                new CardImported{Question = "q2", Answer = "a2", Tags = "t2,t3", Id = "2"},
                new CardMovedTo{CardId = "2", BinIndex = 3},
                new CardImported{Question = "q3", Answer = "a3", Tags = "", Id = "3"},
                new CardChanged{Question = "q1v2", Answer = "a1v2", Tags = "t1", CardId = "1"},
                new CardMovedTo{CardId = "1", BinIndex = 2},
                new CardDeleted{CardId = "3"}, 
                
                new BoxConfigured{Bins = new[]{new BoxConfigured.Bin{LowerDueThreshold = 10,UpperDueThreshold = 20} }}
            });
            var sut = new SyncCommandMessageContextManagement(es);
            
            
            var result = sut.Load(new SyncCommand());

            
            var ctxModel = result.Ctx as SyncCommandMessageContextModel;
            
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