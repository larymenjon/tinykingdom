using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class UploadToFirebase : MonoBehaviour
{
    [Header("Firebase Connection")]
    [SerializeField] private string databaseUrl;
    [SerializeField] private string authToken;
    [SerializeField] private string userId;
    [SerializeField] private string remotePath = "saves";
    [SerializeField] private bool appendUserId = true;

    public bool CanUpload
    {
        get { return !string.IsNullOrWhiteSpace(GetEndpoint()); }
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
            onCompleted?.Invoke(false, "Firebase endpoint is not configured.");
            yield break;
        }

        string body = JsonUtility.ToJson(saveData);
        byte[] bodyBytes = Encoding.UTF8.GetBytes(body);

        using (UnityWebRequest request = new UnityWebRequest(endpoint, "PUT"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyBytes);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            bool success = request.result == UnityWebRequest.Result.Success;
            string responseMessage = success ? request.downloadHandler.text : request.error;
            onCompleted?.Invoke(success, responseMessage);
        }
    }

    private string GetEndpoint()
    {
        if (string.IsNullOrWhiteSpace(databaseUrl))
        {
            return string.Empty;
        }

        StringBuilder builder = new StringBuilder();
        builder.Append(databaseUrl.TrimEnd('/'));
        builder.Append('/');
        builder.Append(remotePath.Trim('/'));

        if (appendUserId && !string.IsNullOrWhiteSpace(userId))
        {
            builder.Append('/');
            builder.Append(userId);
        }

        builder.Append(".json");

        if (!string.IsNullOrWhiteSpace(authToken))
        {
            builder.Append("?auth=");
            builder.Append(UnityWebRequest.EscapeURL(authToken));
        }

        return builder.ToString();
    }
}
