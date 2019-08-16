using System;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using flashcardbox.adapters;
using FluentAssertions;
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
        
        
        [Fact]
        public void Store()
        {
            const string DB_TEST_PATH = "sampledb_store";
            if (Directory.Exists(DB_TEST_PATH)) Directory.Delete(DB_TEST_PATH, true);
            Directory.CreateDirectory(DB_TEST_PATH);
            
            var sut = new FlashcardboxDb(DB_TEST_PATH);
            var flashcards = new[] {
                new FlashcardRecord{Question = "q1", Answer = "a1", Tags = "t1", BinIndex = "1", Id = "1"},
                new FlashcardRecord{Question = "q2.1\nq2.2", Answer = "a2.1\na2.2", Tags = "t2,t3", BinIndex = "", Id = ""}
            };
            
            
            sut.StoreFlashcards(flashcards);

            var result = sut.LoadFlashcards();
            result.Should().BeEquivalentTo(flashcards);
        }
        
        
        [Fact]
        public void LoadConfig()
        {
            var sut = new FlashcardboxDb("../../../sampledb_load");
            var result = sut.LoadConfig();

            result.Should().BeEquivalentTo(new FlashcardboxConfig {
                Bins = new[]
                {
                    new FlashcardboxConfig.Bin
                    {
                        UpperDueThreshold = 20,
                        LowerDueThreshold = 10
                    },
                    new FlashcardboxConfig.Bin
                    {
                        UpperDueThreshold = 50,
                        LowerDueThreshold = 30
                    }
                }
            });
        }
    }
}