using UnityEngine.EventSystems;

namespace SaturnGame.UI
{
/// <summary>
/// A version of StandaloneInputModule that can also utilize the touch ring to press buttons.
/// See also https://gist.github.com/stramit/76e53efd67a2e1cf3d2f
/// </summary>
public class TouchRingStandaloneInputModule : StandaloneInputModule
{
    private TouchState currentTouchState = TouchState.CreateNew();
    private TouchState previousTouchState = TouchState.CreateNew();

    // UpdateModule is called every tick, for each input module, regardless of whether it's active.
    // We should track the input method state here.
    public override void UpdateModule()
    {
        // Swap previous and current. (This avoid having to newly allocate anything.)
        (previousTouchState, currentTouchState) = (currentTouchState, previousTouchState);
        // Update current.
        TouchRingManager.Instance.CurrentTouchState.CopyTo(ref currentTouchState);

        base.UpdateModule();
    }

    // Process is called whenever the input module is active and triggers events based on the input state.
    public override void Process()
    {
        // iterate through all possible touchables
        foreach (ITouchable touchable in TouchRegistry.RegisteredTouchables)
        {
            bool previouslyTouched = touchable.Touched(previousTouchState);
            bool currentlyTouched = touchable.Touched(currentTouchState);
            switch (currentlyTouched)
            {
                case true when !previouslyTouched:
                    // No need to use ExecuteEvents.Execute as we already have the specific component we need.
                    touchable.OnTouchPress();
                    break;
                case false when previouslyTouched:
                    // No need to use ExecuteEvents.Execute as we already have the specific component we need.
                    touchable.OnTouchRelease();
                    break;
            }
        }

        base.Process();
    }
}
}
