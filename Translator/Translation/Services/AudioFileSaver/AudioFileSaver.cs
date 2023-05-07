using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Translation.Core.Domain;
using Translation.Interface;

namespace Translation.Services.AudioFileSaver
{
    public class AudioFileSaver : IAudioFileSaver
    {
        private bool IsWriting = false;
        private FileStream fileStream;
        private string _filePath;

        public void WriteToFile(string filePath, AudioDataInput audioDataInput)
        {
            _filePath = filePath;

            // Check if the folder exist or not
            if (!File.Exists(filePath))
            {
                var fs = new FileStream(filePath, FileMode.Create);
                fs.Dispose();
            }

            if (!IsWriting)
            {
                fileStream = new FileStream(filePath, FileMode.Append);
                IsWriting = true;
            }

            fileStream.Write(audioDataInput.Bytes, 0, audioDataInput.ByteCount);
        }

        public async Task SaveFile()
        {
            try
            {
                if (string.IsNullOrEmpty(_filePath))
                    throw new Exception("FilePath is null");

                if (IsWriting && fileStream != null)
                {
                    // byte Channels = 1;
                    // BitsPerSample = 16;
                    // SamplesPerSecond = 16000;

                    fileStream.Dispose();
                    fileStream = null;
                    IsWriting = false;

                    await WriteWavHeader(_filePath, 16000, 16, 1);
                    _filePath = null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

        }

        async Task WriteWavHeader(string filePath, int sampleRate, short bitsPerSample, short channels)
        {
            await Task.Run(() =>
            {
                using (var fs = File.Open(filePath, FileMode.Open, System.IO.FileAccess.ReadWrite))
                {
                    using (BinaryWriter writer = new BinaryWriter(fs, System.Text.Encoding.UTF8))
                    {
                        writer.Seek(0, SeekOrigin.Begin);

                        // ChunkID               
                        writer.Write('R');
                        writer.Write('I');
                        writer.Write('F');
                        writer.Write('F');

                        // ChunkSize               
                        writer.Write(BitConverter.GetBytes(fs.Length + 36), 0, 4);

                        // Format               
                        writer.Write('W');
                        writer.Write('A');
                        writer.Write('V');
                        writer.Write('E');

                        //SubChunk               
                        writer.Write('f');
                        writer.Write('m');
                        writer.Write('t');
                        writer.Write(' ');

                        // SubChunk1Size - 16 for PCM
                        writer.Write(BitConverter.GetBytes(16), 0, 4);

                        // AudioFormat - PCM=1
                        writer.Write(BitConverter.GetBytes((short)1), 0, 2);

                        // Channels: Mono=1, Stereo=2
                        writer.Write(BitConverter.GetBytes(channels), 0, 2);

                        // SampleRate
                        writer.Write(sampleRate);

                        // ByteRate
                        var byteRate = sampleRate * 1 * bitsPerSample / 8;
                        writer.Write(BitConverter.GetBytes(byteRate), 0, 4);

                        // BlockAlign
                        var blockAlign = channels * bitsPerSample / 8;
                        writer.Write(BitConverter.GetBytes((short)blockAlign), 0, 2);

                        // BitsPerSample
                        writer.Write(BitConverter.GetBytes(bitsPerSample), 0, 2);

                        // SubChunk2ID
                        writer.Write('d');
                        writer.Write('a');
                        writer.Write('t');
                        writer.Write('a');

                        // Subchunk2Size
                        writer.Write(BitConverter.GetBytes(fs.Length), 0, 4);
                    }
                }
            });
        }
    }
}
