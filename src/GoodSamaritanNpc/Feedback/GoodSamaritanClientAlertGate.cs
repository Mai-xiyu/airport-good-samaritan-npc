namespace GoodSamaritanNpc;

internal static class GoodSamaritanClientAlertGate
{
    private static float allowQuestionMarkerUntil;
    private static float allowAreaIndicatorUntil;

    internal static void NoteLogMessage(string message)
    {
        if (!string.IsNullOrEmpty(message) && GoodSamaritanText.IsWitnessLog(message))
        {
            allowQuestionMarkerUntil = Time.time + 3f;
            if (!GoodSamaritanText.IsDirectReportLog(message))
            {
                allowAreaIndicatorUntil = Time.time + 3f;
            }
        }
    }

    internal static bool ShouldEnhanceQuestionIndicator()
    {
        return Time.time <= allowQuestionMarkerUntil;
    }

    internal static bool ShouldEnhanceAreaIndicator()
    {
        return Time.time <= allowAreaIndicatorUntil;
    }
}
