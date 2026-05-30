namespace GoodSamaritanNpc;

internal static class GoodSamaritanClientAlertGate
{
    private static float allowQuestionMarkerUntil;
    private static float allowAreaIndicatorUntil;
    private static float directReportUntil;
    private static bool witnessSourceIndicatorConsumed;

    internal static void NoteLogMessage(string message)
    {
        if (!string.IsNullOrEmpty(message) && GoodSamaritanText.IsWitnessLog(message))
        {
            allowQuestionMarkerUntil = Time.time + 3f;
            witnessSourceIndicatorConsumed = false;
            if (GoodSamaritanText.IsDirectReportLog(message))
            {
                directReportUntil = Time.time + 3f;
                allowAreaIndicatorUntil = 0f;
            }
            else
            {
                allowAreaIndicatorUntil = Time.time + 3f;
                directReportUntil = 0f;
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

    internal static bool ConsumeNpcQuestionAsWitnessSource()
    {
        if (!ShouldEnhanceQuestionIndicator())
        {
            return false;
        }

        witnessSourceIndicatorConsumed = true;
        return true;
    }

    internal static bool ConsumePlayerQuestionAsWitnessSource()
    {
        if (!ShouldEnhanceQuestionIndicator() || witnessSourceIndicatorConsumed)
        {
            return false;
        }

        if (Time.time <= allowAreaIndicatorUntil || Time.time <= directReportUntil)
        {
            witnessSourceIndicatorConsumed = true;
            return true;
        }

        return false;
    }
}
