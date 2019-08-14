using flashcardbox.messages.commands.selectduecard;
using flashcardbox.messages.queries.progress;
using FluentAssertions;
using Xunit;

namespace flashcardbox.tests
{
    public class ProgressProcessor_tests
    {
        private static readonly FlashcardboxConfig __CONFIG = new FlashcardboxConfig {
            Bins = new [] {
                new FlashcardboxConfig.Bin {
                    LowerDueThreshold = 2,
                    UpperDueThreshold = 4
                }, 
                new FlashcardboxConfig.Bin {
                    LowerDueThreshold = 3,
                    UpperDueThreshold = 5
                }
            }
        };
        
        
        [Fact]
        public void Due_bin_with_matching_bins_and_config()
        {
            var ctx = new SelectDueCardContextModel
            {
                Bins = new [] {
                    new[]{"01", "02"},
                    new[]{"11", "12", "13"},
                    new[]{"21"},
                    new[]{"31", "32", "33", "34"}
                },
                DueBinIndex = 2,
                Config = __CONFIG
            };
            var sut = new ProgressProcessor();

            var result = sut.Process(new ProgressQuery(), ctx) as ProgressQueryResult;
            
            result.Should().BeEquivalentTo(new ProgressQueryResult {
                Bins = new [] {
                    new ProgressQueryResult.Bin {
                        Count = 2
                    }, 
                    new ProgressQueryResult.Bin {
                        Count = 3,
                        LowerDueThreshold = 2,
                        UpperDueThreshold = 4
                    }, 
                    new ProgressQueryResult.Bin {
                        Count = 1,
                        LowerDueThreshold = 3,
                        UpperDueThreshold = 5,
                        IsDue = true
                    }, 
                    new ProgressQueryResult.Bin {
                        Count = 4
                    }, 
                }
            });
            result.TotalCount.Should().Be(10);
        }
        
        
        [Fact]
        public void Less_bins_than_config()
        {
            var ctx = new SelectDueCardContextModel
            {
                Bins = new [] {
                    new[]{"01", "02"},
                    new[]{"11", "12", "13"},
                },
                DueBinIndex = 2,
                Config = __CONFIG
            };
            var sut = new ProgressProcessor();

            var result = sut.Process(new ProgressQuery(), ctx) as ProgressQueryResult;
            
            result.Should().BeEquivalentTo(new ProgressQueryResult {
                Bins = new [] {
                    new ProgressQueryResult.Bin {
                        Count = 2
                    }, 
                    new ProgressQueryResult.Bin {
                        Count = 3,
                        LowerDueThreshold = 2,
                        UpperDueThreshold = 4
                    }, 
                }
            });
            result.TotalCount.Should().Be(5);
        }
        
        [Fact]
        public void More_bins_than_config()
        {
            var ctx = new SelectDueCardContextModel
            {
                Bins = new [] {
                    new[]{"01", "02"},
                    new[]{"11", "12", "13"},
                    new[]{"21"},
                    new[]{"31", "32", "33", "34"},
                    new[]{"41", "42", "43", "44", "45"}
                },
                DueBinIndex = -1,
                Config = __CONFIG
            };
            var sut = new ProgressProcessor();

            var result = sut.Process(new ProgressQuery(), ctx) as ProgressQueryResult;
            
            result.Should().BeEquivalentTo(new ProgressQueryResult {
                Bins = new [] {
                    new ProgressQueryResult.Bin {
                        Count = 2
                    }, 
                    new ProgressQueryResult.Bin {
                        Count = 3,
                        LowerDueThreshold = 2,
                        UpperDueThreshold = 4
                    }, 
                    new ProgressQueryResult.Bin {
                        Count = 1,
                        LowerDueThreshold = 3,
                        UpperDueThreshold = 5
                    }, 
                    new ProgressQueryResult.Bin {
                        Count = 4
                    }, 
                    new ProgressQueryResult.Bin {
                        Count = 5
                    }, 
                }
            });
            result.TotalCount.Should().Be(15);
        }
    }
}