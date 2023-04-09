using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Tilemaps;

using UnityEditor;

using static Noise;

public class Chunk {

    private int chunkSize;
    private Vector2 chunkCoords; 

    private List<GameObject> tiles = new List<GameObject> {};
    private List<GameObject> obstacles = new List<GameObject> {};

    private NoiseParameters param;

    private GameObject treePrefab;
    private GameObject tilePrefab;

    Sprite[] groundSprites;
    Sprite[] grassSprites;

    public float[,] noiseMap;

    public float[,] noise1;
    public float[,] noise2;
    public float[,] noise3;
    public float[,] noise4;

    public bool drawn = false;

    ChunkGenerator gen;

    // Dictionary<Vector2, Tile> locationToTile = new Dictionary<Vector2, Tile>();


    public Chunk( int chunkSize, Vector2 chunkCoords, NoiseParameters _param, GameObject tilePrefab, GameObject _treePrefab ) {

        this.chunkSize = chunkSize;
        this.chunkCoords = chunkCoords;
        this.tilePrefab = tilePrefab;

        gen = GameObject.FindObjectOfType<ChunkGenerator> ();
        
        Vector2 offset = new Vector2( ( chunkCoords.x * chunkSize ), ( chunkCoords.y * chunkSize ) );


        // noiseMap = Noise.GenerateNoiseMap( _param );


        // noise1 = Noise.GenerateNoiseMap( chunkSize, chunkSize, param.seed.GetHashCode(), 
        //                                 param.scale, param.octaves, param.persistence, param.lacunarity, 
        //                                 offset + new Vector2( ( chunkSize ), 0 ) );

        // noise2 = Noise.GenerateNoiseMap( chunkSize, chunkSize, param.seed.GetHashCode(), 
        //                                 param.scale, param.octaves, param.persistence, param.lacunarity, 
        //                                 offset + new Vector2( ( -chunkSize ), 0 ) );

        // noise3 = Noise.GenerateNoiseMap( chunkSize, chunkSize, param.seed.GetHashCode(), 
        //                                 param.scale, param.octaves, param.persistence, param.lacunarity, 
        //                                 offset + new Vector2( 0, ( chunkSize ) ) );

        // noise4 = Noise.GenerateNoiseMap( chunkSize, chunkSize, param.seed.GetHashCode(), 
        //                                 param.scale, param.octaves, param.persistence, param.lacunarity, 
        //                                 offset + new Vector2( 0, ( -chunkSize ) ) );


        // Debug.Log( gen );
        // Debug.Log( _param );
        

        gen.RequestNoiseData( DrawTiles, _param );
        // AddObstacles();
    }

    public void DrawTiles( MapData mapData ) {

        

        noiseMap = mapData.noise;
        noise1 = mapData.east;
        noise2 = mapData.west;
        noise3 = mapData.north;
        noise4 = mapData.south;

        for ( int i = 0; i < chunkSize; i++ ) {

            for ( int j = 0; j < chunkSize; j++ ) {

                Vector3 worldCoords = new Vector3( i + ( chunkCoords.x * chunkSize ), j + ( chunkCoords.y * chunkSize ), 1 );
                Vector2 location = new Vector2( i + ( chunkCoords.x * chunkSize ), j + ( chunkCoords.y * chunkSize ) );

                int x = (int) ( i + ( chunkCoords.x * chunkSize ) );
                int y = (int) ( j + ( chunkCoords.y * chunkSize ) );

                GameObject newTile = CreateTile( new Vector2( i, j ), worldCoords, noiseMap[i, j] );
                tiles.Add( newTile );

            }
        }

        

    }

    GameObject AddObstacles() {
        float x = Random.Range( 0, chunkSize ) + ( chunkCoords.x * chunkSize );
        float y = Random.Range( 0, chunkSize ) + ( chunkCoords.y * chunkSize );
        
        GameObject tree = GameObject.Instantiate( treePrefab, new Vector3( x, y, -1 ), Quaternion.identity );
        
        obstacles.Add( tree );
        return tree;
    }

    GameObject CreateTile( Vector2 location, Vector3 worldCoords, float noise ) {

        // GameObject tile = new GameObject();
        // tile.AddComponent<SpriteRenderer>(); 
        GameObject tile = GameObject.Instantiate( tilePrefab, worldCoords, Quaternion.identity );  

        groundSprites = Resources.LoadAll<Sprite>( "ground" );
        grassSprites = Resources.LoadAll<Sprite>( "grass" );

        if ( !isGrass( noise ) ) {

            int idx = Random.Range( 0, groundSprites.Length - 1 );
            tile.GetComponent<SpriteRenderer>().sprite = groundSprites[idx];
        } else {
            
            int idx = Random.Range( 0, groundSprites.Length - 1 );

            char[] adjacentEncoding = { '0', '0', '0', '0' };

            int x = (int) location.x;
            int y = (int) location.y;

            if ( x >= chunkSize - 1 ) {
                if ( isGrass( noise1[0, y] ) ) { adjacentEncoding[0] = '1'; }
            } else {
                if ( isGrass( noiseMap[x + 1, y] ) ) { adjacentEncoding[0] = '1'; }
            }

            if ( x <= 0 ) {
                if ( isGrass( noise2[chunkSize - 1, y] ) ) { adjacentEncoding[1] = '1'; }
            } else {
                if ( isGrass( noiseMap[x - 1, y] ) ) { adjacentEncoding[1] = '1'; }
            }

            if ( y >= chunkSize - 1 ) {
                if ( isGrass( noise3[x, 0] ) ) { adjacentEncoding[2] = '1'; }
            } else {
                if ( isGrass( noiseMap[x, y + 1] ) ) { adjacentEncoding[2] = '1'; }
            }

            if ( y <= 0 ) {
                if ( isGrass( noise4[x, chunkSize - 1] ) ) { adjacentEncoding[3] = '1'; }
            } else {
                if ( isGrass( noiseMap[x, y - 1] ) ) { adjacentEncoding[3] = '1'; }
            }

            // if ( x >= chunkSize - 1 || isGrass( noiseMap[x + 1, y] ) ) { adjacentEncoding[0] = '1'; }
            // if ( x <= 0 || isGrass( noiseMap[x - 1, y] ) ) { adjacentEncoding[1] = '1'; }
            // if ( y >= chunkSize - 1 || isGrass( noiseMap[x, y + 1] ) ) { adjacentEncoding[2] = '1'; }
            // if ( y <= 0 || isGrass( noiseMap[x, y - 1] ) ) { adjacentEncoding[3] = '1'; }

            tile.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>( "grassTiles/_" + new string( adjacentEncoding ) );
        }

        return tile;
    }

    private bool isGrass( float noise ) { return (noise <= 0.7); }

    public Vector2 GetCoords() { return chunkCoords; }

    public List<GameObject> GetTiles() { return tiles; }
    public List<GameObject> GetObstacles() { return obstacles; }

}
