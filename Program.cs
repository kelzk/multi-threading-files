using System.Collections.Concurrent;

namespace Threads
{
    internal class Program
    {
        private static int _completedThreadNum = 0;
        private static int _numOfWords = 0;
        private static int _wordLength = 0;
        private static string? _longestString = null;
        private static string? _shortestString = null;
        private static ConcurrentDictionary<string, int> _dictionary = new ConcurrentDictionary<string, int>();
        public static void AnalyzeFile(object stateInfo)
        {
            string path = stateInfo as string;
            string text = File.ReadAllText(path);
            string[] words = text.Split(" ");
            Interlocked.Add(ref _numOfWords, words.Length);
            foreach (string word in words)
            {
                _dictionary.AddOrUpdate(word, 1, (key, value) => value + 1);
                int length = word.Length;
                if (char.IsPunctuation(word[length - 1]))
                {
                    length -= 1;
                }
                Interlocked.Add(ref _wordLength, length);

                if (length > _longestString.Length)
                {
                    Interlocked.Exchange(ref _longestString, word);
                }
                if (length < _shortestString.Length)
                {
                    Interlocked.Exchange(ref _shortestString, word);
                }
            }
            Interlocked.Increment(ref _completedThreadNum);
        }
        public static void Main()
        {
            ThreadPool.QueueUserWorkItem(AnalyzeFile, "../../../text/text1.txt");
            ThreadPool.QueueUserWorkItem(AnalyzeFile, "../../../text/text2.txt");
            while (_completedThreadNum < 2)
            {
                ;
            }
            IEnumerator<KeyValuePair<string, int>> enumerator = _dictionary.GetEnumerator();
            int occurence = 0;
            string mostCommonWord = "";
            while (enumerator.MoveNext())
            {
                KeyValuePair<string, int> pair = enumerator.Current;
                if (pair.Value > occurence)
                {
                    occurence = pair.Value;
                    mostCommonWord = pair.Key;
                }
            }

            Console.WriteLine($"num of words: {_numOfWords}\n" +
                              $"word length: {_wordLength}\n" +
                              $"Average word length: {_wordLength / _numOfWords}\n" +
                              $"longest string: {_longestString}\n" +
                              $"shortest string: {_shortestString}\n" +
                              $"most common word: \"{mostCommonWord}\" with occurence {occurence} times\n");
        }
    }
}
