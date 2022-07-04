using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LevelGenerator : MonoBehaviour {

    public Level[] levels;
    Level currLevel;
    int levelIndex = 0;
    
    [Space]
    public int difficulty = 1;
    public Difficulty[] difficulties;

    GameObject player;
    GameObject levelParent;
    GameObject layerParent;

    List<GameObject> layerMeshCombiners = new List<GameObject>();
    int combinerIndex = 0;
    int childrenPerCombiner;

    MeshCombiner layerCombiner;

    #region Singleton
    public static LevelGenerator instance;

    public void Awake()
    {
        instance = this;
    }
    #endregion

    private void Start()
    {
        player = PlayerManager.instance.player;

        NextLevel();
    }

    public void NextLevel()
    {
        Destroy(levelParent);

        currLevel = levels[levelIndex];

        player.SetActive(false);

        GenerateLevel();
        GenerateNavMesh();
        
        player.SetActive(true);

        levelIndex++;
        difficulty++;
    }

    void GenerateLevel()
    {
        levelParent = new GameObject();
        levelParent.transform.parent = this.transform;
        levelParent.name = currLevel.name;

        childrenPerCombiner = 1000;

        foreach(Layer l in currLevel.layers)
        {
            if(l.name == "Enemies")
            {


                l.GetTile("Enemy").prefabs = difficulties[difficulty - 1].enemies;
            }

            if (l.combineMeshes)
            {
                layerMeshCombiners = new List<GameObject>();
                combinerIndex = 0;
            }

            layerParent = new GameObject(l.name);
            layerParent.transform.parent = levelParent.transform;

            if (l.combineMeshes)
            {
                LayerSetup(l);
            }

            for (int x = 0; x < l.map.width; x++)
            {
                for (int y = 0; y < l.map.height; y++)
                {
                    GenerateTile(x, y, l);
                }
            }

            if (l.combineMeshes)
            {
                foreach(GameObject c in layerMeshCombiners)
                {
                    c.GetComponent<MeshCombiner>().CombineMesh();
                }
            }
        }

    }

    void GenerateNavMesh()
    {
        GetComponent<NavMeshSurface>().BuildNavMesh();
    }

    void GenerateTile(int x, int y, Layer layer)
    {
        Color pixelColor = layer.map.GetPixel(x, y);

        if(pixelColor.a == 0)
        {
            return; //If empty pixel, go to next tile
        }

        foreach(Tile t in layer.tiles)
        {
            if (t.color.Equals(pixelColor))
            {

                if (t.color.Equals(Color.blue))
                {
                    player.transform.position = new Vector3(x, 1f, y);
                }
                else
                {
                    GameObject prefabToSpawn = t.prefabs[Random.Range(0, t.prefabs.Length)];

                    GameObject tile = Instantiate(prefabToSpawn, new Vector3(x, layer.depth, y), Quaternion.identity);

                    if(layerMeshCombiners[combinerIndex].transform.childCount >= childrenPerCombiner)
                    {
                        combinerIndex++;
                        LayerSetup(layer);
                    }

                    if (layer.combineMeshes)
                    {
                        tile.transform.parent = layerMeshCombiners[combinerIndex].transform;
                    } else
                    {
                        tile.transform.parent = layerParent.transform;
                    }
                }
            }
        }
    }

    void LayerSetup(Layer l)
    {
        GameObject combiner = new GameObject(l.name + " combiner " + combinerIndex);
        combiner.transform.parent = layerParent.transform;
        combiner.layer = 10;
        combiner.tag = "Level";


        layerMeshCombiners.Add(combiner);

        if (l.combineMeshes)
        {
            combiner.AddComponent<MeshFilter>();
            combiner.AddComponent<MeshRenderer>();
            combiner.AddComponent<MeshCollider>();
            combiner.AddComponent<MeshCombiner>();
        }
    }
}

[System.Serializable]
public class Tile
{
    public string name;
    public Color color;
    public GameObject[] prefabs;
}

[System.Serializable]
public class Layer
{
    public string name;
    public float depth = 0f;
    public Texture2D map;
    public bool combineMeshes;

    [Space]
    public Tile[] tiles;

    public Tile GetTile(string name)
    {
        foreach(Tile t in tiles)
        {
            if(t.name == name)
            {
                return t;
            }
        }

        return null;
    }
}

[System.Serializable]
public class Difficulty
{
    public string name = "Easy";
    public GameObject[] enemies;
}
