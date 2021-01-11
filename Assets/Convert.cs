using System.IO;
using UnityEditor;
using UnityEngine;

namespace RealisticTerrainGenerator
{
    public static class Convert
    {
        public static float cs = 256,cs2=65536,bias=.4f;
        public static void Png2Surface(byte[] png, Surface targetSurface, string mode = "RGB")
        {
            targetSurface.map = Png2Map(png, mode, targetSurface.heightScale);
            targetSurface.UpdateHeight();
        }
        public static float[,] Png2Map(byte[] png, string mode = "RGB", float heightScale = 80)
        {
            Texture2D texture = new Texture2D(2, 2, textureFormat: TextureFormat.RGBAFloat, false)
            {
                filterMode = FilterMode.Point
            };
            texture.LoadImage(png);
            int w= texture.width;
            int h = texture.height;
            float[,] o = new float[h,w];
            if (mode == "GRAY")
                for (int i = 0; i < w; i++)
                {
                    for (int j = 0; j < h; j++)
                    {
                        Color p = texture.GetPixel(i, j);
                        o[j,i] = (p.r - bias) * heightScale;
                    }
                }
            else if (mode == "RGB")
                for (int i = 0; i < w; i++)
                {
                    for (int j = 0; j < h; j++)
                    {
                        Color p = texture.GetPixel(i, j);
                        o[j,i] = (p.r- bias + p.g / cs + p.b / cs2)* heightScale;
                    }
                }
            return o;
        }
        public static Color[,] Png2ColorMap(byte[] png)
        {
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(png);
            int w = texture.width;
            int h = texture.height;
            Color[,] o = new Color[w, h];
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    o[j,i] = texture.GetPixel(i, j);
                }
            }
            return o;
        }
        public static byte[] Surface2Png(Surface s, string mode = "RGB")
        {
            return Map2Png(s.map, mode, heightScale: s.heightScale);
        }
        static float mod1(float x)
        {
            return x-Mathf.Floor(x);
        }
        public static byte[] Map2Png(float[,] map, string mode = "RGB", float heightScale = 80)
        {
            int w = map.GetLength(1);
            int h = map.GetLength(0);
            Texture2D o = new Texture2D(w, h,textureFormat: TextureFormat.RGBAFloat,false);
            o.filterMode = FilterMode.Point;
            Color[] img = new Color[w * h];
            if (mode == "GRAY")
                for (int i = 0; i < w; i++)
                {
                    for (int j = 0; j < h; j++)
                    {
                        float v = (map[j,i] ) / heightScale-bias;
                        img[i + j * w] = new Color(v, v, v);
                    }
                }
            else if (mode == "RGB")
                for (int i = 0; i < w; i++)
                {
                    for (int j = 0; j < h; j++)
                    {
                        float v = (map[j,i] ) / heightScale; //-0.5 ~ +0.5
                        img[i + j * w] = new Color(v- (mod1(v * cs))/cs + bias, (mod1(v *cs) ) - ((mod1(v * cs2 ) ) / cs), mod1(v *cs2 ));
                    }
                }
            o.SetPixels(img);
            o.Apply();
            return o.EncodeToPNG();
        }
    }
    public static class IO
    {
        public static Object SavePng(byte[] png, string path)
        {
            AssetDatabase.DeleteAsset(path);
            File.WriteAllBytes(path, png);
            AssetDatabase.ImportAsset(path);
            return AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D));
        }
        public static byte[] ReadPng(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Open);
            byte[] png = new byte[fs.Length];
            fs.Read(png, 0, (int)fs.Length);
            fs.Close();
            return png;
        }
    }
}