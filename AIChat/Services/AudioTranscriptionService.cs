using AIChat.Configuration;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Options;
using NAudio.Wave;
using System.Text;

namespace AIChat.Services;

public class AudioTranscriptionService
{
    private readonly AISettingsOption _settings;

    public AudioTranscriptionService(IOptions<AISettingsOption> options)
    {
        _settings = options.Value;
    }
    public async Task<string> TranscribeAudioAsync(Stream audioStream, string fileName)
    {
        SpeechRecognitionResult result;

        var speechConfig = SpeechConfig.FromSubscription(_settings.SpeechKey, _settings.SpeechRegion);
        speechConfig.SpeechRecognitionLanguage = "en-US";
        speechConfig.OutputFormat = OutputFormat.Detailed;

        var tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".wav");

        using var memoryStream = new MemoryStream();
        await audioStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        try
        {
            if (fileName.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
            {
                using var mp3Reader = new Mp3FileReader(memoryStream);
                using var pcmStream = WaveFormatConversionStream.CreatePcmStream(mp3Reader);
                WaveFileWriter.CreateWaveFile(tempPath, pcmStream);
            }
            else if (fileName.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
            {
                using var fs = File.Create(tempPath);
                memoryStream.Position = 0;
                await memoryStream.CopyToAsync(fs);
            }
            else
            {
                throw new NotSupportedException("Unsupported audio format. Only .wav and .mp3 files are supported.");
            }

            using var audioInput = AudioConfig.FromWavFileInput(tempPath);

            using var recognizer = new SpeechRecognizer(speechConfig, audioInput);

            var transcriptionBuilder = new StringBuilder();
            var stopRecognition = new TaskCompletionSource<int>();

            recognizer.Recognized += (s, e) =>
            {
               if (e.Result.Reason == ResultReason.RecognizedSpeech)
                {
                    transcriptionBuilder.AppendLine(e.Result.Text);
                }
                else if (e.Result.Reason == ResultReason.NoMatch)
                {
                    transcriptionBuilder.AppendLine("No speech could be recognized.");
                }
            };

            recognizer.SessionStopped += (s, e) =>
            {
                stopRecognition.TrySetResult(0);
            };

            recognizer.Canceled += (s, e) =>
            {
                stopRecognition.TrySetResult(0);
            };

            await recognizer.StartContinuousRecognitionAsync();
            await stopRecognition.Task;
            await recognizer.StopContinuousRecognitionAsync();

            return transcriptionBuilder.ToString();
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }
}
