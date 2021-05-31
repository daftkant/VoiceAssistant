using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoiceAssistant
{
    public enum State
    {
        Start,
        Authenticating,
        Listening,
        Processing,
        AnalyzingTone,
        Talking,     
    }
}