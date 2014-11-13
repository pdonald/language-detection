// Copyright 2014 Pēteris Ņikiforovs
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

using NUnit.Framework;

namespace LanguageDetection.Tests
{
    [TestFixture]
    public class LanguageDetectorTests
    {
        [Test]
        public void Latvian()
        {
            string[] texts = new[] {
                "čau, man iet labi, un kā iet tev?",
                "Ukrainas prezidenta pienākumu izpildītājs Oleksandrs Turčinovs trešdienas vakarā devis Krimas separātistu "
            };

            Test("lv", texts, new[] { new[] { "en", "fr", }, new[] { "lt", "pt" } });
        }

        [Test]
        public void French()
        {
            string[] texts = new[] {
                "Le français est une langue indo-européenne de la famille des langues romanes. Le français s'est formé en France",
                "La langue française commence à prendre de l'importance en 1250, lorsque Saint Louis commande une traduction de la Bible en français.",
                "Nous voulons donc que dorénavant tous arrêts, et ensemble toutes autres procédures, soit de nos cours souveraines ou autres subalternes et inférieures, soit des registres, enquêtes,",
                "il neige et le soleil brille et nous regardons la radio et la télé. le cinéma est très français"
            };

            Test("fr", texts, new[] { new[] { "en", "it", "es", "pt" } });
        }

        [Test]
        public void Issue_1()
        {
            string[] texts = new[] { "Výsledky kvalifikace slopestylu na ZOH v Soči" };

            Test("cs", texts);
            Test("cs", texts, new[] { new[] { "cs", "sk" } });
            Test("cs", texts, new[] { new[] { "en", "cs", "sk", "lv" } });
        }

        private void Test(string lang, string[] texts, string[][] pairs = null)
        {
            LanguageDetector detector;
            
            detector = new LanguageDetector();
            detector.AddAllLanguages();

            foreach (string text in texts)
                Assert.AreEqual(lang, detector.Detect(text));

            if (pairs != null)
            {
                foreach (string[] pair in pairs)
                {
                    detector = new LanguageDetector();
                    detector.AddLanguages(pair);
                    detector.AddLanguages(lang);
                    
                    foreach (string text in texts)
                        Assert.AreEqual(lang, detector.Detect(text));
                }
            }
        }
    }
}
