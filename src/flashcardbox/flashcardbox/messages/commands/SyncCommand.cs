using nsimplemessagepump.contract;

namespace flashcardbox.messages.commands
{
    public class SyncCommand : Command {}

    public class SyncSuccess : Success {
        public int Added;
        public int Changed;
        public int Missing;
        public int TotalCount;
    }
}