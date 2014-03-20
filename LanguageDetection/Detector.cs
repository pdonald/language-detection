// Copyright 2014 Pēteris Ņikiforovs
// Copyright 2014 Nakatani Shuyo
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace LanguageDetection
{
    public class Detector
    {
        private static readonly Regex UrlRegex = new Regex("https?://[-_.?&~;+=/#0-9A-Za-z]{1,2076}", RegexOptions.Compiled);
        private static readonly Regex EmailRegex = new Regex("[-_.0-9A-Za-z]{1,64}@[-_0-9A-Za-z]{1,255}[-_.0-9A-Za-z]{1,255}", RegexOptions.Compiled);
        private const string ResourceNamePrefix = "LanguageDetection.Profiles.";
        private const double AlphaDefault = 0.5;
        private const double AlphaWidth = 0.05;
        private const int MaxIterations = 1000;
        private const double ProbabilityThreshold = 0.1;
        private const double ConvergenceThreshold = 0.99999;
        private const int BaseFrequency = 10000;

        private List<LanguageProfile> languages;
        private Dictionary<string, Dictionary<LanguageProfile, double>> wordLanguageProbabilities;

        public Detector()
        {
            Alpha = AlphaDefault;
            RandomSeed = null;
            Trials = 7;
            NGramLength = 3;
            MaxTextLength = 10000;

            languages = new List<LanguageProfile>();
            wordLanguageProbabilities = new Dictionary<string, Dictionary<LanguageProfile, double>>();
        }

        public double Alpha { get; set; }
        public int? RandomSeed { get; set; }
        public int Trials { get; set; }
        public int NGramLength { get; set; }
        public int MaxTextLength { get; set; }

        public void AddAllLanguages()
        {
            AddLanguages(GetType().Assembly.GetManifestResourceNames()
                                           .Where(name => name.StartsWith(ResourceNamePrefix))
                                           .Select(name => name.Substring(ResourceNamePrefix.Length).Replace(".bin.gz", ""))
                                           .ToArray());
        }

        public void AddLanguages(params string[] languages)
        {
            Assembly assembly = GetType().Assembly;

            foreach (string language in languages)
            {
                using (Stream stream = assembly.GetManifestResourceStream(ResourceNamePrefix + language + ".bin.gz"))
                using (Stream decompressedStream = new GZipStream(stream, CompressionMode.Decompress))
                {
                    LanguageProfile profile = new LanguageProfile();
                    profile.Load(decompressedStream);
                    AddLanguageProfile(profile);
                }
            }
        }
        
        private void AddLanguageProfile(LanguageProfile profile)
        {
            languages.Add(profile);

            foreach (string word in profile.Frequencies.Keys)
            {
                if (!wordLanguageProbabilities.ContainsKey(word))
                    wordLanguageProbabilities[word] = new Dictionary<LanguageProfile, double>();

                if (word.Length >= 1 && word.Length <= NGramLength)
                {
                    double prob = (double)profile.Frequencies[word] / profile.WordCount[word.Length - 1];
                    wordLanguageProbabilities[word][profile] = prob;
                }
            }
        }

        public string Detect(string text)
        {
            DetectedLanguage language = DetectAll(text).FirstOrDefault();
            return language != null ? language.Language : null;
        }

        public IEnumerable<DetectedLanguage> DetectAll(string text)
        {
            List<string> ngrams = ExtractNGrams(NormalizeText(text));
            if (ngrams.Count == 0)
                return new DetectedLanguage[0];

            double[] languageProbabilities = new double[languages.Count];
            
            Random random = RandomSeed != null ? new Random(RandomSeed.Value) : new Random();

            for (int t = 0; t < Trials; t++)
            {
                double[] probs = InitializeProbabilities();
                double alpha = Alpha + random.NextDouble() * AlphaWidth;

                for (int i = 0; ; i++)
                {
                    int r = random.Next(ngrams.Count);
                    UpdateProbabilities(probs, ngrams[r], alpha);
                    
                    if (i % 5 == 0)
                    {
                        if (NormalizeProbabilities(probs) > ConvergenceThreshold || i >= MaxIterations) 
                            break;
                    }
                }

                for (int j = 0; j < languageProbabilities.Length; j++) 
                    languageProbabilities[j] += probs[j] / Trials;
            }

            return SortProbabilities(languageProbabilities);
        }

        private List<string> ExtractNGrams(string text)
        {
            List<string> list = new List<string>();

            NGram ngram = new NGram();

            foreach (char c in text)
            {
                ngram.Add(c);

                for (int n = 1; n <= NGram.N_GRAM; n++)
                {
                    string w = ngram.Get(n);

                    if (w != null && wordLanguageProbabilities.ContainsKey(w))
                        list.Add(w);
                }
            }

            return list;
        }

        #region Normalize text
        private string NormalizeText(string text)
        {
            if (text.Length > MaxTextLength)
                text = text.Substring(0, MaxTextLength);

            text = RemoveAddresses(text);
            text = NormalizeAlphabet(text);
            text = NormalizeVietnamese(text);
            text = NormalizeWhitespace(text);

            return text;
        }

        private static string NormalizeAlphabet(string text)
        {
            int latinCount = 0;
            int nonLatinCount = 0;

            for (int i = 0; i < text.Length; ++i)
            {
                char c = text[i];

                if (c <= 'z' && c >= 'A')
                {
                    ++latinCount;
                }
                else if (c >= '\u0300' && !(c >= 0x1e00 && c <= 0x1eff))
                {
                    ++nonLatinCount;
                }
            }

            if (latinCount * 2 < nonLatinCount)
            {
                StringBuilder textWithoutLatin = new StringBuilder();
                for (int i = 0; i < text.Length; ++i)
                {
                    char c = text[i];
                    if (c > 'z' || c < 'A')
                        textWithoutLatin.Append(c);
                }
                text = textWithoutLatin.ToString();
            }

            return text;
        }

        private static string NormalizeVietnamese(string text)
        {
            // todo
            return text;
        }

        private static string NormalizeWhitespace(string text)
        {
            StringBuilder sb = new StringBuilder(text.Length);

            char? prev = null;

            foreach (char c in text)
            {
                if (c != ' ' || prev != ' ') 
                    sb.Append(c);
                prev = c;
            }

            return sb.ToString();
        }

        private static string RemoveAddresses(string text)
        {
            text = UrlRegex.Replace(text, " ");
            text = EmailRegex.Replace(text, " ");
            return text;
        }
        #endregion

        #region Probabilities
        private double[] InitializeProbabilities()
        {
            double[] prob = new double[languages.Count];
            for (int i = 0; i < prob.Length; i++) 
                prob[i] = 1.0 / languages.Count;
            return prob;
        }

        private void UpdateProbabilities(double[] prob, string word, double alpha)
        {
            if (word == null || !wordLanguageProbabilities.ContainsKey(word)) 
                return;

            var languageProbabilities = wordLanguageProbabilities[word];
            double weight = alpha / BaseFrequency;
            
            for (int i = 0; i < prob.Length; i++)
            {
                LanguageProfile profile = languages[i];
                prob[i] *= weight + (languageProbabilities.ContainsKey(profile) ? languageProbabilities[profile] : 0);
            }
        }

        private static double NormalizeProbabilities(double[] probs)
        {
            double maxp = 0, sump = 0;
            
            for (int i = 0; i < probs.Length; ++i) 
                sump += probs[i];
            
            for (int i = 0; i < probs.Length; ++i)
            {
                double p = probs[i] / sump;
                if (maxp < p) maxp = p;
                probs[i] = p;
            }

            return maxp;
        }

        private IEnumerable<DetectedLanguage> SortProbabilities(double[] probs)
        {
            List<DetectedLanguage> list = new List<DetectedLanguage>();

            for (int j = 0; j < probs.Length; j++)
            {
                double p = probs[j];

                if (p > ProbabilityThreshold)
                {
                    for (int i = 0; i <= list.Count; i++)
                    {
                        if (i == list.Count || list[i].Probability < p)
                        {
                            list.Insert(i, new DetectedLanguage { Language = languages[j].Code, Probability = p });
                            break;
                        }
                    }
                }
            }

            return list;
        }
        #endregion

        public class DetectedLanguage
        {
            public string Language { get; set; }
            public double Probability { get; set; }
        }

        private class NGram
        {
            public const int N_GRAM = 3;
            
            private StringBuilder buffer = new StringBuilder(" ", N_GRAM);
            private bool capital = false;

            public void Add(char c)
            {
                char lastChar = buffer[buffer.Length - 1];

                if (lastChar == ' ')
                {
                    buffer = new StringBuilder(" ");
                    capital = false;
                    if (c == ' ') return;
                }
                else if (buffer.Length >= N_GRAM)
                {
                    buffer.Remove(0, 1);
                }

                buffer.Append(c);

                if (char.IsUpper(c))
                {
                    if (char.IsUpper(lastChar))
                        capital = true;
                }
                else
                {
                    capital = false;
                }
            }

            public string Get(int n)
            {
                if (capital)
                    return null;

                if (n < 1 || n > N_GRAM || buffer.Length < n) 
                    return null;

                if (n == 1)
                {
                    char c = buffer[buffer.Length - 1];
                    if (c == ' ') return null;
                    return c.ToString();
                }
                else
                {
                    return buffer.ToString(buffer.Length - n, buffer.Length - n);
                }
            }
        }
    }
}
