namespace GoodSamaritanNpc;

public sealed class GoodSamaritanHighlightTarget : MonoBehaviour
{
    private Outline outline;
    private float hideAt;

    public GoodSamaritanHighlightTarget(IntPtr ptr) : base(ptr)
    {
    }

    public void Update()
    {
        if (outline == null)
        {
            return;
        }

        if (Time.time <= hideAt)
        {
            return;
        }

        outline.enabled = false;
        enabled = false;
    }

    [HideFromIl2Cpp]
    internal void Show(Color color, float seconds, float width)
    {
        EnsureOutline();
        if (outline == null)
        {
            return;
        }

        outline.OutlineMode = Outline.Mode.OutlineAll;
        outline.OutlineColor = color;
        outline.OutlineWidth = width;
        outline.enabled = true;
        hideAt = Mathf.Max(hideAt, Time.time + Mathf.Max(0.25f, seconds));
        enabled = true;
    }

    [HideFromIl2Cpp]
    private void EnsureOutline()
    {
        if (outline != null)
        {
            return;
        }

        var go = ((Component)this).gameObject;
        outline = go.AddComponent<Outline>();
        outline.enabled = false;
    }
}
