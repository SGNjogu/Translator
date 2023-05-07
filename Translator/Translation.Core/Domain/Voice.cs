using Translation.Core.Utils;

namespace Translation.Core.Domain
{
    public class Voice
    {
        public string Code { get; set; }
        public bool IsNeuralVoice { get; set; } = false;
        public Gender Gender { get; set; }
    }
}
