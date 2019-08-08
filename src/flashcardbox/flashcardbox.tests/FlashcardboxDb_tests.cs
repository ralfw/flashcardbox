using System;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using Xunit;
using Xunit.Abstractions;

namespace flashcardbox.tests
{
    public class FlashcardboxDb_tests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public FlashcardboxDb_tests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        
        [Fact]
        public void Load()
        {
            var sut = new FlashcardboxDb("../../../sampledb_load");
            var result = sut.LoadFlashcards().ToArray();
            
            foreach(var r in result)
                _testOutputHelper.WriteLine($"{r.Id}={r.Question}/{r.Answer} [{r.Tags}] @{r.BinIndex}");
            
            Assert.Equal(3,result.Length);
            Assert.Equal("1", result[0].Id);
            Assert.Equal("q1", result[0].Question);
            Assert.Equal("a1", result[0].Answer);
            Assert.Equal("", result[0].BinIndex);
            Assert.Equal("2", result[1].Id);
            Assert.Equal("t1", result[1].Tags);
            Assert.Equal("1", result[1].BinIndex);
            Assert.Equal("", result[2].Id);
            Assert.Equal("2", result[2].BinIndex);
        }
    }
}