using System;
using flashcardbox.messages.commands.selectduecard;
using nsimplemessagepump.contract;
using nsimplemessagepump.contract.messagecontext;
using nsimplemessagepump.contract.messageprocessing;

namespace flashcardbox.messages.queries.progress
{
    internal class ProgressProcessor : IQueryProcessor {
        public QueryResult Process(IMessage _, IMessageContextModel ctx)
        {
            var model = ctx as SelectDueCardContextModel;

            var result = new ProgressQueryResult {
                Bins = new ProgressQueryResult.Bin[Math.Max(model.Bins.Length, model.Config?.Bins.Length+2 ?? 0)]
            };
            
            for(var i = 0; i < result.Bins.Length; i++)
                result.Bins[i] = new ProgressQueryResult.Bin {
                    Count = i < model.Bins.Length ? model.Bins[i].Length : 0
                };

            if (model.Config != null)
                for (var i = 1; i < result.Bins.Length; i++) {
                    var j = i - 1;
                    if (j < model.Config.Bins.Length) {
                        result.Bins[i].LowerDueThreshold = model.Config.Bins[j].LowerDueThreshold;
                        result.Bins[i].UpperDueThreshold = model.Config.Bins[j].UpperDueThreshold;
                    }
                }

            if (model.DueBinIndex > 0 && model.DueBinIndex < result.Bins.Length)
                result.Bins[model.DueBinIndex].IsDue = true;
            
            return result;
        }
    }
}