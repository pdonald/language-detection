using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

using Microsoft.Build.Utilities;

using Newtonsoft.Json;

namespace LanguageDetection.Build
{
    public class BinarizeLanguageProfileTask : Task
    {
        public string InputFilename { get; set; }
        public string OutputFilename { get; set; }

        public override bool Execute()
        {
            try
            {
                string json = File.ReadAllText(InputFilename);
                JsonLanguageProfile jsonProfile = JsonConvert.DeserializeObject<JsonLanguageProfile>(json);

                LanguageProfile profile = new LanguageProfile();
                profile.Code = jsonProfile.name;
                profile.Frequencies = jsonProfile.freq;
                profile.WordCount = jsonProfile.n_words;

                using (Stream stream = new FileStream(OutputFilename, FileMode.Create, FileAccess.Write))
                using (Stream compressedStream = new GZipStream(stream, CompressionMode.Compress))
                    profile.Save(compressedStream);

                return true;
            }
            catch (Exception e)
            {
                Log.LogErrorFromException(e);
                return false;
            }
        }

        private class JsonLanguageProfile
        {
            public string name = null;
            public Dictionary<string, int> freq = null;
            public int[] n_words = null;
        }
    }
}
