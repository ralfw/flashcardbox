using System.Collections.Generic;
using flashcardbox.events;
using flashcardbox.messages.commands.selectduecard;
using FluentAssertions;
using nsimpleeventstore;
using Xunit;

namespace flashcardbox.tests
{
    public class SelectDueCardContextManager_tests
    {
        [Fact]
        public void Load()
        {
            var es = new InMemoryEventstore();
            var sut = new SelectDueCardContextManager(es);
            es.Record(new Event[] {
                new BoxConfigured {
                    Bins = new[] {
                        new BoxConfigured.Bin {
                            LowerDueThreshold = 10,
                            UpperDueThreshold = 20
                        }, 
                    }
                }, 
                
                new CardMovedTo{CardId = "1", BinIndex = 1},
                new CardMovedTo{CardId = "2.1", BinIndex = 2},
                new CardMovedTo{CardId = "2.2", BinIndex = 1},
                new CardMovedTo{CardId = "3.1", BinIndex = 3},
                new CardMovedTo{CardId = "3.2", BinIndex = 3},
                new CardMovedTo{CardId = "3.3", BinIndex = 1},
                new CardMovedTo{CardId = "3.x", BinIndex = 3},
                
                new CardMovedTo{CardId = "2.2", BinIndex = 2},
                new CardFoundMissing{CardId = "3.x"}, 
                new CardMovedTo{CardId = "3.3", BinIndex = 3}
            });

            var result = sut.Load(new SelectDueCardCommand()).Ctx as SelectDueCardContextModel;
            
            result.Should().BeEquivalentTo(new SelectDueCardContextModel {
                Bins = new[] {
                    new string[0],
                    new[]{"1"},
                    new[]{"2.1", "2.2"},
                    new[]{"3.1", "3.2", "3.3"}
                },
                
                Config = new FlashcardboxConfig {
                    Bins = new [] {
                        new FlashcardboxConfig.Bin {
                            LowerDueThreshold = 10,
                            UpperDueThreshold = 20
                        }
                    }
                }
            });
        }
        
        
        [Fact]
        public void Load_no_config()
        {
            var es = new InMemoryEventstore();
            var sut = new SelectDueCardContextManager(es);
            es.Record(new Event[] {
                new CardMovedTo{CardId = "1", BinIndex = 1},
            });

            var result = sut.Load(new SelectDueCardCommand()).Ctx as SelectDueCardContextModel;
            
            result.Should().BeEquivalentTo(new SelectDueCardContextModel {
                Bins = new[] {
                    new string[0],
                    new[]{"1"},
                },
                
                Config = null
            });
        }
        
        
        [Fact]
        public void Load_no_cards()
        {
            var es = new InMemoryEventstore();
            var sut = new SelectDueCardContextManager(es);
            es.Record(new Event[] {
            });

            var result = sut.Load(new SelectDueCardCommand()).Ctx as SelectDueCardContextModel;
            
            result.Should().BeEquivalentTo(new SelectDueCardContextModel {
                Bins = new[] {
                    new string[0]
                },
                
                Config = null
            });
        }
    }
}