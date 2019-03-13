using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using System.IO;

public class Foo : MonoBehaviour
{
    public Camera Camera;
    // public List<string> Text; // for debug
    public Text Label;
    public string SavePath;
    public string FilePath;

    public Font[] Fonts;

    public List<Texture2D> words = new List<Texture2D>(10000);
    private Rect[] rects;

    [MenuItem("Text Generator/Incubate porcupines")]
    static void DoStuff()
    {
        var timer = System.Diagnostics.Stopwatch.StartNew();
        
        var instance = (Foo)GameObject.FindGameObjectWithTag("Bar").GetComponent<Foo>();
        var allText = File.ReadAllText(instance.FilePath);
        var separated = allText.Split();
        instance.words.ForEach(x => DestroyImmediate(x));
        instance.words.Clear();
        var i = 0;
        foreach (var item in separated)
        {
            instance.Label.text = item;
            instance.Camera.Render();
            instance.words.Add(instance.CopyToTexture());
            i++;
            if (i > 19200)
                break;
        }
        Texture2D atlas = new Texture2D(8192, 8192, TextureFormat.RGBA32, false);
        instance.rects = atlas.PackTextures(instance.words.ToArray(), 2, 8192);
        
        File.WriteAllBytes(System.IO.Path.Combine(instance.SavePath, "test.jpg"), atlas.EncodeToJPG());

        timer.Stop();
        Debug.Log(timer.ElapsedMilliseconds);
    }

    Texture2D CopyToTexture()
    {
        var currentRT = RenderTexture.active;
        RenderTexture.active = Camera.targetTexture;

        Camera.Render();

        Texture2D image = new Texture2D(Camera.targetTexture.width, Camera.targetTexture.height);
        image.ReadPixels(new Rect(0, 0, Camera.targetTexture.width, Camera.targetTexture.height), 0, 0);
        image.Apply();

        RenderTexture.active = currentRT;
        return image;
    }

    [Serializable]
    class MetaData
    {
        public Rect Rect;
        public string word;
    }
}
