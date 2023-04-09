using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Tilemaps;
using static Noise;
using System;
using System.Threading.Tasks;

public class ChunkManager : MonoBehaviour {

    private GameObject playerObj;
    private List<Chunk> chunks = new List<Chunk> { };
    
    public GameObject wallPrefab;
    public GameObject tilePrefab;
    public GameObject treePrefab;

    [Header("World Generation Parameters")]

    public int chunkSize;
    public int viewDist;
    
	public float scale;
	public int octaves;
	[Range(0,1)]
	public float persistance;
	public float lacunarity;
	public string seed;
	public Vector2 offset;
    
    


    // Dictionary<Vector2, string> openWith = new Dictionary<string, string>();

    // Start is called before the first frame update
    void Start() {
        if (playerObj == null) { playerObj = GameObject.Find("Player"); }
        
        DrawChunks();
    }


    public void lockPlayer() {

        int restrictedSize = chunkSize * 2;

        for ( int x = -restrictedSize; x <= restrictedSize; x++ ) {
            for ( int y = -restrictedSize; y <= restrictedSize; y++ ) {

                if ( Mathf.Abs(x) == restrictedSize || Mathf.Abs(y) == restrictedSize ) {
                    Debug.Log( "Wall" );
                    Instantiate( wallPrefab, new Vector3( x, y, 0 ) + playerObj.transform.position, Quaternion.identity );
                }

            }
        }    

        

    }

    // Update is called once per frame
    void Update() {
        
        DrawChunks();
        DeleteExtra();
    }

    void DrawChunks() { 

        int playerX = Mathf.RoundToInt( GetPlayerChunkCoords().x );
        int playerY = Mathf.RoundToInt( GetPlayerChunkCoords().y );

        for ( int i = -viewDist; i <= viewDist; i++ ) {
            for ( int j = -viewDist; j <= viewDist; j++ ) {

                    int trueX = i + playerX;
                    int trueY = j + playerY;

                    if ( isWithinRange( new Vector2( trueX, trueY ) ) && !HasDuplicate( trueX, trueY ) ) {

                        NoiseParameters param = new NoiseParameters( chunkSize, chunkSize, seed.GetHashCode(), scale, octaves, persistance, lacunarity, new Vector2( trueX * chunkSize, trueY * chunkSize ) );
                        Chunk newChunk = new Chunk( chunkSize, new Vector2( trueX, trueY ), param, tilePrefab, treePrefab );
                        
                        chunks.Add( newChunk );
                    }

            }
        }

        


    }


    void DeleteExtra() {

        for ( int i = chunks.Count - 1; i >= 0; i-- ) {

            if ( !isWithinRange( chunks[i].GetCoords() ) ) {

                List<GameObject> tiles = chunks[i].GetTiles();

                for ( int j = 0; j < tiles.Count; j++ ) {
                    if ( tiles[j] != null ) { 
                        Destroy( tiles[j] );
                    }
                }

                List<GameObject> obstacles = chunks[i].GetObstacles();

                for ( int j = 0; j < obstacles.Count; j++ ) {
                    if ( obstacles[j] != null ) { 
                        Destroy( obstacles[j] );
                    }
                }

                chunks.RemoveAt(i);

            }
        }
    }

    bool isWithinRange( Vector2 coords ) {
        int xDist = Mathf.RoundToInt( Mathf.Abs( coords.x - GetPlayerChunkCoords().x ) );
        int yDist = Mathf.RoundToInt( Mathf.Abs( coords.y - GetPlayerChunkCoords().y ) );

        return viewDist >= ( xDist + yDist );
    }

    bool HasDuplicate( int x, int y ) {

        for ( int i = 0; i < chunks.Count; i++ ) {

            if ( x == chunks[i].GetCoords().x && y == chunks[i].GetCoords().y ) { return true; }
        }

        return false;
    }


    Vector2 GetPlayerPosition() { return playerObj.transform.position; }

    Vector2 GetPlayerChunkCoords() {
        Vector2 playerPosition = GetPlayerPosition();
        return new Vector2( Mathf.FloorToInt( playerPosition.x / chunkSize ), Mathf.FloorToInt( playerPosition.y / chunkSize ) );
    }

    
}
