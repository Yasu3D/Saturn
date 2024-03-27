using System;
using System.Collections.Generic;
using System.Linq;
using System.IO.Ports;
using JetBrains.Annotations;
using UnityEngine;
using Newtonsoft.Json;

namespace SaturnGame.RhythmGame
{
public class InputManager : MonoBehaviour
{
    [SerializeField] private bool useTouchController = true;

    [Header("MANAGERS")] [SerializeField] private ScoringManager scoringManager;
    [SerializeField] private TimeManager timeManager;

    public TouchState CurrentTouchState;

    // TODO: Break this out to a separate class, refactor etc
    // Warning: Please only use BaseStream to read/write. The .NET SerialPort implementation has several issues,
    // especially regarding its internal buffer/cache. This can all be avoided if you use BaseStream directly and
    // never use any of the Read/Write methods on SerialPort itself. I think the mono implementation is actually
    // basically okay, but let's be cautious and consistent. See https://sparxeng.com/blog/software/must-use-net-system-io-ports-serialport
    private SerialPort leftRingPort;
    private SerialPort rightRingPort;

    private readonly bool[,] leftTouchData = new bool[30, 4];
    private readonly bool[,] rightTouchData = new bool[30, 4];

    // Start is called before the first frame update
    private async void Start()
    {
        if (useTouchController)
            await InitializeTouchController();
    }

    // Touch controller code is adapted from yellowberryHN/LilyConsole
    private async Awaitable InitializeTouchController()
    {
        SerialPort leftPort = new("COM4", 115200);
        SerialPort rightPort = new("COM3", 115200);
        await InitializeSerialPort(leftPort, (byte)'L');
        leftRingPort = leftPort;
        await InitializeSerialPort(rightPort, (byte)'R');
        rightRingPort = rightPort;
    }

    private static async Awaitable InitializeSerialPort([NotNull] SerialPort port, byte side)
    {
        if (!port.IsOpen) port.Open();
        Debug.Log($"Port {(char)side}: port opened.");

        // In milliseconds.
        // During streaming mode, board sends data every 8ms.
        port.ReadTimeout = 1000;
        port.WriteTimeout = 1000;

        // Send once to get board to shut up.
        await SendData(port, new[] { (byte)SerialCommand.GetSyncBoardVer });
        // Wait to make sure we've received all incoming bytes and they are all discarded.
        await Awaitable.WaitForSecondsAsync(0.020f);
        port.DiscardInBuffer();
        Debug.Log($"Port {(char)side}: shut up!");

        // // Get sync board version
        // await SendData(port, new byte[] { (byte)SerialCommand.GET_SYNC_BOARD_VER });
        // await ReadData(port, 8);
        // // Currently, we discard this data as unused.
        // Debug.Log($"Port {(char)side}: got sync board version.");

        // Get unit board versions
        await SendData(port, new[] { (byte)SerialCommand.GetUnitBoardVer });
        byte[] unitVersionResponse = await ReadData(port, 45);
        byte reportedSide = unitVersionResponse[7];
        if (reportedSide != side)
            throw new Exception("Sync Board disagrees which side it is! Wanted {(char)side}, got {reportedSide}.");
        Debug.Log($"Port {(char)side}: got unit board version.");

        const byte onThreshold = 17;
        const byte offThreshold = 12;
        // TODO: don't hardcode checksum
        byte[] thresholdCommand =
        {
            (byte)SerialCommand.SetThresholds, onThreshold, onThreshold, onThreshold, onThreshold, onThreshold,
            onThreshold, offThreshold, offThreshold, offThreshold, offThreshold, offThreshold, offThreshold, 0x14,
        };
        await SendData(port, thresholdCommand);
        byte[] thresholdResponse = await ReadData(port, 3);
        Debug.Log($"Threshold ({onThreshold}/{offThreshold}) response:");
        Debug.Log(BitConverter.ToString(thresholdResponse));

        // Start touch stream
        await SendData(port,
            new byte[] { (byte)SerialCommand.StartAutoScan, 0x7F, 0x3F, 0x64, 0x28, 0x44, 0x3B, 0x3A });
        byte[] ack = await ReadData(port, 3);
        if (ack[0] != (byte)SerialCommand.StartAutoScan)
            throw new Exception("Start Scan message was not acknowledged.");
        Debug.Log($"Port {(char)side}: started touch stream.");
    }

    private static async Awaitable SendData([NotNull] SerialPort port, [NotNull] byte[] data)
    {
        await port.BaseStream.WriteAsync(data, 0, data.Length);
    }

    // Taken verbatim from LilyConsole
    /// <summary>
    /// Validates the checksum on the end of a given full payload.
    /// </summary>
    /// <param name="packet">The bytes of the payload to be validated.</param>
    /// <returns>The validity of the checksum</returns>
    private static bool ValidChecksum([NotNull] IReadOnlyList<byte> packet)
    {
        byte chk = 0x80;
        for (int i = 0; i < packet.Count - 1; i++)
            chk ^= packet[i];
        return packet[^1] == chk;
    }

    private static async Awaitable<byte[]> ReadData(SerialPort port, int size)
    {
        byte[] buffer = new byte[size];
        int bytesRead = 0;
        while (bytesRead < size)
            bytesRead += await port.BaseStream.ReadAsync(buffer, bytesRead, buffer.Length - bytesRead);
        if (!ValidChecksum(buffer))
        {
            // TODO: some kind of better error handling here - e.g. reboot the touch connection.
            throw new Exception("Bad serial checksum!");
        }

        return buffer;
    }

    // Write to a 30x4 array of segments in the order given by the sync board.
    // The translation from the first index to anglePos depends on which side the board is on.
    private static void ReadTouchDataNonBlocking([NotNull] SerialPort port, bool[,] dest)
    {
        byte[] buffer = new byte[36];
        bool hasData = false;
        while (port.BytesToRead >= buffer.Length)
        {
            int readCommandByte = port.BaseStream.Read(buffer, 0, 1);
            if (readCommandByte != 1) Debug.Log("Didn't successfully read touch command.");
            if (buffer[0] != (byte)SerialCommand.TouchData)
            {
                Debug.Log($"Found {BitConverter.ToString(new[] { buffer[0] })} instead of a TOUCH_DATA command (81)!");
                continue;
            }

            int readBytes = port.BaseStream.Read(buffer, 1, buffer.Length - 1) + readCommandByte;
            if (readBytes != 36)
            {
                Debug.Log($"read {readBytes} bytes instead of expected 36");
                continue;
            }

            if (!ValidChecksum(buffer))
            {
                // TODO: some kind of better error handling here - e.g. reboot the touch connection.
                throw new Exception("Bad serial checksum!");
            }

            hasData = true;
        }

        if (!hasData) return;

        // Buffer layout:
        //  1 byte : SerialCommand.TOUCH_DATA
        // 24 bytes: 4x 6-byte row
        //           A row represents each panel with a single byte. A row contains 6 panels.
        //           The panel byte representation just uses the 5 least significant bits of the byte.
        //  9 bytes: ???
        //  1 byte : Loop state, increases by 1 every time the sync board sends a frame
        //  1 byte : Checksum, already validated above

        if (buffer[0] != (byte)SerialCommand.TouchData)
        {
            throw new Exception(
                $"Expected to receive TOUCH_DATA, but got {BitConverter.ToString(new[] { buffer[0] })}");
        }

        // PANELS_PER_SIDE * COLS_PER_PANEL = 30
        const int panelsPerSide = 6;
        const int colsPerPanel = 5;
        for (int depth = 0; depth < 4; depth++)
        {
            for (int panel = 0; panel < panelsPerSide; panel++)
            {
                int panelRowData = buffer[1 + depth * panelsPerSide + panel];

                for (int panelColumn = 0; panelColumn < colsPerPanel; panelColumn++)
                {
                    int panelColumnMask = 1 << panelColumn;
                    bool active = (panelRowData & panelColumnMask) != 0;

                    int angleOffset = panel * colsPerPanel + panelColumn;
                    dest[angleOffset, depth] = active;
                }
            }
        }

        // loop state is currently discarded.
    }

    // Update is called once per frame
    private void Update()
    {
        if (timeManager.State != TimeManager.SongState.Playing) return;

        // Initializes to all false.
        bool[,] segments = new bool[60, 4];
        if (Input.GetKey("[6]"))
        {
            foreach (int i in Enumerable.Range(56, 4))
            {
                foreach (int j in Enumerable.Range(1, 2)) segments[i, j] = true;
            }

            foreach (int i in Enumerable.Range(0, 4))
            {
                foreach (int j in Enumerable.Range(1, 2)) segments[i, j] = true;
            }
        }

        if (Input.GetKey("[9]"))
        {
            foreach (int i in Enumerable.Range(3, 7))
            {
                foreach (int j in Enumerable.Range(1, 2)) segments[i, j] = true;
            }
        }

        if (Input.GetKey("[8]"))
        {
            foreach (int i in Enumerable.Range(11, 8))
            {
                foreach (int j in Enumerable.Range(1, 2)) segments[i, j] = true;
            }
        }

        if (Input.GetKey("[7]"))
        {
            foreach (int i in Enumerable.Range(19, 7))
            {
                foreach (int j in Enumerable.Range(1, 2)) segments[i, j] = true;
            }
        }

        if (Input.GetKey("[4]"))
        {
            foreach (int i in Enumerable.Range(26, 8))
            {
                foreach (int j in Enumerable.Range(1, 2)) segments[i, j] = true;
            }
        }

        if (Input.GetKey("[1]"))
        {
            foreach (int i in Enumerable.Range(34, 7))
            {
                foreach (int j in Enumerable.Range(1, 2)) segments[i, j] = true;
            }
        }

        if (Input.GetKey("[2]"))
        {
            foreach (int i in Enumerable.Range(41, 8))
            {
                foreach (int j in Enumerable.Range(1, 2)) segments[i, j] = true;
            }
        }

        if (Input.GetKey("[3]"))
        {
            foreach (int i in Enumerable.Range(49, 7))
            {
                foreach (int j in Enumerable.Range(1, 2)) segments[i, j] = true;
            }
        }

        // Thank you, ChatGPT
        // Top row (1-6)
        if (Input.GetKey("1")) segments[42, 3] = true;
        if (Input.GetKey("2")) segments[43, 3] = true;
        if (Input.GetKey("3")) segments[44, 3] = true;
        if (Input.GetKey("4")) segments[45, 3] = true;
        if (Input.GetKey("5")) segments[46, 3] = true;
        if (Input.GetKey("6")) segments[47, 3] = true;

        // Second row (q to y, mapping directly below 1-6)
        if (Input.GetKey("q")) segments[42, 2] = true;
        if (Input.GetKey("w")) segments[43, 2] = true;
        if (Input.GetKey("e")) segments[44, 2] = true;
        if (Input.GetKey("r")) segments[45, 2] = true;
        if (Input.GetKey("t")) segments[46, 2] = true;
        if (Input.GetKey("y")) segments[47, 2] = true;

        // Third row (a to h, mapping directly below q to y)
        if (Input.GetKey("a")) segments[42, 1] = true;
        if (Input.GetKey("s")) segments[43, 1] = true;
        if (Input.GetKey("d")) segments[44, 1] = true;
        if (Input.GetKey("f")) segments[45, 1] = true;
        if (Input.GetKey("g")) segments[46, 1] = true;
        if (Input.GetKey("h")) segments[47, 1] = true;

        // Bottom row (z to n, mapping directly below a to h)
        if (Input.GetKey("z")) segments[42, 0] = true;
        if (Input.GetKey("x")) segments[43, 0] = true;
        if (Input.GetKey("c")) segments[44, 0] = true;
        if (Input.GetKey("v")) segments[45, 0] = true;
        if (Input.GetKey("b")) segments[46, 0] = true;
        if (Input.GetKey("n")) segments[47, 0] = true;

        if (useTouchController)
        {
            // LEFT
            if (leftRingPort is not null)
            {
                ReadTouchDataNonBlocking(leftRingPort, leftTouchData);
                for (int angleOffset = 0; angleOffset < 30; angleOffset++)
                {
                    // touchData 0 is at the top, and then increasing CCW
                    int anglePos = SaturnMath.Modulo(angleOffset + 15, 60);

                    for (int depthPos = 0; depthPos < 4; depthPos++)
                    {
                        if (leftTouchData[angleOffset, 3 - depthPos]) segments[anglePos, depthPos] = true;
                    }
                }
            }

            // Right
            if (rightRingPort is not null)
            {
                ReadTouchDataNonBlocking(rightRingPort, rightTouchData);
                for (int angleOffset = 0; angleOffset < 30; angleOffset++)
                {
                    // touchData 0 is at the top, and then increasing CW
                    int anglePos = SaturnMath.Modulo(14 - angleOffset, 60);

                    for (int depthPos = 0; depthPos < 4; depthPos++)
                    {
                        if (rightTouchData[angleOffset, 3 - depthPos]) segments[anglePos, depthPos] = true;
                    }
                }
            }
        }

        CurrentTouchState = new TouchState(segments);
        scoringManager.NewTouchState(CurrentTouchState);
    }


    /// send the unknown ones at your own peril.
    private enum SerialCommand
    {
        // Allow unused members here for now, but reconsider once the input management refactor is a bit more nailed down.
        // ReSharper disable UnusedMember.Local
        NextWrite = 0x20,
        Unknown6 = 0x6F,
        Unknown7 = 0x71,
        NextRead = 0x72,
        BeginWrite = 0x77,
        TouchData = 0x81,
        Unknown4 = 0x91,
        Unknown5 = 0x93,
        SetThresholds = 0x94,
        GetSyncBoardVer = 0xA0,
        Unknown1 = 0xA2,
        UnknownRead = 0xA3,
        GetUnitBoardVer = 0xA8,
        Unknown3 = 0xA9,
        Unknown10 = 0xBC,
        Unknown9 = 0xC0,
        Unknown8 = 0xC1,
        StartAutoScan = 0xC9,
        // ReSharper restore UnusedMember.Local
    }
}

/// <summary>
/// TouchState is an immutable representation of the touch array state.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class TouchState
{
    // Segments is a 2d array:
    // - first index "anglePos": angular segment indicator using polar notation [0, 60)
    //   (0 is on the right, the top is 14-15)
    // - second index "depthPos": forward/backward segment indicator [0, 4), outside to inside
    //   (0 is the outermost segment, 3 is the innermost segment right up against the screen)
    [JsonProperty("_segments")] // legacy Replay compatibility
    private readonly bool[,] segments;

    // Note: parameter name here must match the field name in order for JSON deserialization to work.
    public TouchState([NotNull] [JsonProperty("_segments")] bool[,] segments)
    {
        if (segments.GetLength(0) != 60 || segments.GetLength(1) != 4)
        {
            throw new ArgumentException(
                $"Wrong dimensions for touch segments {segments.GetLength(0)}, {segments.GetLength(1)} (should be 60, 4)");
        }

        this.segments = (bool[,])segments.Clone();
    }

    public bool EqualsSegments([CanBeNull] TouchState other)
    {
        if (other is null) return false;
        // Assume segments are the same size, should be enforced by constructor.
        return Enumerable.Range(0, segments.GetLength(0))
            .All(i => Enumerable.Range(0, segments.GetLength(1))
                .All(j => segments[i, j] == other.segments[i, j]));
    }

    public bool IsPressed(int anglePos, int depthPos)
    {
        return segments[anglePos, depthPos];
    }

    public bool AnglePosPressedAtAnyDepth(int anglePos)
    {
        foreach (int depthPos in Enumerable.Range(0, 4))
        {
            if (IsPressed(anglePos, depthPos)) return true;
        }

        return false;
    }

    /// <summary>
    /// SegmentsPressedSince returns a new TouchState that only marks newly activated segments,
    /// when compared to the provided previous state.
    /// </summary>
    /// <param name="previous"></param>
    /// <returns></returns>
    [NotNull]
    public TouchState SegmentsPressedSince([CanBeNull] TouchState previous)
    {
        // Initializes to all false.
        bool[,] newSegments = new bool[60, 4];
        foreach (int i in Enumerable.Range(0, segments.GetLength(0)))
        foreach (int j in Enumerable.Range(0, segments.GetLength(1)))
        {
            if ((previous is null || !previous.IsPressed(i, j)) && IsPressed(i, j))
                newSegments[i, j] = true;
        }

        return new TouchState(newSegments);
    }
}
}