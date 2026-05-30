namespace GoodSamaritanNpc;

public sealed class GoodSamaritanWitness : MonoBehaviour
{
    internal NpcAiController Npc;
    internal double NextReportTime;

    public GoodSamaritanWitness(IntPtr ptr) : base(ptr)
    {
    }
}
