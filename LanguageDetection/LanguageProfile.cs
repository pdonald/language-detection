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
