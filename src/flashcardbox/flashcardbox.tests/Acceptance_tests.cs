using System.IO;
using flashcardbox.messages.commands.registeranswer;
using flashcardbox.messages.commands.selectduecard;
using flashcardbox.messages.commands.sync;
using flashcardbox.messages.queries.duecard;
using flashcardbox.messages.queries.progress;
using FluentAssertions;
using nsimpleeventstore;
using nsimplemessagepump;
using nsimplemessagepump.contract;
using Xunit;

namespace flashcardbox.tests
{
    public class Acceptance_tests
    {
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

            var progress = mp.Handle(new ProgressQuery()).Msg as ProgressQueryResult;
            progress.Should().BeEquivalentTo(new ProgressQueryResult {
                Bins = new[] {
                    new ProgressQueryResult.Bin(), 
                    new ProgressQueryResult.Bin()
                }
            });

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
                }
            });
        }
    }
}