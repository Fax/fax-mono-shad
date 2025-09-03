// To be reimplemented without OpenAL

// I am making it static so I can just call it from wherever I need it.
using System.Diagnostics;
using Microsoft.Xna.Framework.Audio;

public static class Sfx
{

    static byte[] shootBuffer;
    static DynamicSoundEffectInstance shootSound;


    static byte[] explosionBuffer;
    static DynamicSoundEffectInstance explosionSound;
    static byte[] plingBuffer;
    static DynamicSoundEffectInstance plingSound;

    // Init the current device. the int[]null thing is a trial and error as I have no idea what to pass as flags.
    // I need to read the docs.
    public static void Init()
    {
        shootBuffer = SoundGenerator.GenerateShootSound();
        shootSound = new DynamicSoundEffectInstance(44100, AudioChannels.Mono);

        var eb = SoundGenerator.GenerateDrumSound(
            new float[] { 200f, 120f, 60f },   // low rumble frequencies
                durationMs: 1200,
                waveType: WaveType.Noise,          // noise core
                customDecay: 0.996f);

        var eb2 = SoundGenerator.GenerateDrumSound(
                    new float[] { 60f, 300f, },
                    durationMs: 1200,
                    waveType: WaveType.Square,
                    customDecay: 0.999f
                );

        explosionBuffer = SoundGenerator.Mix(eb, eb2);
        explosionSound = new DynamicSoundEffectInstance(44100, AudioChannels.Mono);

        /// random sounds!
        var pling = SoundGenerator.GenerateSound(
            new float[] { 880f, 1320f, 1760f },
            durationMs: 100,
            waveType: WaveType.Triangle
        );
        var pling2 = SoundGenerator.GenerateNote(
            noteName: "C",
            octave: 7,
            alteration: "",
            durationMs: 150,
            waveType: WaveType.Sine
            );
        plingBuffer = SoundGenerator.Mix(pling, pling2);
        plingSound = new DynamicSoundEffectInstance(44100, AudioChannels.Mono);

    }

    // don't forget to turn it off
    public static void Shutdown()
    {

    }

    // Call this whenever you need the shot sound
    public static void PlayShoot()
    {
        shootSound.Volume = 0.6f;
        if (shootSound.State == SoundState.Playing)
        {
            shootSound.Stop();
        }
        shootSound.SubmitBuffer(shootBuffer);
        shootSound.Play();
    }

    public static void PlayBoom()
    {

        explosionSound.Volume = 0.6f;
        if (explosionSound.State == SoundState.Playing)
        {
            explosionSound.Stop();
        }
        explosionSound.SubmitBuffer(explosionBuffer);
        explosionSound.Play();
    }
    public static void PlayPling()
    {

        plingSound.Volume = 0.2f;
        if (plingSound.State == SoundState.Playing)
        {
            plingSound.Stop();
        }
        plingSound.SubmitBuffer(plingBuffer);
        plingSound.Play();
    }


}