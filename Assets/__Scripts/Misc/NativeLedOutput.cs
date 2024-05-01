using System;
using System.Linq;
using FTD2XX_NET;
using JetBrains.Annotations;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace SaturnGame
{
public class NativeLedOutput
{
    private readonly FTDI ftdi = new();
    private bool initialized;

    // Cabinet FT232H device has usb ID 0403:6014.
    private const uint LedDeviceId = 0x04036014;

    private const byte Zero = 0b11000000;
    private const byte One = 0b11111100;

    // byte value to array of 8 bytes that can be sent to bitbang the ws281x
    private readonly byte[][] bitBangLookup = new byte[256][];

    // BitBangBytesSizeInBytes represents how many bytes are required to bitbang a single byte. Theoretically it may
    // be possible to use fewer bytes, for instance by using a slower clock and merging multiple bitbanged bits into a
    // single byte representation. (E.g. 10001110 could be 0b01 at 3MHz. In that case, this value would change to 4.)
    private const ushort BitBangByteSizeInBytes = 8;

    public NativeLedOutput()
    {
        // Turn on checked arithmetic - we are only doing this once and accidental overflows are easy to make.
        checked
        {
            for (int byteValue = 0; byteValue < 256; byteValue++)
            {
                // This is the destination for the 64 bits that will represent this bit value in the ws281x protocol.
                byte[] bitBangValues = new byte[BitBangByteSizeInBytes];
                for (int bit = 0; bit < 8; bit++)
                {
                    // Each bit in the byte needs to be represented as Zero or One. This is confusing because Zero and One
                    // are "bytes" - but you should think of them as sequences of 8 bits that represent the corresponding
                    // value (0 or 1) in the ws281x protocol. bit = 0 is the most significant bit as MSB should be first
                    // according to the ws281x spec.

                    // bitMask has a 1 in the bit column we want to check and a 0 everywhere else.
                    // e.g. 0b00100000 for bit = 2 or 0b00000001 for bit = 7
                    byte bitMask = (byte)(1 << (7 - bit));

                    if ((byteValue & bitMask) == 0)
                        bitBangValues[bit] = Zero;
                    else
                        bitBangValues[bit] = One;
                }

                bitBangLookup[byteValue] = bitBangValues;
            }
        }
    }

    private static void CheckFtStatus(string name, FTDI.FT_STATUS status)
    {
        if (status == FTDI.FT_STATUS.FT_OK)
            return;

        throw new Exception($"got FT_STATUS {status} from {name}");
    }

    private void Write([NotNull] byte[] buffer)
    {
        uint bytesWritten = uint.MaxValue;
        FTDI.FT_STATUS status = ftdi.Write(buffer, buffer.Length, ref bytesWritten);
        CheckFtStatus("Write", status);
        if (bytesWritten == uint.MaxValue)
            throw new Exception("Write() returned FT_OK, but didn't set bytesWritten (or set it to uint.MaxValue)");

        if (bytesWritten != buffer.Length) Debug.LogError($"wrong number of bytes written {bytesWritten}");
    }

    [NotNull]
    private byte[] ReadBlocking(int numBytes)
    {
        // ReSharper disable GrammarMistakeInComment CommentTypo
        // Notes from ftd2xx.h:
        // - This function does not return until dwBytesToRead bytes have been read into the buffer. The number of
        // bytes in the receive queue can be determined by calling FT_GetStatus or FT_GetQueueStatus, and
        // passed to FT_Read as dwBytesToRead so that the function reads the device and returns immediately.
        // When a read timeout value has been specified in a previous call to FT_SetTimeouts, FT_Read returns
        // when the timer expires or dwBytesToRead have been read, whichever occurs first. If the timeout
        // occurred, FT_Read reads available data into the buffer and returns FT_OK.
        // - An application should use the function return value and lpdwBytesReturned when processing the buffer.
        // If the return value is FT_OK, and lpdwBytesReturned is equal to dwBytesToRead then FT_Read has
        // completed normally. If the return value is FT_OK, and lpdwBytesReturned is less then dwBytesToRead
        // then a timeout has occurred and the read has been partially completed. Note that if a timeout occurred
        // and no data was read, the return value is still FT_OK.
        // - A return value of FT_IO_ERROR suggests an error in the parameters of the function, or a fatal error like a
        // USB disconnect has occurred.
        // ReSharper restore GrammarMistakeInComment CommentTypo

        byte[] buffer = new byte[numBytes];
        uint bytesRead = uint.MaxValue;
        CheckFtStatus("Read", ftdi.Read(buffer, (uint)buffer.Length, ref bytesRead));
        if (bytesRead != numBytes)
            throw new Exception($"Failed to read all bytes - only read {bytesRead} out of {numBytes} expected.");

        return buffer;
    }

    /// <summary>
    /// Several commands allow for a number to be represented as ValueL, ValueH. This function implements the conversion
    /// </summary>
    /// <param name="number">input number to convert to two bytes</param>
    /// <param name="low">"low" byte (8 LSB)</param>
    /// <param name="high">"high" byte (8 MSB)</param>
    private static void UshortTo2Bytes(ushort number, out byte low, out byte high)
    {
        low = (byte)(number % 256);
        high = (byte)(number / 256);
    }

    public void Init()
    {
        uint deviceCount = uint.MaxValue; // Resharper complains about passing uninitialized var as ref.
        CheckFtStatus("GetNumberOfDevices", ftdi.GetNumberOfDevices(ref deviceCount));
        switch (deviceCount)
        {
            case uint.MaxValue:
            {
                throw new Exception(
                    $"GetNumberOfDevices returned FT_OK but did not set deviceCount, or set deviceCount to {uint.MaxValue}");
            }
            case 0:
            {
                Debug.LogError("Found 0 FTDI devices.");
                return;
            }
        }

        Debug.Log($"found {deviceCount} devices");

        // Warning: untested
        FTDI.FT_DEVICE_INFO_NODE[] deviceList = new FTDI.FT_DEVICE_INFO_NODE[deviceCount];
        CheckFtStatus("GetDeviceList", ftdi.GetDeviceList(deviceList));

        uint? deviceLocation = null;
        foreach (FTDI.FT_DEVICE_INFO_NODE deviceInfo in deviceList)
        {
            if (deviceInfo.ID == LedDeviceId)
                deviceLocation = deviceInfo.LocId;
        }

        if (deviceLocation == null)
        {
            Debug.LogError("Couldn't find a device with the correct VID:PID");
            return;
        }

        CheckFtStatus("OpenByLocation", ftdi.OpenByLocation(deviceLocation.Value));

        // Setup MPSSE
        // Reset device
        CheckFtStatus("ResetDevice", ftdi.ResetDevice());
        // Purge RX and TX buffers
        CheckFtStatus("Purge", ftdi.Purge(FTDI.FT_PURGE.FT_PURGE_RX | FTDI.FT_PURGE.FT_PURGE_TX));
        // Set USB IN/OUT transfer size
        CheckFtStatus("InTransferSize", ftdi.InTransferSize(65536));
        // Disable Event/Error chars
        CheckFtStatus("SetChars", ftdi.SetCharacters(0, false, 0, false));
        // Set read/write timeouts to 5s
        CheckFtStatus("SetTimeouts", ftdi.SetTimeouts(5_000, 5_000));
        // Set latency to 1ms
        CheckFtStatus("SetLatency", ftdi.SetLatency(1));
        // Reset controller and set to MPSSE mode
        CheckFtStatus("SetBitMode(RESET)", ftdi.SetBitMode(0, FTDI.FT_BIT_MODES.FT_BIT_MODE_RESET));
        CheckFtStatus("SetBitMode(MPSSE)", ftdi.SetBitMode(0, FTDI.FT_BIT_MODES.FT_BIT_MODE_MPSSE));

        // Test that device is behaving as expected.
        const byte testBadCommand = 0xAB; // this is not a valid opcode
        Write(new[] { testBadCommand });
        // The device should respond with BadCommand, then echo back the invalid command
        byte[] badCommandResponse = ReadBlocking(2);
        byte[] expected = { MpsseCommands.BadCommand, testBadCommand };
        if (!badCommandResponse.SequenceEqual(expected))
        {
            Debug.LogError($"misbehaving device, got back {BitConverter.ToString(badCommandResponse)}");
            return;
        }

        // Disable divide by 5 (use 60MHz master clock, not 12MHz)
        // Disable adaptive clocking
        // Disable three-phase clocking
        Write(new[]
        {
            MpsseCommands.DisableClockDivideBy5, MpsseCommands.DisableAdaptiveClocking,
            MpsseCommands.DisableThreePhaseDataClocking,
        });
        // There should not be a response.

        // Set clock divisor to "4" - this gives 60MHz / ((1 + 4) * 2) = 6MHz clock speed.
        // ReSharper disable once CommentTypo
        // See https://www.ftdichip.com/Support/Documents/AppNotes/AN_108_Command_Processor_for_MPSSE_and_MCU_Host_Bus_Emulation_Modes.pdf
        byte[] clockDivisorCommand = new byte[3];
        clockDivisorCommand[0] = MpsseCommands.SetClockDivisor;
        UshortTo2Bytes(4, out clockDivisorCommand[1], out clockDivisorCommand[2]);
        Write(clockDivisorCommand);

        // Configure "low byte" idle states. "Low byte" is the first 8 data lines.
        // We actually don't care about anything except DO (data out), which needs to be set to output and idle low.
        // We set everything to output except DI, and everything idle low except SK (clock).
        // I don't really know if this matters.
        const byte outputLines = LowBytePins.SK | LowBytePins.DO | LowBytePins.CS | LowBytePins.GPIOL0 |
                                 LowBytePins.GPIOL1 | LowBytePins.GPIOL2 | LowBytePins.GPIOL3;
        const byte idleHighLines = LowBytePins.SK;
        Write(new[] { MpsseCommands.SetDataBitsLowByte, idleHighLines, outputLines });

        // Skip setting "high byte" idle states and directions, these are unused.

        initialized = true;
    }

    private void WriteBitBangByteToBuffer([NotNull] byte[] buffer, int offset, byte value)
    {
        byte[] byteValue = bitBangLookup[value];
        for (int i = 0; i < byteValue.Length; i++) buffer[offset + i] = byteValue[i];
    }

    public void SetLeds(Color32[] colors)
    {
        if (!initialized) return;

        // 3 bytes per led (G, R, B).
        const int bitBangByteSizePerLed = 3 * BitBangByteSizeInBytes;

        if (colors.Length * bitBangByteSizePerLed > ushort.MaxValue)
        {
            throw new ArgumentException(
                $"Too many LEDS - {colors.Length} results in {colors.Length * bitBangByteSizePerLed} bytes to send");
        }

        ushort bitBangSize = (ushort)(colors.Length * bitBangByteSizePerLed);

        // 3 bytes (command and size low, size high), then the bitbang values.
        byte[] outputBuffer = new byte[3 + bitBangSize];

        // Write command and length data. (Output command, length low byte, length high byte.)
        outputBuffer[0] = MpsseCommands.OutputBytesOnPosClockMsb;
        UshortTo2Bytes(bitBangSize, out outputBuffer[1], out outputBuffer[2]);

        for (int i = 0; i < colors.Length; i++)
        {
            // Note that this is GRB, not RGB. See ws2813 datasheet.
            WriteBitBangByteToBuffer(outputBuffer, 3 + i * bitBangByteSizePerLed, colors[i].g);
            WriteBitBangByteToBuffer(outputBuffer, 3 + i * bitBangByteSizePerLed + BitBangByteSizeInBytes, colors[i].r);
            WriteBitBangByteToBuffer(outputBuffer, 3 + i * bitBangByteSizePerLed + BitBangByteSizeInBytes * 2,
                colors[i].b);
        }

        Write(outputBuffer);
    }

    public void Destroy()
    {
        ftdi.Dispose();
    }

    private static class MpsseCommands
    {
        // ReSharper disable once CommentTypo
        // See https://www.ftdichip.com/Support/Documents/AppNotes/AN_108_Command_Processor_for_MPSSE_and_MCU_Host_Bus_Emulation_Modes.pdf
        public const byte OutputBytesOnPosClockMsb = 0x10;
        public const byte SetDataBitsLowByte = 0x80;
        public const byte SetClockDivisor = 0x86;
        public const byte DisableClockDivideBy5 = 0x8A;
        public const byte DisableThreePhaseDataClocking = 0x8D;
        public const byte DisableAdaptiveClocking = 0x97;
        public const byte BadCommand = 0xFA;
    }

    private static class LowBytePins
    {
        // ReSharper disable InconsistentNaming, IdentifierTypo, UnusedMember.Local
        public const byte SK = 1 << 0;
        public const byte DO = 1 << 1;
        public const byte DI = 1 << 2;
        public const byte CS = 1 << 3;
        public const byte GPIOL0 = 1 << 4;
        public const byte GPIOL1 = 1 << 5;
        public const byte GPIOL2 = 1 << 6;
        public const byte GPIOL3 = 1 << 7;

        // ReSharper restore InconsistentNaming, IdentifierTypo, UnusedMember.Local
    }
}
}
