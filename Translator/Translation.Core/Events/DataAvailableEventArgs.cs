using System;
using Translation.Core.Domain;

namespace Translation.Core.Events
{
    public delegate void OnDataAvailable(object sender, DataAvailableEventArgs args);

    public class DataAvailableEventArgs : EventArgs
    {
        public AudioDataInput AudioDataInput { get; set; }

        public DataAvailableEventArgs()
        {
            AudioDataInput = new AudioDataInput();
        }
    }
}
