using System;
using System.Linq;
using flashcardbox.backend.integration;
using flashcardbox.messages.commands;
using flashcardbox.messages.queries;
using nsimplemessagepump.contract;

namespace flashcardbox
{
    class UI
    {
        private readonly IMessageHandling _mh;

        public UI(IMessageHandling mh) {
            _mh = mh;
        }
        
        public void Show() {
            Sync();
            Display_progress();
            
            while (true) {
                if (Try_get_next_card(out var card) is false)
                    Exit_on_no_card();
                var answer = Test_card(card);
                Register_answer(card, answer);
            }
        }


        private void Sync() {
            var stats = _mh.Handle(new SyncCommand()) as SyncSuccess;
            
            Console.WriteLine($"Synced!");
            Console.WriteLine($"  {stats.TotalCount} cards");
            Console.WriteLine($"  {stats.Added} added, {stats.Changed} changed, {stats.Missing} missing");
            Console.WriteLine();
        }

        private void Display_progress() {
            var progress = _mh.Handle(new ProgressQuery());

            var stats = string.Join(",", progress.Bins.Select(Map));
            Console.WriteLine($"Progress: {stats}");


            string Map(ProgressQueryResult.Bin bin)
                => $"{(bin.IsDue ? "*" : "")}({bin.Count},[{bin.LowerDueThreshold},{bin.UpperDueThreshold}])";
        }

        private void Exit_on_no_card() {
            Console.WriteLine("*** No card available for learning!");
            Environment.Exit(1);
        }

        
        private bool Try_get_next_card(out DueCardFoundQueryResult card) {
            card = null;
            
            var result = _mh.Handle(new DueCardQuery());
            if (result is DueCardNotFoundQueryResult) {
                var status = _mh.Handle(new SelectDueCardCommand());
                if (status is Failure) return false;

                result = _mh.Handle(new DueCardQuery());
            }

            card = result as DueCardFoundQueryResult;
            return true;
        }

        
        private bool Test_card(DueCardFoundQueryResult card) {
            Console.WriteLine();
            Console.WriteLine($"[");
            Console.WriteLine(card.Question);
            Console.WriteLine($"]@{card.BinIndex}");
            Console.Write("---SPACE to show answer---");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine("[");
            Console.WriteLine(card.Answer);
            Console.WriteLine($"]#{card.Tags}");
            
            Console.Write("---Did you know the answer? (y/n) ");
            var reply = Console.ReadKey().Key;
            Console.WriteLine();
            return reply == ConsoleKey.Y || reply == ConsoleKey.J || reply == ConsoleKey.Spacebar;
        }
        
        
        private void Register_answer(DueCardFoundQueryResult card, bool answeredCorrectly) {
            _mh.Handle(new RegisterAnswerCommand{
                CardId = card.CardId,
                CorrectlyAnswered = answeredCorrectly
            });
        }
    }
}