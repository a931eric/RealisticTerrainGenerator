using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Net.Http;

namespace RealisticTerrainGenerator
{



    public enum BrushType
    {
        __off__,
        Structure,
        C0, C1, C2, C3, C4, C5, C6, C7
    }
    
    public struct Brush
    {
        public BrushType type;
        [Range(-3f, 8)]
        public float size;
        [Range(-3f, 3f)]
        public float weight;
    }

    [ExecuteInEditMode]
    public class RealisticTerrain : MonoBehaviour
    {
        public Material Tex,Fake;
        public Texture2D Sat;
        public bool useTexture = true;
        bool p_useTexture = false;

        public Surface structure, realHeight;
        public Surface[] c;
        public Surface[] allSurfaces;
        bool modified = false;

        [ShowInInspector]
        public Brush brush;


        [BoxGroup("Base setting")]
        [Range(1, 1024)]
        public int width = 256, height = 256;
        [BoxGroup("Base setting")]
        [Range(1, 255)]
        public int chunkSize = 200;
        [Range(-3, 3)]
        public float c0,c1, c2, c3, c4, c5, c6, c7;
        float c1_, c2_, c3_, c4_, c5_, c6_, c7_, c0_;

      

        [BoxGroup("Base setting")]
        [Button("Apply")]
        public void InitSurfaces()
        {
            structure.Resize(width, height, chunkSize);
            realHeight.Resize(width, height, chunkSize);
            foreach (Surface s in c)
            {
                s.Resize(width, height, chunkSize);
            }
            foreach (var m in structure.GetComponentsInChildren<MeshRenderer>())
                m.material = Fake;
            foreach (var m in realHeight.GetComponentsInChildren<MeshRenderer>())
                m.material = Fake;
        }
        private void Update()
        {
            if (c1 != c1_) { c1_ = c1; c[1].Flatten(c1); GAN(); }
            if (c2 != c2_) { c2_ = c2; c[2].Flatten(c2); GAN(); }
            if (c3 != c3_) { c3_ = c3; c[3].Flatten(c3); GAN(); }
            if (c4 != c4_) { c4_ = c4; c[4].Flatten(c4); GAN(); }
            if (c5 != c5_) { c5_ = c5; c[5].Flatten(c5); GAN(); }
            if (c6 != c6_) { c6_ = c6; c[6].Flatten(c6); GAN(); }
            if (c7 != c7_) { c7_ = c7; c[7].Flatten(c7); GAN(); }
            if (c0 != c0_) { c0_ = c0; c[0].Flatten(c0); GAN(); }// HAIYAA repeating code
            if (useTexture != p_useTexture)
            {
                Tex.mainTexture = Sat;
                p_useTexture = useTexture;
                foreach (var m in realHeight.GetComponentsInChildren<MeshRenderer>())
                    m.material = useTexture ? Tex : Fake;
            }
        }
        void OnEnable()
        {
            //if (UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetPrefabStage(gameObject) != null) { return; }
            //if (Application.isPlaying) { enabled = false; return; }
            modified = false;
            SceneView.duringSceneGui += OnScene;
            brush.weight = 0.8f;
            brush.size = 1.5f;
            allSurfaces = GetComponentsInChildren<Surface>();
            Sat = new Texture2D(1, 1);
            if (path == "")
            {
                InitSurfaces();
            }
            else
            {
                Load();
                useTexture = true;
                p_useTexture = false;
            }
            structure.Hide();
            realHeight.Display();
            structure.DisableCollider();
            realHeight.EnableCollider();
            
            Tex.mainTexture=Sat;
            
        }
        void OnDisable()
        {
            //if (UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetPrefabStage(gameObject) != null) { return; }
            //if (Application.isPlaying) { enabled = false; return; }
            //Save();  /* strange exception if call Save() here */
            SceneView.duringSceneGui -= OnScene;

        }

        private void Start()
        {
            //if (UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetPrefabStage(gameObject) != null) { return; }
            //if (Application.isPlaying) { enabled = false; return; }
            structure.Hide();
            realHeight.Display();
        }

        bool wasDraging = false;
        public bool autoUpdate = false;
        public void OnScene(SceneView _)
        {

            Event e = Event.current;
            if (e.type == EventType.MouseDrag)
            {
                
                structure.EnableCollider();
                realHeight.DisableCollider();
                if (structure.Draw(brush) || c[0].Draw(brush) || c[1].Draw(brush) || c[2].Draw(brush) || c[3].Draw(brush) || c[4].Draw(brush) || c[5].Draw(brush) || c[6].Draw(brush) || c[7].Draw(brush))
                {
                    if (!wasDraging)
                    {
                        structure.Display();
                        realHeight.Hide();
                    }
                    wasDraging = true;
                    modified = true;
                }
            }
            
            if (e.type == EventType.MouseUp)
            {
                wasDraging = false;
                if (autoUpdate)
                {
                    structure.DisableCollider();
                    realHeight.EnableCollider();
                    if (modified)
                    {
                        structure.Hide();
                        realHeight.Display();
                        GAN();
                        modified = false; 
                    }
                }
            }
        }

        public string path;
        [Button()]
        public void Save()
        {
            //if (!modified) return; 
            if (path == "")
            {
                path = AssetDatabase.GUIDToAssetPath(AssetDatabase.CreateFolder("Assets/Heightmaps", "h0"));
                EditorUtility.SetDirty(this);
            }
            foreach (Surface s in allSurfaces)
            {
                s.SaveHeightmap_(path + "/" + s.gameObject.name + ".png");
            }
            IO.SavePng( Sat.EncodeToPNG(), path + "/" + "sat.png");
            modified = false;
        }
        [Button()]
        public void Load()
        {
            foreach (Surface s in allSurfaces)
            {
                s.LoadHeightmap_(path + "/" + s.gameObject.name + ".png");
            }
            Sat.LoadImage(IO.ReadPng(path + "/" + "sat.png"));
            structure.Hide();
            realHeight.Display();
            structure.DisableCollider();
            realHeight.EnableCollider();
            wasDraging = false;
            foreach (var m in structure.GetComponentsInChildren<MeshRenderer>())
                m.material = Fake;
            foreach (var m in realHeight.GetComponentsInChildren<MeshRenderer>())
                m.material = Fake;
            if (useTexture)
            {
                p_useTexture = false;
            }
        }

        class PostData
        {
            public string structure;
            public string[] latent;
        }
        class ResponseData
        {
            public string hei_path;
            public string sat_path;
        }


        public string serverURL;
        public bool waitingGAN = false;
        [Button()]
        public void GAN_button()
        {
            waitingGAN = false;
            GAN_();
        }
        public void GAN()
        {
            GAN_();
        }
        async void GAN_()
        {
            if (waitingGAN) return;
            waitingGAN = true;
            HttpClient client = new HttpClient { Timeout = System.TimeSpan.FromSeconds(30) };
            string response;
            var latent = new string[8];
            for(int i=0;i<c.Length;i++)
            {
                latent[i] = System.Convert.ToBase64String(Convert.Surface2Png(c[i]));
            }
            using (var content = new StringContent(JsonUtility.ToJson(new PostData
            {
                structure = System.Convert.ToBase64String(Convert.Surface2Png(structure)),
                latent = latent
            }
            ), System.Text.Encoding.UTF8, "application/json"))
            {
                var postResult = await client.PostAsync(serverURL+ "/generate", content);
                response = await postResult.Content.ReadAsStringAsync();
            }
            ResponseData responseData = JsonUtility.FromJson<ResponseData>(response);
            print("response:\n" + responseData.hei_path+","+responseData.sat_path);
            byte[] heiData = await client.GetByteArrayAsync(serverURL+"/" + responseData.hei_path);
            Convert.Png2Surface(heiData, realHeight);
            byte[] satData = await client.GetByteArrayAsync(serverURL + "/" + responseData.sat_path);
            Sat.LoadImage(satData);
            waitingGAN = false;
        }
        public string heiLoc, satLoc;
        [Button()]
        public async void Fetch()
        {
            HttpClient client = new HttpClient { Timeout = System.TimeSpan.FromSeconds(30) };
            byte[] heiData = await client.GetByteArrayAsync(serverURL + "/" +heiLoc);
            Convert.Png2Surface(heiData, realHeight);
            byte[] satData = await client.GetByteArrayAsync(serverURL + "/" + satLoc);
            Sat.LoadImage(satData);
        }
        [Button()]
        public void SetLatent_()
        {
            double[] la = { 0.6694, 0.8358, 1.2639, 1.2298, 0.1992, -0.6653, 0.2231, -0.1572 };
            double[] lb = { 0.5868, 0.3955, 1.2334, 0.8947, 0.2984, -1.4603, 0.4955, 0.2712 };
            double[] lc = { -1.2452, -0.8758, 0.8310, 0.6876, -0.9335, -2.2361, 0.7873, -1.7139 };
            double[] ld = { -0.6908, -1.0387, 1.5280, 0.5463, -1.1701, -2.1211, -0.0423, -2.4578 };
            
            for(int i = 0; i < 8; i++)
            {
                for (int x = 0; x < width; x++)
                {
                    float xp = (float)x / width;
                    
                    xp = xp * 3 - 1f;
                    for (int y = 0; y < height; y++)
                    {
                        float yp = (float)y / height;
                        
                        yp = yp * 3 - 1f;
                        
                        c[i].map[y, x] =(float) (xp * yp * la[i] + xp * (1-yp) * lb[i] + (1-xp) * yp * lc[i] + (1-xp) * (1-yp) * ld[i]);
                        realHeight.map[y, x] = xp;
                        
                    }
                }
            }
            realHeight.UpdateHeight();
        }
        public enum Presets
        {
            Arge,
            Cana,
            Peru,
            Heng,
            Hima,
            Taiw,
            火,
            莽原,
            藍黃白,
            白,
            淺黃白
        }
        [BoxGroup("Presets")]
        [ShowInInspector]
        Presets preset;
        [BoxGroup("Presets")]
        [Button()]
        public void ApplyPreset()
        {
            waitingGAN = false;
            switch (preset)
            {
                case Presets.Arge:
                    SetLatent(new double[] { 0.9835, 1.2640, 0.2286, 0.4306, 0.7124, 1.2041, -0.2137, 1.0511 });
                    return;
                case Presets.Cana:
                    SetLatent(new double[] { 0.9009, 0.8237, 0.1981, 0.0955, 0.8115, 0.4091, 0.0587, 1.4795 });
                    return;
                case Presets.Heng:
                    SetLatent(new double[] { -0.5768, -1.0298, -0.7151, -0.1617, -0.4465, -0.9950, 0.2835, -0.7756 });
                    return;
                case Presets.Hima:
                    SetLatent(new double[] { -0.9311, -0.4475, -0.2043, -0.1115, -0.4204, -0.3666, 0.3506, -0.5056 });
                    return;
                case Presets.Peru:
                    SetLatent(new double[] { -0.3766, -0.6104, 0.4927, -0.2529, -0.6570, -0.2516, -0.4791, -1.2494 });
                    return;
                case Presets.火:
                    SetLatent(new double[] { -2,-2,-2,-2,-2,-2,-2,-2 });
                    return;
                case Presets.莽原:
                    SetLatent(new double[] { -0.57, -0.83, -1.99, 1.82, -2, 3, -1.74, -0.7 });
                    return;
                case Presets.藍黃白:
                    SetLatent(new double[] { 0.69,0.39,-1.11,1.7,-1.63,-0.36,-2.75,1.41 });
                    return;
                case Presets.Taiw:
                    SetLatent(new double[] { -1.6983, -0.4617, -1.4814, 0.1801, -0.7580, -0.8502, 0.7216, -0.1693 });
                    return;
                case Presets.白:
                    SetLatent(new double[] { 2, 2.83, 2.73, 0.7, 2.78, 2.7,0.59, 2.58 });
                    return;
                case Presets.淺黃白:
                    SetLatent(new double[] {1.98,-.38,3,-1.06,-1.7,3,0.59,2.9 });
                    return;

            }
        }
        public void SetLatent(double [] latent)
        {
            /*
            print(c[0]);
            print(latent[0]);
            print(c[0].map);*/
            for (int i = 0; i < 8; i++)
            {
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        c[i].map[y, x] = (float)latent[i];
                    }
                }
            }
            c0 = (float)latent[0];
            c1 = (float)latent[1];
            c2 = (float)latent[2];
            c3 = (float)latent[3];
            c4 = (float)latent[4];
            c5 = (float)latent[5];
            c6 = (float)latent[6];
            c7 = (float)latent[7];
            if (autoUpdate) GAN();
        }
    }
}
