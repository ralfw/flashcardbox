using System;
using System.IO;

namespace flashcardbox
{
    class CLI
    {
        public CLI(string[] args) {
            if (args.Length < 1) {
                Console.WriteLine("*** Missing path to flashcard database directory!");
                Console.WriteLine("Usage: flashcardbox <db directory path>");
                Environment.Exit(1);
            }

            DbPath = args[0];
        }
        
        
        public string DbPath { get; }

        public string EventstorePath => Path.Combine(DbPath, "eventstore");
    }
}