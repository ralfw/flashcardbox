using System;
using flashcardbox.backend.adapters;
using flashcardbox.backend.integration;
using nsimpleeventstore;

namespace flashcardbox
{
    class Program
    {
        static void Main(string[] args) {
            var cli = new CLI(args);
            
            var db = new FlashcardboxDb(cli.DbPath);
            var es = new FilebasedEventstore(cli.EventstorePath);
            var mh = new MessageHandling(es, db);
            
            var ui = new UI(mh);
            
            ui.Show();
        }
    }
}