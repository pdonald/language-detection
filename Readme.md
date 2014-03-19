# Language Detection

.NET Port of [Language Detection Library for Java](https://code.google.com/p/language-detection/) by [@shuyo](https://github.com/shuyo)

## Install

Add a reference to `LanguageDetection.dll`.

## Use

    using LanguageDetection;
    
    Detector detector = new Detector();
    detector.AddAllLanguages();
    Assert.AreEqual("lv", detector.Detect("čau, man iet labi, un kā iet tev?"));
    
or

    Detector detector = new Detector();
    detector.AddLanguages("lv", "lt", "en");
    Assert.AreEqual("lv", detector.Detect("čau, man iet labi, un kā iet tev?"));

## License

Apache 2.0