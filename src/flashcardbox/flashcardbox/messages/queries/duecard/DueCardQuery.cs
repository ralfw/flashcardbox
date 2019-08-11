using nsimplemessagepump.contract;

namespace flashcardbox.messages.queries.duecard
{
    public class DueCardQuery : Query {}

    public class DueCardQueryResult : QueryResult
    {
        public string CardId;
        public string Question;
        public string Answer;
        public string Tags;
        public int BinIndex;
    }
}