using System;
using System.Collections.Generic;
using System.Linq;
using flashcardbox.adapters;
using flashcardbox.events;
using nsimpleeventstore;
using nsimplemessagepump.contract;
using nsimplemessagepump.contract.messagecontext;

namespace flashcardbox.messages.commands.selectduecard
{
    internal class SelectDueCardContextModel : IMessageContextModel
    {
        public string[][] Bins;
        public int DueBinIndex;
        public FlashcardboxConfig Config;
    }
    
    
    /*
     * For all cards find the current bin. Within the bins order them chronologically, ie
     * cards will be added at the end of all cards already present.
     *
     * There is always a bin 0, even if never any card was moved there.
     * 
     * Find the most recent config; maybe there is none.
     */
    internal class SelectDueCardContextManager : IMessageContextModelLoader
    {
        private readonly IEventstore _es;

        public SelectDueCardContextManager(IEventstore es) { _es = es; }


        public (IMessageContextModel Ctx, string Version) Load(IMessage msg)
            => (new SelectDueCardContextModel {
                    Bins = Fill_bins(_es.Replay(typeof(CardMovedTo), typeof(CardFoundMissing)).Events),
                    DueBinIndex = Get_due_bin_index(_es.Replay(typeof(DueCardSelected)).Events),
                    Config = Get_config(_es.Replay(typeof(BoxConfigured)).Events)
                },
                "");


        private static string[][] Fill_bins(Event[] events) {
            var sparseBins = Fill_bins_sparsely(events);
            return Map_sparse_bins(sparseBins);
        }

        private static Dictionary<int, List<string>> Fill_bins_sparsely(Event[] events) {
            var sparseBins = new Dictionary<int, List<string>> {[0] = new List<string>()};
            foreach (var e in events)
                switch (e) {
                    case CardMovedTo cmt:
                        Remove_card(cmt.CardId);
                        Add_card(cmt.CardId, cmt.BinIndex);
                        break;
                    case CardFoundMissing cfm:
                        Remove_card(cfm.CardId);
                        break;
                }
            return sparseBins;
            
            
            void Remove_card(string cardId) {
                foreach (var bin in sparseBins)
                    if (bin.Value.Remove(cardId))
                        break;
            }

            void Add_card(string cardId, int binIndex) {
                if (sparseBins.ContainsKey(binIndex) is false)
                    sparseBins[binIndex] = new List<string>();
                sparseBins[binIndex].Add(cardId);
            }
        }

        private static string[][] Map_sparse_bins(Dictionary<int, List<string>> sparseBins) {
            var bins = new List<string[]>();
            foreach (var i in Enumerable.Range(0, Math.Max(2, sparseBins.Keys.Max() + 1)))
                bins.Add(sparseBins.ContainsKey(i) 
                            ? sparseBins[i].ToArray() 
                            : new string[0]);
            return bins.ToArray();
        }



        private static int Get_due_bin_index(Event[] events) {
            var e = events.LastOrDefault();
            if (e == null) return -1;
            return (e as DueCardSelected).BinIndex;
        }


        private static FlashcardboxConfig Get_config(Event[] events)
        {
            if (events.Length == 0) return null;
            
            return new FlashcardboxConfig {
                Bins = (events.Last() as BoxConfigured).Bins.Select(Map).ToArray()
            };

            
            FlashcardboxConfig.Bin Map(BoxConfigured.Bin bin)
                => new FlashcardboxConfig.Bin {
                    LowerDueThreshold = bin.LowerDueThreshold,
                    UpperDueThreshold = bin.UpperDueThreshold
                };
        }
    }
}