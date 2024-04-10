using System.Collections.Generic;
using JetBrains.Annotations;
using SaturnGame.UI;

public class TouchRegistry
{
    private static TouchRegistry instance;
    [NotNull] public static TouchRegistry Instance {
        get { return instance ??= new TouchRegistry(); }
    }

    private readonly HashSet<ITouchable> touchables;

    private TouchRegistry()
    {
        touchables = new HashSet<ITouchable>();
    }

    public static void RegisterTouchable(ITouchable touchable)
    {
        Instance.touchables.Add(touchable);
    }

    public static void UnregisterTouchable(ITouchable touchable)
    {
        Instance.touchables.Remove(touchable);
    }

    public static IEnumerable<ITouchable> RegisteredTouchables => Instance.touchables;
}
