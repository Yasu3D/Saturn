using System;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Random = System.Random;

namespace SaturnGame
{
/// <summary>
/// <c>TouchState</c> is an immutable reference to a mutable touch data array.
/// <c>TouchState</c> should be thought of as a "container" that references the underlying touch data.<br />
/// Do not use the default constructor (<c>default</c> or <c>new TouchState()</c>) which will create an invalid value.
/// Prefer <see cref="CreateNew"/>.
/// </summary>
/// <remarks>
/// For performance reasons, we avoid recreating the underlying "segment data" (<see cref="SegmentData"/>) - in total
/// the array is 240 bytes (240 booleans at 1B per bool), plus overhead for the object. If the game is running at 500fps
/// (values higher than this have been observed!), then that amounts to 120KB of allocations PER SECOND that the GC has
/// to deal with!
/// <br />
/// However, re-using the <see cref="SegmentData"/> is also somewhat dangerous - some part of the code could store a
/// reference to it, then use that reference on a later frame after the underlying data has been changed.
/// <br />
/// To mitigate the chance of this happening, we use the <see cref="TouchState"/> container, which is basically a
/// reference to the <see cref="SegmentData"/> plus an ID of the current <see cref="SegmentData"/>. Whenever the <see
/// cref="SegmentData"/> is reused, the ID of the <see cref="SegmentData"/> is changed, and stale <see
/// cref="TouchState"/> references will throw an exception as the ID no longer matches. This is _not_ a foolproof
/// protection, as it won't catch issues at compile time, isn't guaranteed to catch issues in runtime, and has
/// thread-safety issues. But it should help.
/// <br />
/// Because <see cref="TouchState"/> is a struct, it is passed by reference. So, if it is passed from class A to class
/// B, changes to the <see cref="TouchState"/> of the class A will not change class B's <see cref="TouchState"/>, and
/// only change the underlying referenced <see cref="SegmentData"/>. This is intentional, and basically the whole point
/// - class B might not expect their <see cref="TouchState"/> to change just because class A updates it. In this case,
/// class B will encounter an exception if it tries to reference the unexpectedly modified touch data. Either class A
/// should pass the updated <see cref="TouchState"/> to class B, or class B should use <see cref="Copy"/> or <see
/// cref="CopyTo"/> to make their own copy of the underlying SegmentData.
/// <br />
/// Warning: Although this makes a best effort to disallow dereferencing modified data, it doesn't do these checks in a
/// threadsafe way and it may still fail due to race conditions. This is a failsafe and it's ultimately up to the caller
/// to handle the data lifetimes correctly.
/// </remarks>
[JsonObject(MemberSerialization.OptIn)]
public readonly struct TouchState
{
    private class SegmentData
    {
        private static readonly Random Random = new();

        // Segments is readonly, and its reference should never escape TouchState, to prevent any modification that
        // does not also set ID.
        // TODO: Consider using a jagged or 1D array as an optimization. Array dereferences into a multi-dimensional
        // array are always method calls and are less performant than jagged or 1D array dereferences.
        // See https://docs.unity3d.com/2023.1/Documentation/Manual/BestPracticeUnderstandingPerformanceInUnity8.html
        [NotNull] public readonly bool[,] Segments = new bool[60, 4];

        // The ID unique identifies the current segment data. If the segment data changes due to reusing the
        // SegmentData object, the ID must be changed.
        public int ID { get; private set; }

        // UpdateId should be called whenever Segments is changed.
        private void UpdateId()
        {
            ID = Random.Next();
        }

        // Initialize a valid empty SegmentData. This will allocate a new Segments array via the default field value.
        public SegmentData()
        {
            // Segments should be initialized to all false.
            UpdateId();
        }

        public void UpdateSegmentData([NotNull] Action<bool[,]> update)
        {
            UpdateId();
            update(Segments);
            UpdateId();
        }

        public void CopySegmentData([NotNull] bool[,] segments)
        {
            UpdateId();
            Array.Copy(segments, Segments, Segments.Length);
            UpdateId();
        }
    }

    #region Internals

    // A reference to the segmentData for this TouchState.
    // Note: this should not be null, but is marked CanBeNull to make sure the default constructor is handled correctly.
    [CanBeNull] private readonly SegmentData segmentData;

    // A local record of the ID of the SegmentData, to ensure that the data has not been modified.
    private readonly int dataId;

    private void CheckDataId()
    {
        if (segmentData is null)
            throw new Exception("TouchState has not been initialized!");
        if (segmentData?.ID != dataId)
            throw new Exception("TouchState data has been modified!");
    }

    /// <summary>
    /// Segments is a 2d array: <br />
    /// - first index "anglePos": angular segment indicator using polar notation [0, 60)
    ///   (0 is on the right, the top is 14-15) <br />
    /// - second index "depthPos": forward/backward segment indicator [0, 4), outside to inside
    ///   (0 is the outermost segment, 3 is the innermost segment right up against the screen)
    /// </summary>
    /// <exception cref="Exception">can be thrown if the TouchState is uninitialized, or the underlying segment data
    /// has been modified</exception>
    [JsonProperty("_segments")] // legacy Replay compatibility
    [NotNull]
    private bool[,] Segments
    {
        get
        {
            CheckDataId();
            return segmentData!.Segments;
        }
    }

    // This creates a new TouchState that reuses the underlying segment data without reallocating it.
    private TouchState([NotNull] SegmentData source)
    {
        segmentData = source;
        dataId = source.ID;
    }

    #endregion

    #region Methods that allocate new touch data

    /// <summary>
    /// Allocates new touch data and copies the provided segments array into it.
    /// </summary>
    /// <remarks>
    /// Because this allocates new <see cref="SegmentData"/>, it should not be used in the game loop (e.g. in
    /// <c>Update()</c>). Prefer allocating new <see cref="SegmentData"/> once during object construction via <see
    /// cref="CreateNew"/>, then using <see cref="StealAndUpdateSegments"/> to re-use the underlying segments array
    /// whenever a new <see cref="TouchState"/> is needed.
    /// </remarks>
    /// <param name="segments">segment data, uses the same layout as <see cref="Segments"/></param>
    /// <exception cref="ArgumentException">thrown if the <paramref name="segments"/> is not a 60x4 array</exception>
    // Note: `[JsonProperty("_segments")]` is needed for JSON deserialization of legacy replays to work. Must match
    // the JsonProperty attribute on `Segments`
    public TouchState([NotNull] [JsonProperty("_segments")] bool[,] segments)
    {
        if (segments.GetLength(0) != 60 || segments.GetLength(1) != 4)
        {
            throw new ArgumentException(
                $"Wrong dimensions for touch segments {segments.GetLength(0)}, {segments.GetLength(1)} (should be 60, 4)");
        }

        segmentData = new SegmentData();
        segmentData.CopySegmentData(segments);
        dataId = segmentData.ID;
    }

    /// <summary>
    /// Creates a correctly initialized empty <see cref="TouchState"/> with newly allocated segment data.
    /// </summary>
    /// <remarks>
    /// Because this allocates new <see cref="SegmentData"/>, it should not be used in the game loop (e.g. in
    /// <c>Update()</c>). This should be called once during object/manager construction, and on future frames, updated
    /// with <see cref="StealAndUpdateSegments"/>, <see cref="CopyTo"/>, or <see cref="WriteSegmentsPressedSince"/>.
    /// </remarks>
    /// <remarks> This should ALWAYS be preferred over the default value <c>default(TouchState)</c> or constructor
    /// <see cref="TouchState()"/>.</remarks>
    /// <returns>The new empty <see cref="TouchState"/></returns>
    public static TouchState CreateNew()
    {
        SegmentData newData = new();
        return new TouchState(newData);
    }

    /// <summary>
    /// Allocates segment data for a new <see cref="TouchState"/>, and copy the segment data from this <see
    /// cref="TouchState"/> into the new one. This ensures that the newly returned <see cref="TouchState"/> will
    /// continue to be valid even after the segment data from this <see cref="TouchState"/> is re-used.
    /// </summary>
    /// <remarks>
    /// Because this allocates new <see cref="SegmentData"/>, it should not be used in the game loop (e.g. in
    /// <c>Update()</c>). Prefer allocating new <see cref="SegmentData"/> once during object construction (using this
    /// method or via <see cref="CreateNew"/>), then using <see cref="CopyTo"/> to re-use the underlying segments array
    /// whenever a new <see cref="TouchState"/> is needed.
    /// </remarks>
    /// <returns>A <see cref="TouchState"/> with newly allocated segment data, which matches the data from this <see
    /// cref="TouchState"/></returns>
    /// <exception cref="ArgumentException">thrown if this <see cref="TouchState"/> is uninitialized</exception>
    /// <exception cref="Exception">can be thrown if the underlying segment data has been modified</exception>
    public TouchState Copy()
    {
        if (segmentData is null)
            throw new ArgumentException("Tried to copy an uninitialized TouchState.");

        return new TouchState(Segments);
    }

    #endregion

    #region Methods that re-use touch data

    /// <summary>
    /// Creates a new <see cref="TouchState"/> that reuses the underlying segment data from <paramref
    /// name="touchState"/> without reallocating it, and reassigns the ref <paramref name="touchState"/> to the newly
    /// created <see cref="TouchState"/>. If the <paramref name="touchState"/> is not initialized, allocate new segment
    /// data.
    /// </summary>
    /// <param name="touchState">Allocated touch data will be reused from here, and the referenced var will be
    /// reassigned to the newly updated <see cref="TouchState"/>. Can be uninitialized.</param>
    /// <param name="updateSegments">Action which writes the new segment data as a 60x4 array (see <see
    /// cref="Segments"/>). The array is not guaranteed to be zeroed, so all values should be written.</param>
    public static void StealAndUpdateSegments(ref TouchState touchState, [NotNull] Action<bool[,]> updateSegments)
    {
        SegmentData data = touchState.segmentData ?? new SegmentData();
        data.UpdateSegmentData(updateSegments);
        touchState = new TouchState(data);
    }

    /// <summary>
    /// Reuses the underlying segment data from <paramref name="dest"/>, copying in the segment data values from this
    /// <see cref="TouchState"/>. If <paramref name="dest"/> is uninitialized, allocated new segment data. Reassigns the
    /// ref <paramref name="dest"/> to the newly created <see cref="TouchState"/>.
    /// </summary>
    /// <param name="dest">the <see cref="TouchState"/> from which we re-use the allocated segment data array. Can be
    /// uninitialized.</param>
    /// <exception cref="Exception">can be thrown if this <see cref="TouchState"/> is uninitialized, or the underlying
    /// segment data has been modified</exception>
    public void CopyTo(ref TouchState dest)
    {
        SegmentData data = dest.segmentData ?? new SegmentData();
        data.CopySegmentData(Segments);
        dest = new TouchState(data);
    }

    /// <summary>
    /// Calculates the newly activated segments in this <see cref="TouchState"/> compared to <paramref
    /// name="previous"/>, writing them into the underlying segment data reused from <paramref name="dest"/>. If
    /// <paramref name="dest"/> is uninitialized, allocate new segment data to use. Reassign the ref <paramref
    /// name="dest"/> to the newly written <see cref="TouchState"/> containing the data for the newly pressed segments.
    /// </summary>
    /// <param name="dest">The reference to the <see cref="TouchState"/> whose underlying data will be reused. Can be
    /// uninitialized.</param>
    /// <param name="previous">The previous touch state to compare against</param>
    /// <exception cref="Exception">can be thrown if this <see cref="TouchState"/> or <paramref name="previous"/> is
    /// uninitialized, or the underlying segment data for either has been modified</exception>
    public void WriteSegmentsPressedSince(ref TouchState dest, TouchState previous)
    {
        TouchState current = this;
        // TODO: avoid capturing current in the lambda to reduce alloc
        StealAndUpdateSegments(ref dest, newSegments =>
        {
            for (int i = 0; i < current.Segments.GetLength(0); i++)
            for (int j = 0; j < current.Segments.GetLength(1); j++)
                newSegments[i, j] = !previous.IsPressed(i, j) && current.IsPressed(i, j);
        });
    }

    #endregion

    #region Logical methods for reading the touch data

    public bool EqualsSegments(TouchState? other)
    {
        if (other is null) return false;
        // Assume segments are the same size, should be enforced by constructor.
        for (int i = 0; i < Segments.GetLength(0); i++)
        for (int j = 0; j < Segments.GetLength(1); j++)
            if (Segments[i, j] != other.Value.Segments[i, j]) return false;

        return true;
    }

    public bool IsPressed(int anglePos, int depthPos)
    {
        return Segments[anglePos, depthPos];
    }

    public bool AnglePosPressedAtAnyDepth(int anglePos)
    {
        for (int depthPos = 0; depthPos < 4; depthPos++)
            if (IsPressed(anglePos, depthPos)) return true;

        return false;
    }

    #endregion
}
}
