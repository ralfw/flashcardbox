using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using CsvHelper;

namespace flashcardbox
{
    public class FlashcardboxDb
    {
        private const string FLASHCARDS_FILENAME = "flashcards.csv";
        private const string FLASHCARDBOX_CONFIG_FILENAME = "flashcardbox.config.json";
        
        private readonly string _path;

        public FlashcardboxDb(string path) {
            if (Directory.Exists(path) is false) throw new InvalidOperationException($"Flashcardbox directory {path} does not exist!");
            _path = path;
        }

        public IEnumerable<FlashcardRecord> LoadFlashcards()
        {
            var filepath = Path.Combine(_path, FLASHCARDS_FILENAME);
            using (var reader = new StreamReader(filepath))
            using (var csv = new CsvReader(reader))
            {
                // csv.GetRecords<FlashcardReocord>() somehow does not work :-(
                // Need to do reading and mapping manually. But that's still easier with the
                // lib, because it parses multi-line values.
                csv.Configuration.HasHeaderRecord = true;
                csv.Configuration.Delimiter = ";";

                csv.Read(); // header
                csv.ReadHeader(); // parse (badly named method!)

                while (csv.Read()) {
                    yield return new FlashcardRecord {
                        Id = csv.GetField("Id"),
                        Question = csv.GetField("Question"),
                        Answer = csv.GetField("Answer"),
                        Tags = csv.GetField("Tags"),
                        BinIndex = csv.GetField("BinIndex")
                    };
                }
            }
        }

        public void StoreFlashcards(IEnumerable<FlashcardRecord> records)
        {
        }

        public FlashcardboxConfig LoadConfig()
        {
            throw new NotImplementedException();
        }
    }

    public class FlashcardboxConfig
    {
        public class Bin {
            public int UpperDueThreshold;
            public int LowerDueThreshold;
        }
        
        public Bin[] Bins;
    }

    public class FlashcardRecord {
        public string Id { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
        public string Tags { get; set; }
        public string BinIndex { get; set; }
    }
}