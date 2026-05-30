namespace GoodSamaritanNpc;

public sealed class GoodSamaritanMarker : MonoBehaviour
{
    private static AudioClip alertClip;

    private TextMesh textMesh;
    private GameObject markerObject;
    private AudioSource audioSource;
    private float hideAt;
    private float nextAudioTime;

    public GoodSamaritanMarker(IntPtr ptr) : base(ptr)
    {
    }

    public void Awake()
    {
        EnsureVisual();
    }

    public void Update()
    {
        if (markerObject == null)
        {
            return;
        }

        bool visible = Time.time < hideAt;
        if (markerObject.activeSelf != visible)
        {
            markerObject.SetActive(visible);
        }

        if (!visible)
        {
            return;
        }

        var cam = Camera.main;
        if (cam != null)
        {
            markerObject.transform.rotation = Quaternion.LookRotation(markerObject.transform.position - cam.transform.position);
        }

        float pulse = 1f + Mathf.Sin(Time.time * 9f) * 0.12f;
        markerObject.transform.localScale = new Vector3(pulse, pulse, pulse);
    }

    [HideFromIl2Cpp]
    internal void Show(float seconds, bool playVoice)
    {
        if (!GoodSamaritanPlugin.Settings.EnableCustomClientMarker.Value)
        {
            return;
        }

        EnsureVisual();
        hideAt = Mathf.Max(hideAt, Time.time + Mathf.Max(0.5f, seconds));
        markerObject?.SetActive(true);

        if (playVoice && GoodSamaritanPlugin.Settings.EnableVoiceLine.Value && Time.time >= nextAudioTime)
        {
            EnsureAudio();
            audioSource?.PlayOneShot(GetAlertClip(), 0.65f);
            nextAudioTime = Time.time + 3f;
        }
    }

    internal static void ShowOn(NpcAiController npc, float seconds, bool playVoice)
    {
        if (GoodSamaritanManager.IsUnityNull(npc))
        {
            return;
        }

        ShowOn((Component)npc!, seconds, playVoice);
    }

    internal static void ShowOn(Component component, float seconds, bool playVoice)
    {
        if (!GoodSamaritanPlugin.Settings.EnableCustomClientMarker.Value || GoodSamaritanManager.IsUnityNull(component))
        {
            return;
        }

        var go = component!.gameObject;
        if (GoodSamaritanManager.IsUnityNull(go))
        {
            return;
        }

        var marker = go.GetComponent<GoodSamaritanMarker>();
        if (GoodSamaritanManager.IsUnityNull(marker))
        {
            marker = go.AddComponent<GoodSamaritanMarker>();
        }

        marker!.Show(seconds, playVoice);
    }

    [HideFromIl2Cpp]
    private void EnsureVisual()
    {
        if (markerObject != null)
        {
            return;
        }

        markerObject = new GameObject("GoodSamaritanExclamation");
        markerObject.transform.SetParent(((Component)this).transform, false);
        markerObject.transform.localPosition = new Vector3(0f, 2.55f, 0f);
        markerObject.transform.localScale = Vector3.one;

        textMesh = markerObject.AddComponent<TextMesh>();
        textMesh.text = "!";
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.fontSize = 96;
        textMesh.characterSize = 0.08f;
        textMesh.color = new Color(1f, 0.18f, 0.03f, 1f);
        markerObject.SetActive(false);
    }

    [HideFromIl2Cpp]
    private void EnsureAudio()
    {
        if (audioSource != null)
        {
            return;
        }

        audioSource = ((Component)this).gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = ((Component)this).gameObject.AddComponent<AudioSource>();
        }

        audioSource.spatialBlend = 1f;
        audioSource.minDistance = 2f;
        audioSource.maxDistance = 18f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
    }

    private static AudioClip GetAlertClip()
    {
        if (alertClip != null)
        {
            return alertClip;
        }

        const int sampleRate = 22050;
        const float duration = 0.45f;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)sampleRate;
            float env = Mathf.Clamp01(t / 0.04f) * Mathf.Clamp01((duration - t) / 0.08f);
            float freq = 420f + Mathf.Sin(t * 31f) * 65f;
            float tone = Mathf.Sin(2f * Mathf.PI * freq * t) + 0.35f * Mathf.Sin(2f * Mathf.PI * freq * 2.02f * t);
            samples[i] = tone * env * 0.18f;
        }

        alertClip = AudioClip.Create("GoodSamaritanWitnessAlert", sampleCount, 1, sampleRate, false);
        alertClip.SetData(samples, 0);
        return alertClip;
    }
}
