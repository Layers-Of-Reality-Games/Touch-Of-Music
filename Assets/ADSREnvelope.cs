// ADSREnvelope.cs
using UnityEngine;

[System.Serializable]
public class ADSREnvelope
{
    [Header("ADSR Parameters")]
    [Range(0.001f, 2.0f)]
    public float attack = 0.01f;
    
    [Range(0.001f, 2.0f)] 
    public float decay = 0.3f;
    
    [Range(0.0f, 1.0f)]
    public float sustain = 0.7f;
    
    [Range(0.001f, 5.0f)]
    public float release = 1.5f;
    
    private float currentTime;
    private float peakAmplitude;
    private EnvelopeState state;
    private bool noteOn;
    
    public enum EnvelopeState
    {
        Off,
        Attack,
        Decay,
        Sustain,
        Release
    }
    
    public EnvelopeState State => state;
    public bool IsActive => state != EnvelopeState.Off;
    
    public void TriggerNote(float amplitude)
    {
        peakAmplitude = amplitude;
        currentTime = 0f;
        state = EnvelopeState.Attack;
        noteOn = true;
    }
    
    public void ReleaseNote()
    {
        if (state != EnvelopeState.Off)
        {
            state = EnvelopeState.Release;
            currentTime = 0f;
            noteOn = false;
        }
    }
    
    public float GetAmplitude(float deltaTime)
    {
        if (state == EnvelopeState.Off)
            return 0f;
            
        currentTime += deltaTime;
        
        switch (state)
        {
            case EnvelopeState.Attack:
                if (currentTime >= attack)
                {
                    state = EnvelopeState.Decay;
                    currentTime = 0f;
                    return peakAmplitude;
                }
                return Mathf.Lerp(0f, peakAmplitude, currentTime / attack);
                
            case EnvelopeState.Decay:
                if (currentTime >= decay)
                {
                    state = EnvelopeState.Sustain;
                    currentTime = 0f;
                    return peakAmplitude * sustain;
                }
                
                float decayProgress = currentTime / decay;
                return Mathf.Lerp(peakAmplitude, peakAmplitude * sustain, 
                    1f - Mathf.Exp(-decayProgress * 3f));
                
            case EnvelopeState.Sustain:
                float sustainDecay = Mathf.Exp(-currentTime * 0.3f);
                return peakAmplitude * sustain * sustainDecay;
                
            case EnvelopeState.Release:
                if (currentTime >= release)
                {
                    state = EnvelopeState.Off;
                    return 0f;
                }
                float releaseProgress = currentTime / release;
                float startAmplitude = peakAmplitude * sustain * Mathf.Exp(-currentTime * 0.3f);
                return startAmplitude * Mathf.Exp(-releaseProgress * 4f);
                
            default:
                return 0f;
        }
    }
    
    public void Reset()
    {
        state = EnvelopeState.Off;
        currentTime = 0f;
        noteOn = false;
    }
}
