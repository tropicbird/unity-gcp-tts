using Newtonsoft.Json.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class GoogleTextToSpeech : MonoBehaviour
{
    private string url = "https://texttospeech.googleapis.com/v1beta1/text:synthesize";

    // MEMO: Accsess token can be generated by "gcloud auth application-default print-access-token" in terminal
    //       The access token is valid only one hour.
    private string accessToken = "<ACCESS_TOKEN>";
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void ConvertTextToSpeech(string text)
    {
        StartCoroutine(SendRequest(text));
    }

    private IEnumerator SendRequest(string text)
    {
        string jsonBody = @"
        {
            'audioConfig': {
                'audioEncoding':'LINEAR16',
                'pitch': '0.00',
                'speakingRate': '1.00'
            },
            'input': {
                'text': '" + text + @"'
            },
            'voice': {
                'languageCode': 'ja-JP',
                'name': 'ja-JP-Neural2-C'
            }
        }";

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Authorization", $"Bearer {accessToken}");
        request.SetRequestHeader("Content-Type", "application/json; charset=utf-8");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string jsonResponse = request.downloadHandler.text;
            JObject json = JObject.Parse(jsonResponse);
            string audioContent = json["audioContent"].ToString();

            byte[] audioData = System.Convert.FromBase64String(audioContent);

            // MEMO: AudioClip is generated
            AudioClip audioClip = WavUtility.ToAudioClip(audioData);

            // MEMO: Sound is played
            audioSource.clip = audioClip;
            audioSource.Play();
        }
        else
        {
            Debug.LogError($"Error: {request.error}");
            Debug.Log($"Sending request with body: {jsonBody}");
        }
    }
}
