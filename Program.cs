using System.Collections.Concurrent;

namespace Threads
{
    internal class Program
    {
        private static int _completedThreadNum = 0;
        private static int _numOfWords = 0;
        private static int _wordLength = 0;
        private static List<string> _longestStrings = new List<string>();
        private static List<string> _shortestStrings = new List<string>();
        private static object listLock = new object();
        private static ConcurrentDictionary<string, int> _dictionary = new ConcurrentDictionary<string, int>();

        public static void AnalyzeFile(object stateInfo)
        {
            string path = stateInfo as string;
            string text = File.ReadAllText(path);
            string[] words = text.Split(" ");
            Interlocked.Add(ref _numOfWords, words.Length);
            string longestString, shortestString;
            longestString = shortestString = words[0];

            foreach (string word in words)
            {
                _dictionary.AddOrUpdate(word, 1, (key, value) => value + 1);
                int length = word.Length;

                if (char.IsPunctuation(word[length - 1]))
                {
                    length -= 1;
                }

                Interlocked.Add(ref _wordLength, length);

                if (length > longestString.Length)
                {
                    longestString = word;
                }

                if (length < shortestString.Length)
                {
                    shortestString = word;
                }
            }

            lock (listLock)
            {
                _longestStrings.Add(longestString);
                _shortestStrings.Add(shortestString);
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
            string? mostCommonWord = null;

            while (enumerator.MoveNext())
            {
                KeyValuePair<string, int> pair = enumerator.Current;

                if (pair.Value > occurence)
                {
                    occurence = pair.Value;
                    mostCommonWord = pair.Key;
                }
            }

            string longestWord = _longestStrings[0];

            foreach (string word in _longestStrings)
            {
                if (longestWord.Length < word.Length)
                {
                    longestWord = word;
                }
            }

            string shortestWord = _shortestStrings[0];

            foreach (string word in _shortestStrings)
            {
                if (shortestWord.Length > word.Length)
                {
                    shortestWord = word;
                }
            }

            Console.WriteLine($"num of words: {_numOfWords}\n" +
                              $"word length: {_wordLength}\n" +
                              $"Average word length: {_wordLength / _numOfWords}\n" +
                              $"longest string: {longestWord}\n" +
                              $"shortest string: {shortestWord}\n" +
                              $"most common word: \"{mostCommonWord}\" with occurence {occurence} times\n");

            Console.ReadKey();
        }
    }
}
