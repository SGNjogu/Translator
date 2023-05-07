using Newtonsoft.Json;

namespace Translation.Core.Domain
{
    public class Language
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string VoiceName { get; set; }
        public Voice Voice { get; set; }
        [JsonIgnore]
        public string Flag { get; set; }
        public string DisplayName { get; set; }
        public bool UseNeuralVoice { get; set; }

        public override string ToString()
        {
            return $"Name: {Name} Code: {Code} Voice: {VoiceName}";
        }
    }
}
