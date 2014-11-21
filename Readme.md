# Language Detection

.NET Port of [Language Detection Library for Java](https://code.google.com/p/language-detection/) by [@shuyo](https://github.com/shuyo)

## Install

Add a reference to `LanguageDetection.dll`.

## Use

```csharp
using LanguageDetection;
```
    
Load all supported languages
    
```csharp
LanguageDetector detector = new LanguageDetector();
detector.AddAllLanguages();
Assert.AreEqual("lv", detector.Detect("čau, man iet labi, un kā iet tev?"));
```
    
or a small subset

```csharp
LanguageDetector detector = new LanguageDetector();
detector.AddLanguages("lv", "lt", "en");
Assert.AreEqual("lv", detector.Detect("čau, man iet labi, un kā iet tev?"));
```

You can also change parameters

```csharp
LanguageDetector detector = new LanguageDetector();
detector.RandomSeed = 1;
detector.ConvergenceThreshold = 0.9;
detector.MaxIterations = 50;
```
    
## License

Apache 2.0