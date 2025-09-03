using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;

public enum WaveType
{
    Sine,
    Triangle,
    Sawtooth,
    Square,
    Noise
}

public class SoundGenerator
{

    public static byte[] Mix(byte[] a, byte[] b)
    {
        int len = Math.Min(a.Length, b.Length);
        byte[] result = new byte[len];
        for (int i = 0; i < len; i += 2)
        {
            short sa = BitConverter.ToInt16(a, i);
            short sb = BitConverter.ToInt16(b, i);
            int mixed = sa + sb;
            if (mixed > short.MaxValue) mixed = short.MaxValue;
            if (mixed < short.MinValue) mixed = short.MinValue;
            short s = (short)mixed;
            result[i] = (byte)(s & 0xff);
            result[i + 1] = (byte)((s >> 8) & 0xff);
        }
        return result;
    }
    public static byte[] GenerateShootSound(float f1 = 50f, float f2 = 400f, float f3 = 80f, float d = 0.9992f)
    {
        int sampleRate = 44100;
        int durationMs = 5000;
        int sampleCount = sampleRate * durationMs / 1000;
        byte[] buffer = new byte[sampleCount * 2]; // 16-bit PCM = 2 bytes per sample

        float frequency = f1; // Starting frequency
        float frequency2 = f2; // Starting frequency
        float frequency3 = f3; // Starting frequency
        float decay = d;
        float phase1 = 0f;              // in radians
        float phase3 = MathF.PI / 2f;   // 90 degrees
        for (int i = 0; i < sampleCount; i++)
        {

            // Create a decaying sine wave
            float t = i / (float)sampleRate;
            float dynamicPhase = (float)(Math.Sin(t * 2) * Math.PI / 4);

            float amplitude = (float)Math.Pow(decay, i);
            float sample = (float)(Math.Sin(2 * Math.PI * frequency * t + phase1) * amplitude);
            float sample2 = (float)(Math.Sin(2 * Math.PI * frequency2 * t + dynamicPhase) * amplitude);
            float sample3 = (float)(Math.Sin(2 * Math.PI * frequency3 * t + phase3) * amplitude);

            // Convert to 16-bit signed PCM
            short value = (short)(((sample + sample2 + sample3) / 3) * short.MaxValue);
            buffer[i * 2] = (byte)(value & 0xff);
            buffer[i * 2 + 1] = (byte)((value >> 8) & 0xff);
        }

        return buffer;
        // var sound = new DynamicSoundEffectInstance(sampleRate, AudioChannels.Mono);
        // sound.SubmitBuffer(buffer);
        // return sound;
    }

    public static byte[] GenerateSound(float[] frequencies, int durationMs, float[] phases = null, WaveType waveType = WaveType.Sine)
    {
        int sampleRate = 44100;
        int sampleCount = sampleRate * durationMs / 1000;
        byte[] buffer = new byte[sampleCount * 2]; // 16-bit PCM = 2 bytes per sample

        float decay = 0.99995f;

        // Ensure phases array matches frequencies array length if provided
        if (phases != null && phases.Length != frequencies.Length)
        {
            throw new ArgumentException("Phases array length must match frequencies array length");
        }

        for (int i = 0; i < sampleCount; i++)
        {
            // Create a decaying sine wave
            float t = i / (float)sampleRate;
            float dynamicPhase = (float)(Math.Sin(t * 2) * Math.PI / 4);

            float amplitude = (float)Math.Pow(decay, i);
            float combinedSample = 0f;

            // Add all frequencies together
            for (int j = 0; j < frequencies.Length; j++)
            {
                float phaseToUse = phases != null ? phases[j] : 0f;
                float waveValue = GenerateWaveform(waveType, frequencies[j], t, phaseToUse);
                combinedSample += waveValue * amplitude;
            }

            // Average the combined samples
            combinedSample /= frequencies.Length;

            // Convert to 16-bit signed PCM
            short value = (short)(combinedSample * short.MaxValue);
            buffer[i * 2] = (byte)(value & 0xff);
            buffer[i * 2 + 1] = (byte)((value >> 8) & 0xff);
        }

        return buffer;
    }

    private static float GenerateWaveform(WaveType waveType, float frequency, float time, float phase)
    {
        float phaseTime = 2 * MathF.PI * frequency * time + phase;

        switch (waveType)
        {
            case WaveType.Sine:
                return MathF.Sin(phaseTime);

            case WaveType.Triangle:
                // Triangle wave: proper implementation
                // Normalize phase to 0-1 range
                float trianglePhase = (phaseTime / (2 * MathF.PI)) % 1.0f;
                if (trianglePhase < 0) trianglePhase += 1.0f;

                // Create triangle: ramp up 0->1 then ramp down 1->0
                if (trianglePhase < 0.5f)
                    return (4 * trianglePhase) - 1; // -1 to 1 over first half
                else
                    return 3 - (4 * trianglePhase); // 1 to -1 over second half

            case WaveType.Sawtooth:
                // Sawtooth wave: -1 to 1 linear ramp
                float sawPhase = (phaseTime / (2 * MathF.PI)) % 1.0f;
                if (sawPhase < 0) sawPhase += 1.0f;
                return (2 * sawPhase) - 1; // Convert 0-1 to -1 to 1

            case WaveType.Square:
                // Square wave: -1 or 1
                return MathF.Sin(phaseTime) >= 0 ? 1.0f : -1.0f;

            case WaveType.Noise:
                // White noise: random values between -1 and 1
                // Use time as seed for consistent but random-seeming output
                int seed = (int)(time * frequency * 48000) + (int)(phase * 1000);
                Random rand = new Random(seed);
                return (float)(rand.NextDouble() * 2.0 - 1.0);

            default:
                return MathF.Sin(phaseTime);
        }
    }

    public static byte[] GenerateNote(string noteName, int octave, string alteration = "", int durationMs = 1000, WaveType waveType = WaveType.Sine)
    {
        // Calculate the base frequency for the note
        float baseFrequency = GetNoteFrequency(noteName, octave, alteration);

        // Create array with base frequency + 5 overtones
        float[] frequencies = new float[6];
        frequencies[0] = baseFrequency;

        // Add overtones (harmonics at 2x, 3x, 4x, 5x, 6x the base frequency)
        for (int i = 1; i < 6; i++)
        {
            frequencies[i] = baseFrequency * (i + 1);
        }

        // Generate the sound with all frequencies
        return GenerateSound(frequencies, durationMs, null, waveType);
    }

    private static float GetNoteFrequency(string noteName, int octave, string alteration)
    {
        // Note names to semitone offsets from C
        var noteOffsets = new Dictionary<string, int>
        {
            {"C", 0}, {"D", 2}, {"E", 4}, {"F", 5},
            {"G", 7}, {"A", 9}, {"B", 11}
        };

        // Normalize note name to uppercase
        string normalizedNote = noteName.ToUpper();

        if (!noteOffsets.ContainsKey(normalizedNote))
        {
            throw new ArgumentException($"Invalid note name: {noteName}");
        }

        // Get base semitone offset
        int semitoneOffset = noteOffsets[normalizedNote];

        // Apply alteration
        switch (alteration.ToLower())
        {
            case "#":
            case "sharp":
                semitoneOffset += 1;
                break;
            case "b":
            case "flat":
                semitoneOffset -= 1;
                break;
            case "":
                // No alteration
                break;
            default:
                throw new ArgumentException($"Invalid alteration: {alteration}");
        }

        // Calculate frequency using A4 = 440Hz as reference
        // A4 is octave 4, note A (semitone 9)
        // Formula: f = 440 * 2^((semitone - 69) / 12)
        // where semitone = octave * 12 + noteOffset
        int totalSemitones = octave * 12 + semitoneOffset;
        int a4Semitone = 4 * 12 + 9; // A4 = 57 semitones

        float frequency = 440f * (float)Math.Pow(2.0, (totalSemitones - a4Semitone) / 12.0);

        return frequency;
    }

    public static byte[] GenerateDrumSound(float[] frequencies, int durationMs, float[] phases = null, WaveType waveType = WaveType.Sine, float customDecay = 0.995f)
    {
        int sampleRate = 44100;
        int sampleCount = sampleRate * durationMs / 1000;
        byte[] buffer = new byte[sampleCount * 2]; // 16-bit PCM = 2 bytes per sample

        float decay = customDecay; // Custom decay for drums

        // Ensure phases array matches frequencies array length if provided
        if (phases != null && phases.Length != frequencies.Length)
        {
            throw new ArgumentException("Phases array length must match frequencies array length");
        }

        for (int i = 0; i < sampleCount; i++)
        {
            // Create a decaying wave
            float t = i / (float)sampleRate;

            float amplitude = (float)Math.Pow(decay, i);
            float combinedSample = 0f;

            // Add all frequencies together
            for (int j = 0; j < frequencies.Length; j++)
            {
                float phaseToUse = phases != null ? phases[j] : 0f;
                float waveValue = GenerateWaveform(waveType, frequencies[j], t, phaseToUse);
                combinedSample += waveValue * amplitude;
            }

            // Don't average for drums - we want full volume
            // Clamp to prevent clipping
            combinedSample = Math.Max(-1f, Math.Min(1f, combinedSample));

            // Convert to 16-bit signed PCM
            short value = (short)(combinedSample * short.MaxValue);
            buffer[i * 2] = (byte)(value & 0xff);
            buffer[i * 2 + 1] = (byte)((value >> 8) & 0xff);
        }

        return buffer;
    }
}