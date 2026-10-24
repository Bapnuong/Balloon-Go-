using System;
using System.Collections.Generic;
using UnityEngine;

public enum Sfx
{
    Flap,
    Coin,
    Hit,
    Click,
    Buy,
    Error,
    Record,
    Tick
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    const int SampleRate = 44100;

    public bool SoundEnabled { get; private set; } = true;

    AudioSource sfxSource;
    AudioSource uiSource;
    AudioSource ambienceSource;
    readonly Dictionary<Sfx, AudioClip> clips = new Dictionary<Sfx, AudioClip>();

    void Awake()
    {
        Instance = this;
        SoundEnabled = SaveSystem.LoadSound();
        AudioListener.volume = SoundEnabled ? 1f : 0f;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 0f;

        uiSource = gameObject.AddComponent<AudioSource>();
        uiSource.playOnAwake = false;
        uiSource.spatialBlend = 0f;
        uiSource.volume = 0.8f;

        ambienceSource = gameObject.AddComponent<AudioSource>();
        ambienceSource.playOnAwake = false;
        ambienceSource.spatialBlend = 0f;
        ambienceSource.loop = true;
        ambienceSource.volume = 0.1f;

        BuildClips();
        StartAmbience();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Play(Sfx id, float volume = 1f)
    {
        if (!SoundEnabled || sfxSource == null) return;
        AudioClip clip;
        if (clips.TryGetValue(id, out clip))
            sfxSource.PlayOneShot(clip, volume);
    }

    public void PlayClick() => Play(Sfx.Click, 0.45f);

    public void PlayUi(Sfx id, float volume = 0.7f)
    {
        if (!SoundEnabled || uiSource == null) return;
        AudioClip clip;
        if (clips.TryGetValue(id, out clip))
            uiSource.PlayOneShot(clip, volume);
    }

    public void SetSoundEnabled(bool enabled)
    {
        SoundEnabled = enabled;
        AudioListener.volume = enabled ? 1f : 0f;
        SaveSystem.SaveSound(enabled);
    }

    public bool ToggleSound()
    {
        SetSoundEnabled(!SoundEnabled);
        if (SoundEnabled) PlayClick();
        return SoundEnabled;
    }

    public void SetPaused(bool paused)
    {
        if (ambienceSource != null)
            ambienceSource.volume = paused ? 0.03f : 0.1f;
    }

    void StartAmbience()
    {
        var clip = MakeAmbience();
        ambienceSource.clip = clip;
        ambienceSource.Play();
    }

    void BuildClips()
    {
        clips[Sfx.Flap] = MakeFlap();
        clips[Sfx.Coin] = MakeCoin();
        clips[Sfx.Hit] = MakeHit();
        clips[Sfx.Click] = MakeClick();
        clips[Sfx.Buy] = MakeBuy();
        clips[Sfx.Error] = MakeError();
        clips[Sfx.Record] = MakeRecord();
        clips[Sfx.Tick] = MakeTick();
    }

    // ---------- clip builders ----------
    static AudioClip Build(string name, float seconds, Func<float, float> sample)
    {
        int count = Mathf.Max(1, Mathf.RoundToInt(seconds * SampleRate));
        var data = new float[count];
        float peak = 0.0001f;
        for (int i = 0; i < count; i++)
        {
            float v = sample(i / (float)SampleRate);
            data[i] = v;
            float a = Mathf.Abs(v);
            if (a > peak) peak = a;
        }
        float gain = 0.82f / peak;
        for (int i = 0; i < count; i++)
            data[i] *= gain;

        var clip = AudioClip.Create(name, count, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    static float Env(float t, float attack, float decay)
    {
        float a = attack <= 0f ? 1f : Mathf.Clamp01(t / attack);
        return a * Mathf.Exp(-t * decay);
    }

    AudioClip MakeFlap()
    {
        float phase = 0f;
        float lp = 0f;
        var rng = new System.Random(7);
        return Build("flap", 0.15f, t =>
        {
            float n = (float)(rng.NextDouble() * 2.0 - 1.0);
            lp += (n - lp) * 0.18f;
            float f = Mathf.Lerp(180f, 520f, Mathf.Clamp01(t / 0.15f));
            phase += 2f * Mathf.PI * f / SampleRate;
            float env = Env(t, 0.004f, 26f);
            return (Mathf.Sin(phase) * 0.35f + lp * 0.45f) * env;
        });
    }

    AudioClip MakeCoin()
    {
        return Build("coin", 0.32f, t =>
        {
            float f1 = t < 0.09f ? 987.77f : 1318.51f;
            float env = Env(t, 0.003f, 11f);
            float shimmer = Mathf.Sin(2f * Mathf.PI * 2637f * t) * 0.12f * Env(t, 0.001f, 30f);
            return (Mathf.Sin(2f * Mathf.PI * f1 * t) * 0.55f + shimmer) * env;
        });
    }

    AudioClip MakeHit()
    {
        float phase = 0f;
        float lp = 0f;
        var rng = new System.Random(21);
        return Build("hit", 0.5f, t =>
        {
            float n = (float)(rng.NextDouble() * 2.0 - 1.0);
            lp += (n - lp) * 0.08f;
            float f = Mathf.Lerp(150f, 42f, Mathf.Clamp01(t / 0.5f));
            phase += 2f * Mathf.PI * f / SampleRate;
            float env = Env(t, 0.002f, 5.5f);
            return (Mathf.Sin(phase) * 0.7f + lp * 0.5f) * env;
        });
    }

    AudioClip MakeClick()
    {
        return Build("click", 0.06f, t =>
        {
            float env = Env(t, 0.001f, 65f);
            return Mathf.Sin(2f * Mathf.PI * 720f * t) * env;
        });
    }

    AudioClip MakeBuy()
    {
        float[] notes = { 523.25f, 659.25f, 783.99f, 1046.5f };
        return Build("buy", 0.55f, t =>
        {
            int idx = Mathf.Clamp(Mathf.FloorToInt(t / 0.085f), 0, notes.Length - 1);
            float local = t - idx * 0.085f;
            float env = Env(local, 0.004f, 9f);
            return Mathf.Sin(2f * Mathf.PI * notes[idx] * t) * 0.5f * env;
        });
    }

    AudioClip MakeError()
    {
        return Build("error", 0.28f, t =>
        {
            float env = Env(t, 0.004f, 10f);
            float s = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * 155f * t));
            return s * 0.35f * env;
        });
    }

    AudioClip MakeRecord()
    {
        float[] notes = { 523.25f, 659.25f, 783.99f, 1046.5f, 1318.51f };
        return Build("record", 0.9f, t =>
        {
            int idx = Mathf.Clamp(Mathf.FloorToInt(t / 0.11f), 0, notes.Length - 1);
            float local = t - idx * 0.11f;
            float env = Env(local, 0.003f, 5f);
            float second = Mathf.Sin(2f * Mathf.PI * notes[idx] * 0.5f * t) * 0.18f * env;
            return (Mathf.Sin(2f * Mathf.PI * notes[idx] * t) * 0.45f + second) * env;
        });
    }

    AudioClip MakeTick()
    {
        return Build("tick", 0.08f, t =>
        {
            float env = Env(t, 0.001f, 55f);
            return Mathf.Sin(2f * Mathf.PI * 1180f * t) * env;
        });
    }

    AudioClip MakeAmbience()
    {
        int count = SampleRate * 8;
        var data = new float[count];
        var rng = new System.Random(4242);
        float lp1 = 0f;
        float lp2 = 0f;
        for (int i = 0; i < count; i++)
        {
            float t = i / (float)SampleRate;
            float n = (float)(rng.NextDouble() * 2.0 - 1.0);
            lp1 += (n - lp1) * 0.008f;
            lp2 += (lp1 - lp2) * 0.08f;

            float pad =
                Mathf.Sin(2f * Mathf.PI * 110f * t) * 0.34f +
                Mathf.Sin(2f * Mathf.PI * 164.81f * t + 0.7f) * 0.22f +
                Mathf.Sin(2f * Mathf.PI * 220f * t + 1.3f) * 0.12f;

            float lfo = 0.62f + 0.38f * Mathf.Sin(2f * Mathf.PI * 0.07f * t);
            data[i] = pad * 0.075f * lfo + lp2 * 0.42f;
        }

        // Gentle fade at the loop ends avoids clicks.
        int fade = SampleRate / 2;
        for (int i = 0; i < fade; i++)
        {
            float k = i / (float)fade;
            data[i] *= k;
            data[count - 1 - i] *= k;
        }

        float peak = 0.0001f;
        for (int i = 0; i < count; i++)
        {
            peak = Mathf.Max(peak, Mathf.Abs(data[i]));
        }
        float gain = 0.5f / peak;
        for (int i = 0; i < count; i++)
        {
            data[i] *= gain;
        }

        var clip = AudioClip.Create("ambience", count, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
