using System;
using flashcardbox.backend.integration;
using flashcardbox.messages.commands;
using flashcardbox.messages.queries;
using nsimplemessagepump.contract;
using NSubstitute;

namespace flashcardbox.ui.tests
{
    class Program
    {
        static void Main(string[] args)
        {
            var mh = Substitute.For<IMessageHandling>();
            
            mh.Handle(Arg.Any<SyncCommand>()).Returns(new SyncSuccess {
                TotalCount = 10,
                Added = 3,
                Changed = 2,
                Missing = 1
            });

            mh.Handle(Arg.Any<ProgressQuery>()).Returns(new ProgressQueryResult {
                Bins = new[] {
                    new ProgressQueryResult.Bin {
                        Count = 3,
                        LowerDueThreshold = 0,
                        UpperDueThreshold = 0
                    }, 
                    new ProgressQueryResult.Bin {
                        Count = 2,
                        LowerDueThreshold = 2,
                        UpperDueThreshold = 4,
                        IsDue = true
                    }, 
                    new ProgressQueryResult.Bin {
                        Count = 4,
                        LowerDueThreshold = 3,
                        UpperDueThreshold = 5
                    }, 
                    new ProgressQueryResult.Bin {
                        Count = 1,
                        LowerDueThreshold = 0,
                        UpperDueThreshold = 0
                    }
                }
            });

            mh.Handle(Arg.Any<DueCardQuery>()).Returns(
                new DueCardFoundQueryResult {
                    CardId = "1", Question = "Haus", Answer = "куща", Tags = "", BinIndex = 1
                },
                new DueCardFoundQueryResult {
                    CardId = "2", Question = "Auto", Answer = "кола", Tags = "", BinIndex = 2
                },
                new DueCardFoundQueryResult {
                    CardId = "2", Question = "a-Konjugation für имам", Answer = @"имам
имаш
има
имаме
имате
имат", Tags = "", BinIndex = 1
                }
            );

            mh.Handle(Arg.Any<RegisterAnswerCommand>()).Returns(new Success());
            
            mh.Handle(Arg.Any<SelectDueCardCommand>()).Returns(
                new Success(),
                new Success(),
                new Success(),
                new Failure("FIN"));
            
            var sut = new UI(mh);
            
            sut.Show();
        }
    }
}