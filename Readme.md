# Language Detection

.NET Port of [Language Detection Library for Java](https://code.google.com/p/language-detection/) by [@shuyo](https://github.com/shuyo)

## Install

Add a reference to `LanguageDetection.dll`.

## Use

    using LanguageDetection;
    
Load all supported languages
    
    LanguageDetector detector = new LanguageDetector();
    detector.AddAllLanguages();
    Assert.AreEqual("lv", detector.Detect("čau, man iet labi, un kā iet tev?"));
    
or a small subset

    LanguageDetector detector = new LanguageDetector();
    detector.AddLanguages("lv", "lt", "en");
    Assert.AreEqual("lv", detector.Detect("čau, man iet labi, un kā iet tev?"));

## License

Apache 2.0