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

using System.Collections.Generic;
using System.IO;

namespace LanguageDetection
{
    class LanguageProfile
    {
        public string Code { get; set; }
        public Dictionary<string, int> Frequencies { get; set; }
        public int[] WordCount { get; set; }

        public void Load(Stream stream)
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                Code = reader.ReadString();

                int frequenciesCount = reader.ReadInt32();
                Frequencies = new Dictionary<string, int>(frequenciesCount);
                for (int i = 0; i < frequenciesCount; i++)
                    Frequencies.Add(reader.ReadString(), reader.ReadInt32());

                int wordCounts = reader.ReadInt32();
                WordCount = new int[wordCounts];
                for (int i = 0; i < wordCounts; i++)
                    WordCount[i] = reader.ReadInt32();
            }
        }

        public void Save(Stream stream)
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(Code);

                writer.Write(Frequencies.Count);
                foreach (string word in Frequencies.Keys)
                {
                    writer.Write(word);
                    writer.Write(Frequencies[word]);
                }

                writer.Write(WordCount.Length);
                foreach (int count in WordCount)
                    writer.Write(count);
            }
        }
    }
}
