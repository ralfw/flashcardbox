using nsimplemessagepump.contract;

namespace flashcardbox.messages.queries.duecard
{
    public class DueCardQuery : Query {}

    public abstract class DueCardQueryResult : QueryResult {}

    public class DueCardFoundQueryResult : DueCardQueryResult {
        public string CardId;
        public string Question;
        public string Answer;
        public string Tags;
        public int BinIndex;
    }
    
    public class DueCardNotFoundQueryResult : DueCardQueryResult {}
}