using UnityEngine;
using Sirenix.OdinInspector;
using UnityEditor;

namespace RealisticTerrainGenerator
{


    [ExecuteInEditMode]
    public class Surface : MonoBehaviour
    {
        public int width = 256, height = 256;
        public int chunkSize = 200;

        public bool useCollider=false;
        public bool useColliderInPlayMode = false;
        public bool clampZero = false;

        public void Display()
        {
            if (chunks == null) return;
            foreach (Chunk c in chunks)
            {
                c.mr.enabled = true;
                //if(useCollider)
                //    c.meshCollider.enabled = true;
            }
                
        }
        public void Hide()
        {
            if (chunks == null) return;
            foreach (Chunk c in chunks)
            {
                c.mr.enabled = false;
               // if (useCollider)
                //    c.meshCollider.enabled = false;
            }

        }

        public void Resize(int w,int h,int chunkSize_=200, bool newMap = true)
        {
            width = w;
            height = h;
            chunkSize = chunkSize_;
            if(newMap)map = new float[height, width];
            int childCount = transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }
            ChunkNX = width / chunkSize + (width % chunkSize == 0 ? 0 : 1);
            ChunkNY = height / chunkSize + (height % chunkSize == 0 ? 0 : 1);
            chunks = new Chunk[ChunkNY, ChunkNX];
            for (int i = 0; i < ChunkNY; i++)
            {
                for (int j = 0; j < ChunkNX; j++)
                {
                    Chunk newChunk = Instantiate(chunkPrefab, transform).GetComponent<Chunk>();
                    chunks[i, j] = newChunk;
                    newChunk.posX = chunkSize * j; newChunk.posY = chunkSize * i;
                    newChunk.W = w;newChunk.H = h;
                    newChunk.w = (j == ChunkNX - 1 && width % chunkSize != 0) ? width % chunkSize : chunkSize;
                    if (j != ChunkNX - 1) newChunk.w++;
                    newChunk.h = (i == ChunkNY - 1 && height % chunkSize != 0) ? height % chunkSize : chunkSize;
                    if (i != ChunkNY - 1) newChunk.h++;
                    newChunk.useCollider = useCollider;
                    newChunk.Init();
                    newChunk.UpdateHeight(map);
                }
            }
        }
        int ChunkNX = 0;
        int ChunkNY = 0;

        public GameObject chunkPrefab;
        Chunk[,] chunks;
        public float[,] map;

        float[] gauss = new float[3000];

        public void UpdateHeight()
        {
            foreach (Chunk c in chunks)
            {
                c.UpdateHeight(map);
            }
        }
        
        public  void EnableCollider()
        {
            foreach (Chunk c in chunks)
            {
                c.meshCollider.enabled = true;
            }
        }
        public void DisableCollider()
        {
            foreach (Chunk c in chunks)
            {
                c.meshCollider.enabled = false;
            }
        }
        private void Start()
        {
            if (UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetPrefabStage(gameObject) != null) { return; }
            if (Application.isPlaying)
            {
                enabled = false; return;
            }
            for (int i = 0; i < gauss.Length; i++)
            {
                gauss[i] = Mathf.Exp(-i * 0.005f);
            }
            UpdateHeight();
        }

        public bool Draw(Brush brush)
        {
            if (brush.type.ToString()!=gameObject.name) return false;
            Event e = Event.current;
            Vector3 mousePosition = e.mousePosition;
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);

                int x, z;
                float smoothness = 10f / Mathf.Exp(brush.size);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector3 hitPoint = transform.worldToLocalMatrix.MultiplyPoint(hit.point);
                x = (int)hitPoint.x;
                z = (int)hitPoint.z;

                for (int i = x - 400; i < x + 400; i++)
                {
                    for (int j = z - 400; j < z + 400; j++)
                    {
                        if (i < width && i >= 0 && j < height && j >= 0)
                        {
                            int d = (int)Mathf.Floor(smoothness * ((i - x) * (i - x) + (j - z) * (j - z)));
                            if (d < gauss.Length)
                                if (clampZero)
                                    map[j, i] = Mathf.Max(0, map[j, i] + gauss[d] / Mathf.Sqrt(smoothness) * (e.shift ? -1 : 1) * brush.weight);
                                else
                                    map[j, i] = map[j, i] + gauss[d] / Mathf.Sqrt(smoothness) * (e.shift ? -1 : 1) * brush.weight;

                        }
                    }

                }

                UpdateHeight();
                return true;
            }
            return false;
        }
        [BoxGroup("Heightmap")]
        public float heightScale = 30;
        public void SaveHeightmap_(string path)
        {
            byte[] png = Convert.Map2Png(map, mode: "RGB",heightScale: heightScale);
            IO.SavePng(png, path);
        }
        [BoxGroup("Heightmap")]
        public float load_bias = 0;
        public void LoadHeightmap_(string path)
        {
            byte[] png = IO.ReadPng(path);
            map = Convert.Png2Map(png, mode: "RGB", heightScale: heightScale);
            for (int i = 0; i < map.GetLength(0); i++)
                for (int j = 0; j < map.GetLength(0); j++)
                    map[i,j] += load_bias;

            Resize(map.GetLength(1), map.GetLength(0),newMap:false);
        }
        [Button()]
        public void Flatten(float value)
        {
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    map[i, j] = value;             
                }
            }
            UpdateHeight();
        }
    }
}
