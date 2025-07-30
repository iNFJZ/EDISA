namespace Shared.LanguageService
{
    public interface ILanguageService
    {
        string GetText(string key, string language = "en", params object[] args);
        string GetTimeAgo(DateTime dateTime, string language = "en");
        bool IsValidLanguage(string language);
        List<string> GetSupportedLanguages();
    }
} 