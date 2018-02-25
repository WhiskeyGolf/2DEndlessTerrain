using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SacredTreeStudios.Atlas2D
{
    public class PerlinTerrain : MonoBehaviour 
    {

        public Sprite sandTile;
        public Sprite waterTile;
        public Sprite grassTile;

        public GameObject Tile;

        public int octaves;
        public float persistance;
        public float lacunarity;

        public int seed;
        public float scale;
        public Vector2 offset;

        public int chunkSize;
        public int chunksLoaded;


        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;


        System.Random pseudoRNG;

        void Awake()
        {
            pseudoRNG = new System.Random(seed);
        }

        void Update()
        {
            int currentChunkX = ((int)transform.position.x / chunkSize) * chunkSize;
            int currentChunky = ((int) transform.position.y / chunkSize) * chunkSize;
            if(!ChunkManager.chunkDictionary.ContainsKey(new Vector2(currentChunkX, currentChunky)))
            {
                CreateChunk(new Vector2(currentChunkX, currentChunky));
            }
        }

        void CreateChunk(Vector2 index)
        {
            GameObject chunk = new GameObject();
            chunk.AddComponent<Chunk>();
            chunk.GetComponent<Chunk>().chunkIndex = index;
            ChunkManager.chunkDictionary.Add(index, chunk);

            Vector2[] octaveOffsets = new Vector2[octaves];
            for (int i = 0; i < octaves; i++)
            {
                float offsetX = pseudoRNG.Next(-100000, 100000) + offset.x;
                float offsetY = pseudoRNG.Next(-100000, 100000) + offset.y;
                octaveOffsets[i] = new Vector2(offsetX, offsetY);
            }

            if (scale <= 0)
            {
                scale = .00001f;
            }

            for (int y = (int)chunk.GetComponent<Chunk>().chunkIndex.y; y < (int)chunk.GetComponent<Chunk>().chunkIndex.y + chunkSize; y++)
            {
                for (int x = (int)chunk.GetComponent<Chunk>().chunkIndex.x; x < (int)chunk.GetComponent<Chunk>().chunkIndex.x + chunkSize; x++)
                {
                    float amplitude = 1;
                    float frequency = 1;
                    float noiseHeight = 0;
                    for (int i = 0; i < octaves; i++)
                    {
                        float sampleX = x / scale * frequency + octaveOffsets[i].x;
                        float sampleY = y / scale * frequency + octaveOffsets[i].y;
                        float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2f - 1f;
                        noiseHeight += perlinValue * amplitude;
                        amplitude *= persistance;
                        frequency *= lacunarity;
                    }

                    if (noiseHeight > maxNoiseHeight)
                    {
                        maxNoiseHeight = noiseHeight;
                    }
                    else if (noiseHeight < minNoiseHeight)
                    {
                        minNoiseHeight = noiseHeight;
                    }
                    noiseHeight = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseHeight);
                    SpawnTerrainTile(x, y, noiseHeight, chunk.transform);
                }
            }
        }
    	
        void SpawnTerrainTile(int x, int y, float tile, Transform parent)
        {
            GameObject tileClone = Instantiate(Tile, new Vector3(x, y), Quaternion.identity) as GameObject;
            tileClone.transform.parent = parent;
            if (tile < .3f)
            {
                tileClone.GetComponent<SpriteRenderer>().sprite = waterTile;
            }
            else if (tile <= .4f)
            {
                tileClone.GetComponent<SpriteRenderer>().sprite = sandTile;
            }
            else if (tile > .4f)
            {
                tileClone.GetComponent<SpriteRenderer>().sprite = grassTile;
            }
        }
    }
}