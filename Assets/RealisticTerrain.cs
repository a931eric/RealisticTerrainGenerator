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
        public bool useTexture = false;
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
        [Range(-4, 4)]
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

        void OnEnable()
        {
            if (UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetPrefabStage(gameObject) != null) { return; }
            if (Application.isPlaying) { enabled = false; return; }
            modified = false;
            SceneView.duringSceneGui += OnScene;
            brush.weight = 0.8f;
            brush.size = 1.5f;
            allSurfaces = GetComponentsInChildren<Surface>();
            if (path == "")
            {
                InitSurfaces();
            }
            else
            {
                Load();
            }
            structure.Hide();
            realHeight.Display();
            structure.DisableCollider();
            realHeight.EnableCollider();
            Sat = new Texture2D(1,1);
            Tex.mainTexture=Sat;
        }
        void OnDisable()
        {
            if (UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetPrefabStage(gameObject) != null) { return; }
            if (Application.isPlaying) { enabled = false; return; }
            //Save();  /* strange exception if call Save() here */
            SceneView.duringSceneGui -= OnScene;

        }

        private void Start()
        {
            if (UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetPrefabStage(gameObject) != null) { return; }
            if (Application.isPlaying) { enabled = false; return; }
            structure.Hide();
            realHeight.Display();
        }

        bool wasDraging = false;
        public bool autoUpdate = false;
        public void OnScene(SceneView _)
        {
            if (c1 != c1_) { c1_ = c1; c[1].Flatten(c1); GAN(); }
            if (c2 != c2_) { c2_ = c2; c[2].Flatten(c2); GAN(); }
            if (c3 != c3_) { c3_ = c3; c[3].Flatten(c3); GAN(); }
            if (c4 != c4_) { c4_ = c4; c[4].Flatten(c4); GAN(); }
            if (c5 != c5_) { c5_ = c5; c[5].Flatten(c5); GAN(); }
            if (c6 != c6_) { c6_ = c6; c[6].Flatten(c6); GAN(); }
            if (c7 != c7_) { c7_ = c7; c[7].Flatten(c7); GAN(); }
            if (c0 != c0_) { c0_ = c0; c[0].Flatten(c0); GAN(); }
            if (useTexture != p_useTexture)
            {
                Tex.mainTexture = Sat;
                p_useTexture = useTexture;
                foreach (var m in realHeight.GetComponentsInChildren<MeshRenderer>())
                    m.material = useTexture ? Tex : Fake;
            }
            Event e = Event.current;
            if (e.type == EventType.MouseDrag)
            {
                
                structure.EnableCollider();
                realHeight.DisableCollider();
                if (structure.Draw(brush)|| c[0].Draw(brush)|| c[1].Draw(brush)|| c[2].Draw(brush)|| c[3].Draw(brush))
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
            modified = false;
        }
        [Button()]
        public void Load()
        {
            foreach (Surface s in allSurfaces)
            {
                s.LoadHeightmap_(path + "/" + s.gameObject.name + ".png");
            }
            structure.Hide();
            realHeight.Display();
            structure.DisableCollider();
            realHeight.EnableCollider();
            wasDraging = false;
            foreach (var m in structure.GetComponentsInChildren<MeshRenderer>())
                m.material = Fake;
            foreach (var m in realHeight.GetComponentsInChildren<MeshRenderer>())
                m.material = Fake;
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
    }
}
