using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AIChatbot
{
    public class FAQBot
    {
        private readonly List<FAQ> _faqs;
        private readonly Dictionary<string, ConversationData> _conversations = new Dictionary<string, ConversationData>();

        private readonly HashSet<string> _greetings = new HashSet<string>
        {
            "hi","hello","hey","yo","hii","hola","sup","good morning","good evening", "whats up"
        };

        private readonly HashSet<string> _byeWords = new HashSet<string>
        {
            "bye","goodbye","cya","see ya","later","ttyl","farewell", "exit", "clear"
        };

        private readonly HashSet<string> _thanksWords = new HashSet<string>
        {
            "thanks","thank you","thx","ty", "salamat"
        };

        private readonly HashSet<string> _yesWords = new HashSet<string>
        {
            "yes","yep","yeah","sure","ok","okay", "k"
        };

        private readonly HashSet<string> _noWords = new HashSet<string>
        {
            "no","nope","nah","not really", "naw"
        };

        public FAQBot()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "FAQs.json");

            if (!File.Exists(path))
            {
                Console.WriteLine("ERROR: FAQs.json not found at: " + path);
                _faqs = new List<FAQ>();
                return;
            }

            _faqs = JsonConvert.DeserializeObject<List<FAQ>>(File.ReadAllText(path)) ?? new List<FAQ>();
            Console.WriteLine($"Loaded {_faqs.Count} FAQ items.");
        }

        public async Task<string> GetResponseAsync(string userInput, string conversationId = null)
        {
            await Task.CompletedTask; // Make it async for future extensions

            userInput = userInput?.Trim().ToLower() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(userInput))
                return "Please send a message.";

            // Get or create conversation data
            if (conversationId == null)
                conversationId = Guid.NewGuid().ToString();

            if (!_conversations.ContainsKey(conversationId))
                _conversations[conversationId] = new ConversationData();

            var conversationData = _conversations[conversationId];

            if (conversationData.AwaitingCloseConfirm)
            {
                if (_yesWords.Contains(userInput))
                {
                    conversationData.AwaitingCloseConfirm = false;
                    _conversations.Remove(conversationId);
                    return "Okay, closing chat now. Feel free to start a new conversation anytime!";
                }

                if (_noWords.Contains(userInput))
                {
                    conversationData.AwaitingCloseConfirm = false;
                    return "Alright, how else can I help you?";
                }

                return "Please say yes or no.";
            }

            // --- Greetings ---
            if (_greetings.Contains(userInput))
            {
                return "Hello! How can I help you today?";
            }

            // --- Bye or Thanks triggers close confirmation ---
            if (_byeWords.Contains(userInput) || _thanksWords.Contains(userInput))
            {
                conversationData.AwaitingCloseConfirm = true;
                return "Do you want to close the chat?";
            }

            // --- Direct Keyword Match ---
            var directMatch = FindKeywordMatch(userInput);
            if (directMatch != null)
            {
                return directMatch.answer;
            }

            // --- One-word logic ---
            if (userInput.Split(' ').Length == 1)
            {
                var oneWord = FindOneWordMatch(userInput);
                if (oneWord != null)
                {
                    return oneWord.answer;
                }
            }

            // --- Suggestions ---
            var suggestion = FindSuggestion(userInput);
            if (suggestion != null)
            {
                return $"Did you mean: {suggestion.question}?";
            }

            // --- Default fallback ---
            return "I am not sure about that yet. Try asking about accounts, support, settings, or help.";
        }

        private FAQ FindKeywordMatch(string input)
        {
            var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            return _faqs
                .Select(f => new
                {
                    FAQ = f,
                    Score = ScoreKeywords(words, f.question.ToLower())
                })
                .OrderByDescending(x => x.Score)
                .Where(x => x.Score >= 0.2)
                .Select(x => x.FAQ)
                .FirstOrDefault();
        }

        private FAQ FindOneWordMatch(string input)
        {
            return _faqs
                .Select(f => new
                {
                    FAQ = f,
                    Score = f.question.ToLower().Contains(input) ? 1.0 : 0.0
                })
                .Where(x => x.Score > 0)
                .Select(x => x.FAQ)
                .FirstOrDefault();
        }

        private double ScoreKeywords(string[] inputWords, string question)
        {
            double score = 0;
            foreach (var w in inputWords)
            {
                if (question.Contains(w)) score += 1;
            }
            return score / Math.Max(1, question.Split(' ').Length);
        }

        private FAQ FindSuggestion(string input)
        {
            return _faqs
                .Select(f => new { FAQ = f, Score = CosineSim(input, f.question.ToLower()) })
                .Where(x => x.Score > 0.1)
                .OrderByDescending(x => x.Score)
                .Select(x => x.FAQ)
                .FirstOrDefault();
        }

        private double CosineSim(string a, string b)
        {
            var wa = a.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var wb = b.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var all = wa.Union(wb).ToList();
            if (!all.Any()) return 0;

            var va = all.Select(w => wa.Count(x => x == w)).ToArray();
            var vb = all.Select(w => wb.Count(x => x == w)).ToArray();

            double dot = 0, ma = 0, mb = 0;

            for (int i = 0; i < all.Count; i++)
            {
                dot += va[i] * vb[i];
                ma += va[i] * va[i];
                mb += vb[i] * vb[i];
            }

            if (ma == 0 || mb == 0) return 0;

            return dot / (Math.Sqrt(ma) * Math.Sqrt(mb));
        }

        private class ConversationData
        {
            public bool AwaitingCloseConfirm { get; set; } = false;
        }
    }

    public class FAQ
    {
        public string question { get; set; }
        public string answer { get; set; }
    }
}