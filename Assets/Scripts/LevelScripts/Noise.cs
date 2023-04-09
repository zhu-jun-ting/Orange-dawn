using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Noise {

    public struct NoiseParameters {
        
        public int mapWidth;
        public int mapHeight;
        public int seed;
        public float scale;
        public int octaves;
        public float persistance;
        public float lacunarity;
        public Vector2 offset;
        
        public NoiseParameters(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset) {
            this.mapWidth = mapWidth;
            this.mapHeight = mapHeight;
            this.seed = seed;
            this.scale = scale;
            this.octaves = octaves;
            this.persistance = persistance;
            this.lacunarity = lacunarity;
            this.offset = offset;
        }
    }

    public static float[,] GenerateNoiseMap( NoiseParameters parameters ) {

        int mapWidth = parameters.mapWidth;
        int mapHeight = parameters.mapHeight;
        int seed = parameters.seed;
        float scale = parameters.scale;
        int octaves = parameters.octaves;
        float persistance = parameters.persistance;
        float lacunarity = parameters.lacunarity;
        Vector2 offset = parameters.offset;

		float[,] noiseMap = new float[mapWidth,mapHeight];

		System.Random prng = new System.Random (seed);
		Vector2[] octaveOffsets = new Vector2[octaves];

		float maxPossibleHeight = 0;
		float amplitude = 1;
		float frequency = 1;

		for (int i = 0; i < octaves; i++) {
			float offsetX = prng.Next (-100000, 100000) + offset.x;
			float offsetY = prng.Next (-100000, 100000) - offset.y;
			octaveOffsets [i] = new Vector2 (offsetX, offsetY);

			maxPossibleHeight += amplitude;
			amplitude *= persistance;
		}

		if (scale <= 0) {
			scale = 0.0001f;
		}

		float maxLocalNoiseHeight = float.MinValue;
		float minLocalNoiseHeight = float.MaxValue;

		float halfWidth = mapWidth / 2f;
		float halfHeight = mapHeight / 2f;


		for (int y = 0; y < mapHeight; y++) {
			for (int x = 0; x < mapWidth; x++) {

				amplitude = 1;
				frequency = 1;
				float noiseHeight = 0;

				for (int i = 0; i < octaves; i++) {
					float sampleX = (x-halfWidth + octaveOffsets[i].x) / scale * frequency;
					float sampleY = (y-halfHeight + octaveOffsets[i].y) / scale * frequency;

					float perlinValue = Mathf.PerlinNoise (sampleX, sampleY) * 2 - 1;
					noiseHeight += perlinValue * amplitude;

					amplitude *= persistance;
					frequency *= lacunarity;
				}

				if (noiseHeight > maxLocalNoiseHeight) {
					maxLocalNoiseHeight = noiseHeight;
				} else if (noiseHeight < minLocalNoiseHeight) {
					minLocalNoiseHeight = noiseHeight;
				}
				noiseMap [x, y] = noiseHeight;
			}
		}

		for (int y = 0; y < mapHeight; y++) {
			for (int x = 0; x < mapWidth; x++) {
				float normalizedHeight = (noiseMap [x, y] + 1) / (maxPossibleHeight/0.9f);
				noiseMap [x, y] = Mathf.Clamp(normalizedHeight,0, int.MaxValue);
			}
		}

		return noiseMap;
	}

    
}
