using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class UploadToPlayFab : MonoBehaviour
{
    [Header("PlayFab Connection")]
    [SerializeField] private string titleId;
    [SerializeField] private string sessionTicket;
    [SerializeField] private string dataKey = "save";
    [SerializeField] private string endpointOverride;

    public bool CanUpload
    {
        get { return !string.IsNullOrWhiteSpace(GetEndpoint()) && !string.IsNullOrWhiteSpace(sessionTicket); }
    }

    public IEnumerator UploadGameSave(GameSaveData saveData, Action<bool, string> onCompleted)
    {
        if (saveData == null)
        {
            onCompleted?.Invoke(false, "Save data is null.");
            yield break;
        }

        string endpoint = GetEndpoint();
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            onCompleted?.Invoke(false, "PlayFab endpoint is not configured.");
            yield break;
        }

        if (string.IsNullOrWhiteSpace(sessionTicket))
        {
            onCompleted?.Invoke(false, "Session ticket is not configured.");
            yield break;
        }

        string body = BuildUpdateUserDataBody(JsonUtility.ToJson(saveData));
        byte[] bodyBytes = Encoding.UTF8.GetBytes(body);

        using (UnityWebRequest request = new UnityWebRequest(endpoint, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyBytes);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("X-Authorization", sessionTicket);

            yield return request.SendWebRequest();

            bool success = request.result == UnityWebRequest.Result.Success;
            string responseMessage = success ? request.downloadHandler.text : request.error;
            onCompleted?.Invoke(success, responseMessage);
        }
    }

    private string GetEndpoint()
    {
        if (!string.IsNullOrWhiteSpace(endpointOverride))
        {
            return endpointOverride;
        }

        if (string.IsNullOrWhiteSpace(titleId))
        {
            return string.Empty;
        }

        return "https://" + titleId + ".playfabapi.com/Client/UpdateUserData";
    }

    private string BuildUpdateUserDataBody(string saveJson)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append("{\"Data\":{");
        builder.Append('\"');
        builder.Append(EscapeJsonString(dataKey));
        builder.Append("\":\"");
        builder.Append(EscapeJsonString(saveJson));
        builder.Append("\"}}");
        return builder.ToString();
    }

    private static string EscapeJsonString(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        StringBuilder builder = new StringBuilder(value.Length + 16);

        for (int i = 0; i < value.Length; i++)
        {
            char character = value[i];

            switch (character)
            {
                case '\\':
                    builder.Append("\\\\");
                    break;
                case '\"':
                    builder.Append("\\\"");
                    break;
                case '\b':
                    builder.Append("\\b");
                    break;
                case '\f':
                    builder.Append("\\f");
                    break;
                case '\n':
                    builder.Append("\\n");
                    break;
                case '\r':
                    builder.Append("\\r");
                    break;
                case '\t':
                    builder.Append("\\t");
                    break;
                default:
                    if (character < 32)
                    {
                        builder.Append("\\u");
                        builder.Append(((int)character).ToString("x4"));
                    }
                    else
                    {
                        builder.Append(character);
                    }

                    break;
            }
        }

        return builder.ToString();
    }
}
