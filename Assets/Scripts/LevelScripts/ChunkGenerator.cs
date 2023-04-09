using UnityEngine;
using System.Collections;
using System;
using System.Threading;
using System.Collections.Generic;

public class ChunkGenerator : MonoBehaviour
{

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();

    void Update() {

        

		if (mapDataThreadInfoQueue.Count > 0) {
			for (int i = 0; i < mapDataThreadInfoQueue.Count; i++) {
				MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue ();
				threadInfo.callback (threadInfo.parameter);
			}

            // Debug.Log( "oke" );
		}
	}

    public void RequestNoiseData( Action<MapData> callback, Noise.NoiseParameters parameters ) {
        ThreadStart threadStart = delegate {
			MapDataThread (callback, parameters);
		};

		new Thread (threadStart).Start ();
    }

    void MapDataThread(Action<MapData> callback, Noise.NoiseParameters parameters) {
		float[,] noise = Noise.GenerateNoiseMap( parameters );

        Vector2 original = parameters.offset;

        parameters.offset = original + new Vector2( 0, parameters.mapHeight );
        float[,] north = Noise.GenerateNoiseMap( parameters );

        parameters.offset = original + new Vector2( 0, -parameters.mapHeight );
        float[,] south = Noise.GenerateNoiseMap( parameters );

        parameters.offset = original + new Vector2( parameters.mapWidth, 0 );
        float[,] east = Noise.GenerateNoiseMap( parameters );

        parameters.offset = original + new Vector2( -parameters.mapWidth, 0 );
        float[,] west = Noise.GenerateNoiseMap( parameters );

        MapData mapData = new MapData( noise, north, south, east, west );

		lock (mapDataThreadInfoQueue) {
			mapDataThreadInfoQueue.Enqueue (new MapThreadInfo<MapData> (callback, mapData));
		}
	}

    struct MapThreadInfo<T> {
		public readonly Action<T> callback;
		public readonly T parameter;

		public MapThreadInfo (Action<T> callback, T parameter)
		{
			this.callback = callback;
			this.parameter = parameter;
		}
		
	}
}

public struct MapData {
	public float[,] noise;

    public float[,] north;
    public float[,] south;
    public float[,] east;
    public float[,] west;

	public MapData ( float[,] noise, float[,] north, float[,] south, float[,] east, float[,] west ) {
		this.noise = noise;
		
        this.north = north;
        this.south = south;
        this.east = east;
        this.west = west;
	}
}
