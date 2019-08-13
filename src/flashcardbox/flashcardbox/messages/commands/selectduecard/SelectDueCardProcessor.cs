using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using flashcardbox.events;
using nsimpleeventstore;
using nsimplemessagepump.contract;
using nsimplemessagepump.contract.messagecontext;
using nsimplemessagepump.contract.messageprocessing;

namespace flashcardbox.messages.commands.selectduecard
{
    /*
     * The processor selects the due card based on how cards are distributed across bins compared
     * to the current box configuration.
     *
     * 1. check current due bin if it should stay due
     * 2. else: check bin 1 if it should become due; first work on any new cards and ones which were not remembered correctly
     * 3. else: check bins n..2
     * 4. else: fill bin 1 from bin 0 until upper due threshold is reached
     * 5. else: select first bin to contain cards (1..n)
     *
     * with a due bin selected select the due card in it (1st card in bin).
     *
     * Fail on missing config!
     */
    internal class SelectDueCardProcessor : ICommandProcessor {
        public (CommandStatus, Event[], string, Notification[]) Process(IMessage msg, IMessageContextModel ctx, string version)
        {
            var model = ctx as SelectDueCardContextModel;
            if (model.Config == null)
                return (new Failure("Cannot select due card! Missing box configuration."), new Event[0], "", new Notification[0]);

            var events = new List<Event>();
            
            var binsToCheck = new[] {model.DueBinIndex, 1}.Concat(Enumerable.Range(2, model.Config.Bins.Length - 1).Reverse());
            var dueBinIndex = binsToCheck.FirstOrDefault(binIndex => Check_bin(binIndex, model));
            if (dueBinIndex > 0)
            {
                // there is at least 1 card in a due bin, otherwise it would not have been selected as due
                events.Add(new DueCardSelected{CardId = model.Bins[dueBinIndex].First(), BinIndex = dueBinIndex});
            }
            else
            {
                // fill bin 1 from bin 0
                var additionalCardsForBin1 = model.Bins[0].Take(model.Config.Bins[0].UpperDueThreshold - model.Bins[1].Length).ToArray();
                events.AddRange(additionalCardsForBin1.Select(cardId => new CardMovedTo{CardId = cardId, BinIndex = 1}));
                model.Bins[1] = model.Bins[1].Concat(additionalCardsForBin1).ToArray();

                if (Check_bin(1, model)) {
                    events.Add(new DueCardSelected {CardId = model.Bins[1].First(), BinIndex = 1});
                }
                else
                {
                    for(var i=1; i<model.Bins.Length-2; i++)
                        if (model.Bins[i].Length > 0) {
                            events.Add(new DueCardSelected {CardId = model.Bins[i].First(), BinIndex = i});
                            break;
                        }
                }
            }

            if (events.Count == 0)
                return (new Failure("Not enough cards in box to select due bin."), new Event[0], "",new Notification[0]);
            
            return (new Success(), events.ToArray(), "", new Notification[0]);
        }


        private static bool Check_bin(int binIndex, SelectDueCardContextModel model) {
            if (binIndex <= 0) return false;
            if (binIndex >= model.Bins.Length) return false;
            if (model.Bins[binIndex].Length == 0) return false;

            var configIndex = binIndex - 1;
            if (configIndex < 0) return false;
            if (configIndex >= model.Config.Bins.Length) return false;

            if (binIndex == model.DueBinIndex)
                return model.Bins[binIndex].Length >= model.Config.Bins[configIndex].LowerDueThreshold;
            
            return model.Bins[binIndex].Length >= model.Config.Bins[configIndex].UpperDueThreshold;
        }
    }
}