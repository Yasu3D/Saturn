using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using SaturnGame.Data;
using UnityEngine;
using ErrorEventArgs = Newtonsoft.Json.Serialization.ErrorEventArgs;

namespace SaturnGame.RhythmGame
{
public class ReplayManager : MonoBehaviour, IInputProvider
{
    [SerializeField] private string replayFile = "replay.json.gz";
    [SerializeField] private TimeManager timeManager;

    // If PlayingFromReplay is true, we update CurrentTouchState from the Replay and ignore inputs.
    // If false, gameplay is normal and inputs are stored into Replay as they happen.
    public bool PlayingFromReplay;
    private int replayFrameIndex = -1;

    public TouchStateHandler TouchStateHandler { private get; set; }

    private class ReplayFrame
    {
        public readonly TouchState TouchState;
        public readonly float TimeMs;

        [JsonConstructor]
        public ReplayFrame([JsonProperty("TouchState")] TouchState touchState, [JsonProperty("TimeMs")] float timeMs)
        {
            // Copy the TouchState so that it persists, since the original underlying array will likely be reused.
            TouchState = touchState.Copy();
            TimeMs = timeMs;
        }
    }

    private List<ReplayFrame> Replay { get; set; } = new();

    public void RecordFrame(TouchState touchState, float timeMs)
    {
        if (!PlayingFromReplay) Replay.Add(new ReplayFrame(touchState, timeMs));
    }

    private static void JsonError(object sender, [NotNull] ErrorEventArgs errorArgs)
    {
        Debug.LogError(errorArgs.ErrorContext.Error);
        errorArgs.ErrorContext.Handled = true;
    }

    // TODO maybe: stream to file continuously rather than all at the end
    public async Awaitable WriteReplayFile()
    {
        string chartRelativePath = Path.GetRelativePath(SongDatabase.SongPacksPath,
            PersistentStateManager.Instance.SelectedDifficulty.ChartFilepath);
        string timestamp = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
        string replayFileName = $"replay-{chartRelativePath}-{timestamp}.json.gz";
        string escapedReplayFileName = String.Join('_', replayFileName.Split(Path.GetInvalidFileNameChars()));
        string replayPath = Path.Combine(Application.persistentDataPath, escapedReplayFileName);
        Debug.Log($"Writing replay with {Replay.Count} frames to {replayPath}...");
        await using FileStream replayFileStream = File.Create(replayPath);
        await using GZipStream compressedStream = new(replayFileStream, System.IO.Compression.CompressionLevel.Fastest);
        await using StreamWriter writer = new(compressedStream);
        JsonSerializer serializer = new();
        serializer.Error += JsonError;
        // "threadsafe" here is not entirely accurate, but this should hopefully allow us to take a reference
        // to the Replay that will _not_ be modified as more frames are added, so the JsonSerializer can safely
        // iterate over it.
        IEnumerable<ReplayFrame> threadsafeReplay = Replay.Take(Replay.Count);
        await Awaitable.BackgroundThreadAsync();
        serializer.Serialize(writer, threadsafeReplay);
        await Awaitable.MainThreadAsync();
        Debug.Log($"Replay {replayFileName} successfully written!");
    }

    public async Awaitable ReadReplayFile()
    {
        string replayPath = Path.Combine(Application.persistentDataPath, replayFile);
        Debug.Log($"Reading replay from {replayPath}");
        PlayingFromReplay = true;
        await using FileStream replayFileStream = File.OpenRead(replayPath);
        await using GZipStream compressedStream = new(replayFileStream, CompressionMode.Decompress);
        using StreamReader streamReader = new(compressedStream);
        JsonSerializer serializer = new();
        serializer.Error += JsonError;
        await Awaitable.BackgroundThreadAsync();
        List<ReplayFrame> readReplay =
            (List<ReplayFrame>)serializer.Deserialize(streamReader, typeof(List<ReplayFrame>));
        await Awaitable.MainThreadAsync();
        if (readReplay is null)
        {
            PlayingFromReplay = false;
            throw new Exception("Failed to read replay");
        }

        Replay = readReplay;
        Debug.Log($"Loaded replay {replayPath} with {Replay.Count} frames");
        replayFrameIndex = 0;
    }

    private void Update()
    {
        if (!PlayingFromReplay || replayFrameIndex < 0) return;

        while (replayFrameIndex < Replay.Count && Replay[replayFrameIndex].TimeMs <= timeManager.VisualTimeMs)
        {
            TouchStateHandler?.Invoke(Replay[replayFrameIndex].TouchState, Replay[replayFrameIndex].TimeMs);
            replayFrameIndex++;
        }
    }
}
}
