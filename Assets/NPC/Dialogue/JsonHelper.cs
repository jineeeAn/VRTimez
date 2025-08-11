using System;
using UnityEngine;

public static class JsonHelper
{
    [Serializable] private class Wrapper<T> { public T[] Items; }

    public static T[] FromJsonArray<T>(string json)
    {
        // 배열을 감싸서 JsonUtility 가 읽게 함
        string wrapped = "{\"Items\":" + json + "}";
        return JsonUtility.FromJson<Wrapper<T>>(wrapped).Items;
    }
}
