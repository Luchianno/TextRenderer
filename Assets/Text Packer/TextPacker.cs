using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using System.IO;

// [CreateAssetMenu(menuName = "TextPacker/Settings")]
public class TextPacker : MonoBehaviour
{
    public Camera Camera;
    // public List<string> Text; // for debug
    public Text Label;
    public string SavePath;
    public string FilePath;
    public string SeparatorCharacters = " ;,.";
    public ImageType ImageFormat;
    public bool SaveInSeparateImages = false;

    public Font[] Fonts;

    public List<Texture2D> words = new List<Texture2D>(10000);
    public Rect[] rects;

    public bool InProgress { get; protected set; } = false;
    public int WordsSaved { get; protected set; }
    public int Progress { get; protected set; }

public void Generate(){

}

    public IEnumerator generateRoutine()
    {
        var timer = System.Diagnostics.Stopwatch.StartNew();
        InProgress = true;
        var allText = string.Empty;
        try
        {
            allText = File.ReadAllText(FilePath);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Problem with reading the text file");
            Debug.LogError(e.ToString());
        }

        var separated = allText.Trim().Split(SeparatorCharacters.ToArray(), StringSplitOptions.RemoveEmptyEntries);

        words.ForEach(x => DestroyImmediate(x));
        words.Clear();

        var i = 0;
        foreach (var item in separated)
        {
            Label.text = item;
            Camera.Render();
            words.Add(CopyToTexture());
            i++;
            if (i > 19200)
                break;
            
        }

        if (SaveInSeparateImages)
        {

        }
        else
        {
            Texture2D atlas = new Texture2D(8192, 8192, TextureFormat.RGBA32, false);
            rects = atlas.PackTextures(words.ToArray(), 2, 8192);
            foreach (var item in rects)
            {

            }

            byte[] bytes = null;
            string extension = null;
            switch (ImageFormat)
            {
                case ImageType.JPG:
                    bytes = atlas.EncodeToJPG();
                    extension = "jpg";
                    break;
                case ImageType.PNG:
                    bytes = atlas.EncodeToPNG();
                    extension = "png";
                    break;
                    // case ImageType.JPG:
                    //     bytes = atlas.EncodeToJPG();
                    //     break;
                    // case ImageType.JPG:
                    //     bytes = atlas.EncodeToJPG();
                    //     break;

            }
            File.WriteAllBytes(System.IO.Path.Combine(SavePath, $"test.{extension}"), bytes);
        }
        InProgress = false;
        timer.Stop();
        Debug.Log($"Time Elapsed {timer.ElapsedMilliseconds}ms");
    }

    public Texture2D CopyToTexture()
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
        public int FileId;
        public Rect Rect;
        public string Word;
        public string Font;
        public int FontSize;
    }

    public enum ImageType
    {
        JPG,
        PNG
    }
}
