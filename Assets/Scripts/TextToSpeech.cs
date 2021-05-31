/**
* Copyright 2021 IBM Corp & Joshua Mall. All Rights Reserved.
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
using IBM.Cloud.SDK;
using IBM.Cloud.SDK.Utilities;
using IBM.Cloud.SDK.Authentication;
using IBM.Cloud.SDK.Authentication.Iam;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace VoiceAssistant
{
    public class TextToSpeech : MonoBehaviour
    {
        #region Declarations
        private string iamApikey = "XcrGx2balWRWqHkdBiNONBF5DDAWUcIRcvyihQF16LUN";
        private string serviceUrl = "https://api.au-syd.text-to-speech.watson.cloud.ibm.com/instances/0c1c4f7c-6eda-4198-9f15-57ff895fecf0";
        private TextToSpeechService service;
        private string allisionVoice = "en-US_AllisonV3Voice";
        private string synthesizeMimeType = "audio/wav";
        private AudioClip _recording = null;
        private byte[] audioStream = null;
        private bool talk; 
        public bool Talk
        {
            get { return talk; } 
            set { talk = value; }  
        }
        private string text;
        public string Text
        {
            get { return text; }
            set { text = value; }
        }       
        private Welcome welcomeScript;
        #endregion

        private void Start()
        {
            welcomeScript = this.GetComponent<Welcome>();
            //Verify credentials 
            LogSystem.InstallDefaultReactors();
            Runnable.Run(CreateService());
        }

        void Update()
        {
            if (talk)
            {
                talk = false;
                Runnable.Run(StartSynthesize(welcomeScript.AiSpeech.text));
            }
        }

        #region CreateService
        private IEnumerator CreateService()
        {
            if (string.IsNullOrEmpty(iamApikey))
            {
                throw new IBMException("Please add IAM ApiKey to the Iam Apikey field in the inspector.");
            }

            IamAuthenticator authenticator = new IamAuthenticator(apikey: iamApikey);

            while (!authenticator.CanAuthenticate())
            {
                yield return null;
            }

            service = new TextToSpeechService(authenticator);
            if (!string.IsNullOrEmpty(serviceUrl))
            {
                service.SetServiceUrl(serviceUrl);
            }
        }
        #endregion

        #region StartSynthesize
        private IEnumerator StartSynthesize(string text)
        {
            //"Processing";
            welcomeScript.CurrentState = State.Processing;

            byte[] synthesizeResponse = null;
            AudioClip clip = null;
            service.Synthesize(
                callback: (DetailedResponse<byte[]> response, IBMError error) =>
                {
                    synthesizeResponse = response.Result;
                    Log.Debug("ExampleTextToSpeechV1", "Synthesize done!");
                    clip = WaveFile.ParseWAV("myClip", synthesizeResponse);
                    PlayClip(clip);
                },
                text: text,
                voice: allisionVoice,
                accept: synthesizeMimeType
            );

            while (synthesizeResponse == null)
                yield return null;

            yield return new WaitForSeconds(clip.length);

            //"Listening"
            welcomeScript.CurrentState = State.Listening;
        }
        #endregion
               
        #region PlayClip
        private void PlayClip(AudioClip clip)
        {
            //"Talking";
            welcomeScript.CurrentState = State.Talking;
            if (Application.isPlaying && clip != null)
            {
                GameObject audioObject = new GameObject("AudioObject");
                AudioSource source = audioObject.AddComponent<AudioSource>();
                source.spatialBlend = 0.0f;
                source.loop = false;
                source.clip = clip;
                source.Play();

                GameObject.Destroy(audioObject, clip.length);
                talk = false;
            }
        }
        #endregion

    }
}