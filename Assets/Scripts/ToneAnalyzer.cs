/**
* (C) Copyright 2021 IBM Corp. & Joshua Mall. All Rights Reserved.
*
*
*Licensed under the Apache License, Version 2.0 (the "License");
*you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
*Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*
*/

using IBM.Watson.ToneAnalyzer.V3;
using IBM.Watson.ToneAnalyzer.V3.Model;
using IBM.Cloud.SDK;
using IBM.Cloud.SDK.Authentication;
using IBM.Cloud.SDK.Authentication.Iam;
using IBM.Cloud.SDK.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
//using System.Text.Json;
//using System.Text.Json.Serialization;


namespace VoiceAssistant
{
    public class ToneAnalyzer : MonoBehaviour
    {       
        #region Declarations
        private string iamApikey = "Z1HSfpSQZDwHDfpd17Vt1NFXEfHV-7MZ1xks3tkSH85a";
        private string serviceUrl = "https://api.au-syd.tone-analyzer.watson.cloud.ibm.com/instances/c6e5d10b-4cdf-46a0-90ad-74d27fe90aba";
        private string versionDate = "2021-09-21";
        
        private ToneAnalyzerService service;
        private bool toneTested = false;
        private bool toneChatTested = false;

        private Welcome welcomeScript;
        #endregion

        // Start is called before the first frame update
        void Start()
        {
            welcomeScript = this.GetComponent<Welcome>();
            //Verify credentials 
            LogSystem.InstallDefaultReactors();
            Runnable.Run(CreateService());
        }

        #region CreateService
        private IEnumerator CreateService()
        {
            if (string.IsNullOrEmpty(iamApikey))
            {
                throw new IBMException("Please provide IAM ApiKey for the service.");
            }

            //  Create credential and instantiate service
            IamAuthenticator authenticator = new IamAuthenticator(apikey: iamApikey);

            //  Wait for tokendata
            while (!authenticator.CanAuthenticate())
                yield return null;

            service = new ToneAnalyzerService(versionDate, authenticator);
            if (!string.IsNullOrEmpty(serviceUrl))
            {
                service.SetServiceUrl(serviceUrl);
            }

            //Runnable.Run(Examples());
        }
        #endregion

        #region StartAnalyzer
        public IEnumerator StartAnalyzer(string stringToTestTone)
        {
            Runnable.Run(Analyzer(stringToTestTone));
            return null;
        }
        #endregion

        #region Analyzer
        private IEnumerator Analyzer(string stringToTestTone)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(stringToTestTone);
            MemoryStream toneInput = new MemoryStream(bytes);

            string someString = Encoding.ASCII.GetString(bytes);//
            Debug.Log("bytes =" + someString);//

            List<string> tones = new List<string>()
            {
                "emotion",
                "language",
                "social"
            };
            service.Tone(callback: OnTone, toneInput: toneInput, sentences: true, tones: tones, contentLanguage: "en", acceptLanguage: "en", contentType: "text/plain;charset=utf-8");

            while (!toneTested)
            {
                yield return null;
            }
        }
        #endregion

        #region OnTone
        private void OnTone(DetailedResponse<ToneAnalysis> response, IBMError error)
        {
            if (error != null)
            {
                Debug.Log("service.Tone Error here");
                Log.Debug("ExampleToneAnalyzerV3.OnTone()", "Error: {0}: {1}", error.StatusCode, error.ErrorMessage);
            }
            else
            {
                Log.Debug("ExampleToneAnalyzerV3.OnTone()", "{0}", response.Response);
                //if (welcomeScript.CBTCounter > 0)
                //{
                    ParseResponse(response.Response);
                //}
            }

            toneTested = true;

            //Ask follow up question
            if (!welcomeScript.FollowUpCompleted && welcomeScript.WelcomeCompleted)
            {
                welcomeScript.FollowUp = true;
            }

            //Commence CBT conversation path 
            if (welcomeScript.FollowUpCompleted && welcomeScript.WelcomeCompleted)
            {
                if (welcomeScript.CBTCounter == 0)
                {
                    welcomeScript.CBTCounter = 1;
                }
                welcomeScript.StartCbt();
            }
        }
        #endregion

        #region ParseResponse
        private void ParseResponse(string response)
        {
            int index;
            int index2;
            string score = "0.0";
            string toneId = "";
            Tones t = new Tones(); 

            if (response.Contains("score"))
            {
                try
                {
                    index = response.IndexOf("score") + 7;
                    score = response.Substring(index, 8);
                    Log.Debug("SCORE =", "{0}", score);
                }
                catch (Exception e)
                {
                    Debug.Log("Error ");
                    score = "0.0";
                }
            }

            if (response.Contains("tone_id"))
            {
                try
                {
                    index = response.IndexOf("tone_id") + 10;
                    toneId = response.Substring(index, 10);
                    index2 = toneId.IndexOf('"');
                    toneId = toneId.Substring(0, index2);
                    Log.Debug("TONE =", "{0}", toneId);
                }
                catch (Exception e)
                {
                    Debug.Log("Error ");
                    toneId = "sadness";
                }
            }

            //t.Score = float.Parse(score);
            t.Score = score;
            t.ToneID = toneId;

            welcomeScript.tonesList.Add(t);
        }
        #endregion

        #region StartAnalyzerConversation
        public IEnumerator StartAnalyzerConversation(List<Utterance> utterances)
        {
            Runnable.Run(AnalyzerConversation(utterances));
            return null;
        }
        #endregion

        #region AnalyzerConversation
        private IEnumerator AnalyzerConversation(List<Utterance> utterances)
        {
            service.ToneChat(callback: OnToneChat, utterances: utterances, contentLanguage: "en", acceptLanguage: "en");

            while (!toneChatTested)
            {
                yield return null;
            }
        }
        #endregion
          
        #region OnToneChat
        private void OnToneChat(DetailedResponse<UtteranceAnalyses> response, IBMError error)
        {
            if (error != null)
            {
                Log.Debug("ExampleToneAnalyzerV3.OnToneChat()", "Error: {0}: {1}", error.StatusCode, error.ErrorMessage);
            }
            else
            {
                Log.Debug("ExampleToneAnalyzerV3.OnToneChat() \n", "\n {0} \n", response.Response);
            }

            toneChatTested = true;
        }
        #endregion
    }
}
