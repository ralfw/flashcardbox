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
        [Fact]
        public void Process_without_previous_config()
        {
            const string BOX_PATH = "sampledb_sync_processing";
            
            var ctx = new SyncContextModel {
                Flashcards = new Dictionary<string, (string binIndex, string hash)> {
                    {"2", ("2", FlashcardHash.Calculate("unchanged q2","a2","t2"))},
                    {"3", ("4", FlashcardHash.Calculate("q3","a3",""))}, // to be changed
                    {"99", ("3", "xyz")} // to be deleted
                },
                // a config will be registered
                Config = new FlashcardboxConfig()
            };
            var db = new FlashcardboxDb(BOX_PATH);

            var sut = new SyncProcessor(db);
            
            File.Copy(Path.Combine(BOX_PATH, "flashcards original.csv"),
                      Path.Combine(BOX_PATH, "flashcards.csv"), true);

            
            var (status, events, version, notifications) = sut.Process(new SyncCommand(), ctx, "");
            
            
            status.Should().BeEquivalentTo(new SyncSuccess {
                Added = 2,
                Changed = 1,
                Missing = 1,
                TotalCount = 4
            });
            
            events.Should().BeEquivalentTo(new Event[]{
                new NewCardEncountered{Question = "new q1", Answer = "a1", Tags = "t1", Id = events[0].Id}, 
                new CardMovedTo{CardId = events[0].Id, BinIndex = 0, Id = events[1].Id}, 
                
                new CardWasChanged{CardId = "3", Question = "changed q3v2", Answer = "a3", Tags = "", Id = events[2].Id}, 
                
                new NewCardEncountered{Question = "new with bin q4", Answer = "a4", Tags = "t4", Id = events[3].Id}, 
                new CardMovedTo{CardId = events[3].Id, BinIndex = 9, Id = events[4].Id}, 
                
                new CardFoundMissing(){CardId = "99", Id = events[5].Id},
                
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
            
            var ctx = new SyncContextModel {
                Flashcards = new Dictionary<string, (string binIndex, string hash)> {
                    {"2", ("2", FlashcardHash.Calculate("unchanged q2","a2","t2"))},
                    {"3", ("4", FlashcardHash.Calculate("q3","a3",""))}, // to be changed
                    {"99", ("3", "xyz")} // to be deleted
                },
                // this config will get overruled!
                Config = new FlashcardboxConfig{Bins = new[]{new FlashcardboxConfig.Bin{
                    LowerDueThreshold = 1,
                    UpperDueThreshold = 2
                }}}
            };
            var db = new FlashcardboxDb(BOX_PATH);

            var sut = new SyncProcessor(db);
            
            File.Copy(Path.Combine(BOX_PATH, "flashcards original.csv"),
                      Path.Combine(BOX_PATH, "flashcards.csv"), true);

            
            var (status, events, version, notifications) = sut.Process(new SyncCommand(), ctx, "");

            
            Assert.IsType<SyncSuccess>(status);
            events.Last().Should().BeEquivalentTo(
                new BoxConfigured{ Bins = new[]{new BoxConfigured.Bin{LowerDueThreshold = 10, UpperDueThreshold = 20}}, Id = events[6].Id} 
            );
        }


        [Fact]
        public void Process_no_new_config_if_it_hasnt_changed()
        {
            const string BOX_PATH = "sampledb_sync_processing";

            var ctx = new SyncContextModel
            {
                Flashcards = new Dictionary<string, (string binIndex, string hash)>
                {
                    {"2", ("2", FlashcardHash.Calculate("unchanged q2", "a2", "t2"))},
                    {"3", ("4", FlashcardHash.Calculate("q3", "a3", ""))}, // to be changed
                    {"99", ("3", "xyz")} // to be deleted
                },
                // no config will be registered because file and event do not differ
                Config = new FlashcardboxConfig
                {
                    Bins = new[]
                    {
                        new FlashcardboxConfig.Bin
                        {
                            LowerDueThreshold = 10,
                            UpperDueThreshold = 20
                        }
                    }
                }
            };
            var db = new FlashcardboxDb(BOX_PATH);

            var sut = new SyncProcessor(db);

            File.Copy(Path.Combine(BOX_PATH, "flashcards original.csv"),
                Path.Combine(BOX_PATH, "flashcards.csv"), true);


            var (status, events, version, notifications) = sut.Process(new SyncCommand(), ctx, "");


            Assert.IsType<SyncSuccess>(status);
            events.Last().Should().BeOfType<CardFoundMissing>();
        }
    }
}