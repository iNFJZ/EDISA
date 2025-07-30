using System.Text.Json;

namespace Shared.LanguageService
{
    public class LanguageServiceImplementation : ILanguageService
    {
        private static readonly Dictionary<string, Dictionary<string, object>> _languageData = new();
        private static bool _isInitialized = false;
        private static readonly object _lockObject = new object();

        public LanguageServiceImplementation()
        {
            Initialize();
        }

        private static void Initialize()
        {
            if (_isInitialized) return;

            lock (_lockObject)
            {
                if (_isInitialized) return;

                try
                {
                    var languageFiles = new[] { "en", "vi", "ja" };
                    var basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LanguageFiles");
                    
                    if (!Directory.Exists(basePath))
                    {
                        basePath = Path.Combine(Directory.GetCurrentDirectory(), "LanguageFiles");
                    }
                    
                    if (!Directory.Exists(basePath))
                    {
                        basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Shared", "LanguageFiles");
                    }
                    
                    if (!Directory.Exists(basePath))
                    {
                        basePath = Path.Combine(Directory.GetCurrentDirectory(), "Shared", "LanguageFiles");
                    }

                    foreach (var lang in languageFiles)
                    {
                        var filePath = Path.Combine(basePath, $"{lang}.json");
                        if (File.Exists(filePath))
                        {
                            var jsonContent = File.ReadAllText(filePath);
                            var languageDict = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonContent);
                            if (languageDict != null)
                            {
                                _languageData[lang] = languageDict;
                            }
                        }
                    }

                    _isInitialized = true;
                }
                catch (Exception ex)
                {
                    foreach (var lang in new[] { "en", "vi", "ja" })
                    {
                        _languageData[lang] = new Dictionary<string, object>();
                    }
                    _isInitialized = true;
                }
            }
        }

        public string GetText(string key, string language = "en", params object[] args)
        {
            if (!_isInitialized)
                Initialize();

            if (!_languageData.ContainsKey(language))
                language = "en";

            if (!_languageData[language].ContainsKey(key))
            {
                            if (!_languageData["en"].ContainsKey(key))
            {
                return key;
            }
                
                var text = _languageData["en"][key].ToString();
                return args.Length > 0 ? string.Format(text, args) : text;
            }

            var localizedText = _languageData[language][key].ToString();
            if (args.Length > 0)
            {
                try
                {
                    return string.Format(localizedText, args);
                }
                catch
                {
                    return localizedText.Replace("{0}", args[0].ToString());
                }
            }
            return localizedText;
        }

        public string GetTimeAgo(DateTime dateTime, string language = "en")
        {
            var timeSpan = DateTime.UtcNow - dateTime;
            
            if (timeSpan.TotalMinutes < 1)
                return GetText("justNow", language);
            if (timeSpan.TotalMinutes < 60)
            {
                var minutes = Math.Max(1, (int)Math.Ceiling(timeSpan.TotalMinutes));
                return GetText("minutesAgo", language, minutes);
            }
            if (timeSpan.TotalHours < 24)
            {
                var hours = Math.Max(1, (int)Math.Ceiling(timeSpan.TotalHours));
                return GetText("hoursAgo", language, hours);
            }
            if (timeSpan.TotalDays < 7)
            {
                var days = Math.Max(1, (int)Math.Ceiling(timeSpan.TotalDays));
                return GetText("daysAgo", language, days);
            }
            if (timeSpan.TotalDays < 30)
            {
                var weeks = Math.Max(1, (int)Math.Ceiling(timeSpan.TotalDays / 7));
                return GetText("weeksAgo", language, weeks);
            }
            if (timeSpan.TotalDays < 365)
            {
                var months = Math.Max(1, (int)Math.Ceiling(timeSpan.TotalDays / 30));
                return GetText("monthsAgo", language, months);
            }
            
            var years = Math.Max(1, (int)Math.Ceiling(timeSpan.TotalDays / 365));
            return GetText("yearsAgo", language, years);
        }

        public bool IsValidLanguage(string language)
        {
            if (!_isInitialized)
                Initialize();

            return _languageData.ContainsKey(language);
        }

        public List<string> GetSupportedLanguages()
        {
            if (!_isInitialized)
                Initialize();

            return _languageData.Keys.ToList();
        }
    }
} 