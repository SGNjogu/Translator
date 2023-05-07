using System;

namespace Translation.Core.Events
{
    /// <summary>
    /// Synthesizer event args
    /// </summary>
    /// <typeparam name="T">Any type T</typeparam>
    public class SynthesizerEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SynthesizerEventArgs{T}" /> class.
        /// </summary>
        /// <param name="eventData">The event data.</param>
        public SynthesizerEventArgs(T eventData)
        {
            this.EventData = eventData;
        }

        /// <summary>
        /// Gets the event data.
        /// </summary>
        public T EventData { get; private set; }
    }
}
