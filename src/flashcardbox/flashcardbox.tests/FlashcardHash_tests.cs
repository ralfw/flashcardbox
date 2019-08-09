using flashcardbox.messages.commands;
using flashcardbox.messages.commands.sync;
using Xunit;
using Xunit.Abstractions;

namespace flashcardbox.tests
{
    public class FlashcardHash_tests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public FlashcardHash_tests(ITestOutputHelper testOutputHelper) {
            _testOutputHelper = testOutputHelper;
        }

        
        [Fact]
        public void Calculate()
        {
            var resultA = FlashcardHash.Calculate("a", "b", "c");
            _testOutputHelper.WriteLine(resultA);
            
            var resultB = FlashcardHash.Calculate("x", "b", "c");
            Assert.NotEqual(resultA, resultB);
            
            resultB = FlashcardHash.Calculate("a", "x", "c");
            Assert.NotEqual(resultA, resultB);
            
            resultB = FlashcardHash.Calculate("a", "b", "x");
            Assert.NotEqual(resultA, resultB);
        }
    }
}