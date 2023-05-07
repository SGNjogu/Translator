namespace Translation.Core.Events
{
    public class AudioInputDataAvailableArgs
    {
        /// <summary>
        /// Recorded audio data
        /// </summary>
        public byte[] Buffer { get; set; }

        /// <summary>
        /// Number of bytes recorded
        /// </summary>
        public int Count { get; set; }
    }

    public delegate void AudioInputDataAvailable(object sender, AudioInputDataAvailableArgs dataAvailableEventArgs);
}
