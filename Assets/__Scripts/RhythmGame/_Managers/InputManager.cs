using System;
using System.Linq;
using UnityEngine;

namespace SaturnGame.RhythmGame
{

    public class InputManager : MonoBehaviour
    {
        [Header("MANAGERS")]
        [SerializeField] private ScoringManager scoringManager;
        [SerializeField] private BgmManager bgmManager;

        public TouchState CurrentTouchState;

        // Start is called before the first frame update
        void Start()
        {
            return;
        }

        // Update is called once per frame
        void Update()
        {
            if (!bgmManager.bgmPlayer.isPlaying) return;

			// Initializes to all false.
            var segments = new bool[60, 4];
            if (Input.GetKey("[6]"))
            {
                foreach (int i in Enumerable.Range(56, 4))
                {
                    foreach (int j in Enumerable.Range(1, 2))
                    {
                        segments[i, j] = true;
                    }
                }
                foreach (int i in Enumerable.Range(0, 4))
                {
                    foreach (int j in Enumerable.Range(1, 2))
                    {
                        segments[i, j] = true;
                    }
                }
            }
            if (Input.GetKey("[9]"))
            {
                foreach (int i in Enumerable.Range(3, 7))
                {
                    foreach (int j in Enumerable.Range(1, 2))
                    {
                        segments[i, j] = true;
                    }
                }
            }
            if (Input.GetKey("[8]"))
            {
                foreach (int i in Enumerable.Range(11, 8))
                {
                    foreach (int j in Enumerable.Range(1, 2))
                    {
                        segments[i, j] = true;
                    }
                }
            }
            if (Input.GetKey("[7]"))
            {
                foreach (int i in Enumerable.Range(19, 7))
                {
                    foreach (int j in Enumerable.Range(1, 2))
                    {
                        segments[i, j] = true;
                    }
                }
            }
            if (Input.GetKey("[4]"))
            {
                foreach (int i in Enumerable.Range(26, 8))
                {
                    foreach (int j in Enumerable.Range(1, 2))
                    {
                        segments[i, j] = true;
                    }
                }
            }
            if (Input.GetKey("[1]"))
            {
                foreach (int i in Enumerable.Range(34, 7))
                {
                    foreach (int j in Enumerable.Range(1, 2))
                    {
                        segments[i, j] = true;
                    }
                }
            }
            if (Input.GetKey("[2]"))
            {
                foreach (int i in Enumerable.Range(41, 8))
                {
                    foreach (int j in Enumerable.Range(1, 2))
                    {
                        segments[i, j] = true;
                    }
                }
            }
            if (Input.GetKey("[3]"))
            {
                foreach (int i in Enumerable.Range(49, 7))
                {
                    foreach (int j in Enumerable.Range(1, 2))
                    {
                        segments[i, j] = true;
                    }
                }
            }

            CurrentTouchState = new TouchState(segments);
            scoringManager.NewTouchState(CurrentTouchState);
        }
    }

    /// <summary>
    /// TouchState is an immutable representation of the touch array state.
    /// </summary>
    public class TouchState
    {
        // Segments is a 2d array:
        // - first index "rotation": rotational segment indicator using polar notation [0, 60)
        //   (0 is on the right, the top is 14-15)
        // - second index "depth": forward/backward segment indicator [0, 4), outside to inside
        //   (0 is the outermost segment, 3 is the innermost segment right up against the screen)
        private readonly bool[,] _segments;

        public TouchState(bool[,] segments)
        {
            if (segments.GetLength(0) != 60 || segments.GetLength(1) != 4)
            {
                throw new ArgumentException($"Wrong dimensions for touch segments {segments.GetLength(0)}, {segments.GetLength(1)} (should be 60, 4)");
            }
            _segments = (bool[,])segments.Clone();
        }

        public bool EqualsSegments(TouchState other)
        {
            if (other is null)
            {
                return false;
            }
            foreach (int i in Enumerable.Range(0, _segments.GetLength(0)))
            {
                foreach (int j in Enumerable.Range(0, _segments.GetLength(1)))
                {
                    if (_segments[i, j] != other._segments[i, j])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public bool IsPressed(int rotation, int depth)
        {
            return _segments[rotation, depth];
        }

		public bool RotationPressedAtAnyDepth(int rotation)
		{
            foreach (int depth in Enumerable.Range(0, 4))
			{
				if (IsPressed(rotation, depth))
				{
                    return true;
                }
			}
            return false;
        }

		/// <summary>
		/// SegmentsPressedSince returns a new TouchState that only marks newly activated segments,
		/// when compared to the provided previous state.
		/// <summary>
		public TouchState SegmentsPressedSince(TouchState previous) {
			// Initializes to all false.
            bool[,] segments = new bool[60, 4];
            foreach (int i in Enumerable.Range(0, _segments.GetLength(0)))
            {
                foreach (int j in Enumerable.Range(0, _segments.GetLength(1)))
                {
					if (previous is null || (IsPressed(i, j) && !previous.IsPressed(i, j)))
                    {
                        segments[i, j] = true;
                    }
                }
            }
            return new TouchState(segments);
        }
    }
}
