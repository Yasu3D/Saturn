using System;
using System.Linq;
using FTD2XX_NET;
using JetBrains.Annotations;
using UnityEngine;

namespace SaturnGame
{
public class NativeLedOutput
{
    private readonly FTDI ftdi = new();
    private bool initialized;

    private static void CheckFtStatus(string name, FTDI.FT_STATUS status)
    {
        if (status == FTDI.FT_STATUS.FT_OK)
            return;

        throw new Exception($"got FT_STATUS {status} from {name}");
    }

    private uint Write(byte[] buffer)
    {
        uint bytesWritten = uint.MaxValue;
        FTDI.FT_STATUS status = ftdi.Write(buffer, buffer.Length, ref bytesWritten);
        CheckFtStatus("Write", status);
        if (bytesWritten == uint.MaxValue)
            throw new Exception("Write() returned FT_OK, but didn't set bytesWritten (or set it to uint.MaxValue)");

        // if (bytesWritten != buffer.Length) not sure what to do here

        return bytesWritten;
    }

    [NotNull]
    private byte[] ReadBlocking(int numBytes)
    {
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

        byte[] buffer = new byte[numBytes];
        uint bytesRead = uint.MaxValue;
        CheckFtStatus("Read", ftdi.Read(buffer, (uint)buffer.Length, ref bytesRead));
        if (bytesRead != numBytes)
            throw new Exception($"Failed to read all bytes - only read {bytesRead} out of {numBytes} expected.");

        return buffer;
    }

    private static byte[] ushortTo2Bytes(ushort num)
    {
        // Several commands allow for a number to be represented as ValueL, ValueH. This function implements the conversion.
        return null;
    }

    public void Init()
    {
        uint deviceCount = uint.MaxValue; // Resharper complains about passing uninitialized var as ref.
        FTDI.FT_STATUS getNumberOfDevicesStatus = ftdi.GetNumberOfDevices(ref deviceCount);
        if (getNumberOfDevicesStatus != FTDI.FT_STATUS.FT_OK)
        {
            Debug.LogError($"got FT_STATUS {getNumberOfDevicesStatus} from GetNumberOfDevices");
            return;
        }

        switch (deviceCount)
        {
            case uint.MaxValue:
            {
                throw new Exception(
                    $"GetNumberOfDevices returned FT_OK ({getNumberOfDevicesStatus}) but did not set deviceCount, or " +
                    $"set deviceCount to {uint.MaxValue}");
            }
            case 0:
            {
                Debug.LogError("Found 0 FTDI devices.");
                return;
            }
        }

        Debug.Log($"found {deviceCount} devices");

        FTDI.FT_STATUS openByIndexStatus = ftdi.OpenByIndex(0);
        if (openByIndexStatus != FTDI.FT_STATUS.FT_OK)
        {
            Debug.LogError($"got FT_STATUS {openByIndexStatus} from OpenByIndex(0)");
            return;
        }

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
        byte testBadCommand = 0xAB; // this is not a valid opcode
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
            MpsseCommands.DisableThreePhaseDataClocking
        });
        // There should not be a response.

        // Set clock divisor to "4" - this gives 60MHz / ((1 + 4) * 2) = 6MHz clock speed.
        // See https://www.ftdichip.com/Support/Documents/AppNotes/AN_108_Command_Processor_for_MPSSE_and_MCU_Host_Bus_Emulation_Modes.pdf
        // TODO: use ushortTo2Bytes
        Write(new byte[] { MpsseCommands.SetClockDivisor, 0x04, 0x00 });

        // Configure "low byte" idle states. "Low byte" is the first 8 data lines.
        // We actually don't care about anything except DO (data out), which needs to be set to output and idle low.
        // We set everything to output except DI, and everything idle low except SK (clock).
        // I don't really know if this matters.
        byte outputLines = LowBytePins.SK | LowBytePins.DO | LowBytePins.CS | LowBytePins.GPIOL0 | LowBytePins.GPIOL1 |
                           LowBytePins.GPIOL2 | LowBytePins.GPIOL3;
        byte idleHighLines = LowBytePins.SK;
        Write(new[] { MpsseCommands.SetDataBitsLowByte, idleHighLines, outputLines });


        Debug.Log("SETUP");

        byte zero = 0b11000000;
        byte one = 0b11111100;
        byte[] outputBuffer =
        {
            MpsseCommands.OutputBytesOnPosClockMsb, 24 * 8, (byte)0,

            one, one, one, one, one, one, one, one,
            zero, zero, zero, zero, zero, zero, zero, zero,
            zero, zero, zero, zero, zero, zero, zero, zero,

            one, one, one, one, one, one, one, one,
            zero, zero, zero, zero, zero, zero, zero, zero,
            zero, zero, zero, zero, zero, zero, zero, zero,

            zero, zero, zero, zero, zero, zero, zero, zero,
            one, one, one, one, one, one, one, one,
            zero, zero, zero, zero, zero, zero, zero, zero,

            zero, zero, zero, zero, zero, zero, zero, zero,
            one, one, one, one, one, one, one, one,
            zero, zero, zero, zero, zero, zero, zero, zero,

            zero, zero, zero, zero, zero, zero, zero, zero,
            zero, zero, zero, zero, zero, zero, zero, zero,
            one, one, one, one, one, one, one, one,

            zero, zero, zero, zero, zero, zero, zero, zero,
            zero, zero, zero, zero, zero, zero, zero, zero,
            one, one, one, one, one, one, one, one,

            one, one, one, one, one, one, one, one,
            one, one, one, one, one, one, one, one,
            one, one, one, one, one, one, one, one,
        };
        Write(outputBuffer);
        Debug.Log(BitConverter.ToString(outputBuffer));
    }

    private static class MpsseCommands
    {
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
        public const byte SK = 1 << 0;
        public const byte DO = 1 << 1;
        public const byte DI = 1 << 2;
        public const byte CS = 1 << 3;
        public const byte GPIOL0 = 1 << 4;
        public const byte GPIOL1 = 1 << 5;
        public const byte GPIOL2 = 1 << 6;
        public const byte GPIOL3 = 1 << 7;
    }
}
}
