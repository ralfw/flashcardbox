using System;
using System.Collections.Generic;
using System.IO;
using CsvHelper;

namespace flashcardbox.backend.adapters
{
    public class FlashcardboxDb
    {
        private const string FLASHCARDS_FILENAME = "flashcards.csv";
        private const string FLASHCARDBOX_CONFIG_FILENAME = "flashcardbox.config.json";
        private static readonly FlashcardboxConfig DEFAULT_FLASHCARDBOX_CONFIG = new FlashcardboxConfig {
            Bins = new[] {
                new FlashcardboxConfig.Bin{ LowerDueThreshold = 4, UpperDueThreshold = 30},
                new FlashcardboxConfig.Bin{ LowerDueThreshold = 40, UpperDueThreshold = 60},
                new FlashcardboxConfig.Bin{ LowerDueThreshold = 120, UpperDueThreshold = 150},
                new FlashcardboxConfig.Bin{ LowerDueThreshold = 200, UpperDueThreshold = 240},
                new FlashcardboxConfig.Bin{ LowerDueThreshold = 370, UpperDueThreshold = 420},
            }
        };
        
        private readonly string _path;

        
        public FlashcardboxDb(string path) {
            if (Directory.Exists(path) is false) throw new InvalidOperationException($"Flashcardbox directory {path} does not exist!");
            _path = path;
        }

        public IEnumerable<FlashcardRecord> LoadFlashcards()
        {
            var filepath = Path.Combine(_path, FLASHCARDS_FILENAME);
            using (var reader = new StreamReader(filepath))
            using (var csv = new CsvReader(reader)) {
                // csv.GetRecords<FlashcardReocord>() somehow does not work :-(
                // Need to do reading and mapping manually. But that's still easier with the
                // lib, because it parses multi-line values.
                csv.Configuration.HasHeaderRecord = true;
                csv.Configuration.Delimiter = ";";

                csv.Read(); // header
                csv.ReadHeader(); // parse (badly named method!)
                // after this the column names are known an get used for field access

                while (csv.Read())
                    yield return new FlashcardRecord {
                        Id = csv.GetField("Id"),
                        Question = csv.GetField("Question"),
                        Answer = csv.GetField("Answer"),
                        Tags = csv.GetField("Tags"),
                        BinIndex = csv.GetField("BinIndex")
                    };
            }
        }

        
        public void StoreFlashcards(IEnumerable<FlashcardRecord> records)
        {
            var filepath = Path.Combine(_path, FLASHCARDS_FILENAME);
            using (var writer = new StreamWriter(filepath)) {
                writer.WriteLine($"Question;Answer;Tags;BinIndex;Id");
                foreach (var r in records)
                    writer.WriteLine($"\"{r.Question}\";\"{r.Answer}\";{r.Tags};{r.BinIndex};{r.Id}");
            }
        }
        

        public FlashcardboxConfig LoadConfig() {
            var filepath = Path.Combine(_path, FLASHCARDBOX_CONFIG_FILENAME);
            if (File.Exists(filepath) is false) return DEFAULT_FLASHCARDBOX_CONFIG;
            
            var configJson = File.ReadAllText(filepath);
            var config = Newtonsoft.Json.JsonConvert.DeserializeObject<FlashcardboxConfig>(configJson);
            return config.Bins.Length == 0 ? DEFAULT_FLASHCARDBOX_CONFIG : config;
        }
    }
}