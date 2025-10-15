# 🌙 Arabic Support for Unity - Enhanced Edition

> *Making Arabic text in Unity as smooth as butter... or should I say, as smooth as زبدة?* 🧈

A friendly fork of [Konash's Arabic Support](https://github.com/Konash/arabic-support-unity) with extra goodies for TextMeshPro lovers! ✨

## 💝 What's This All About?

Arabic text can be tricky in Unity - it reads right-to-left, letters connect in fancy ways, and sometimes things just look... backwards. This enhanced version takes the original Arabic Support library and wraps it in a cozy blanket of TextMeshPro goodness!

## ✨ What Makes This Fork Special?

### 🎯 Auto-Magic TextMeshPro Integration
- **Drop and forget!** Just add a TMP_Text component and boom - Arabic support automatically attaches itself
- No more manual setup for every single text object (your wrists will thank you!)

### 🔄 Smart Processing Modes
Choose your adventure:
- **Once At Awake**: Set it and forget it! Perfect for static text
- **Real-Time Detection**: Watches your text like a hawk 🦅 and fixes it automatically
- **On Demand Only**: For when you like to be in control (we respect that!)

### ⌨️ Typing Effect Friendly
Got a cool typing animation? No problem!
```csharp
handler.StartTypingEffect();
// Do your typing magic here...
handler.EndTypingEffect(); // Auto-processes when done!
```

### 🛡️ Better Error Handling
- Safe character-by-character processing if something goes wrong
- Detailed error logging (because we care about you knowing what happened)
- Won't crash your game if it encounters a grumpy Unicode character

### 🎮 Edit Mode Preview
See your Arabic text correctly formatted right in the Unity Editor - no need to hit play!

## 🚀 Quick Start

### Installation
1. Import the original Arabic Support package (we're standing on the shoulders of giants here!)
2. Add these three scripts to your project:
   - `ArabicSupport.cs` (if not already there)
   - `ArabicTextHandler.cs`
   - `TMP_AutoAttach.cs`

### Basic Usage

**The Lazy Way (Recommended!)** 😴
1. Add a TextMeshPro component to your GameObject
2. Done! The `ArabicTextHandler` attaches itself automatically
3. Type some Arabic text
4. Watch the magic happen! ✨

**The Control Freak Way** 🎮
```csharp
ArabicTextHandler handler = GetComponent<ArabicTextHandler>();
handler.processingMode = ProcessingMode.OnDemandOnly;
handler.textComponent.text = "مرحبا بالعالم";
handler.ForceProcessText();
```

**For Typing Effects** ⌨️
```csharp
handler.StartTypingEffect();
foreach (char c in arabicText)
{
    handler.SetTextWithoutProcessing(currentText + c);
    yield return new WaitForSeconds(0.05f);
}
handler.EndTypingEffect();
```

## ⚙️ Configuration Options

### In The Inspector
- **Processing Mode**: When to fix that Arabic text
- **Show Tashkeel**: Keep those beautiful diacritical marks (◍•ᴗ•◍)
- **Use Hindu Numbers**: ٠١٢٣٤٥٦٧٨٩ instead of 0123456789
- **Startup Delay**: Give your scene a moment to breathe before processing
- **Error Handling**: Skip problematic characters or let exceptions fly

### Programmatic Control
```csharp
// Pause processing during sensitive operations
handler.pauseProcessing = true;

// Change modes on the fly
handler.SetProcessingMode(ProcessingMode.RealTimeDetection);

// Manual override
handler.ForceProcessText();

// Reset if things get weird
handler.ResetState();
```

## 🎨 Features

- ✅ Supports Arabic, Persian, and Urdu
- ✅ Handles Tashkeel (diacritical marks)
- ✅ Hindu-Arabic numerals support
- ✅ Mixed Arabic/English text
- ✅ Automatic TextMeshPro detection
- ✅ Real-time text monitoring
- ✅ Typing effect support
- ✅ Edit mode preview
- ✅ Error recovery
- ✅ Multi-line text support
- ✅ Smart punctuation handling

## 🐛 Troubleshooting

**Text not processing?**
- Check if `pauseProcessing` is false
- Make sure you waited for `startupDelay` (default: 0.1s)
- Try hitting "Force Process Text" button in inspector

**Getting stuck in a loop?**
- Hit the "Reset State" button in inspector
- Check your processing mode settings

**Still having issues?**
- Check the `lastError` field in inspector for clues
- Enable `logErrors` for detailed console messages

## 🙏 Credits & Thanks

- **Original Arabic Support**: Created by the amazing [Abdullah Konash](https://github.com/Konash)
- **This Fork**: Enhanced with TextMeshPro integration and quality-of-life improvements
- **You**: For using this and hopefully making something cool! 🎮

## 📜 License

MIT License - Same as the original! Feel free to use, modify, and share. Just remember to spread the love and credit the original work.

## 💌 Final Words

Making games in Arabic (or Persian, or Urdu) should be easy and fun. This fork tries to make that happen with as little friction as possible. If you make something cool with this, I'd love to see it!

Happy coding, and may your text always flow in the right direction! 🌟

---

*"لا تكن صعباً" - Don't be difficult!* 

(That's what this fork is all about - making things not difficult! 😊)