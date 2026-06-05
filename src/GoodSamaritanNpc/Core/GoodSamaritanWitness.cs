namespace GoodSamaritanNpc;

public sealed class GoodSamaritanWitness : MonoBehaviour
{
    internal Component Source;
    internal NpcAiController Npc;
    internal double NextReportTime;

    public GoodSamaritanWitness(IntPtr ptr) : base(ptr)
    {
    }

    internal Component SourceOrSelf => !GoodSamaritanManager.IsUnityNull(Source) ? Source : (Component)this;
}
