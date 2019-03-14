using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using System.IO;
using System.Threading.Tasks;
using Stopwatch = System.Diagnostics.Stopwatch;
using Unity.EditorCoroutines.Editor;

public class TextPacker : MonoBehaviour
{
    public Camera Camera;
    // public List<string> Text; // for debug
    public Text Label;
    public string DestinationFolder;
    public string InputTextPath;
    public string SeparatorCharacters = " ;,.";
    public ImageType ImageFormat;
    public bool SaveInSeparateImages = false;

    public Font[] Fonts;

    public bool InProgress { get; protected set; } = false;
    public int WordsSaved { get; protected set; }
    public float Progress { get; protected set; } = 0f;
    [Header("Randomization seed to randomize fonts distribution for text")]
    public int RandomSeed = 42;

    List<Texture2D> wordTextures = new List<Texture2D>(10000);
    List<MetaData> texturesMetaData = new List<MetaData>(10000);
    Rect[] rects;

    EditorCoroutine routine;

    public void Generate()
    {
        routine = EditorCoroutineUtility.StartCoroutine(generateBlocking(), this);
    }

    public void StopGeneration()
    {
        EditorCoroutineUtility.StopCoroutine(routine);
        InProgress = false;
    }

    public IEnumerator generateBlocking()
    {
        var timer = Stopwatch.StartNew();
        InProgress = true;
        var allText = string.Empty;
        try
        {
            allText = File.ReadAllText(InputTextPath);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Problem with reading the text file");
            Debug.LogError(e.ToString());
            InProgress = false;
            yield break;
        }
        Debug.Log("Read the file");

        var separated = allText.Trim().Split(SeparatorCharacters.ToArray(), StringSplitOptions.RemoveEmptyEntries);
        Debug.Log($"Text is split. {separated.Length} words detected." +
            $"Max Length of word: {separated.Max(x => x.Length)}");

        wordTextures.ForEach(x => DestroyImmediate(x));
        wordTextures.Clear();

        texturesMetaData.Clear();

        var rand = new System.Random(RandomSeed);

        for (int i = 0; i < separated.Length; i++)
        {
            Label.text = separated[i];
            Label.font = Fonts[rand.Next(0, Fonts.Length)];
            wordTextures.Add(CopyToTexture());
            texturesMetaData.Add(new MetaData() { Word = Label.text, Font = Label.font.name, FontSize = Label.fontSize });

            Progress = (float)i / (float)separated.Length;
            if (i % 100 == 0)
                yield return null;

            if (i > 19200)
                break;
        }

        Debug.Log("Textures Generated");

        string extension = null;
        switch (ImageFormat)
        {
            case ImageType.JPG:
                extension = "jpg";
                break;
            case ImageType.PNG:
                extension = "png";
                break;
        }

        if (SaveInSeparateImages)
        {
            for (int i = 0; i < separated.Length; i++)
            {
                byte[] bytes = null;
                switch (ImageFormat)
                {
                    case ImageType.JPG:
                        bytes = wordTextures[i].EncodeToJPG();
                        break;
                    case ImageType.PNG:
                        bytes = wordTextures[i].EncodeToPNG();
                        break;
                }

                string fileName = $"TextAtlas{i:000000000}.{extension}";
                File.WriteAllBytes(System.IO.Path.Combine(DestinationFolder, fileName), bytes);
                this.texturesMetaData[i].Rect = new Rect(0, 0, wordTextures[i].width, wordTextures[i].height);
                this.texturesMetaData[i].FileName = fileName;
            }
        }
        else
        {
            var atlas = new Texture2D(8192, 8192, TextureFormat.RGBA32, false);
            Debug.Log("Packing textures");
            var packerTimer = Stopwatch.StartNew();
            rects = atlas.PackTextures(wordTextures.ToArray(), 2, 8192);

            Debug.Log($"Textures packed ({packerTimer.ElapsedMilliseconds}ms)");

            byte[] bytes = null;
            switch (ImageFormat)
            {
                case ImageType.JPG:
                    bytes = atlas.EncodeToJPG();
                    break;
                case ImageType.PNG:
                    bytes = atlas.EncodeToPNG();
                    break;
            }

            File.WriteAllBytes(System.IO.Path.Combine(DestinationFolder, $"TextAtlas.{extension}"), bytes);

            for (int i = 0; i < rects.Length; i++)
            {
                var temp = rects[i];
                this.texturesMetaData[i].Rect = new Rect((int)(temp.x * atlas.width),
                                                        (int)(temp.y * atlas.height),
                                                        (int)(temp.width * atlas.width),
                                                        (int)(temp.height * atlas.height));
                this.texturesMetaData[i].FileName = $"TextAtlas.{extension}";
            }

        }
        var json = JsonUtility.ToJson(new MetaDataList(texturesMetaData), true);

        File.WriteAllText(System.IO.Path.Combine(DestinationFolder, $"MetaData.json"), json);

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
        // Graphics.CopyTexture(Camera.targetTexture, image);

        RenderTexture.active = currentRT;
        return image;
    }

    [Serializable]
    class MetaData
    {
        public string FileName;
        public Rect Rect;
        public string Word;
        public string Font;
        public int FontSize;
    }

    [Serializable]
    class MetaDataList
    {
        public MetaDataList(List<MetaData> list)
        {
            this.List = list;
        }

        public List<MetaData> List;
    }

    public enum ImageType
    {
        JPG,
        PNG
    }
}
