using flashcardbox.events;
using flashcardbox.messages.commands.selectduecard;
using FluentAssertions;
using nsimpleeventstore;
using nsimplemessagepump.contract;
using Xunit;

namespace flashcardbox.tests
{
    public class SelectDueCardProcessor_tests
    {
        /*
         * 3 bins configured  -> there should be 5 bins(0, 1..3, 4 (archive))
         * - select bin 1, although others are full, too
         * - select bin 3, because bin 1 is empty, but 2 is full, too
         * - select bin 2, because bin 1 is empty and 3, too
         */
        private static FlashcardboxConfig __CONFIG = new FlashcardboxConfig {
            Bins = new [] {
                new FlashcardboxConfig.Bin {
                    LowerDueThreshold = 2,
                    UpperDueThreshold = 4
                }, 
                new FlashcardboxConfig.Bin {
                    LowerDueThreshold = 3,
                    UpperDueThreshold = 5
                }, 
                new FlashcardboxConfig.Bin {
                    LowerDueThreshold = 4,
                    UpperDueThreshold = 6
                }, 
            }
        };
        
        
        [Fact]
        public void No_due_bin_start_with_Bin_1()
        {
            var sut = new SelectDueCardProcessor();
            var ctx = new SelectDueCardContextModel
            {
                Bins = new[]
                {
                    new string[0], // bin 0
                    new[]{"11","12","13","14"},
                    new[]{"21","22","23","24","25"},
                    new[]{"31","32","33","34","35","36"},
                    new[]{"41"} // bin 4, archive
                },
                
                DueBinIndex = -1,
                
                Config = __CONFIG
            };
            
            var (status, events, _, _) = sut.Process(new SelectDueCardCommand(), ctx, "");

            status.Should().BeOfType<Success>();
            events.Should().BeEquivalentTo(new[] {
                new DueCardSelected {CardId = "11", BinIndex = 1, Id = events[0].Id}
            });
        }
        
        
        [Fact]
        public void No_due_bin_continue_at_from_last_bin_after_bin_1()
        {
            var sut = new SelectDueCardProcessor();
            var ctx = new SelectDueCardContextModel
            {
                Bins = new[]
                {
                    new string[0], // bin 0
                    new[]{"11"},
                    new[]{"21","22","23","24","25"},
                    new[]{"31","32","33","34","35","36"},
                    new[]{"41"} // bin 4, archive
                },
                
                DueBinIndex = -1,
                
                Config = __CONFIG
            };
            
            var (status, events, _, _) = sut.Process(new SelectDueCardCommand(), ctx, "");

            status.Should().BeOfType<Success>();
            events.Should().BeEquivalentTo(new[] {
                new DueCardSelected {CardId = "31", BinIndex = 3, Id = events[0].Id}
            });
        }
        
        [Fact]
        public void No_due_bin_continue_down_from_last_bin()
        {
            var sut = new SelectDueCardProcessor();
            var ctx = new SelectDueCardContextModel
            {
                Bins = new[]
                {
                    new string[0], // bin 0
                    new[]{"11"},
                    new[]{"21","22","23","24","25"},
                    new[]{"31"},
                    new[]{"41"} // bin 4, archive
                },
                
                DueBinIndex = -1,
                
                Config = __CONFIG
            };
            
            var (status, events, _, _) = sut.Process(new SelectDueCardCommand(), ctx, "");

            status.Should().BeOfType<Success>();
            events.Should().BeEquivalentTo(new[] {
                new DueCardSelected {CardId = "21", BinIndex = 2, Id = events[0].Id}
            });
        }
        
        [Fact]
        public void Start_with_due_bin_and_keep_it()
        {
            var sut = new SelectDueCardProcessor();
            var ctx = new SelectDueCardContextModel
            {
                Bins = new[]
                {
                    new string[0], // bin 0
                    new[]{"11","12","13","14"},
                    new[]{"21","22","23"},
                    new[]{"31","32","33","34","35","36"},
                    new[]{"41"} // bin 4, archive
                },
                
                DueBinIndex = 2,
                
                Config = __CONFIG
            };
            
            var (status, events, _, _) = sut.Process(new SelectDueCardCommand(), ctx, "");

            status.Should().BeOfType<Success>();
            events.Should().BeEquivalentTo(new[] {
                new DueCardSelected {CardId = "21", BinIndex = 2, Id = events[0].Id}
            });
        }
        
        [Fact]
        public void Start_with_due_bin_and_give_it_up_continue_with_bin_1()
        {
            var sut = new SelectDueCardProcessor();
            var ctx = new SelectDueCardContextModel
            {
                Bins = new[]
                {
                    new string[0], // bin 0
                    new[]{"11","12","13","14"},
                    new[]{"21"},
                    new[]{"31","32","33","34","35","36"},
                    new[]{"41"} // bin 4, archive
                },
                
                DueBinIndex = 2,
                
                Config = __CONFIG
            };
            
            var (status, events, _, _) = sut.Process(new SelectDueCardCommand(), ctx, "");

            status.Should().BeOfType<Success>();
            events.Should().BeEquivalentTo(new[] {
                new DueCardSelected {CardId = "11", BinIndex = 1, Id = events[0].Id}
            });
        }
        
        [Fact]
        public void Fill_up_bin_1_from_bin_0_and_make_it_due()
        {
            var sut = new SelectDueCardProcessor();
            var ctx = new SelectDueCardContextModel
            {
                Bins = new[]
                {
                    new[]{"12","13","14","15"}, // bin 0
                    new[]{"11"},
                    new[]{"21"},
                    new[]{"31"},
                    new[]{"41"} // bin 4, archive
                },
                
                DueBinIndex = -1,
                
                Config = __CONFIG
            };
            
            var (status, events, _, _) = sut.Process(new SelectDueCardCommand(), ctx, "");

            status.Should().BeOfType<Success>();
            events.Should().BeEquivalentTo(
                new CardMovedTo{CardId = "12", BinIndex = 1, Id = events[0].Id}, 
                new CardMovedTo{CardId = "13", BinIndex = 1, Id = events[1].Id}, 
                new CardMovedTo{CardId = "14", BinIndex = 1, Id = events[2].Id}, 
                new DueCardSelected {CardId = "11", BinIndex = 1, Id = events[3].Id});
        }
        
        [Fact]
        public void Fill_up_bin_1_completely_from_bin_0()
        {
            var sut = new SelectDueCardProcessor();
            var ctx = new SelectDueCardContextModel
            {
                Bins = new[]
                {
                    new[]{"11","12","13","14","15"}, // bin 0
                    new string[0],
                    new[]{"21"},
                    new[]{"31"},
                    new[]{"41"} // bin 4, archive
                },
                
                DueBinIndex = -1,
                
                Config = __CONFIG
            };
            
            var (status, events, _, _) = sut.Process(new SelectDueCardCommand(), ctx, "");

            status.Should().BeOfType<Success>();
            events.Should().BeEquivalentTo(
                new CardMovedTo{CardId = "11", BinIndex = 1, Id = events[0].Id}, 
                new CardMovedTo{CardId = "12", BinIndex = 1, Id = events[1].Id}, 
                new CardMovedTo{CardId = "13", BinIndex = 1, Id = events[2].Id}, 
                new CardMovedTo{CardId = "14", BinIndex = 1, Id = events[3].Id}, 
                new DueCardSelected {CardId = "11", BinIndex = 1, Id = events[4].Id});
        }

        [Fact]
        public void Select_first_bin_to_contain_cards_starting_with_bin_1()
        {
            var sut = new SelectDueCardProcessor();
            var ctx = new SelectDueCardContextModel
            {
                Bins = new[]
                {
                    new string[0],
                    new[] {"11"},
                    new[] {"21"},
                    new[] {"31"},
                    new[] {"41"} // bin 4, archive
                },

                DueBinIndex = -1,

                Config = __CONFIG
            };

            var (status, events, _, _) = sut.Process(new SelectDueCardCommand(), ctx, "");

            status.Should().BeOfType<Success>();
            events.Should().BeEquivalentTo(
                new DueCardSelected {CardId = "11", BinIndex = 1, Id = events[0].Id});
        }
        
        [Fact]
        public void Select_first_bin_to_contain_cards_after_bin_1()
        {
            var sut = new SelectDueCardProcessor();
            var ctx = new SelectDueCardContextModel
            {
                Bins = new[]
                {
                    new string[0],
                    new string[0],
                    new[] {"21"},
                    new[] {"31"},
                    new[] {"41"} // bin 4, archive
                },

                DueBinIndex = -1,

                Config = __CONFIG
            };

            var (status, events, _, _) = sut.Process(new SelectDueCardCommand(), ctx, "");

            status.Should().BeOfType<Success>();
            events.Should().BeEquivalentTo(
                new DueCardSelected {CardId = "21", BinIndex = 2, Id = events[0].Id});
        }
        
        [Fact]
        public void Report_failure_if_no_non_archive_bins_dont_contain_cards()
        {
            var sut = new SelectDueCardProcessor();
            var ctx = new SelectDueCardContextModel
            {
                Bins = new[]
                {
                    new string[0],
                    new string[0],
                    new string[0],
                    new string[0],
                    new[] {"41"} // bin 4, archive
                },

                DueBinIndex = -1,

                Config = __CONFIG
            };

            var (status, _, _, _) = sut.Process(new SelectDueCardCommand(), ctx, "");

            status.Should().BeOfType<Failure>();
        }
        
        [Fact]
        public void Config_defines_more_bins_than_have_been_filled()
        {
            var sut = new SelectDueCardProcessor();
            var ctx = new SelectDueCardContextModel
            {
                Bins = new[]
                {
                    new string[0], // bin 0
                    new[]{"11","12","13","14"},
                },
                
                DueBinIndex = -1,
                
                Config = __CONFIG
            };
            
            var (status, events, _, _) = sut.Process(new SelectDueCardCommand(), ctx, "");

            status.Should().BeOfType<Success>();
            events.Should().BeEquivalentTo(new[] {
                new DueCardSelected {CardId = "11", BinIndex = 1, Id = events[0].Id}
            });
        }
        
        [Fact]
        public void Config_defines_fewer_bin_than_have_been_filled()
        {
            var sut = new SelectDueCardProcessor();
            var ctx = new SelectDueCardContextModel
            {
                Bins = new[]
                {
                    new string[0], // bin 0
                    new string[0],
                    new string[0],
                    new string[0],
                    new[]{"41"}, // new archive
                    new[]{"51"} // former archive
                },
                
                DueBinIndex = -1,
                
                Config = __CONFIG
            };
            
            var (status, _, _, _) = sut.Process(new SelectDueCardCommand(), ctx, "");

            status.Should().BeOfType<Failure>();
        }
    }
}