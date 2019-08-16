using System.IO;
using System.Linq;
using flashcardbox.backend.adapters;
using flashcardbox.backend.pipelines.commands.registeranswer;
using flashcardbox.backend.pipelines.commands.selectduecard;
using flashcardbox.backend.pipelines.commands.sync;
using flashcardbox.backend.pipelines.queries.duecard;
using flashcardbox.backend.pipelines.queries.progress;
using flashcardbox.messages;
using FluentAssertions;
using nsimpleeventstore;
using nsimplemessagepump;
using nsimplemessagepump.contract;
using Xunit;
using Xunit.Abstractions;

namespace flashcardbox.tests
{
    public class Acceptance_tests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public Acceptance_tests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void Cards_only_move_forward()
        {
            const string DBPATH = "sampledb_acceptance";
            File.Copy(Path.Combine(DBPATH, "flashcards v1.csv"), Path.Combine(DBPATH, "flashcards.csv"), true);
            
            var db = new FlashcardboxDb(DBPATH);
            var es = new InMemoryEventstore();
            var mp = new MessagePump(es);

            mp.On<SyncCommand>().Load(new SyncContextManagement(es)).Do(new SyncProcessor(db));
            mp.On<SelectDueCardCommand>().Load(new SelectDueCardContextManager(es)).Do(new SelectDueCardProcessor());
            mp.On<RegisterAnswerCommand>().Load(new RegisterAnswerContextManager(es)).Do(new RegisterAnswerProcessor());
            
            mp.On<DueCardQuery>().Load(new DueCardContextManager(es)).Do(new DueCardProcessor());
            mp.On<ProgressQuery>().Load(new ProgressContextManager(es)).Do(new ProgressProcessor());

            // box not yet initialized
            var progress = mp.Handle(new ProgressQuery()).Msg as ProgressQueryResult;
            progress.Should().BeEquivalentTo(new ProgressQueryResult {
                Bins = new[] {
                    new ProgressQueryResult.Bin(), 
                    new ProgressQueryResult.Bin()
                }
            });

            // box initialized
            var status = mp.Handle(new SyncCommand()).Msg as CommandStatus;
            status.Should().BeOfType<Success>();
            progress = mp.Handle(new ProgressQuery()).Msg as ProgressQueryResult;
            progress.Should().BeEquivalentTo(new ProgressQueryResult {
                Bins = new[] {
                    new ProgressQueryResult.Bin {
                        Count = 10
                    }, 
                    new ProgressQueryResult.Bin {
                        LowerDueThreshold = 2,
                        UpperDueThreshold = 4
                    },
                    new ProgressQueryResult.Bin {
                        LowerDueThreshold = 3,
                        UpperDueThreshold = 5
                    },
                    new ProgressQueryResult.Bin()
                }
            });
            
            // learn first card
            status = mp.Handle(new SelectDueCardCommand()).Msg as CommandStatus;
            status.Should().BeOfType<Success>();
            progress = mp.Handle(new ProgressQuery()).Msg as ProgressQueryResult;
            progress.Should().BeEquivalentTo(new ProgressQueryResult {
                Bins = new[] {
                    new ProgressQueryResult.Bin {
                        Count = 6
                    }, 
                    new ProgressQueryResult.Bin {
                        Count = 4,
                        LowerDueThreshold = 2,
                        UpperDueThreshold = 4,
                        IsDue = true
                    },
                    new ProgressQueryResult.Bin {
                        LowerDueThreshold = 3,
                        UpperDueThreshold = 5
                    },
                    new ProgressQueryResult.Bin()
                }
            });
            
            var card = mp.Handle(new DueCardQuery()).Msg as DueCardFoundQueryResult;
            card.Question.Should().Be("a");
            
            status = mp.Handle(new RegisterAnswerCommand{CardId = card.CardId, CorrectlyAnswered = true}).Msg as CommandStatus;
            status.Should().BeOfType<Success>();
            progress = mp.Handle(new ProgressQuery()).Msg as ProgressQueryResult;
            progress.Should().BeEquivalentTo(new ProgressQueryResult {
                Bins = new[] {
                    new ProgressQueryResult.Bin {
                        Count = 6
                    }, 
                    new ProgressQueryResult.Bin {
                        Count = 3,
                        LowerDueThreshold = 2,
                        UpperDueThreshold = 4,
                        IsDue = true
                    },
                    new ProgressQueryResult.Bin {
                        Count = 1,
                        LowerDueThreshold = 3,
                        UpperDueThreshold = 5
                    },
                    new ProgressQueryResult.Bin()
                }
            });
            
            // learn second card
            mp.Handle(new SelectDueCardCommand());
            card = mp.Handle(new DueCardQuery()).Msg as DueCardFoundQueryResult;
            card.Question.Should().Be("1");
            mp.Handle(new RegisterAnswerCommand{CardId = card.CardId, CorrectlyAnswered = true});
            progress = mp.Handle(new ProgressQuery()).Msg as ProgressQueryResult;
            progress.Should().BeEquivalentTo(new ProgressQueryResult {
                Bins = new[] {
                    new ProgressQueryResult.Bin {
                        Count = 6
                    }, 
                    new ProgressQueryResult.Bin {
                        Count = 2,
                        LowerDueThreshold = 2,
                        UpperDueThreshold = 4,
                        IsDue = true
                    },
                    new ProgressQueryResult.Bin {
                        Count = 2,
                        LowerDueThreshold = 3,
                        UpperDueThreshold = 5
                    },
                    new ProgressQueryResult.Bin()
                }
            });
            
            // learn card 3
            mp.Handle(new SelectDueCardCommand());
            card = mp.Handle(new DueCardQuery()).Msg as DueCardFoundQueryResult;
            card.Question.Should().Be("2");
            mp.Handle(new RegisterAnswerCommand{CardId = card.CardId, CorrectlyAnswered = true});
            progress = mp.Handle(new ProgressQuery()).Msg as ProgressQueryResult;
            progress.Should().BeEquivalentTo(new ProgressQueryResult {
                Bins = new[] {
                    new ProgressQueryResult.Bin {
                        Count = 6
                    }, 
                    new ProgressQueryResult.Bin {
                        Count = 1,
                        LowerDueThreshold = 2,
                        UpperDueThreshold = 4,
                        IsDue = true
                    },
                    new ProgressQueryResult.Bin {
                        Count = 3,
                        LowerDueThreshold = 3,
                        UpperDueThreshold = 5
                    },
                    new ProgressQueryResult.Bin()
                }
            });
            
            // learning card 4 first leads to a refill of bin 1
            mp.Handle(new SelectDueCardCommand());
            card = mp.Handle(new DueCardQuery()).Msg as DueCardFoundQueryResult;
            card.Question.Should().Be("b");
            mp.Handle(new RegisterAnswerCommand{CardId = card.CardId, CorrectlyAnswered = true});
            progress = mp.Handle(new ProgressQuery()).Msg as ProgressQueryResult;
            progress.Should().BeEquivalentTo(new ProgressQueryResult {
                Bins = new[] {
                    new ProgressQueryResult.Bin {
                        Count = 3
                    }, 
                    new ProgressQueryResult.Bin {
                        Count = 3,
                        LowerDueThreshold = 2,
                        UpperDueThreshold = 4,
                        IsDue = true
                    },
                    new ProgressQueryResult.Bin {
                        Count = 4,
                        LowerDueThreshold = 3,
                        UpperDueThreshold = 5
                    },
                    new ProgressQueryResult.Bin()
                }
            });
            
            // learning card 5 fills up bin 2...
            mp.Handle(new SelectDueCardCommand());
            card = mp.Handle(new DueCardQuery()).Msg as DueCardFoundQueryResult;
            card.Question.Should().Be("c");
            mp.Handle(new RegisterAnswerCommand{CardId = card.CardId, CorrectlyAnswered = true});
            
            // ...but bin 1 stays due because its lower threshold hasn't been reached
            mp.Handle(new SelectDueCardCommand());
            card = mp.Handle(new DueCardQuery()).Msg as DueCardFoundQueryResult;
            card.Question.Should().Be("d");
            mp.Handle(new RegisterAnswerCommand{CardId = card.CardId, CorrectlyAnswered = true});
            
            progress = mp.Handle(new ProgressQuery()).Msg as ProgressQueryResult;
            progress.Should().BeEquivalentTo(new ProgressQueryResult {
                Bins = new[] {
                    new ProgressQueryResult.Bin {
                        Count = 3
                    }, 
                    new ProgressQueryResult.Bin {
                        Count = 1,
                        LowerDueThreshold = 2,
                        UpperDueThreshold = 4,
                        IsDue = true
                    },
                    new ProgressQueryResult.Bin {
                        Count = 6,
                        LowerDueThreshold = 3,
                        UpperDueThreshold = 5
                    },
                    new ProgressQueryResult.Bin()
                }
            });
            
            // this only changes now!
            mp.Handle(new SelectDueCardCommand());
            card = mp.Handle(new DueCardQuery()).Msg as DueCardFoundQueryResult;
            card.Question.Should().Be("a");
            mp.Handle(new RegisterAnswerCommand{CardId = card.CardId, CorrectlyAnswered = true});
            
            progress = mp.Handle(new ProgressQuery()).Msg as ProgressQueryResult;
            progress.Should().BeEquivalentTo(new ProgressQueryResult {
                Bins = new[] {
                    new ProgressQueryResult.Bin {
                        Count = 3
                    }, 
                    new ProgressQueryResult.Bin {
                        Count = 1,
                        LowerDueThreshold = 2,
                        UpperDueThreshold = 4,
                    },
                    new ProgressQueryResult.Bin {
                        Count = 5,
                        LowerDueThreshold = 3,
                        UpperDueThreshold = 5,
                        IsDue = true
                    },
                    new ProgressQueryResult.Bin {
                        Count = 1
                    }
                }
            });
            
            // learn all cards:
            // 1(3,1,4,2), 2(3,1,3,3), b(3,1,2,4), e(0,3,4,3), 3(0,2,5,3), f(0,1-g,5-cde3f,4-a12b)
            // c(0,1-g,5-de3f,4-a12bc), d(0,1-g,3-e3f,5-a12bcd), e(0,1-g,2-3f,6-a12bcde)
            // g(0,0,3-3fg,7-a12bcde), 3(0,0,2-fg,8-a12bcde3), f(0,0,1-g,9-a12bcde3f), g(0,0,0,10-a12bcde3fg)
            
            foreach (var q in "12be3fcdeg3fg".ToCharArray()) {
                mp.Handle(new SelectDueCardCommand());
                card = mp.Handle(new DueCardQuery()).Msg as DueCardFoundQueryResult;
                mp.Handle(new RegisterAnswerCommand{CardId = card.CardId, CorrectlyAnswered = true});
                card.Question.Should().Be(q.ToString());
            }

            mp.Handle(new SelectDueCardCommand()).Msg.Should().BeOfType<Failure>();
        }

        
        [Fact]
        public void Some_cards_are_not_memorized_right_away()
        {
            const string DBPATH = "sampledb_acceptance";
            File.Copy(Path.Combine(DBPATH, "flashcards v1.csv"), Path.Combine(DBPATH, "flashcards.csv"), true);
            
            var db = new FlashcardboxDb(DBPATH);
            var es = new InMemoryEventstore();
            var mp = new MessagePump(es);

            mp.On<SyncCommand>().Load(new SyncContextManagement(es)).Do(new SyncProcessor(db));
            mp.On<SelectDueCardCommand>().Load(new SelectDueCardContextManager(es)).Do(new SelectDueCardProcessor());
            mp.On<RegisterAnswerCommand>().Load(new RegisterAnswerContextManager(es)).Do(new RegisterAnswerProcessor());
            
            mp.On<DueCardQuery>().Load(new DueCardContextManager(es)).Do(new DueCardProcessor());
            mp.On<ProgressQuery>().Load(new ProgressContextManager(es)).Do(new ProgressProcessor());

            // box initialized
            var status = mp.Handle(new SyncCommand()).Msg as CommandStatus;

            // 0:a12bcde3fg,1:,2:,3:
            Memorized("a"); //0:cde3fg,1:12b*,2:a,3:
            Memorized("1"); //0:cde3fg,1:2b*,2:a1,3:
            Forgotten("2"); //0:cde3fg,1:b2*,2:a1,3:
            Memorized("b"); //0:cde3fg,1:2*,2:a1b,3:
            Memorized("2"); //0:cde3fg,1:2*,2:a1b,3: -> 0:3fg,1:2cde*,2:a1b,3: -> 0:3fg,1:cde*,2:a1b2,3:
            Forgotten("c"); //0:3fg,1:dec*,2:a1b2,3:
            Memorized("d"); //0:3fg,1:ec*,2:a1b2d,3:
            Memorized("e"); //0:3fg,1:c*,2:a1b2de,3:
            Memorized("a"); //0:3fg,1:c*,2:1b2de*,3:a
            Forgotten("1"); //0:3fg,1:c1,2:b2de*,3:a
            Forgotten("b"); //0:3fg,1:c1b,2:2de*,3:a
            Memorized("2"); //0:3fg,1:c1b,2:de*,3:a2
            Memorized("c"); //0:3fg,1:c1b,2:de*,3:a2 -> 0:fg,1:c1b3,2:de*,3:a2 -> 0:fg,1:1b3*,2:dec,3:a2
            Memorized("1"); //0:fg,1:b3*,2:dec1,3:a2
            Memorized("b"); //0:fg,1:3*,2:dec1b,3:a2
            Forgotten("d"); //0:fg,1:3d,2:ec1b*,3:a2
            Memorized("e"); //0:fg,1:3d,2:c1b*,3:a2e
            Memorized("c"); //0:fg,1:3d,2:1b*,3:a2ec
            Memorized("3"); //0:,1:dfg*,2:1b3,3:a2ec
            Memorized("d"); //0:,1:fg*,2:1b3d,3:a2ec
            Memorized("f"); //0:,1:g*,2:1b3df,3:a2ec
            Forgotten("1"); //0:,1:g1,2:b3df*,3:a2ec
            Memorized("b"); //0:,1:g1,2:3df*,3:a2ecb
            Memorized("3"); //0:,1:g1,2:df*,3:a2ecb3
            Memorized("g"); //0:,1:1*,2:dfg,3:a2ecb3
            Memorized("1"); //0:,1:*,2:dfg1,3:a2ecb3
            Memorized("d"); //0:,1:,2:fg1*,3:a2ecb3d
            Forgotten("f"); //0:,1:f,2:g1*,3:a2ecb3d
            Memorized("f"); //0:,1:*,2:g1f*,3:a2ecb3d
            Memorized("g"); //0:,1:*,2:1f*,3:a2ecb3dg
            Memorized("1"); //0:,1:*,2:f*,3:a2ecb3dg1
            Memorized("f"); //0:,1:*,2:*,3:a2ecb3dg1f

            mp.Handle(new SelectDueCardCommand()).Msg.Should().BeOfType<Failure>();
            

            void Memorized(string question) {
                mp.Handle(new SelectDueCardCommand());
                var card = mp.Handle(new DueCardQuery()).Msg as DueCardFoundQueryResult;
                mp.Handle(new RegisterAnswerCommand{CardId = card.CardId, CorrectlyAnswered = true});
                card.Question.Should().Be(question);
            }
            
            void Forgotten(string question) {
                mp.Handle(new SelectDueCardCommand());
                var card = mp.Handle(new DueCardQuery()).Msg as DueCardFoundQueryResult;
                mp.Handle(new RegisterAnswerCommand{CardId = card.CardId, CorrectlyAnswered = false});
                card.Question.Should().Be(question);
            }
        }
        
        

        private void DumpProgress(MessagePump mp)
        {
            _testOutputHelper.WriteLine("------");
            var progress = mp.Handle(new ProgressQuery()).Msg as ProgressQueryResult;
            for(var i=0; i<progress.Bins.Length; i++)
                _testOutputHelper.WriteLine($"{i}.({progress.Bins[i].Count}){(progress.Bins[i].IsDue?'*':' ')}");
        }
        
        
        private void DumpES(IEventstore es) {
            _testOutputHelper.WriteLine("<<<");
            var i = 1;
            foreach(var e in es.Replay().Events)
                _testOutputHelper.WriteLine($"{++i}: {e}");
            _testOutputHelper.WriteLine(">>>");
        }


        private void DumpDb(MessagePump mp, FlashcardboxDb db)
        {
            _testOutputHelper.WriteLine("<<<");
            mp.Handle(new SyncCommand());
            var cards = db.LoadFlashcards();
            foreach (var card in cards.OrderBy(c => c.BinIndex)) {
                _testOutputHelper.WriteLine($"{card.BinIndex}:{card.Question}");
            }
            _testOutputHelper.WriteLine(">>>");
        }
    }
}