
using System;

public class JsonResponseEvents
{
    public event Action<string> onJsonResponse;
    public event Action<string> onJsonValidationResponse;

    public void JsonResponse(string responseText) {
        onJsonResponse?.Invoke(responseText);
    }

    public void JsonValidationResponse(string responseText) {
        onJsonResponse?.Invoke(responseText);
    }
}
