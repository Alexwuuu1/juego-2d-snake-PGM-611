using UnityEngine;

namespace SnakeTrio
{
    public sealed class SnakeAudio : MonoBehaviour
    {
        public static SnakeAudio Instance;
        AudioSource music, effects;
        AudioClip clickClip, eatClip, hitClip, winClip;
        public bool Muted => SnakePreferences.Muted;
        public float MusicVolume => SnakePreferences.MusicVolume;
        public float EffectsVolume => SnakePreferences.EffectsVolume;
        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this; DontDestroyOnLoad(gameObject);
            music = gameObject.AddComponent<AudioSource>();
            effects = gameObject.AddComponent<AudioSource>();
            music.clip = Melody(); music.loop = true;
            clickClip = Tone(540, .07f); eatClip = Tone(940, .14f, 1350);
            hitClip = Tone(210, .28f, 65); winClip = Tone(660, .45f, 1320);
            ApplySettings(); music.Play();
        }
        public void SetMusicVolume(float value) { SnakePreferences.MusicVolume = value; ApplySettings(); }
        public void SetEffectsVolume(float value) { SnakePreferences.EffectsVolume = value; ApplySettings(); }
        public void ToggleMute() { SnakePreferences.Muted = !Muted; ApplySettings(); }
        void ApplySettings()
        {
            music.volume = MusicVolume; effects.volume = EffectsVolume;
            music.mute = effects.mute = Muted;
        }
        public void Click() { effects.PlayOneShot(clickClip); }
        public void Eat() { effects.PlayOneShot(eatClip); }
        public void Hit() { effects.PlayOneShot(hitClip); }
        public void Win() { effects.PlayOneShot(winClip); }
        void OnDestroy()
        {
            if (Instance != this) return;
            Destroy(music.clip); Destroy(clickClip); Destroy(eatClip); Destroy(hitClip); Destroy(winClip);
            Instance = null;
        }
        static AudioClip Tone(float start, float duration, float end = 0)
        {
            const int rate = 22050; var samples = new float[(int)(rate * duration)];
            float phase = 0; if (end == 0) end = start;
            for (int i = 0; i < samples.Length; i++)
            { float t = (float)i / samples.Length; phase += 2 * Mathf.PI * Mathf.Lerp(start, end, t) / rate;
              samples[i] = Mathf.Sin(phase) * Mathf.Sin(Mathf.PI * t) * .55f; }
            var clip = AudioClip.Create("Efecto sintetizado", samples.Length, 1, rate, false);
            clip.SetData(samples, 0); return clip;
        }
        static AudioClip Melody()
        {
            const int rate = 22050; float[] notes = { 261.63f, 329.63f, 392f, 329.63f, 293.66f, 349.23f, 440f, 392f };
            var samples = new float[rate * 4];
            for (int i = 0; i < samples.Length; i++)
            { float time = (float)i / rate; int n = (int)(time * 2); float local = time % .5f;
              float envelope = Mathf.Sin(Mathf.PI * local / .5f);
              samples[i] = envelope * (Mathf.Sin(2 * Mathf.PI * notes[n] * time) * .13f
                  + Mathf.Sin(2 * Mathf.PI * 130.815f * time) * .06f); }
            var clip = AudioClip.Create("Jardín nocturno", samples.Length, 1, rate, false);
            clip.SetData(samples, 0); return clip;
        }
    }
}
