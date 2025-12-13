

namespace Translator.Types
{
    public class TextResult
    {
        public string Text { get; set; }
        public string Script { get; set; }
    }

    public class Alignment
    {
        public string Proj { get; set; }
    }
    public class SentenceLength
    {
        public int SourceLen { get; set; }
        public int TransLen { get; set; }
    }
    public class DetectedLanguage
    {
        public string Language { get; set; }
        public float Score { get; set; }
    }
    public class Translation
    {
        public string Text { get; set; }
        public TextResult Transliteration { get; set; }
        public string To { get; set; }
        public Alignment Alignment { get; set; }
        public SentenceLength SentenceLength { get; set; }
    }
    public class TranslationResult
    {
        public DetectedLanguage DetectedLanguage { get; set; }
        public TextResult SourceText { get; set; }
        public Translation[] Translations { get; set; }
    }
}
