/**
* Copyright 2021 Joshua Mall. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*
*/

using IBM.Watson.TextToSpeech.V1;
using IBM.Watson.TextToSpeech.V1.Model;
using IBM.Cloud.SDK.Utilities;
using IBM.Cloud.SDK.Authentication;
using IBM.Cloud.SDK.Authentication.Iam;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using IBM.Cloud.SDK;


namespace VoiceAssistant
{
    public class Welcome : MonoBehaviour
    {
        #region Declarations
        private TextToSpeech textToSpeech;
        private SpeechToText speechToText;
        private ToneAnalyzer toneAnalyzer;        
        private bool welcome;
        public bool WelcomeCompleted;
        private bool followup;
        public bool FollowUp
        {
            get { return followup; }
            set { followup = value; }
        }
        public bool FollowUpCompleted;
      
        public bool WelcomeMsg
        {
            get { return welcome; }
            set { welcome = value; }
        }
        private State currentState;
        public State CurrentState
        {
            get { return currentState; }
            set { currentState = value; }
        }
        public Text UIText;
        public Text UserSpeech;
        public Text AiSpeech;

        public Tones primaryTone;
        public List<Tones> tonesList = new List<Tones>();
        private int cbtCounter = 0;
        public int CBTCounter
        {
            get { return cbtCounter; }
            set { cbtCounter = value; }
        }
        #endregion

        // Start is called before the first frame update
        void Start()
        {            
            textToSpeech = this.GetComponent<TextToSpeech>();
            speechToText = this.GetComponent<SpeechToText>();
            toneAnalyzer = this.GetComponent<ToneAnalyzer>();
            welcome = false;
            followup = false;
            currentState = State.Start;
         }

        // Update is called once per frame
        void Update()
        {
            if (welcome)
            {
                currentState = State.Authenticating;
                SayWelcomeMessage();
            }

            if (followup)
            {
                SayFollowUpMessage();
            }
                      

            switch (currentState)
            {
                case State.Start:
                    UIText.text = "";
                    break;

                case State.Authenticating:
                    UIText.text = "Authenticating";
                    break;

                case State.Listening:
                    UIText.text = "Listening";
                    speechToText.Active = true;
                    break;

                case State.Processing:
                    UIText.text = "Processing";
                    break;

                case State.AnalyzingTone:
                    UIText.text = "Analyzing Tone";
                    break;

                case State.Talking:
                    UIText.text = "Talking";
                    break;

                default:
                    break;
            }
        }

        #region StartCbt
        public void StartCbt()
        {
           switch (cbtCounter)
            {
                case 1:
                    cbtCounter++;
                    ParseEmotionList(tonesList);
                    SayMessage("I am detecting " + primaryTone.ToneID + ", why do you feel this way?");
                    break;

                case 2:
                    cbtCounter++;
                    SayMessage("Do you feel this " + primaryTone.ToneID + " often?");                    
                    break;

                case 3:
                    cbtCounter++;
                    CbtTreatment(primaryTone.ToneID);
                    break;

                case 4:
                    //start loop 
                    cbtCounter=0;
                    FollowUpCompleted = false;
                    SayWelcomeMessage();
                    break;

                default:
                    //start loop 
                    cbtCounter = 0;
                    FollowUpCompleted = false;
                    SayWelcomeMessage();
                    break;
            }
        }
        #endregion

        #region ParseEmotionList
        private void ParseEmotionList(List<Tones> tones)
        {
            float score = 0.0f;
            float toneScore = 0.0f;

            foreach (Tones t in tones)
            {
                try
                {
                    toneScore = float.Parse(t.Score);
                    if (toneScore > score)
                    {
                        primaryTone = t;
                        Log.Debug("Primary tone = ", "{0}", t.ToneID);
                    }
                }
                catch (Exception e)
                {
                    primaryTone.ToneID = "Error";
                }
            }
        }
        #endregion

        #region CbtTreatment
        private void CbtTreatment(string emotion)
        {
            switch (emotion)
            {
                case "anger":
                    SayMessage("May I suggest next time you feel angry to think of a time when you felt peaceful and calm.");
                    break;

                case "joy":
                    SayMessage("Always remember what makes you happy and joyous today.");
                    break;

                case "analytical":
                    SayMessage("If you ever feel over analytical, just remember to relax and enjoy some downtime or some hobbies even a walk in the park.");
                    break;

                case "confident":
                    SayMessage("Always remember what makes you confident today.");
                    break;

                case "tentative":
                    SayMessage("May I suggest next time you feel tentative to think of a time when you felt certain and confident.");
                    break;

                case "fear":
                    SayMessage("May I suggest next time you feel fear to think of a time when you felt  courage and confidence.");
                    break;

                case "sadness":
                    SayMessage("May I suggest next time you feel sadness to think of a time when you felt happy and joyous. ");
                    break;

                default:
                    break;
            }
        }
        #endregion

        #region SayMessage
        public void SayMessage(string text)
        {
            AiSpeech.text = text;
            textToSpeech.Talk = true;
        }
        #endregion

        #region SayWelcomeMessage
        private void SayWelcomeMessage()
        {
            welcome = false;
            SayMessage("Hello, my name is 20 1 B prototype health assistant, Tell me about your day?");
            WelcomeCompleted = true;
        }
        #endregion

        #region SayFollowUpMessage
        private void SayFollowUpMessage()
        {
            followup = false;
            SayMessage("Please tell me more, maybe you could elaborate further?");
            FollowUpCompleted = true;
        }
        #endregion
    }
}