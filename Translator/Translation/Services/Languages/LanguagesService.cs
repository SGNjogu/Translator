using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Translation.AppSettings;
using Translation.DataService.Interfaces;
using Translation.Models;
using Translation.Utils;
using Xamarin.Forms;
using Language = Translation.Models.Language;

namespace Translation.Services.Languages
{
    public class LanguagesService : ILanguagesService
    {
        private List<Language> _approvedLanguages = new List<Language>();
        private List<Language> _autoDetectApprovedLanguages = new List<Language>();
        private Dictionary<string, string> DefaultLanguages = new Dictionary<string, string>();

        private readonly object _lockObject = new object();

        private readonly IDataService _dataService;
        public LanguagesService(IDataService dataService)
        {
            _dataService = dataService;
            MessagingCenter.Subscribe<string>(this, "UpdateLanguageLists", (sender) =>
            {
                _ = UpdateLanguageLists();
            });
            _ = GetSupportedLanguages();
        }

        private async Task UpdateLanguageLists()
        {
            _approvedLanguages = new List<Language>();
            _autoDetectApprovedLanguages = new List<Language>();

            await GetSupportedLanguages().ConfigureAwait(true);
            GetAutoDetectSupportedLanguages();
        }

        public async Task<List<Language>> GetSupportedLanguages()
        {
            try
            {
                if (_approvedLanguages != null && _approvedLanguages.Any()) return _approvedLanguages;

                var _supportedLanguages = new List<Language>
                {
                // The first five are quick view languages
                    new Language() { DisplayName = "Français", DisplayCode = "FR", CountryCode = "FR", CountryName = "France", CountryNativeName = "France",  Name = "Français (France)", EnglishName = "French", Code = "fr-FR", Voice = "fr-FR-Julie-Apollo", Flag = ImageUtility.ReturnImageSourceFromFile("france.png") },
                    new Language() { DisplayName = "Español", DisplayCode = "SPA",CountryCode = "ES", CountryName = "Spain", CountryNativeName = "España", Name = "Español (Spain)", EnglishName = "Spanish", Code = "es-ES", Voice = "es-ES-Laura-Apollo", Flag = ImageUtility.ReturnImageSourceFromFile("spain.png") },
                    new Language() { DisplayName = "Italiano", DisplayCode = "ITA",CountryCode = "IT", CountryName = "Italy", CountryNativeName = "Italia", Name = "Italiano (Italy)", EnglishName = "Italian", Code = "it-IT", Voice = "it-IT-Cosimo-Apollo", Flag = ImageUtility.ReturnImageSourceFromFile("italy.png") },
                    new Language() { DisplayName = "Nederlands", DisplayCode = "NET",CountryCode = "NL", CountryName = "Netherlands", CountryNativeName = "Nederland", Name = "Nederlands (Netherlands)", EnglishName = "Dutch", Code = "nl-NL", Voice = "nl-NL-HannaRUS", Flag = ImageUtility.ReturnImageSourceFromFile("netherlands.png") },
                    new Language() { DisplayName = "Svenska", DisplayCode = "SWE",CountryCode = "SE", CountryName = "Sweden", CountryNativeName = "Sverige", Name = "Svenska (Sweden)", EnglishName = "Swedish", Code = "sv-SE", Voice = "sv-SE-HedvigRUS", Flag = ImageUtility.ReturnImageSourceFromFile("sweden.png") },
                    new Language() { DisplayName = "العربية", DisplayCode = "SA",CountryCode = "SA", CountryName = "Saudi Arabia", CountryNativeName = "المملكة العربية السعودية", Name = "العربية (Saudi Arabia)", EnglishName = "Arabic", Code = "ar-SA", Voice = "ar-SA-Naayf", Flag = ImageUtility.ReturnImageSourceFromFile("saudi_arabia.png") },
                    new Language() { DisplayName = "العربية", DisplayCode = "AE",CountryCode = "AE", CountryName = "United Arab Emirates", CountryNativeName = "اَلْإِمَارَات الْعَرَبِيَة الْمُتَحِدَة", Name = "الِْإمَارَات (United Arab Emirates)", EnglishName = "Arabic", Code = "ar-AE", Voice = "ar-AE-FatimaNeural", Flag = ImageUtility.ReturnImageSourceFromFile("united_arab_emirates.png") },
                    new Language() { DisplayName = "العربية", DisplayCode = "DZ",CountryCode = "DZ", CountryName = "Algeria", CountryNativeName = "الجزائر", Name = "الجزائر (Algeria)", EnglishName = "Arabic", Code = "ar-DZ", Voice = "ar-DZ-AminaNeural", Flag = ImageUtility.ReturnImageSourceFromFile("Algeria.png") },
                    new Language() { DisplayName = "العربية", DisplayCode = "BH",CountryCode = "BH", CountryName = "Bahrain", CountryNativeName = "البحرين", Name = "البحرين (Bahrain)", EnglishName = "Arabic", Code = "ar-BH", Voice = "ar-BH-AliNeural", Flag = ImageUtility.ReturnImageSourceFromFile("bahrain.png") },
                    new Language() { DisplayName = "العربية", DisplayCode = "EG",CountryCode = "EG", CountryName = "Egypt", CountryNativeName = "مصر", Name = "مصر (Egypt)", EnglishName = "Arabic", Code = "ar-EG", Voice = "ar-EG-SalmaNeural", Flag = ImageUtility.ReturnImageSourceFromFile("egypt.png") },
                    new Language() { DisplayName = "العربية", DisplayCode = "IQ",CountryCode = "IQ", CountryName = "Iraq", CountryNativeName = "العراق", Name = "العراق (Iraq)", EnglishName = "Arabic", Code = "ar-IQ", Voice = "ar-IQ-BasselNeural", Flag = ImageUtility.ReturnImageSourceFromFile("iraq.png") },
                    new Language() { DisplayName = "العربية", DisplayCode = "JO",CountryCode = "JO", CountryName = "Jordan", CountryNativeName = "الأردن", Name = "الأردن (Jordan)", EnglishName = "Arabic", Code = "ar-JO", Voice = "ar-JO-SanaNeural", Flag = ImageUtility.ReturnImageSourceFromFile("jordan.png") },
                    new Language() { DisplayName = "العربية", DisplayCode = "KW",CountryCode = "KW", CountryName = "Kuwait", CountryNativeName = "الكويت", Name = "الكويت (Kuwait)", EnglishName = "Arabic", Code = "ar-KW", Voice = "ar-KW-FahedNeural", Flag = ImageUtility.ReturnImageSourceFromFile("kuwait.png") },
                    new Language() { DisplayName = "العربية", DisplayCode = "LB",CountryCode = "LB", CountryName = "Lebanon", CountryNativeName = "لبنان", Name = "لبنان (Lebanon)", EnglishName = "Arabic", Code = "ar-LB", Voice = "ar-LB-LaylaNeural", Flag = ImageUtility.ReturnImageSourceFromFile("lebanon.png") },
                    new Language() { DisplayName = "العربية", DisplayCode = "LY",CountryCode = "LY", CountryName = "Libya", CountryNativeName = "ليبيا", Name = "ليبيا (Libya)", EnglishName = "Arabic", Code = "ar-LY", Voice = "ar-LY-ImanNeural", Flag = ImageUtility.ReturnImageSourceFromFile("libya.png") },
                    new Language() { DisplayName = "العربية", DisplayCode = "MA",CountryCode = "MA", CountryName = "Morocco", CountryNativeName = "المغرب", Name = "المغرب (Morocco)", EnglishName = "Arabic", Code = "ar-MA", Voice = "ar-MA-JamalNeural", Flag = ImageUtility.ReturnImageSourceFromFile("morocco.png") },
                    new Language() { DisplayName = "العربية", DisplayCode = "OM",CountryCode = "OM", CountryName = "Oman", CountryNativeName = "سلطنة عمان", Name = "سلطنة عمان (Oman)", EnglishName = "Arabic", Code = "ar-OM", Voice = "ar-OM-AbdullahNeural", Flag = ImageUtility.ReturnImageSourceFromFile("oman.png") },
                    new Language() { DisplayName = "العربية", DisplayCode = "QA",CountryCode = "QA", CountryName = "Qatar", CountryNativeName = "دولة قطر", Name = "دولة قطر (Qatar)", EnglishName = "Arabic", Code = "ar-QA", Voice = "ar-QA-AmalNeural", Flag = ImageUtility.ReturnImageSourceFromFile("qatar.png") },
                    new Language() { DisplayName = "العربية", DisplayCode = "SY",CountryCode = "SY", CountryName = "Syria", CountryNativeName = "سوريا", Name = "سوريا (Syria)", EnglishName = "Arabic", Code = "ar-SY", Voice = "ar-SY-AmanyNeural", Flag = ImageUtility.ReturnImageSourceFromFile("syria.png") },
                    new Language() { DisplayName = "العربية", DisplayCode = "TN",CountryCode = "TN", CountryName = "Tunisia", CountryNativeName = "تونس", Name = "تونس (Tunisia)", EnglishName = "Arabic", Code = "ar-TN", Voice = "ar-TN-HediNeural", Flag = ImageUtility.ReturnImageSourceFromFile("tunisia.png") },
                    new Language() { DisplayName = "العربية", DisplayCode = "YE",CountryCode = "YE", CountryName = "Yemen", CountryNativeName = "اليمن", Name = "اليمن (Yemen)", EnglishName = "Arabic", Code = "ar-YE", Voice = "ar-YE-MaryamNeural", Flag = ImageUtility.ReturnImageSourceFromFile("yemen.png") },
                    new Language() { DisplayName = "Deutsch", DisplayCode = "GER",CountryCode = "DE", CountryName = "Germany", CountryNativeName = "Deutschland", Name = "Deutsch (Germany)", EnglishName = "German", Code = "de-DE", Voice = "de-DE-Hedda", Flag = ImageUtility.ReturnImageSourceFromFile("germany.png") },
                    new Language() { DisplayName = "Deutsch", DisplayCode = "AT",CountryCode = "AT", CountryName = "Austria", CountryNativeName = "Österreich", Name = "Österreich (Austria)", EnglishName = "German", Code = "de-AT", Voice = "de-AT-IngridNeural", Flag = ImageUtility.ReturnImageSourceFromFile("austria.png") },
                    new Language() { DisplayName = "Deutsch", DisplayCode = "CH",CountryCode = "CH", CountryName = "Switzerland", CountryNativeName = "Schweiz", Name = "Schweiz (Switzerland)", EnglishName = "German", Code = "de-CH", Voice = "de-CH-JanNeural", Flag = ImageUtility.ReturnImageSourceFromFile("switzerland.png") },
                    new Language() { DisplayName = "English", DisplayCode = "CAN",CountryCode = "CA", CountryName = "Canada", CountryNativeName = "Canada", Name = "English (Canada)", EnglishName = "English", Code = "en-CA", Voice = "en-CA-Linda", Flag = ImageUtility.ReturnImageSourceFromFile("canada.png") },
                    new Language() { DisplayName = "English", DisplayCode = "UK",CountryCode = "GB", CountryName = "United Kingdom", CountryNativeName = "United Kingdom", Name = "English (United Kingdom)", EnglishName = "English", Code = "en-GB", Voice = "en-GB-Susan-Apollo", Flag = ImageUtility.ReturnImageSourceFromFile("united_kingdom.png") },
                    new Language() { DisplayName = "English", DisplayCode = "IND",CountryCode = "IN", CountryName = "India", CountryNativeName = "India", Name = "English (India)", EnglishName = "English", Code = "en-IN", Voice = "en-IN-Heera-Apollo", Flag = ImageUtility.ReturnImageSourceFromFile("india.png") },
                    new Language() { DisplayName = "English", DisplayCode = "US",CountryCode = "US", CountryName = "United States", CountryNativeName = "United States", Name = "English (United States)", EnglishName = "English", Code = "en-US", Voice = "en-US-ZiraRUS", Flag = ImageUtility.ReturnImageSourceFromFile("united_states_of_america.png")},
                    new Language() { DisplayName = "English", DisplayCode = "HK",CountryCode = "HK", CountryName = "Hong Kong", CountryNativeName = "Hong Kong", Name = "English (Hong Kong)", EnglishName = "English", Code = "en-HK", Voice = "en-HK-SamNeural", Flag = ImageUtility.ReturnImageSourceFromFile("hong_kong.png")},
                    new Language() { DisplayName = "English", DisplayCode = "IE",CountryCode = "IE", CountryName = "Ireland", CountryNativeName = "Ireland", Name = "English (Ireland)", EnglishName = "English", Code = "en-IE", Voice = "en-IE-ConnorNeural", Flag = ImageUtility.ReturnImageSourceFromFile("ireland.png")},
                    new Language() { DisplayName = "English", DisplayCode = "KE",CountryCode = "KE", CountryName = "Kenya", CountryNativeName = "Kenya", Name = "English (Kenya)", EnglishName = "English", Code = "en-KE", Voice = "en-KE-AsiliaNeural", Flag = ImageUtility.ReturnImageSourceFromFile("kenya.png")},
                    new Language() { DisplayName = "English", DisplayCode = "NG",CountryCode = "NG", CountryName = "Nigeria", CountryNativeName = "Nigeria", Name = "English (Nigeria)", EnglishName = "English", Code = "en-NG", Voice = "en-NG-AbeoNeural", Flag = ImageUtility.ReturnImageSourceFromFile("nigeria.png")},
                    new Language() { DisplayName = "English", DisplayCode = "NG",CountryCode = "NG", CountryName = "New Zealand", CountryNativeName = "New Zealand", Name = "English (New Zealand)", EnglishName = "English", Code = "en-NZ", Voice = "en-NZ-MitchellNeural", Flag = ImageUtility.ReturnImageSourceFromFile("new_zealand.png")},
                    new Language() { DisplayName = "English", DisplayCode = "PH",CountryCode = "PH", CountryName = "Philippines", CountryNativeName = "Philippines", Name = "English (Philippines)", EnglishName = "English", Code = "en-PH", Voice = "en-PH-JamesNeural", Flag = ImageUtility.ReturnImageSourceFromFile("philippines.png")},
                    new Language() { DisplayName = "English", DisplayCode = "SG",CountryCode = "SG", CountryName = "Singapore", CountryNativeName = "Singapore", Name = "English (Singapore)", EnglishName = "English", Code = "en-SG", Voice = "en-SG-LunaNeural", Flag = ImageUtility.ReturnImageSourceFromFile("singapore.png")},
                    new Language() { DisplayName = "English", DisplayCode = "TZ",CountryCode = "TZ", CountryName = "Tanzania", CountryNativeName = "Tanzania", Name = "English (Tanzania)", EnglishName = "English", Code = "en-TZ", Voice = "en-TZ-ElimuNeural", Flag = ImageUtility.ReturnImageSourceFromFile("tanzania.png")},
                    new Language() { DisplayName = "English", DisplayCode = "ZA",CountryCode = "ZA", CountryName = "South Africa", CountryNativeName = "South Africa", Name = "English (South Africa)", EnglishName = "English", Code = "en-ZA", Voice = "en-ZA-LeahNeural", Flag = ImageUtility.ReturnImageSourceFromFile("south_africa.png")},
                    new Language() { DisplayName = "Español", DisplayCode = "MX",CountryCode = "MX", CountryName = "Mexico", CountryNativeName = "México", Name = "Español (Mexico)", EnglishName = "Spanish", Code = "es-MX", Voice = "es-MX-HildaRUS", Flag = ImageUtility.ReturnImageSourceFromFile("mexico.png") },
                    new Language() { DisplayName = "Español", DisplayCode = "AR",CountryCode = "AR", CountryName = "Argentina", CountryNativeName = "Argentina", Name = "Español (Argentina)", EnglishName = "Spanish", Code = "es-AR", Voice = "es-AR-ElenaNeural", Flag = ImageUtility.ReturnImageSourceFromFile("argentina.png") },
                    new Language() { DisplayName = "Español", DisplayCode = "BO",CountryCode = "BO", CountryName = "Bolivia", CountryNativeName = "Bolivia", Name = "Español (Bolivia)", EnglishName = "Spanish", Code = "es-BO", Voice = "es-BO-MarceloNeural", Flag = ImageUtility.ReturnImageSourceFromFile("bolivia.png") },
                    new Language() { DisplayName = "Español", DisplayCode = "CL",CountryCode = "CL", CountryName = "Chile", CountryNativeName = "Chile", Name = "Español (Chile)", EnglishName = "Spanish", Code = "es-CL", Voice = "es-CL-CatalinaNeural", Flag = ImageUtility.ReturnImageSourceFromFile("chile.png") },
                    new Language() { DisplayName = "Español", DisplayCode = "CO",CountryCode = "CO", CountryName = "Colombia", CountryNativeName = "Colombia", Name = "Español (Colombia)", EnglishName = "Spanish", Code = "es-CO", Voice = "es-CO-GonzaloNeural", Flag = ImageUtility.ReturnImageSourceFromFile("colombia.png") },
                    new Language() { DisplayName = "Español", DisplayCode = "CR",CountryCode = "CR", CountryName = "Costa Rica", CountryNativeName = "Costa Rica", Name = "Español (Costa Rica)", EnglishName = "Spanish", Code = "es-CR", Voice = "es-CR-JuanNeural", Flag = ImageUtility.ReturnImageSourceFromFile("costa_rica.png") },
                    new Language() { DisplayName = "Español", DisplayCode = "CU",CountryCode = "CU", CountryName = "Cuba", CountryNativeName = "Cuba", Name = "Español (Cuba)", EnglishName = "Spanish", Code = "es-CU", Voice = "es-CU-BelkysNeural", Flag = ImageUtility.ReturnImageSourceFromFile("cuba.png") },
                    new Language() { DisplayName = "Español", DisplayCode = "DO",CountryCode = "DO", CountryName = "Dominican Republic", CountryNativeName = "República Dominicana", Name = "Español (Dominican Republic)", EnglishName = "Spanish", Code = "es-DO", Voice = "es-DO-EmilioNeural", Flag = ImageUtility.ReturnImageSourceFromFile("dominican_republic.png") },
                    new Language() { DisplayName = "Español", DisplayCode = "EC",CountryCode = "EC", CountryName = "Ecuador", CountryNativeName = "Ecuador", Name = "Español (Ecuador)", EnglishName = "Spanish", Code = "es-EC", Voice = "es-EC-AndreaNeural", Flag = ImageUtility.ReturnImageSourceFromFile("ecuador.png") },
                    new Language() { DisplayName = "Español", DisplayCode = "GQ",CountryCode = "GQ", CountryName = "Equatorial Guinea", CountryNativeName = "Guinea Ecuatorial", Name = "Español (Equatorial Guinea)", EnglishName = "Spanish", Code = "es-GQ", Voice = "es-GQ-JavierNeural", Flag = ImageUtility.ReturnImageSourceFromFile("equatorial_guinea.png") },
                    new Language() { DisplayName = "Español", DisplayCode = "GT",CountryCode = "GT", CountryName = "Guatemala", CountryNativeName = "Guatemala", Name = "Español (Guatemala)", EnglishName = "Spanish", Code = "es-GT", Voice = "es-GT-AndresNeural", Flag = ImageUtility.ReturnImageSourceFromFile("guatemala.png") },
                    new Language() { DisplayName = "Español", DisplayCode = "HN",CountryCode = "HN", CountryName = "Honduras", CountryNativeName = "Honduras", Name = "Español (Honduras)", EnglishName = "Spanish", Code = "es-HN", Voice = "es-HN-CarlosNeural", Flag = ImageUtility.ReturnImageSourceFromFile("honduras.png") },
                    new Language() { DisplayName = "Español", DisplayCode = "NI",CountryCode = "NI", CountryName = "Nicaragua", CountryNativeName = "Nicaragua", Name = "Español (Nicaragua)", EnglishName = "Spanish", Code = "es-NI", Voice = "es-NI-FedericoNeural", Flag = ImageUtility.ReturnImageSourceFromFile("nicaragua.png") },
                    new Language() { DisplayName = "Español", DisplayCode = "PA",CountryCode = "PA", CountryName = "Panama", CountryNativeName = "Panamá", Name = "Español (Panama)", EnglishName = "Spanish", Code = "es-PA", Voice = "es-PA-MargaritaNeural", Flag = ImageUtility.ReturnImageSourceFromFile("panama.png") },
                    new Language() { DisplayName = "Español", DisplayCode = "PE",CountryCode = "PE", CountryName = "Peru", CountryNativeName = "Perú", Name = "Español (Peru)", EnglishName = "Spanish", Code = "es-PE", Voice = "es-PE-AlexNeural", Flag = ImageUtility.ReturnImageSourceFromFile("peru.png") },
                    new Language() { DisplayName = "Español", DisplayCode = "PR",CountryCode = "PR", CountryName = "Puerto Rico", CountryNativeName = "Puerto Rico", Name = "Español (Puerto Rico)", EnglishName = "Spanish", Code = "es-PR", Voice = "es-PR-KarinaNeural", Flag = ImageUtility.ReturnImageSourceFromFile("puerto_rico.png") },
                    new Language() { DisplayName = "Español", DisplayCode = "PY",CountryCode = "PY", CountryName = "Paraguay", CountryNativeName = "Paraguay", Name = "Español (Paraguay)", EnglishName = "Spanish", Code = "es-PY", Voice = "es-PY-MarioNeural", Flag = ImageUtility.ReturnImageSourceFromFile("paraguay.png") },
                    new Language() { DisplayName = "Español", DisplayCode = "SV",CountryCode = "SV", CountryName = "El Salvador", CountryNativeName = "El Salvador", Name = "Español (El Salvador)", EnglishName = "Spanish", Code = "es-SV", Voice = "es-SV-LorenaNeural", Flag = ImageUtility.ReturnImageSourceFromFile("el_salvador.png") },
                    new Language() { DisplayName = "Español", DisplayCode = "US",CountryCode = "US", CountryName = "United States", CountryNativeName = "Estados Unidos", Name = "Español (United States)", EnglishName = "Spanish", Code = "es-US", Voice = "es-US-AlonsoNeural", Flag = ImageUtility.ReturnImageSourceFromFile("united_states_of_america.png") },
                    new Language() { DisplayName = "Español", DisplayCode = "UY",CountryCode = "UY", CountryName = "Uruguay", CountryNativeName = "Uruguay", Name = "Español (Uruguay)", EnglishName = "Spanish", Code = "es-UY", Voice = "es-UY-MateoNeural", Flag = ImageUtility.ReturnImageSourceFromFile("uruguay.png") },
                    new Language() { DisplayName = "Español", DisplayCode = "VE",CountryCode = "VE", CountryName = "Venezuela", CountryNativeName = "Venezuela", Name = "Español (Venezuela)", EnglishName = "Spanish", Code = "es-VE", Voice = "es-VE-PaolaNeural", Flag = ImageUtility.ReturnImageSourceFromFile("venezuela.png") },
                    new Language() { DisplayName = "Français", DisplayCode = "CAN",CountryCode = "CA", CountryName = "Canada", CountryNativeName = "Canada", Name = "Français (Canada)", EnglishName = "French", Code = "fr-CA", Voice = "fr-CA-Caroline", Flag = ImageUtility.ReturnImageSourceFromFile("canada.png") },
                    new Language() { DisplayName = "Français", DisplayCode = "BE",CountryCode = "BE", CountryName = "Belgium", CountryNativeName = "Belgique", Name = "Español (Belgium)", EnglishName = "French", Code = "fr-BE", Voice = "fr-BE-CharlineNeural", Flag = ImageUtility.ReturnImageSourceFromFile("belgium.png") },
                    new Language() { DisplayName = "Français", DisplayCode = "CH",CountryCode = "CH", CountryName = "Switzerland", CountryNativeName = "Suisse", Name = "Español (Switzerland)", EnglishName = "French", Code = "fr-CH", Voice = "fr-CH-ArianeNeural", Flag = ImageUtility.ReturnImageSourceFromFile("switzerland.png") },
                    new Language() { DisplayName = "Nederlands", DisplayCode = "BE",CountryCode = "BE", CountryName = "Belgium", CountryNativeName = "België", Name = "Nederlands (Belgium)", EnglishName = "Dutch", Code = "nl-BE", Voice = "nl-BE-ArnaudNeural", Flag = ImageUtility.ReturnImageSourceFromFile("belgium.png") },
                    new Language() { DisplayName = "Kiswahili", DisplayCode = "KE",CountryCode = "KE", CountryName = "Kenya", CountryNativeName = "Kenya", Name = "Kiswahili (Kenya)", EnglishName = "Swahili", Code = "sw-KE", Voice = "sw-KE-ZuriNeural", Flag = ImageUtility.ReturnImageSourceFromFile("kenya.png") },
                    new Language() { DisplayName = "Kiswahili", DisplayCode = "TZ",CountryCode = "TZ", CountryName = "Tanzania", CountryNativeName = "Tanzania", Name = "Kiswahili (Tanzania)", EnglishName = "Swahili", Code = "sw-TZ", Voice = "sw-TZ-DaudiNeural", Flag = ImageUtility.ReturnImageSourceFromFile("tanzania.png") },
                    new Language() { DisplayName = "தமிழ்", DisplayCode = "IND",CountryCode = "IN", CountryName = "India", CountryNativeName = "இந்தியா", Name = "தமிழ் (India)", EnglishName = "Tamil", Code = "ta-IN", Voice = "ta-IN-Valluvar", Flag = ImageUtility.ReturnImageSourceFromFile("india.png") },
                    new Language() { DisplayName = "தமிழ்", DisplayCode = "LK",CountryCode = "LK", CountryName = "Sri Lanka", CountryNativeName = "இலங்கை", Name = "தமிழ் (Sri Lanka)", EnglishName = "Tamil", Code = "ta-LK", Voice = "ta-LK-KumarNeural", Flag = ImageUtility.ReturnImageSourceFromFile("sri_lanka.png") },
                    new Language() { DisplayName = "தமிழ்", DisplayCode = "MY",CountryCode = "MY", CountryName = "Malaysia", CountryNativeName = "மலேசியா", Name = "தமிழ் (Malaysia)", EnglishName = "Tamil", Code = "ta-MY", Voice = "ta-MY-KaniNeural", Flag = ImageUtility.ReturnImageSourceFromFile("malaysia.png") },
                    new Language() { DisplayName = "தமிழ்", DisplayCode = "SG",CountryCode = "SG", CountryName = "Singapore", CountryNativeName = "சிங்கப்பூர்", Name = "தமிழ் (Singapore)", EnglishName = "Tamil", Code = "ta-SG", Voice = "ta-SG-AnbuNeural", Flag = ImageUtility.ReturnImageSourceFromFile("singapore.png") },
                    new Language() { DisplayName = "اُردُو", DisplayCode = "UR",CountryCode = "PK", CountryName = "Pakistan", CountryNativeName = "پاکستان", Name = "اُردُو (Pakistan)", EnglishName = "Urdu", Code = "ur-PK", Voice = "ur-PK-AsadNeural", Flag = ImageUtility.ReturnImageSourceFromFile("pakistan.png") },
                    new Language() { DisplayName = "اُردُو", DisplayCode = "UR",CountryCode = "IN", CountryName = "India", CountryNativeName = "انڈیا", Name = "اُردُو (India)", EnglishName = "Urdu", Code = "ur-IN", Voice = "ur-IN-GulNeural", Flag = ImageUtility.ReturnImageSourceFromFile("india.png") },
                    new Language() { DisplayName = "Català", DisplayCode = "ES",CountryCode = "ES", CountryName = "Spain", CountryNativeName = "España", Name = "Català (Spain)", EnglishName = "Catalan", Code = "ca-ES", Voice = "ca-ES-HerenaRUS", Flag = ImageUtility.ReturnImageSourceFromFile("spain.png") },
                    new Language() { DisplayName = "Dansk", DisplayCode = "DEN",CountryCode = "DK", CountryName = "Denmark", CountryNativeName = "Danmark", Name = "Dansk (Denmark)", EnglishName = "Danish", Code = "da-DK", Voice = "da-DK-HelleRUS", Flag = ImageUtility.ReturnImageSourceFromFile("denmark.png") },
                    new Language() { DisplayName = "English", DisplayCode = "AUS",CountryCode = "AU", CountryName = "Australia", CountryNativeName = "Australia", Name = "English (Australia)", EnglishName = "English", Code = "en-AU", Voice = "en-AU-Catherine", Flag = ImageUtility.ReturnImageSourceFromFile("australia.png") },
                    new Language() { DisplayName = "Suomi", DisplayCode = "FIN",CountryCode = "FI", CountryName = "Finland", CountryNativeName = "Suomi", Name = "Suomi (Finland)", EnglishName = "Finnish", Code = "fi-FI", Voice = "fi-FI-HeidiRUS", Flag = ImageUtility.ReturnImageSourceFromFile("finland.png") },
                    new Language() { DisplayName = "हिन्दी", DisplayCode = "IND",CountryCode = "IN", CountryName = "India", CountryNativeName = "India", Name = "हिन्दी (India)", EnglishName = "Hindi", Code = "hi-IN", Voice = "hi-IN-Kalpana-Apollo", Flag = ImageUtility.ReturnImageSourceFromFile("india.png") },
                    new Language() { DisplayName = "日本語", DisplayCode = "JAP",CountryCode = "JP", CountryName = "Japan", CountryNativeName = "日本", Name = "日本語 (Japan)", EnglishName = "Japanese", Code = "ja-JP", Voice = "ja-JP-Ayumi-Apollo", Flag = ImageUtility.ReturnImageSourceFromFile("japan.png") },
                    new Language() { DisplayName = "조선말", DisplayCode = "KR",CountryCode = "KR", CountryName = "South Korea", CountryNativeName = "한국", Name = "조선말 (Korea)", EnglishName = "Korean", Code = "ko-KR", Voice = "ko-KR-HeamiRUS", Flag = ImageUtility.ReturnImageSourceFromFile("south_korea.png") },
                    new Language() { DisplayName = "Norsk", DisplayCode = "NOR",CountryCode = "NO", CountryName = "Norway", CountryNativeName = "Norge", Name = "Norsk (Bokmål) (Norway)", EnglishName = "Norwegian", Code = "nb-NO", Voice = "nb-NO-HuldaRUS", Flag = ImageUtility.ReturnImageSourceFromFile("norway.png") },
                    new Language() { DisplayName = "Język polski", DisplayCode = "POL",CountryCode = "PL", CountryName = "Poland", CountryNativeName = "Polska", Name = "Język polski (Poland)", EnglishName = "Polish", Code = "pl-PL", Voice = "pl-PL-PaulinaRUS", Flag = ImageUtility.ReturnImageSourceFromFile("poland.png") },
                    new Language() { DisplayName = "Português", DisplayCode = "BR",CountryCode = "BR", CountryName = "Brazil", CountryNativeName = "Brasil", Name = "Português (Brazil)", EnglishName = "Portuguese", Code = "pt-BR", Voice = "pt-BR-HeloisaRUS", Flag = ImageUtility.ReturnImageSourceFromFile("brazil.png") },
                    new Language() { DisplayName = "Português", DisplayCode = "PT", CountryCode = "PT", CountryName = "Portugal", CountryNativeName = "Portugal", Name = "Português (Portugal)", EnglishName = "Portuguese", Code = "pt-PT", Voice = "pt-PT-HeliaRUS", Flag = ImageUtility.ReturnImageSourceFromFile("portugal.png") },
                    new Language() { DisplayName = "Русский", DisplayCode = "RUS",CountryCode = "RU", CountryName = "Russia", CountryNativeName = "Россия", Name = "Русский (Russia)", EnglishName = "Russian", Code = "ru-RU", Voice = "ru-RU-Irina-Apollo", Flag = ImageUtility.ReturnImageSourceFromFile("russia.png") },
                    new Language() { DisplayName = "తెలుగు", DisplayCode = "IND",CountryCode = "IN", CountryName = "India", CountryNativeName = "India", Name = "తెలుగు (India)", EnglishName = "Telugu", Code = "te-IN", Voice = "te-IN-Chitra", Flag = ImageUtility.ReturnImageSourceFromFile("india.png") },
                    new Language() { DisplayName = "國語", DisplayCode = "CN",CountryCode = "CN", CountryName = "China", CountryNativeName = "中国", Name = "國語 (Mandarin, Simplified)", EnglishName = "Chinese", Code = "zh-CN", Voice = "zh-CN-HuihuiRUS", Flag = ImageUtility.ReturnImageSourceFromFile("china.png") },
                    new Language() { DisplayName = "廣東話", DisplayCode = "HK",CountryCode = "CN", CountryName = "China", CountryNativeName = "中国", Name = "廣東話 (Cantonese, Traditional)", EnglishName = "Chinese", Code = "zh-HK", Voice = "zh-HK-Tracy-Apollo", Flag = ImageUtility.ReturnImageSourceFromFile("hong_kong.png") },
                    new Language() { DisplayName = "福佬話", DisplayCode = "TW",CountryCode = "CN", CountryName = "China", CountryNativeName = "中国", Name = "福佬話 (Taiwanese Mandarin)", EnglishName = "Chinese", Code = "zh-TW", Voice = "zh-TW-Yating-Apollo", Flag = ImageUtility.ReturnImageSourceFromFile("taiwan.png") },
                    new Language() { DisplayName = "ภาษาไทย", DisplayCode = "TH",CountryCode = "TH", CountryName = "Thailand", CountryNativeName = "ประเทศไทย", Name = "ภาษาไทย (Thailand)", EnglishName = "Thai", Code = "th-TH", Voice = "th-TH-Pattara", Flag = ImageUtility.ReturnImageSourceFromFile("thailand.png") },
                    new Language() { DisplayName = "Türkçe", DisplayCode = "TUR",CountryCode = "TR", CountryName = "Turkey", CountryNativeName = "Turkiye", Name = "Türkçe (Turkey)", EnglishName = "Turkey", Code = "tr-TR", Voice = "tr-TR-SedaRUS", Flag = ImageUtility.ReturnImageSourceFromFile("turkey.png") },
                    new Language() { DisplayName = "български език", DisplayCode = "BG",CountryCode = "BG", CountryName = "Bulgaria", CountryNativeName = "Bŭlgariya", Name = "ългарски език (Bulgaria)", EnglishName = "Bulgarian", Code = "bg-BG", Voice = "bg-BG-Ivan", Flag = ImageUtility.ReturnImageSourceFromFile("bulgaria.png") },
                    new Language() { DisplayName = "Hrvatski", DisplayCode = "CRO",CountryCode = "HR", CountryName = "Croatia", CountryNativeName = "Hrvatska", Name = "Hrvatski (Croatia)", EnglishName = "Croatian", Code = "hr-HR", Voice = "hr-HR-Matej", Flag = ImageUtility.ReturnImageSourceFromFile("croatia.png") },
                    new Language() { DisplayName = "Český Jazyk", DisplayCode = "CZ",CountryCode = "CZ", CountryName = "Czech", CountryNativeName = "Česko", Name = "Český Jazyk (Czech)", EnglishName = "Czech", Code = "cs-CZ", Voice = "cs-CZ-Jakub", Flag = ImageUtility.ReturnImageSourceFromFile("czechRepublic.png") },
                    new Language() { DisplayName = "Ελληνικά", DisplayCode = "GR",CountryCode = "GR", CountryName = "Greece", CountryNativeName = "Ελλάς", Name = "Ελληνικά (Greece)", EnglishName = "Greek", Code = "el-GR", Voice = "el-GR-Stefanos", Flag = ImageUtility.ReturnImageSourceFromFile("greece.png") },
                    new Language() { DisplayName = "Magyar", DisplayCode = "HUN",CountryCode = "HU", CountryName = "Hungary", CountryNativeName = "Magyarország", Name = "Magyar (Hungary)", EnglishName = "Hungarian", Code = "hu-HU", Voice = "hu-HU-Szabolcs", Flag = ImageUtility.ReturnImageSourceFromFile("hungary.png") },
                    new Language() { DisplayName = "Română", DisplayCode = "RO",CountryCode = "RO", CountryName = "Romania", CountryNativeName = "România", Name = "Română (Romania)", EnglishName = "Romanian", Code = "ro-RO", Voice = "ro-RO-Andrei", Flag = ImageUtility.ReturnImageSourceFromFile("romania.png") },
                    new Language() { DisplayName = "Slovenčina", DisplayCode = "SK", CountryCode = "SK", CountryName = "Slovakia", CountryNativeName = "Slovensko", Name = "Slovenčina (Slovakia)", EnglishName = "Slovak", Code = "sk-SK", Voice = "sk-SK-Filip", Flag = ImageUtility.ReturnImageSourceFromFile("slovakia.png") },
                    new Language() { DisplayName = "Slovenščina", DisplayCode = "SL",CountryCode = "SI", CountryName = "Slovenia", CountryNativeName = "Slovenija", Name = "Slovenščina (Slovenia)", EnglishName = "Slovenian", Code = "sl-SI", Voice = "sl-SI-Lado", Flag = ImageUtility.ReturnImageSourceFromFile("slovenia.png") },
                    new Language() { DisplayName = "עברית", DisplayCode = "IL",CountryCode = "IL", CountryName = "Israel", CountryNativeName = "Yisrael", Name = "עברית (Israel)", EnglishName = "Hebrew", Code = "he-IL", Voice = "he-IL-Asaf", Flag = ImageUtility.ReturnImageSourceFromFile("israel.png") },
                    new Language() { DisplayName = "Bahasa Indonesia", DisplayCode = "ID",CountryCode = "ID", CountryName = "Indonesia", CountryNativeName = "Indonesia", Name = "Bahasa Indonesia (Indonesia)", EnglishName = "Indonesian", Code = "id-ID", Voice = "id-ID-Andika", Flag = ImageUtility.ReturnImageSourceFromFile("indonesia.png") },
                    new Language() { DisplayName = "بهاس ملايو", DisplayCode = "MY",CountryCode = "MY", CountryName = "Malaysia", CountryNativeName = "Məlejsiə", Name = "بهاس ملايو (Malaysia)", EnglishName = "Malay", Code = "ms-MY", Voice = "ms-MY-Rizwan", Flag = ImageUtility.ReturnImageSourceFromFile("malaysia.png") },
                    new Language() { DisplayName = "Tiếng Việt Nam", DisplayCode = "VN",CountryCode = "VN", CountryName = "Vietnam", CountryNativeName = "Việt Nam", Name = "Tiếng Việt Nam (Vietnam)", EnglishName = "Vietnamese", Code = "vi-VN", Voice = "vi-VN-An", Flag = ImageUtility.ReturnImageSourceFromFile("vietnam.png") },
                    new Language() { DisplayName = "Eesti", DisplayCode = "EE",CountryCode = "EE", CountryName = "Estonia", CountryNativeName = "Eesti", Name = "Eesti (Estonia)", EnglishName = "Estonian", Code = "et-EE", Voice = "et-EE-AnuNeural", Flag = ImageUtility.ReturnImageSourceFromFile("estonia.png") },
                    new Language() { DisplayName = "ગુજરાતી", DisplayCode = "IN",CountryCode = "IN", CountryName = "India", CountryNativeName = "India", Name = "ગુજરાતી (India)", EnglishName = "Gujarati", Code = "gu-IN", Voice = "gu-IN-DhwaniNeural", Flag = ImageUtility.ReturnImageSourceFromFile("india.png") },
                    new Language() { DisplayName = "latviešu", DisplayCode = "LV",CountryCode = "LV", CountryName = "Latvia", CountryNativeName = "Latvija", Name = "latviešu (Latvia)", EnglishName = "Latvian", Code = "lv-LV", Voice = "lv-LV-EveritaNeural", Flag = ImageUtility.ReturnImageSourceFromFile("latvia.png") },
                    new Language() { DisplayName = "Lietuvių", DisplayCode = "LT",CountryCode = "LT", CountryName = "Lithuania", CountryNativeName = "Lietuva", Name = "Lietuvių (Lithuania)", EnglishName = "Lithuanian", Code = "lt-LT", Voice = "lt-LT-OnaNeural", Flag = ImageUtility.ReturnImageSourceFromFile("lithuania.png") },
                    new Language() { DisplayName = "Malti", DisplayCode = "MT",CountryCode = "MT", CountryName = "Malta", CountryNativeName = "Malta", Name = "Malti (Malta)", EnglishName = "Maltese", Code = "mt-MT", Voice = "mt-MT-GraceNeural", Flag = ImageUtility.ReturnImageSourceFromFile("malta.png") },
                    new Language() { DisplayName = "मराठी", DisplayCode = "IN",CountryCode = "IN", CountryName = "India", CountryNativeName = "India", Name = "मराठी (India)", EnglishName = "Marathi", Code = "mr-IN", Voice = "mr-IN-AarohiNeural", Flag = ImageUtility.ReturnImageSourceFromFile("india.png") },
                    new Language() { DisplayName = "Gaeilge", DisplayCode = "IE",CountryCode = "IE", CountryName = "Ireland", CountryNativeName = "Éire", Name = "Gaeilge (Ireland)", EnglishName = "Gaelic", Code = "ga-IE", Voice = "ga-IE-OrlaNeural", Flag = ImageUtility.ReturnImageSourceFromFile("ireland.png") },
                    new Language() { DisplayName = "украї́нська мо́ва", DisplayCode = "UA",CountryCode = "UA", CountryName = "Ukraine", CountryNativeName = "Ukraїna", Name = "украї́нська мо́ва (Ukraine)", EnglishName = "Ukrainian", Code = "uk-UA", Voice = "uk-UA-PolinaNeural", Flag = ImageUtility.ReturnImageSourceFromFile("ukraine.png") },
                    new Language() { DisplayName = "Wikang Filipino", DisplayCode = "PH",CountryCode = "PH", CountryName = "Philippines", CountryNativeName = "Wikang Filipino", Name = "Wikang Filipino (Philippines)", EnglishName = "Filipino", Code = "fil-PH", Voice = "fil-PH-BlessicaNeural", Flag = ImageUtility.ReturnImageSourceFromFile("philippines.png") },
                    //new Language() { DisplayName = "বাংলা", DisplayCode = "BD",CountryCode = "BD", CountryName = "Bangladesh", CountryNativeName = "বাংলা", Name = "বাংলা (Bangladesh)", EnglishName = "Bangla", Code = "bn-BD", Voice = "bn-BD-NabanitaNeural", Flag = ImageUtility.ReturnImageSourceFromFile("bangladesh.png") },
                    new Language() { DisplayName = "नेपाली", DisplayCode = "NP",CountryCode = "NP", CountryName = "Nepal", CountryNativeName = "नेपाली", Name = "नेपाली (Nepal)", EnglishName = "Nepali", Code = "ne-NP", Voice = "ne-NP-HemkalaNeural", Flag = ImageUtility.ReturnImageSourceFromFile("nepal.png") },
                    //new Language() { DisplayName = "српски", DisplayCode = "RS",CountryCode = "RS", CountryName = "Serbia", CountryNativeName = "српски", Name = "српски (Serbia)", EnglishName = "Serbian", Code = "sr-RS", Voice = "sr-RS-SophieNeural", Flag = ImageUtility.ReturnImageSourceFromFile("serbia.png") },
                    new Language() { DisplayName = "Cymraeg", DisplayCode = "WLS",CountryCode = "WLS", CountryName = "Wales", CountryNativeName = "Cymraeg", Name = "Cymraeg (Wales)", EnglishName = "Welsh", Code = "cy-GB", Voice = "cy-GB-NiaNeural", Flag = ImageUtility.ReturnImageSourceFromFile("wales.png") },
                    new Language() { DisplayName = "босански", DisplayCode = "BA",CountryCode = "BA", CountryName = "Bosnia", CountryNativeName = "босански", Name = "босански (Bosnia)", EnglishName = "Bosnian", Code = "bs-BA", Voice = "bs-BA-VesnaNeural", Flag = ImageUtility.ReturnImageSourceFromFile("bosnia.png") },
                    new Language() { DisplayName = "Afrikaans", DisplayCode = "AF",CountryCode = "ZA", CountryName = "South Africa", CountryNativeName = "Suid-Afrika", Name = "Afrikaans (South Africa)", EnglishName = "Afrikaans", Code = "af-ZA", Voice = "af-ZA-AdriNeural", Flag = ImageUtility.ReturnImageSourceFromFile("south_africa.png") },
                    new Language() { DisplayName = "Shqip", DisplayCode = "SQ",CountryCode = "AL", CountryName = "Albania", CountryNativeName = "Shqipërisë", Name = "Albanian (Albania)", EnglishName = "Albanian", Code = "sq-AL", Voice = "sq-AL-AnilaNeural", Flag = ImageUtility.ReturnImageSourceFromFile("albania.png") },
                    new Language() { DisplayName = "āmariññā", DisplayCode = "AM",CountryCode = "ET", CountryName = "Ethiopia", CountryNativeName = "ʾĪtyōṗṗyā", Name = "Amharic (Ethiopia)", EnglishName = "Amharic", Code = "am-ET", Voice = "am-ET-AmehaNeural", Flag = ImageUtility.ReturnImageSourceFromFile("ethiopia.png") },
                    new Language() { DisplayName = "hayeren", DisplayCode = "HY",CountryCode = "AM", CountryName = "Armenia", CountryNativeName = "Hayk", Name = "Armenian (Armenia)", EnglishName = "Armenian", Code = "hy-AM", Voice = "hy-AM-AnahitNeural", Flag = ImageUtility.ReturnImageSourceFromFile("armenia.png") },
                    new Language() { DisplayName = "Íslenska", DisplayCode = "IS",CountryCode = "IS", CountryName = "Iceland", CountryNativeName = "Ísland", Name = "Icelandic (Iceland)", EnglishName = "Icelandic", Code = "is-IS", Voice = "is-IS-GudrunNeural", Flag = ImageUtility.ReturnImageSourceFromFile("iceland.png") },
                    new Language() { DisplayName = "ಕನ್ನಡ", DisplayCode = "KN",CountryCode = "IN", CountryName = "India", CountryNativeName = "ಭಾರತ", Name = "Kannada (India)", EnglishName = "Kannada", Code = "kn-IN", Voice = "kn-IN-GaganNeural", Flag = ImageUtility.ReturnImageSourceFromFile("india.png") },
                    new Language() { DisplayName = "Қазақ Tілі", DisplayCode = "KK",CountryCode = "KZ", CountryName = "Kazakhstan", CountryNativeName = "Қазақстан", Name = "Kazakh (Kazakhstan)", EnglishName = "Kazakh", Code = "kk-KZ", Voice = "kk-KZ-AigulNeural", Flag = ImageUtility.ReturnImageSourceFromFile("kazakhstan.png") },
                    new Language() { DisplayName = "ភាសាខ្មែរ", DisplayCode = "KM",CountryCode = "KH", CountryName = "Cambodia", CountryNativeName = "កម្ពុជា។", Name = "Khmer (Cambodia)", EnglishName = "Khmer", Code = "km-KH", Voice = "km-KH-PisethNeural", Flag = ImageUtility.ReturnImageSourceFromFile("cambodia.png") },
                    new Language() { DisplayName = "ພາສາລາວ", DisplayCode = "LO",CountryCode = "LA", CountryName = "Laos", CountryNativeName = "ປະເທດລາວ", Name = "Lao (Laos)", EnglishName = "Lao", Code = "lo-LA", Voice = "lo-LA-ChanthavongNeural", Flag = ImageUtility.ReturnImageSourceFromFile("laos.png") },
                    new Language() { DisplayName = "മലയാളം", DisplayCode = "ML",CountryCode = "IN", CountryName = "India", CountryNativeName = "ഇന്ത്യ", Name = "Malayalam (India)", EnglishName = "Malayalam", Code = "ml-IN", Voice = "ml-IN-MidhunNeural", Flag = ImageUtility.ReturnImageSourceFromFile("india.png") },
                    new Language() { DisplayName = "မြန်မာ", DisplayCode = "MY",CountryCode = "MM", CountryName = "Myanmar", CountryNativeName = "မြန်မာ", Name = "Burmese (Myanmar)", EnglishName = "Burmese", Code = "my-MM", Voice = "my-MM-NilarNeural", Flag = ImageUtility.ReturnImageSourceFromFile("myanmar.png") },
                    new Language() { DisplayName = "پښتو", DisplayCode = "PS",CountryCode = "AF", CountryName = "Afghanistan", CountryNativeName = "افغانستان", Name = "Pashto (Afghanistan)", EnglishName = "Pashto", Code = "ps-AF", Voice = "ps-AF-GulNawazNeural", Flag = ImageUtility.ReturnImageSourceFromFile("afghanistan.png") },
                    new Language() { DisplayName = "فارسی", DisplayCode = "FA",CountryCode = "IR", CountryName = "Iran", CountryNativeName = "ایران", Name = "Persian (Iran)", EnglishName = "Persian", Code = "fa-IR", Voice = "fa-IR-DilaraNeural", Flag = ImageUtility.ReturnImageSourceFromFile("iran.png") },
                };

                var currentLanguages = await _dataService.GetBackendLanguagesAsync().ConfigureAwait(true);

                if (currentLanguages != null && currentLanguages.Any())
                {
                    foreach (var code in currentLanguages)
                    {
                        var foundLanguage = _supportedLanguages.FirstOrDefault(c => c.Code.ToLower() == code.ToLower());
                        if (foundLanguage != null && !_approvedLanguages.Exists(c => c.Code.ToLower() == code.ToLower()))
                        {
                            _approvedLanguages.Add(foundLanguage);
                        }
                    }
                }

                if (_approvedLanguages.Count() == 0)
                {
                    _approvedLanguages = _supportedLanguages;
                    return _supportedLanguages?.OrderBy(s => s.Name).ToList();
                }
                else
                {
                    return _approvedLanguages?.OrderBy(s => s.Name).ToList();
                }
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw ex;
            }
        }

        public List<Language> GetAutoDetectSupportedLanguages()
        {
            try
            {
                lock (_lockObject)
                {
                    var _autodetectLanguages = new List<Language>
                {
                    new Language() { DisplayName = "Français", DisplayCode = "FR", CountryCode = "FR", CountryName = "France", CountryNativeName = "France",  Name = "Français (France)", EnglishName = "French", Code = "fr-FR", Voice = "fr-FR-Julie-Apollo", Flag = ImageUtility.ReturnImageSourceFromFile("france.png") },
                    new Language() { DisplayName = "Español", DisplayCode = "SPA",CountryCode = "ES", CountryName = "Spain", CountryNativeName = "España", Name = "Español (Spain)", EnglishName = "Spanish", Code = "es-ES", Voice = "es-ES-Laura-Apollo", Flag = ImageUtility.ReturnImageSourceFromFile("spain.png") },
                    new Language() { DisplayName = "Italiano", DisplayCode = "ITA",CountryCode = "IT", CountryName = "Italy", CountryNativeName = "Italia", Name = "Italiano (Italy)", EnglishName = "Italian", Code = "it-IT", Voice = "it-IT-Cosimo-Apollo", Flag = ImageUtility.ReturnImageSourceFromFile("italy.png") },
                    new Language() { DisplayName = "Nederlands", DisplayCode = "NET",CountryCode = "NL", CountryName = "Netherlands", CountryNativeName = "Nederland", Name = "Nederlands (Netherlands)", EnglishName = "Dutch", Code = "nl-NL", Voice = "nl-NL-HannaRUS", Flag = ImageUtility.ReturnImageSourceFromFile("netherlands.png") },
                    new Language() { DisplayName = "Svenska", DisplayCode = "SWE",CountryCode = "SE", CountryName = "Sweden", CountryNativeName = "Sverige", Name = "Svenska (Sweden)", EnglishName = "Swedish", Code = "sv-SE", Voice = "sv-SE-HedvigRUS", Flag = ImageUtility.ReturnImageSourceFromFile("sweden.png") },
                    new Language() { DisplayName = "العربية", DisplayCode = "SA",CountryCode = "SA", CountryName = "Saudi Arabia", CountryNativeName = "المملكة العربية السعودية", Name = "العربية (Saudi Arabia)", EnglishName = "Arabic", Code = "ar-SA", Voice = "ar-SA-Naayf", Flag = ImageUtility.ReturnImageSourceFromFile("saudi_arabia.png") },
                    new Language() { DisplayName = "العربية", DisplayCode = "AE",CountryCode = "AE", CountryName = "United Arab Emirates", CountryNativeName = "اَلْإِمَارَات الْعَرَبِيَة الْمُتَحِدَة", Name = "الِْإمَارَات (United Arab Emirates)", EnglishName = "Arabic", Code = "ar-AE", Voice = "ar-AE-FatimaNeural", Flag = ImageUtility.ReturnImageSourceFromFile("united_arab_emirates.png") },
                    new Language() { DisplayName = "العربية", DisplayCode = "DZ",CountryCode = "DZ", CountryName = "Algeria", CountryNativeName = "الجزائر", Name = "الجزائر (Algeria)", EnglishName = "Arabic", Code = "ar-DZ", Voice = "ar-DZ-AminaNeural", Flag = ImageUtility.ReturnImageSourceFromFile("Algeria.png") },
                    new Language() { DisplayName = "العربية", DisplayCode = "BH",CountryCode = "BH", CountryName = "Bahrain", CountryNativeName = "البحرين", Name = "البحرين (Bahrain)", EnglishName = "Arabic", Code = "ar-BH", Voice = "ar-BH-AliNeural", Flag = ImageUtility.ReturnImageSourceFromFile("bahrain.png") },
                    new Language() { DisplayName = "العربية", DisplayCode = "EG",CountryCode = "EG", CountryName = "Egypt", CountryNativeName = "مصر", Name = "مصر (Egypt)", EnglishName = "Arabic", Code = "ar-EG", Voice = "ar-EG-SalmaNeural", Flag = ImageUtility.ReturnImageSourceFromFile("egypt.png") },
                    new Language() { DisplayName = "العربية", DisplayCode = "IQ",CountryCode = "IQ", CountryName = "Iraq", CountryNativeName = "العراق", Name = "العراق (Iraq)", EnglishName = "Arabic", Code = "ar-IQ", Voice = "ar-IQ-BasselNeural", Flag = ImageUtility.ReturnImageSourceFromFile("iraq.png") },
                    new Language() { DisplayName = "العربية", DisplayCode = "JO",CountryCode = "JO", CountryName = "Jordan", CountryNativeName = "الأردن", Name = "الأردن (Jordan)", EnglishName = "Arabic", Code = "ar-JO", Voice = "ar-JO-SanaNeural", Flag = ImageUtility.ReturnImageSourceFromFile("jordan.png") },
                    new Language() { DisplayName = "العربية", DisplayCode = "KW",CountryCode = "KW", CountryName = "Kuwait", CountryNativeName = "الكويت", Name = "الكويت (Kuwait)", EnglishName = "Arabic", Code = "ar-KW", Voice = "ar-KW-FahedNeural", Flag = ImageUtility.ReturnImageSourceFromFile("kuwait.png") },
                    new Language() { DisplayName = "العربية", DisplayCode = "LY",CountryCode = "LY", CountryName = "Libya", CountryNativeName = "ليبيا", Name = "ليبيا (Libya)", EnglishName = "Arabic", Code = "ar-LY", Voice = "ar-LY-ImanNeural", Flag = ImageUtility.ReturnImageSourceFromFile("libya.png") },
                    new Language() { DisplayName = "العربية", DisplayCode = "MA",CountryCode = "MA", CountryName = "Morocco", CountryNativeName = "المغرب", Name = "المغرب (Morocco)", EnglishName = "Arabic", Code = "ar-MA", Voice = "ar-MA-JamalNeural", Flag = ImageUtility.ReturnImageSourceFromFile("morocco.png") },
                    new Language() { DisplayName = "العربية", DisplayCode = "OM",CountryCode = "OM", CountryName = "Oman", CountryNativeName = "سلطنة عمان", Name = "سلطنة عمان (Oman)", EnglishName = "Arabic", Code = "ar-OM", Voice = "ar-OM-AbdullahNeural", Flag = ImageUtility.ReturnImageSourceFromFile("oman.png") },
                    new Language() { DisplayName = "العربية", DisplayCode = "QA",CountryCode = "QA", CountryName = "Qatar", CountryNativeName = "دولة قطر", Name = "دولة قطر (Qatar)", EnglishName = "Arabic", Code = "ar-QA", Voice = "ar-QA-AmalNeural", Flag = ImageUtility.ReturnImageSourceFromFile("qatar.png") },
                    new Language() { DisplayName = "العربية", DisplayCode = "SY",CountryCode = "SY", CountryName = "Syria", CountryNativeName = "سوريا", Name = "سوريا (Syria)", EnglishName = "Arabic", Code = "ar-SY", Voice = "ar-SY-AmanyNeural", Flag = ImageUtility.ReturnImageSourceFromFile("syria.png") },
                    new Language() { DisplayName = "العربية", DisplayCode = "YE",CountryCode = "YE", CountryName = "Yemen", CountryNativeName = "اليمن", Name = "اليمن (Yemen)", EnglishName = "Arabic", Code = "ar-YE", Voice = "ar-YE-MaryamNeural", Flag = ImageUtility.ReturnImageSourceFromFile("yemen.png") },
                    new Language() { DisplayName = "Deutsch", DisplayCode = "GER",CountryCode = "DE", CountryName = "Germany", CountryNativeName = "Deutschland", Name = "Deutsch (Germany)", EnglishName = "German", Code = "de-DE", Voice = "de-DE-Hedda", Flag = ImageUtility.ReturnImageSourceFromFile("germany.png") },
                    new Language() { DisplayName = "English", DisplayCode = "CAN",CountryCode = "CA", CountryName = "Canada", CountryNativeName = "Canada", Name = "English (Canada)", EnglishName = "English", Code = "en-CA", Voice = "en-CA-Linda", Flag = ImageUtility.ReturnImageSourceFromFile("canada.png") },
                    new Language() { DisplayName = "English", DisplayCode = "UK",CountryCode = "GB", CountryName = "United Kingdom", CountryNativeName = "United Kingdom", Name = "English (United Kingdom)", EnglishName = "English", Code = "en-GB", Voice = "en-GB-Susan-Apollo", Flag = ImageUtility.ReturnImageSourceFromFile("united_kingdom.png") },
                    new Language() { DisplayName = "English", DisplayCode = "IND",CountryCode = "IN", CountryName = "India", CountryNativeName = "India", Name = "English (India)", EnglishName = "English", Code = "en-IN", Voice = "en-IN-Heera-Apollo", Flag = ImageUtility.ReturnImageSourceFromFile("india.png") },
                    new Language() { DisplayName = "English", DisplayCode = "US",CountryCode = "US", CountryName = "United States", CountryNativeName = "United States", Name = "English (United States)", EnglishName = "English", Code = "en-US", Voice = "en-US-ZiraRUS", Flag = ImageUtility.ReturnImageSourceFromFile("united_states_of_america.png")},
                    new Language() { DisplayName = "English", DisplayCode = "HK",CountryCode = "HK", CountryName = "Hong Kong", CountryNativeName = "Hong Kong", Name = "English (Hong Kong)", EnglishName = "English", Code = "en-HK", Voice = "en-HK-SamNeural", Flag = ImageUtility.ReturnImageSourceFromFile("hong_kong.png")},
                    new Language() { DisplayName = "English", DisplayCode = "IE",CountryCode = "IE", CountryName = "Ireland", CountryNativeName = "Ireland", Name = "English (Ireland)", EnglishName = "English", Code = "en-IE", Voice = "en-IE-ConnorNeural", Flag = ImageUtility.ReturnImageSourceFromFile("ireland.png")},
                    new Language() { DisplayName = "English", DisplayCode = "KE",CountryCode = "KE", CountryName = "Kenya", CountryNativeName = "Kenya", Name = "English (Kenya)", EnglishName = "English", Code = "en-KE", Voice = "en-KE-AsiliaNeural", Flag = ImageUtility.ReturnImageSourceFromFile("kenya.png")},
                    new Language() { DisplayName = "English", DisplayCode = "NG",CountryCode = "NG", CountryName = "Nigeria", CountryNativeName = "Nigeria", Name = "English (Nigeria)", EnglishName = "English", Code = "en-NG", Voice = "en-NG-AbeoNeural", Flag = ImageUtility.ReturnImageSourceFromFile("nigeria.png")},
                    new Language() { DisplayName = "English", DisplayCode = "NG",CountryCode = "NG", CountryName = "New Zealand", CountryNativeName = "New Zealand", Name = "English (New Zealand)", EnglishName = "English", Code = "en-NZ", Voice = "en-NZ-MitchellNeural", Flag = ImageUtility.ReturnImageSourceFromFile("new_zealand.png")},
                    new Language() { DisplayName = "English", DisplayCode = "PH",CountryCode = "PH", CountryName = "Philippines", CountryNativeName = "Philippines", Name = "English (Philippines)", EnglishName = "English", Code = "en-PH", Voice = "en-PH-JamesNeural", Flag = ImageUtility.ReturnImageSourceFromFile("philippines.png")},
                    new Language() { DisplayName = "English", DisplayCode = "SG",CountryCode = "SG", CountryName = "Singapore", CountryNativeName = "Singapore", Name = "English (Singapore)", EnglishName = "English", Code = "en-SG", Voice = "en-SG-LunaNeural", Flag = ImageUtility.ReturnImageSourceFromFile("singapore.png")},
                    new Language() { DisplayName = "English", DisplayCode = "TZ",CountryCode = "TZ", CountryName = "Tanzania", CountryNativeName = "Tanzania", Name = "English (Tanzania)", EnglishName = "English", Code = "en-TZ", Voice = "en-TZ-ElimuNeural", Flag = ImageUtility.ReturnImageSourceFromFile("tanzania.png")},
                    new Language() { DisplayName = "English", DisplayCode = "ZA",CountryCode = "ZA", CountryName = "South Africa", CountryNativeName = "South Africa", Name = "English (South Africa)", EnglishName = "English", Code = "en-ZA", Voice = "en-ZA-LeahNeural", Flag = ImageUtility.ReturnImageSourceFromFile("south_africa.png")},
                    new Language() { DisplayName = "Español", DisplayCode = "MX",CountryCode = "MX", CountryName = "Mexico", CountryNativeName = "México", Name = "Español (Mexico)", EnglishName = "Spanish", Code = "es-MX", Voice = "es-MX-HildaRUS", Flag = ImageUtility.ReturnImageSourceFromFile("mexico.png") },
                    new Language() { DisplayName = "Español", DisplayCode = "AR",CountryCode = "AR", CountryName = "Argentina", CountryNativeName = "Argentina", Name = "Español (Argentina)", EnglishName = "Spanish", Code = "es-AR", Voice = "es-AR-ElenaNeural", Flag = ImageUtility.ReturnImageSourceFromFile("argentina.png") },
                    new Language() { DisplayName = "Español", DisplayCode = "BO",CountryCode = "BO", CountryName = "Bolivia", CountryNativeName = "Bolivia", Name = "Español (Bolivia)", EnglishName = "Spanish", Code = "es-BO", Voice = "es-BO-MarceloNeural", Flag = ImageUtility.ReturnImageSourceFromFile("bolivia.png") },
                    new Language() { DisplayName = "Español", DisplayCode = "CL",CountryCode = "CL", CountryName = "Chile", CountryNativeName = "Chile", Name = "Español (Chile)", EnglishName = "Spanish", Code = "es-CL", Voice = "es-CL-CatalinaNeural", Flag = ImageUtility.ReturnImageSourceFromFile("chile.png") },
                    new Language() { DisplayName = "Español", DisplayCode = "CO",CountryCode = "CO", CountryName = "Colombia", CountryNativeName = "Colombia", Name = "Español (Colombia)", EnglishName = "Spanish", Code = "es-CO", Voice = "es-CO-GonzaloNeural", Flag = ImageUtility.ReturnImageSourceFromFile("colombia.png") },
                    new Language() { DisplayName = "Español", DisplayCode = "CR",CountryCode = "CR", CountryName = "Costa Rica", CountryNativeName = "Costa Rica", Name = "Español (Costa Rica)", EnglishName = "Spanish", Code = "es-CR", Voice = "es-CR-JuanNeural", Flag = ImageUtility.ReturnImageSourceFromFile("costa_rica.png") },
                    new Language() { DisplayName = "Español", DisplayCode = "CU",CountryCode = "CU", CountryName = "Cuba", CountryNativeName = "Cuba", Name = "Español (Cuba)", EnglishName = "Spanish", Code = "es-CU", Voice = "es-CU-BelkysNeural", Flag = ImageUtility.ReturnImageSourceFromFile("cuba.png") },
                    new Language() { DisplayName = "Español", DisplayCode = "DO",CountryCode = "DO", CountryName = "Dominican Republic", CountryNativeName = "República Dominicana", Name = "Español (Dominican Republic)", EnglishName = "Spanish", Code = "es-DO", Voice = "es-DO-EmilioNeural", Flag = ImageUtility.ReturnImageSourceFromFile("dominican_republic.png") },
                    new Language() { DisplayName = "Español", DisplayCode = "EC",CountryCode = "EC", CountryName = "Ecuador", CountryNativeName = "Ecuador", Name = "Español (Ecuador)", EnglishName = "Spanish", Code = "es-EC", Voice = "es-EC-AndreaNeural", Flag = ImageUtility.ReturnImageSourceFromFile("ecuador.png") },
                    new Language() { DisplayName = "Español", DisplayCode = "GQ",CountryCode = "GQ", CountryName = "Equatorial Guinea", CountryNativeName = "Guinea Ecuatorial", Name = "Español (Equatorial Guinea)", EnglishName = "Spanish", Code = "es-GQ", Voice = "es-GQ-JavierNeural", Flag = ImageUtility.ReturnImageSourceFromFile("equatorial_guinea.png") },
                    new Language() { DisplayName = "Español", DisplayCode = "GT",CountryCode = "GT", CountryName = "Guatemala", CountryNativeName = "Guatemala", Name = "Español (Guatemala)", EnglishName = "Spanish", Code = "es-GT", Voice = "es-GT-AndresNeural", Flag = ImageUtility.ReturnImageSourceFromFile("guatemala.png") },
                    new Language() { DisplayName = "Español", DisplayCode = "HN",CountryCode = "HN", CountryName = "Honduras", CountryNativeName = "Honduras", Name = "Español (Honduras)", EnglishName = "Spanish", Code = "es-HN", Voice = "es-HN-CarlosNeural", Flag = ImageUtility.ReturnImageSourceFromFile("honduras.png") },
                    new Language() { DisplayName = "Español", DisplayCode = "NI",CountryCode = "NI", CountryName = "Nicaragua", CountryNativeName = "Nicaragua", Name = "Español (Nicaragua)", EnglishName = "Spanish", Code = "es-NI", Voice = "es-NI-FedericoNeural", Flag = ImageUtility.ReturnImageSourceFromFile("nicaragua.png") },
                    new Language() { DisplayName = "Español", DisplayCode = "PA",CountryCode = "PA", CountryName = "Panama", CountryNativeName = "Panamá", Name = "Español (Panama)", EnglishName = "Spanish", Code = "es-PA", Voice = "es-PA-MargaritaNeural", Flag = ImageUtility.ReturnImageSourceFromFile("panama.png") },
                    new Language() { DisplayName = "Español", DisplayCode = "PE",CountryCode = "PE", CountryName = "Peru", CountryNativeName = "Perú", Name = "Español (Peru)", EnglishName = "Spanish", Code = "es-PE", Voice = "es-PE-AlexNeural", Flag = ImageUtility.ReturnImageSourceFromFile("peru.png") },
                    new Language() { DisplayName = "Español", DisplayCode = "PR",CountryCode = "PR", CountryName = "Puerto Rico", CountryNativeName = "Puerto Rico", Name = "Español (Puerto Rico)", EnglishName = "Spanish", Code = "es-PR", Voice = "es-PR-KarinaNeural", Flag = ImageUtility.ReturnImageSourceFromFile("puerto_rico.png") },
                    new Language() { DisplayName = "Español", DisplayCode = "PY",CountryCode = "PY", CountryName = "Paraguay", CountryNativeName = "Paraguay", Name = "Español (Paraguay)", EnglishName = "Spanish", Code = "es-PY", Voice = "es-PY-MarioNeural", Flag = ImageUtility.ReturnImageSourceFromFile("paraguay.png") },
                    new Language() { DisplayName = "Español", DisplayCode = "SV",CountryCode = "SV", CountryName = "El Salvador", CountryNativeName = "El Salvador", Name = "Español (El Salvador)", EnglishName = "Spanish", Code = "es-SV", Voice = "es-SV-LorenaNeural", Flag = ImageUtility.ReturnImageSourceFromFile("el_salvador.png") },
                    new Language() { DisplayName = "Español", DisplayCode = "US",CountryCode = "US", CountryName = "United States", CountryNativeName = "Estados Unidos", Name = "Español (United States)", EnglishName = "Spanish", Code = "es-US", Voice = "es-US-AlonsoNeural", Flag = ImageUtility.ReturnImageSourceFromFile("united_states_of_america.png") },
                    new Language() { DisplayName = "Español", DisplayCode = "UY",CountryCode = "UY", CountryName = "Uruguay", CountryNativeName = "Uruguay", Name = "Español (Uruguay)", EnglishName = "Spanish", Code = "es-UY", Voice = "es-UY-MateoNeural", Flag = ImageUtility.ReturnImageSourceFromFile("uruguay.png") },
                    new Language() { DisplayName = "Español", DisplayCode = "VE",CountryCode = "VE", CountryName = "Venezuela", CountryNativeName = "Venezuela", Name = "Español (Venezuela)", EnglishName = "Spanish", Code = "es-VE", Voice = "es-VE-PaolaNeural", Flag = ImageUtility.ReturnImageSourceFromFile("venezuela.png") },
                    new Language() { DisplayName = "Français", DisplayCode = "CAN",CountryCode = "CA", CountryName = "Canada", CountryNativeName = "Canada", Name = "Français (Canada)", EnglishName = "French", Code = "fr-CA", Voice = "fr-CA-Caroline", Flag = ImageUtility.ReturnImageSourceFromFile("canada.png") },
                    new Language() { DisplayName = "தமிழ்", DisplayCode = "IND",CountryCode = "IN", CountryName = "India", CountryNativeName = "இந்தியா", Name = "தமிழ் (India)", EnglishName = "Tamil", Code = "ta-IN", Voice = "ta-IN-Valluvar", Flag = ImageUtility.ReturnImageSourceFromFile("india.png") },
                    new Language() { DisplayName = "Català", DisplayCode = "ES",CountryCode = "ES", CountryName = "Spain", CountryNativeName = "España", Name = "Català (Spain)", EnglishName = "Catalan", Code = "ca-ES", Voice = "ca-ES-HerenaRUS", Flag = ImageUtility.ReturnImageSourceFromFile("spain.png") },
                    new Language() { DisplayName = "Dansk", DisplayCode = "DEN",CountryCode = "DK", CountryName = "Denmark", CountryNativeName = "Danmark", Name = "Dansk (Denmark)", EnglishName = "Danish", Code = "da-DK", Voice = "da-DK-HelleRUS", Flag = ImageUtility.ReturnImageSourceFromFile("denmark.png") },
                    new Language() { DisplayName = "English", DisplayCode = "AUS",CountryCode = "AU", CountryName = "Australia", CountryNativeName = "Australia", Name = "English (Australia)", EnglishName = "English", Code = "en-AU", Voice = "en-AU-Catherine", Flag = ImageUtility.ReturnImageSourceFromFile("australia.png") },
                    new Language() { DisplayName = "Suomi", DisplayCode = "FIN",CountryCode = "FI", CountryName = "Finland", CountryNativeName = "Suomi", Name = "Suomi (Finland)", EnglishName = "Finnish", Code = "fi-FI", Voice = "fi-FI-HeidiRUS", Flag = ImageUtility.ReturnImageSourceFromFile("finland.png") },
                    new Language() { DisplayName = "हिन्दी", DisplayCode = "IND",CountryCode = "IN", CountryName = "India", CountryNativeName = "India", Name = "हिन्दी (India)", EnglishName = "Hindi", Code = "hi-IN", Voice = "hi-IN-Kalpana-Apollo", Flag = ImageUtility.ReturnImageSourceFromFile("india.png") },
                    new Language() { DisplayName = "日本語", DisplayCode = "JAP",CountryCode = "JP", CountryName = "Japan", CountryNativeName = "日本", Name = "日本語 (Japan)", EnglishName = "Japanese", Code = "ja-JP", Voice = "ja-JP-Ayumi-Apollo", Flag = ImageUtility.ReturnImageSourceFromFile("japan.png") },
                    new Language() { DisplayName = "조선말", DisplayCode = "KR",CountryCode = "KR", CountryName = "South Korea", CountryNativeName = "한국", Name = "조선말 (Korea)", EnglishName = "Korean", Code = "ko-KR", Voice = "ko-KR-HeamiRUS", Flag = ImageUtility.ReturnImageSourceFromFile("south korea.png") },
                    new Language() { DisplayName = "Norsk", DisplayCode = "NOR",CountryCode = "NO", CountryName = "Norway", CountryNativeName = "Norge", Name = "Norsk (Bokmål) (Norway)", EnglishName = "Norwegian", Code = "nb-NO", Voice = "nb-NO-HuldaRUS", Flag = ImageUtility.ReturnImageSourceFromFile("norway.png") },
                    new Language() { DisplayName = "Język polski", DisplayCode = "POL",CountryCode = "PL", CountryName = "Poland", CountryNativeName = "Polska", Name = "Język polski (Poland)", EnglishName = "Polish", Code = "pl-PL", Voice = "pl-PL-PaulinaRUS", Flag = ImageUtility.ReturnImageSourceFromFile("poland.png") },
                    new Language() { DisplayName = "Português", DisplayCode = "BR",CountryCode = "BR", CountryName = "Brazil", CountryNativeName = "Brasil", Name = "Português (Brazil)", EnglishName = "Portuguese", Code = "pt-BR", Voice = "pt-BR-HeloisaRUS", Flag = ImageUtility.ReturnImageSourceFromFile("brazil.png") },
                    new Language() { DisplayName = "Português", DisplayCode = "PT", CountryCode = "PT", CountryName = "Portugal", CountryNativeName = "Portugal", Name = "Português (Portugal)", EnglishName = "Portuguese", Code = "pt-PT", Voice = "pt-PT-HeliaRUS", Flag = ImageUtility.ReturnImageSourceFromFile("portugal.png") },
                    new Language() { DisplayName = "Русский", DisplayCode = "RUS",CountryCode = "RU", CountryName = "Russia", CountryNativeName = "Россия", Name = "Русский (Russia)", EnglishName = "Russian", Code = "ru-RU", Voice = "ru-RU-Irina-Apollo", Flag = ImageUtility.ReturnImageSourceFromFile("russia.png") },
                    new Language() { DisplayName = "తెలుగు", DisplayCode = "IND",CountryCode = "IN", CountryName = "India", CountryNativeName = "India", Name = "తెలుగు (India)", EnglishName = "Telugu", Code = "te-IN", Voice = "te-IN-Chitra", Flag = ImageUtility.ReturnImageSourceFromFile("india.png") },
                    new Language() { DisplayName = "福佬話", DisplayCode = "TW",CountryCode = "CN", CountryName = "China", CountryNativeName = "中国", Name = "福佬話 (Taiwanese Mandarin)", EnglishName = "Chinese", Code = "zh-TW", Voice = "zh-TW-Yating-Apollo", Flag = ImageUtility.ReturnImageSourceFromFile("taiwan.png") },
                    new Language() { DisplayName = "ภาษาไทย", DisplayCode = "TH",CountryCode = "TH", CountryName = "Thailand", CountryNativeName = "ประเทศไทย", Name = "ภาษาไทย (Thailand)", EnglishName = "Thai", Code = "th-TH", Voice = "th-TH-Pattara", Flag = ImageUtility.ReturnImageSourceFromFile("thailand.png") },
                    new Language() { DisplayName = "Türkçe", DisplayCode = "TUR",CountryCode = "TR", CountryName = "Turkey", CountryNativeName = "Turkiye", Name = "Türkçe (Turkey)", EnglishName = "Turkey", Code = "tr-TR", Voice = "tr-TR-SedaRUS", Flag = ImageUtility.ReturnImageSourceFromFile("turkey.png") },
                    new Language() { DisplayName = "български език", DisplayCode = "BG",CountryCode = "BG", CountryName = "Bulgaria", CountryNativeName = "Bŭlgariya", Name = "ългарски език (Bulgaria)", EnglishName = "Bulgarian", Code = "bg-BG", Voice = "bg-BG-Ivan", Flag = ImageUtility.ReturnImageSourceFromFile("bulgaria.png") },
                    new Language() { DisplayName = "Hrvatski", DisplayCode = "CRO",CountryCode = "HR", CountryName = "Croatia", CountryNativeName = "Hrvatska", Name = "Hrvatski (Croatia)", EnglishName = "Croatian", Code = "hr-HR", Voice = "hr-HR-Matej", Flag = ImageUtility.ReturnImageSourceFromFile("croatia.png") },
                    new Language() { DisplayName = "Český Jazyk", DisplayCode = "CZ",CountryCode = "CZ", CountryName = "Czech", CountryNativeName = "Česko", Name = "Český Jazyk (Czech)", EnglishName = "Czech", Code = "cs-CZ", Voice = "cs-CZ-Jakub", Flag = ImageUtility.ReturnImageSourceFromFile("czechRepublic.png") },
                    new Language() { DisplayName = "Ελληνικά", DisplayCode = "GR",CountryCode = "GR", CountryName = "Greece", CountryNativeName = "Ελλάς", Name = "Ελληνικά (Greece)", EnglishName = "Greek", Code = "el-GR", Voice = "el-GR-Stefanos", Flag = ImageUtility.ReturnImageSourceFromFile("greece.png") },
                    new Language() { DisplayName = "Magyar", DisplayCode = "HUN",CountryCode = "HU", CountryName = "Hungary", CountryNativeName = "Magyarország", Name = "Magyar (Hungary)", EnglishName = "Hungarian", Code = "hu-HU", Voice = "hu-HU-Szabolcs", Flag = ImageUtility.ReturnImageSourceFromFile("hungary.png") },
                    new Language() { DisplayName = "Română", DisplayCode = "RO",CountryCode = "RO", CountryName = "Romania", CountryNativeName = "România", Name = "Română (Romania)", EnglishName = "Romanian", Code = "ro-RO", Voice = "ro-RO-Andrei", Flag = ImageUtility.ReturnImageSourceFromFile("romania.png") },
                    new Language() { DisplayName = "Slovenčina", DisplayCode = "SK", CountryCode = "SK", CountryName = "Slovakia", CountryNativeName = "Slovensko", Name = "Slovenčina (Slovakia)", EnglishName = "Slovak", Code = "sk-SK", Voice = "sk-SK-Filip", Flag = ImageUtility.ReturnImageSourceFromFile("slovakia.png") },
                    new Language() { DisplayName = "Slovenščina", DisplayCode = "SL",CountryCode = "SI", CountryName = "Slovenia", CountryNativeName = "Slovenija", Name = "Slovenščina (Slovenia)", EnglishName = "Slovenian", Code = "sl-SI", Voice = "sl-SI-Lado", Flag = ImageUtility.ReturnImageSourceFromFile("slovenia.png") },
                    new Language() { DisplayName = "עברית", DisplayCode = "IL",CountryCode = "IL", CountryName = "Israel", CountryNativeName = "Yisrael", Name = "עברית (Israel)", EnglishName = "Hebrew", Code = "he-IL", Voice = "he-IL-Asaf", Flag = ImageUtility.ReturnImageSourceFromFile("israel.png") },
                    new Language() { DisplayName = "Bahasa Indonesia", DisplayCode = "ID",CountryCode = "ID", CountryName = "Indonesia", CountryNativeName = "Indonesia", Name = "Bahasa Indonesia (Indonesia)", EnglishName = "Indonesian", Code = "id-ID", Voice = "id-ID-Andika", Flag = ImageUtility.ReturnImageSourceFromFile("indonesia.png") },
                    new Language() { DisplayName = "Tiếng Việt Nam", DisplayCode = "VN",CountryCode = "VN", CountryName = "Vietnam", CountryNativeName = "Việt Nam", Name = "Tiếng Việt Nam (Vietnam)", EnglishName = "Vietnamese", Code = "vi-VN", Voice = "vi-VN-An", Flag = ImageUtility.ReturnImageSourceFromFile("vietnam.png") },
                    new Language() { DisplayName = "Eesti", DisplayCode = "EE",CountryCode = "EE", CountryName = "Estonia", CountryNativeName = "Eesti", Name = "Eesti (Estonia)", EnglishName = "Estonian", Code = "et-EE", Voice = "et-EE-AnuNeural", Flag = ImageUtility.ReturnImageSourceFromFile("estonia.png") },
                    new Language() { DisplayName = "ગુજરાતી", DisplayCode = "IN",CountryCode = "IN", CountryName = "India", CountryNativeName = "India", Name = "ગુજરાતી (India)", EnglishName = "Gujarati", Code = "gu-IN", Voice = "gu-IN-DhwaniNeural", Flag = ImageUtility.ReturnImageSourceFromFile("india.png") },
                    new Language() { DisplayName = "latviešu", DisplayCode = "LV",CountryCode = "LV", CountryName = "Latvia", CountryNativeName = "Latvija", Name = "latviešu (Latvia)", EnglishName = "Latvian", Code = "lv-LV", Voice = "lv-LV-EveritaNeural", Flag = ImageUtility.ReturnImageSourceFromFile("latvia.png") },
                    new Language() { DisplayName = "Lietuvių", DisplayCode = "LT",CountryCode = "LT", CountryName = "Lithuania", CountryNativeName = "Lietuva", Name = "Lietuvių (Lithuania)", EnglishName = "Lithuanian", Code = "lt-LT", Voice = "lt-LT-OnaNeural", Flag = ImageUtility.ReturnImageSourceFromFile("lithuania.png") },
                    new Language() { DisplayName = "Malti", DisplayCode = "MT",CountryCode = "MT", CountryName = "Malta", CountryNativeName = "Malta", Name = "Malti (Malta)", EnglishName = "Maltese", Code = "mt-MT", Voice = "mt-MT-GraceNeural", Flag = ImageUtility.ReturnImageSourceFromFile("malta.png") },
                    new Language() { DisplayName = "मराठी", DisplayCode = "IN",CountryCode = "IN", CountryName = "India", CountryNativeName = "India", Name = "मराठी (India)", EnglishName = "Marathi", Code = "mr-IN", Voice = "mr-IN-AarohiNeural", Flag = ImageUtility.ReturnImageSourceFromFile("india.png") },
                    new Language() { DisplayName = "Gaeilge", DisplayCode = "IE",CountryCode = "IE", CountryName = "Ireland", CountryNativeName = "Éire", Name = "Gaeilge (Ireland)", EnglishName = "Gaelic", Code = "ga-IE", Voice = "ga-IE-OrlaNeural", Flag = ImageUtility.ReturnImageSourceFromFile("ireland.png") },
                    new Language() { DisplayName = "украї́нська мо́ва", DisplayCode = "UA",CountryCode = "UA", CountryName = "Ukraine", CountryNativeName = "Ukraїna", Name = "украї́нська мо́ва (Ukraine)", EnglishName = "Ukrainian", Code = "uk-UA", Voice = "uk-UA-PolinaNeural", Flag = ImageUtility.ReturnImageSourceFromFile("ukraine.png") },
                    new Language() { DisplayName = "ಕನ್ನಡ", DisplayCode = "KN",CountryCode = "IN", CountryName = "India", CountryNativeName = "ಭಾರತ", Name = "Kannada (India)", EnglishName = "Kannada", Code = "kn-IN", Voice = "kn-IN-GaganNeural", Flag = ImageUtility.ReturnImageSourceFromFile("india.png") },
                    new Language() { DisplayName = "മലയാളം", DisplayCode = "ML",CountryCode = "IN", CountryName = "India", CountryNativeName = "ഇന്ത്യ", Name = "Malayalam (India)", EnglishName = "Malayalam", Code = "ml-IN", Voice = "ml-IN-MidhunNeural", Flag = ImageUtility.ReturnImageSourceFromFile("india.png") },

                };

                    if (_approvedLanguages != null && _approvedLanguages.Any())
                    {
                        foreach (var language in _approvedLanguages)
                        {
                            var foundLanguage = _autodetectLanguages.FirstOrDefault(l => l.Code.ToLower() == language.Code.ToLower());
                            if (foundLanguage != null && _autoDetectApprovedLanguages.Exists(l => l.Code.ToLower() == language.Code.ToLower()))
                            {
                                _autoDetectApprovedLanguages.Add(foundLanguage);
                            }
                        }
                    }
                    return _autodetectLanguages?.OrderBy(s => s.Name).ToList();
                }
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw ex;
            }
        }

        public async Task<List<Country>> GetCountries()
        {
            var _countries = new List<Country>();
            var languages = await GetSupportedLanguages();

            foreach (var language in languages)
            {
                var code = language.Code.Substring(3);

                if (_countries.Any(s => s.CountryCode == code))
                {
                    var languageFlag = _countries.FirstOrDefault(s => s.CountryCode == code);
                    languageFlag.Languages.Add(language);
                }
                else
                {
                    _countries.Add(new Country
                    {
                        CountryCode = code,
                        CountryName = language.CountryName,
                        CountryNativeName = language.CountryNativeName,
                        Flag = language.Flag,
                        Languages = new ObservableCollection<Language> { language }
                    });
                }
            }

            return _countries;
        }

        /// <summary>
        /// Method to get defualt languages from settings
        /// </summary>
        /// <returns>A dictionary with the Keys being the language type and value the language code</returns>
        public async Task<Dictionary<string, string>> GetDefaultLanguages()
        {
            try
            {
                if (DefaultLanguages != null && DefaultLanguages.Any()) return DefaultLanguages;

                var defaultSourceLanguage = new Language();
                var defaultTargetLanguage = new Language();

                var Languages = await GetSupportedLanguages();

                if (Languages != null)
                {
                    var sourceLanguage = Settings.GetSetting(Settings.Setting.DefaultSourceLanguage);
                    var targetLanguage = Settings.GetSetting(Settings.Setting.DefaultTargetLanguage);

                    if (!string.IsNullOrEmpty(sourceLanguage))
                    {
                        defaultSourceLanguage = Languages.Where(s => s.Code == sourceLanguage).FirstOrDefault();
                        if (defaultSourceLanguage == null)
                            Settings.AddSetting(Settings.Setting.DefaultSourceLanguage, Languages.FirstOrDefault().Code);
                    }
                    else
                    {
                        defaultSourceLanguage = Languages.Where(s => s.Code == "en-GB").FirstOrDefault();
                        if (defaultSourceLanguage != null)
                            Settings.AddSetting(Settings.Setting.DefaultSourceLanguage, "en-GB");
                        else
                            Settings.AddSetting(Settings.Setting.DefaultSourceLanguage, Languages.FirstOrDefault().Code);
                    }

                    if (!string.IsNullOrEmpty(targetLanguage))
                    {
                        defaultTargetLanguage = Languages.Where(s => s.Code == targetLanguage).FirstOrDefault();
                        if (defaultTargetLanguage == null && Languages.Count > 1)
                            Settings.AddSetting(Settings.Setting.DefaultTargetLanguage, Languages.ElementAtOrDefault(1).Code);
                    }
                    else
                    {
                        defaultTargetLanguage = Languages.Where(s => s.Code == "fr-FR").FirstOrDefault();
                        if (defaultTargetLanguage != null)
                            Settings.AddSetting(Settings.Setting.DefaultTargetLanguage, "fr-FR");
                        else
                        {
                            Settings.AddSetting(Settings.Setting.DefaultTargetLanguage, Languages.ElementAtOrDefault(1).Code);
                            defaultTargetLanguage = Languages.FirstOrDefault();
                        }
                    }

                    var source = EnumsConverter.ConvertToString(Settings.Setting.DefaultSourceLanguage);
                    var target = EnumsConverter.ConvertToString(Settings.Setting.DefaultTargetLanguage);

                    DefaultLanguages = new Dictionary<string, string>
                    {
                        { source, defaultSourceLanguage.Code },
                        { target, defaultTargetLanguage.Code }
                    };
                }

                return DefaultLanguages;
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw ex;
            }
        }

        /// <summary>
        /// Method to add language setting
        /// </summary>
        /// <param name="languageCode">Takes in the language code</param>
        /// <param name="setting">Takes in the language settings enum</param>
        public void SetDefaultLanguage(string languageCode, Settings.Setting setting)
        {
            Settings.AddSetting(setting, languageCode);
        }
    }
}
