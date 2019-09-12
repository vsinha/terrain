using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode { NoiseMap, ColorMap, Mesh };
    public DrawMode drawMode;

    public const int mapChunkSize = 241;

    [Range(0, 6)]
    public int levelOfDetail;

    public float noiseScale = 10;

    [Min(1)]
    public int octaves = 3;

    [Range(0, 1)]
    public float persistance = 0.5f;

    public float lacunarity = 5f;

    public int seed;
    public Vector2 offset;

    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;

    public bool autoUpdate;

    public TerrainType[] regions;

    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, offset);

        Color[] colorMap = new Color[mapChunkSize * mapChunkSize];

        for (int y = 0; y < mapChunkSize; y++) {
            for (int x = 0; x < mapChunkSize; x++) {
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++) {
                    if (currentHeight <= regions[i].height) {
                        colorMap[y * mapChunkSize + x] = regions[i].color;
                        break;
                    }
                }
            }
        }

        MapDisplay display = FindObjectOfType<MapDisplay>();

        if (drawMode == DrawMode.NoiseMap) {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));

        } else if (drawMode == DrawMode.ColorMap) {
            var texture = TextureGenerator.TextureFromColorMap(colorMap, mapChunkSize, mapChunkSize);
            display.DrawTexture(texture);

        } else if (drawMode == DrawMode.Mesh) {
            var mesh = MeshGenerator.GenerateTerrainMesh(noiseMap,
                                                         meshHeightMultiplier,
                                                         meshHeightCurve,
                                                         levelOfDetail);
            var texture = TextureGenerator.TextureFromColorMap(colorMap, mapChunkSize, mapChunkSize);
            display.DrawMesh(mesh, texture);
        }
    }

    private void OnValidate()
    {
        if (lacunarity < 1) { lacunarity = 1; }
        if (octaves < 0) { octaves = 0; }
        if (persistance < 0) { octaves = 0; }
    }
}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color color;
}
