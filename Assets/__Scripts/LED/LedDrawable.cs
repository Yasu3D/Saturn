using JetBrains.Annotations;
using UnityEngine;

namespace SaturnGame.LED
{
/// <summary>
/// A single draw operation. Any drawing code must go in Draw().
/// </summary>
/// <remarks>
/// A LedDrawable will automatically add itself to the <c>LedDrawableQueue</c> when enabled.
/// </remarks>
public abstract class LedDrawable : MonoBehaviour
{
    // If Compositor is null, we will write to the BaseLedCompositor instead.
    [CanBeNull] [SerializeField] private LedCompositor compositor;
    [CanBeNull] private LedCompositor Compositor => compositor ?? LedManager.Instance?.BaseLedCompositor;
    public int Layer;

    [CanBeNull]
    public abstract Color[,] Draw();

    public void SetCompositor(LedCompositor newCompositor)
    {
        if (isActiveAndEnabled)
        {
            Debug.LogWarning("set compositor on activated led drawable");
            Compositor?.LedDrawables.Remove(this);
            newCompositor.LedDrawables.Add(this);
        }
        compositor = newCompositor;
    }

    // OnEnable is called whenever the object becomes enabled.
    // It is not guaranteed to be called after OnEnable or Awake of any other object.
    // https://docs.unity3d.com/Manual/ExecutionOrder.html:
    // > Awake is only guaranteed to be called before OnEnable in the scope of each individual object. Across multiple
    // > objects the order is not deterministic and you can’t rely on one object’s Awake being called before another
    // > object’s OnEnable. Any work that depends on Awake having been called for all objects in the scene should be
    // > done in Start.
    // This is a problem because LedManager.Instance will not be set until LedManager.Awake is called.
    // Hence we need to also use Start() - see below.
    private void OnEnable()
    {
        Compositor?.LedDrawables.Add(this);
    }

    // Start is only called once in the lifetime of the behaviour.
    // However, unlike OnEnable, it is guaranteed to be called after LedManager.Awake. See above comment on OnEnable.
    private void Start()
    {
        if (!Compositor?.LedDrawables.Contains(this) ?? false) Compositor.LedDrawables.Add(this);
    }

    private void OnDisable()
    {
        Compositor?.LedDrawables.Remove(this);
    }
}
}
