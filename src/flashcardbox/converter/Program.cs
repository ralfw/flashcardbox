using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace converter
{
    class Program
    {
        static void Main(string[] args)
        {
            var lines = File.ReadAllLines("bulgarisch.txt");
            var rd = new Reader(lines);

            var cards = MapToCards(rd);

            Export(cards);
        }

        private static void Export(IEnumerable<(string question, string answer)> cards)
        {
            const string FILENAME = "bulgarisch.csv";
            File.Delete(FILENAME);
            foreach (var card in cards) {
                Console.WriteLine(card.question);
                
                var text = $"\"{card.question}\"\t\"{card.answer}\"\n";
                File.AppendAllText(FILENAME, text);
            }
        }

        private static IEnumerable<(string question, string answer)> MapToCards(Reader rd)
        {
            while (rd.IsEmpty is false) {
                var question = GetQuestion(rd);
                var answer = GetAnswer(rd);
                yield return (question, answer);
            }
        }

        private static string GetAnswer(Reader rd) {
            var text = new StringBuilder();
            while (rd.TryReadLine(out var line)) {
                if (line == "$$") break;

                text.AppendLine(line);
            }
            return text.ToString().Trim();
        }

        private static string GetQuestion(Reader rd) {
            var text = new StringBuilder();
            while (rd.TryReadLine(out var line))
            {
                var iTab = line.IndexOf("\t");
                if (iTab >= 0) {
                    var questionSegment = line.Substring(0, iTab);
                    text.AppendLine(questionSegment);
                    var answerSegment = line.Substring(iTab + 1);
                    rd.PushBack(answerSegment);
                    break;
                }

                text.AppendLine(line);
            }
            return text.ToString().Trim();
        }
    }

    
    class Reader
    {
        private readonly List<string> _lines;

        public Reader(string[] lines)
        {
            _lines = new List<string>(lines);
        }

        public bool TryReadLine(out string line)
        {
            line = "";
            if (IsEmpty) return false;

            line = _lines[0];
            _lines.RemoveAt(0);
            return true;
        }

        public void PushBack(string line) => _lines.Insert(0, line);

        public bool IsEmpty => _lines.Count == 0;
    }
    
}