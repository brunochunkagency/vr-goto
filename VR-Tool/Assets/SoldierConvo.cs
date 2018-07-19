﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IBM.Watson.DeveloperCloud.Services.Conversation.v1;
using System;
using IBM.Watson.DeveloperCloud.Services.TextToSpeech.v1;
using IBM.Watson.DeveloperCloud.Services.SpeechToText.v1;
using IBM.Watson.DeveloperCloud.DataTypes;
using IBM.Watson.DeveloperCloud.Utilities;
using IBM.Watson.DeveloperCloud.Logging;
using IBM.Watson.DeveloperCloud.Connection;
using FullSerializer;
using UnityEngine.Networking;
using UnityEngine.AI;

public class SoldierConvo : MonoBehaviour
// code derived from Watson Unity SDK and Amara Keller: https://github.com/akeller/Unity-AI-AR/blob/master/SoldierConvo.cs
{
    private string WATSON_CONVERSATION_USER = "71682b37-e552-406c-be57-4593b3ec6a02";
    private string WATSON_CONVERSATION_PASSWORD = "knQYMuKbxxcd";
    private string WATSON_CONVERSATION_WORKSPACE_ID = "ce241112-4b00-48a0-a19a-a6559cd5bfb5";
    private string WATSON_SPEECH_TO_TEXT_USER = "2c8ddb95-e9cc-4a0f-93f3-ef07cfde10e0";
    private string WATSON_SPEECH_TO_TEXT_PASSWORD = "4Z5jZDzaN0Zi";
    private string WATSON_TEXT_TO_SPEECH_USER = "2217f86d-58e6-4661-9c73-434f68d3a636";
    private string WATSON_TEXT_TO_SPEECH_PASSWORD = "1L01y3Ck85h4";
    private string WEATHER_USER = "";
    private string WEATHER_PASSWORD = "";

    private Conversation _conversation;
    private SpeechToText _speechToText;
    private TextToSpeech _textToSpeech;

    private int _recordingRoutine = 0;
    private string _microphoneID = null;
    private AudioClip _recording = null;
    private int _recordingBufferSize = 2;
    private int _recordingHZ = 22050;
    private string _outputText = "";
    private fsSerializer _serializer = new fsSerializer();
    private Dictionary<string, object> _context = null;
    private bool _stopListeningFlag = false;
    private string _location = "";

    private NavMeshAgent _navMeshAgent;
    private Camera _camera;
    private bool _isWalking;

    void Start()
    {
        _isWalking = false;
        GameObject player;
        player = GameObject.Find("Player");
        _navMeshAgent = player.GetComponent<NavMeshAgent>();
        _camera = Camera.main;

        InitializeServices();

        RecordAgain();
    }

    private void OnConversationMessageReceived(object resp, Dictionary<string, object> customData)
    // derived from Watson Unity SDK: https://github.com/watson-developer-cloud/unity-sdk/blob/336ebba141337047fe95ece06e5034fa9818666e/Examples/ServiceExamples/Scripts/ExampleConversation.cs#L118
    {
        Debug.Log("Received Response from Watson Conversation");
        fsData fsdata = null;
        fsResult r = _serializer.TrySerialize(resp.GetType(), resp, out fsdata);
        if (!r.Succeeded)
            throw new WatsonException(r.FormattedMessages);

        MessageResponse messageResponse = new MessageResponse();
        object obj = messageResponse;
        r = _serializer.TryDeserialize(fsdata, obj.GetType(), ref obj);
        if (!r.Succeeded)
            throw new WatsonException(r.FormattedMessages);

        object _tempContext = null;
        (resp as Dictionary<string, object>).TryGetValue("context", out _tempContext);

        if (_tempContext != null)
            _context = _tempContext as Dictionary<string, object>;
        else
            Log.Debug("OnConversationMessageReceived()", "Failed to get context");

        string action = "";
        _location = "";
        if (resp != null && (messageResponse.intents.Length > 0 || messageResponse.entities.Length > 0))
 /*       {
            if (messageResponse.intents.Length > 0)
            {
                string intent = messageResponse.intents[0].intent;
                foreach (string WatsonResponse in messageResponse.output.text)
                {
                    _outputText += WatsonResponse + " ";
                }
                Debug.Log("Received from Watson Conversation - Intent: " + intent + " / Text: " + _outputText);
                if (intent.Contains("start"))
                {
                    walk();
                }
            }
            else
            {
                _outputText = messageResponse.output.text[0];
                Debug.Log("Received from Watson Conversation - Intent: No Intent" + " / Text: " + _outputText);
            }
            */
            {
                if (messageResponse.intents.Length > 0)
                {
                    string intent = messageResponse.intents[0].intent;
                    foreach (string WatsonResponse in messageResponse.output.text)
                    {
                        _outputText += WatsonResponse + " ";
                    }
                //Debug.Log("Received from Watson Conversation - Intent: " + intent + " / Text: " + _outputText);
                Debug.Log("Hier wordt Watson geinitialiseerd");
                if (intent.Contains("virtualholiday"))
                    {
                        Application.LoadLevel("green");
                    Debug.Log("green is geladen door Watson");
                    }
                }
                else
                {
                    _outputText = messageResponse.output.text[0];
                //Debug.Log("Received from Watson Conversation - Intent: No Intent" + " / Text: " + _outputText);
                Debug.Log("Else statement");
            }

                MessageResponseExt messageResponseExtended = new MessageResponseExt();
            object objExtended = messageResponseExtended;
            r = _serializer.TryDeserialize(fsdata, objExtended.GetType(), ref objExtended);
            if (!r.Succeeded)
                throw new WatsonException(r.FormattedMessages);
            action = messageResponseExtended.output.action;
            _location = messageResponseExtended.output.location;

            if ((action == null) || (action.Length == 0))
            {
                CallTextToSpeech(_outputText);
            }
            else
            {
                Invoke(action, 0);
            }
            _outputText = "";
        }
    }




    private void OnSpeechToTextResultReceived(SpeechRecognitionEvent result, Dictionary<string, object> customData)
    {
        if (result != null && result.results.Length > 0)
        {
            foreach (var res in result.results)
            {
                foreach (var alt in res.alternatives)
                {
                    Debug.Log(alt.ToString());
                    if (res.final && alt.confidence > 0)
                    {
                        StopRecording();
                        string text = alt.transcript;
                        Debug.Log("Received from Watson Speech To Text - Text: " + text + " / Confidence: " + alt.confidence);
                        if 
                            (text.Contains("asdf") 
                           /* || text.Contains("bitch") || text.Contains("bea")
                            || text.Contains("Pleaseshowmethebeach") || text.Contains("I want to go to the beach") || text.Contains("coast") || text.Contains("beach") || 
                            text.Contains("beach") || text.Contains("show me the beach") || text.Contains("beats") || text.Contains("sand") || text.Contains("shore") 
                            || text.Contains("trees") || text.Contains("beech") || text.Contains("bees") || text.Contains("beat") */
                            )
                        {
                            walk();
                        }
                        else
                        {
                            if (text.Contains("sdf") 
                                /* || text.Contains("Simming") || text.Contains("Swim") || text.Contains("Swem")
                                || text.Contains("Swimmingpool") || text.Contains("swin") || text.Contains("poo")
                                || text.Contains("pull") || text.Contains("Pool") */
                                )
                            {
                                stop();
                            }
                            else
                            {
                                SendMessageToConversation(text);
                            }
                        }
                    }
                }
            }
        }
    }
/*
    public void walk()
    {

    Application.LoadLevel("page2");

    }

    public void stop()
    {

 Application.LoadLevel("page1");

    }*/



    public void SendMessageToConversation(string spokenText)
    {
        MessageRequest messageRequest = new MessageRequest()
        {
            input = new Dictionary<string, object>()
            {
                { "text", spokenText }
            },
            context = _context
        };

        Debug.Log("Sent to Watson Conversation: " + spokenText);
        if (_conversation.Message(OnConversationMessageReceived, OnFail, WATSON_CONVERSATION_WORKSPACE_ID, messageRequest))
            Log.Debug("Error", "Failed to send message");
    }

    private void CallTextToSpeech(string outputText)
    {
        Debug.Log("Sent to Watson Text To Speech: " + outputText);
        if (!_textToSpeech.ToSpeech(OnSynthesize, OnFail, outputText, false))
            Log.Debug("ExampleTextToSpeech.ToSpeech()", "Failed to synthesize!");
    }

    private void OnSynthesize(AudioClip clip, Dictionary<string, object> customData)
    {
        Debug.Log("Received audio file from Watson Text To Speech");

        if (Application.isPlaying && clip != null)
        {
            GameObject audioObject = new GameObject("AudioObject");
            AudioSource source = audioObject.AddComponent<AudioSource>();
            source.spatialBlend = 0.0f;
            source.loop = false;
            source.clip = clip;
            source.Play();

            Invoke("RecordAgain", source.clip.length);
            Destroy(audioObject, clip.length);
        }
    }

    private void RecordAgain()
    {
        Debug.Log("Played Audio received from Watson Text To Speech");
        if (!_stopListeningFlag)
        {
            OnListen();
        }
    }

    private void OnListen()
    {
        Log.Debug("ExampleStreaming", "Start();");

        Active = true;

        StartRecording();
    }

    private void OnRecognizeSpeaker(SpeakerRecognitionEvent result, Dictionary<string, object> customData)
    {
    }

    public bool Active
    {
        get { return _speechToText.IsListening; }
        set
        {
            if (value && !_speechToText.IsListening)
            {
                _speechToText.DetectSilence = true;
                _speechToText.EnableWordConfidence = false;
                _speechToText.EnableTimestamps = false;
                _speechToText.SilenceThreshold = 0.03f;
                _speechToText.MaxAlternatives = 90;
                _speechToText.EnableInterimResults = true;
                _speechToText.OnError = OnError;
                _speechToText.StartListening(OnSpeechToTextResultReceived, OnRecognizeSpeaker);
            }
            else if (!value && _speechToText.IsListening)
            {
                _speechToText.StopListening();
            }
        }
    }


    private void StartRecording()
    {

        if (_recordingRoutine == 0)
        {
            Debug.Log("Started Recording");
            UnityObjectUtil.StartDestroyQueue();
            _recordingRoutine = Runnable.Run(RecordingHandler());
        }

    }

    private void StopRecording()
    {




    }

    private void OnError(string error)
    {
        Active = false;

        Log.Debug("ExampleStreaming", "Error! {0}", error);
    }

    private IEnumerator RecordingHandler()

    {
        _recording = Microphone.Start(_microphoneID, true, _recordingBufferSize, _recordingHZ);
        yield return null;



        bool bFirstBlock = true;
        int midPoint = _recording.samples / 2;
        float[] samples = null;

        while (_recordingRoutine != 0 && _recording != null)
        {
            int writePos = Microphone.GetPosition(_microphoneID);
            if (writePos > _recording.samples || !Microphone.IsRecording(_microphoneID))
            {
                Log.Error("MicrophoneWidget", "Microphone disconnected.");

                StopRecording();
                yield break;
            }

            if ((bFirstBlock && writePos >= midPoint)
                || (!bFirstBlock && writePos < midPoint))
            {
                samples = new float[midPoint];
                _recording.GetData(samples, bFirstBlock ? 0 : midPoint);

                AudioData record = new AudioData();
                record.MaxLevel = Mathf.Max(samples);
                record.Clip = AudioClip.Create("Recording", midPoint, _recording.channels, _recordingHZ, false);
                record.Clip.SetData(samples, 0);

                _speechToText.OnListen(record);

                bFirstBlock = !bFirstBlock;
            }
            else
            {
                int remaining = bFirstBlock ? (midPoint - writePos) : (_recording.samples - writePos);
                float timeRemaining = (float)remaining / (float)_recordingHZ;

                yield return new WaitForSeconds(timeRemaining);
            }

        }

        yield break;
    }

    private void InitializeServices()
    {
        Credentials credentials = new Credentials(WATSON_CONVERSATION_USER, WATSON_CONVERSATION_PASSWORD, "https://gateway.watsonplatform.net/conversation/api");
        _conversation = new Conversation(credentials);
        _conversation.VersionDate = "2017-05-26";

        Credentials credentials2 = new Credentials(WATSON_TEXT_TO_SPEECH_USER, WATSON_TEXT_TO_SPEECH_PASSWORD, "https://stream.watsonplatform.net/text-to-speech/api");
        _textToSpeech = new TextToSpeech(credentials2);
        _textToSpeech.Voice = VoiceType.en_US_Allison;

        Credentials credentials3 = new Credentials(WATSON_SPEECH_TO_TEXT_USER, WATSON_SPEECH_TO_TEXT_PASSWORD, "https://stream.watsonplatform.net/speech-to-text/api");
        _speechToText = new SpeechToText(credentials3);
    }


    private void OnFail(RESTConnector.Error error, Dictionary<string, object> customData)
    {
        Log.Error("OnFail()", "Error received: {0}", error.ToString());
    }

}

[Serializable]
public class WeatherObservation
{
    public string obs_name;
    public string temp;
}
[Serializable]
public class WeatherInfo
{
    public WeatherObservation observation;
}