using System;
using System.Security.Cryptography;
using System.Text;

namespace flashcardbox.messages.commands
{
    internal static class FlashcardHash
    {
        public static string Calculate(string question, string answer, string tags) {
            var value = question + answer + tags;
            var bytes = new UTF8Encoding().GetBytes(value);
            var hash = ((HashAlgorithm) CryptoConfig.CreateFromName("MD5")).ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
        }
    }
}