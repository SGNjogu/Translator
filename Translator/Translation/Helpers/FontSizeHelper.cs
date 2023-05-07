using System;
using Translation.AppSettings;
using Translation.Utils;

namespace Translation.Helpers
{
    public static class FontSizeHelper
    {
        private static double _transcriptionsFontSize;

        public static void GetTranscriptionsFontSize()
        {
            string fontSize = Settings.GetSetting(Settings.Setting.TranscriptionsFontSize);

            if (string.IsNullOrEmpty(fontSize))
            {
                var resourceFontSize = ResourceFinder.GetResource<object>("TranscriptionsFontSize");

                if (resourceFontSize != null)
                {
                    _transcriptionsFontSize = Convert.ToDouble(resourceFontSize);
                }
                else
                {
                    _transcriptionsFontSize = 15;
                }
                UpdateTranscriptionsFontSize();
            }
            else
            {
                _transcriptionsFontSize = Convert.ToDouble(fontSize);
                UpdateTranscriptionsFontSize();
            }
        }

        public static void IncreaseFontSize()
        {
            if (_transcriptionsFontSize <= 30)
            {
                _transcriptionsFontSize += 2;
                UpdateTranscriptionsFontSize();
            }
        }

        public static void DecreaseFontSize()
        {
            if (_transcriptionsFontSize >= 10)
            {
                _transcriptionsFontSize -= 2;
                UpdateTranscriptionsFontSize();
            }
        }

        private static void UpdateTranscriptionsFontSize()
        {
            Settings.AddSetting(Settings.Setting.TranscriptionsFontSize, _transcriptionsFontSize.ToString());
            ResourceFinder.UpdateResource("TranscriptionsFontSize", _transcriptionsFontSize);
            ResourceFinder.UpdateResource("TranscriptionsMetadataFontSize", _transcriptionsFontSize - 2);
        }
    }
}
