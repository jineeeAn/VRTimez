using System;
using System.IO;
using UnityEngine;

public static class SavWav
{
    public static byte[] GetWavBytes(AudioClip clip, bool trim = false)
    {
        const int HEADER_SIZE = 44;

        MemoryStream stream = new MemoryStream();
        int samples = clip.samples * clip.channels;
        float[] data = new float[samples];
        clip.GetData(data, 0);

        // float -> 16-bit PCM 변환
        short[] intData = new short[data.Length];
        byte[] bytesData = new byte[data.Length * 2];
        const float rescaleFactor = 32767;

        for (int i = 0; i < data.Length; i++)
        {
            intData[i] = (short)(data[i] * rescaleFactor);
            BitConverter.GetBytes(intData[i]).CopyTo(bytesData, i * 2);
        }

        // WAV 헤더 작성
        int hz = clip.frequency;
        int channels = clip.channels;
        int byteRate = hz * channels * 2;

        stream.Write(System.Text.Encoding.UTF8.GetBytes("RIFF"), 0, 4);
        stream.Write(BitConverter.GetBytes(HEADER_SIZE + bytesData.Length - 8), 0, 4);
        stream.Write(System.Text.Encoding.UTF8.GetBytes("WAVE"), 0, 4);
        stream.Write(System.Text.Encoding.UTF8.GetBytes("fmt "), 0, 4);
        stream.Write(BitConverter.GetBytes(16), 0, 4);
        stream.Write(BitConverter.GetBytes((short)1), 0, 2);
        stream.Write(BitConverter.GetBytes((short)channels), 0, 2);
        stream.Write(BitConverter.GetBytes(hz), 0, 4);
        stream.Write(BitConverter.GetBytes(byteRate), 0, 4);
        stream.Write(BitConverter.GetBytes((short)(channels * 2)), 0, 2);
        stream.Write(BitConverter.GetBytes((short)16), 0, 2);
        stream.Write(System.Text.Encoding.UTF8.GetBytes("data"), 0, 4);
        stream.Write(BitConverter.GetBytes(bytesData.Length), 0, 4);
        stream.Write(bytesData, 0, bytesData.Length);

        return stream.ToArray();
    }
}
