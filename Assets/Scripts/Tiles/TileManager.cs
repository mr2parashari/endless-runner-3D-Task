using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    private List<GameObject> activeTiles;  // list for tile
    public GameObject[] tilePrefabs;
     public GameObject enemyPrefab; 

    public float tileLength = 30;
    public int numberOfTiles = 3;
    public int totalNumOfTiles = 8;

    public float zSpawn = 0;

    private Transform playerTransform;

    private int previousIndex;

    void Start()
    {
        activeTiles = new List<GameObject>();
        for (int i = 0; i < numberOfTiles; i++)
        {   //loop for Spawing tiles
            if(i==0)
                SpawnTile();
            else
                SpawnTile(Random.Range(0, totalNumOfTiles));
        }

        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

    }
    void Update()
    {
        if(playerTransform.position.z - 30 >= zSpawn - (numberOfTiles * tileLength))
        {
            int index = Random.Range(0, totalNumOfTiles);
            while(index == previousIndex)
                index = Random.Range(0, totalNumOfTiles);

            DeleteTile();
            SpawnTile(index);
        }
            
    }

    public void SpawnTile(int index = 0)
    {    //tile /environment spawning
         Debug.Log(" Enemy Spawned ");
        GameObject tile = tilePrefabs[index];
        if (tile.activeInHierarchy)
            tile = tilePrefabs[index + 8];

        if(tile.activeInHierarchy)
            tile = tilePrefabs[index + 16];

        tile.transform.position = Vector3.forward * zSpawn;
        tile.transform.rotation = Quaternion.identity;
        tile.SetActive(true);

        activeTiles.Add(tile);
        zSpawn += tileLength;
        previousIndex = index;
        SpawnEnemy(tile.transform.position);
    }
     public void SpawnEnemy(Vector3 tilePosition)
    {     Debug.Log(" Enemy Spawned ");
        FindPlayer();
        if (FindObjectOfType<WheelAI>() != null)
    {
        Debug.Log(" Enemy already exist.");
        return; 
    }
        if (enemyPrefab != null && playerTransform != null)
        {
            Vector3 enemySpawnPos = new Vector3(playerTransform.position.x, playerTransform.position.y, playerTransform.position.z - 3);
            GameObject enemyInstance = Instantiate(enemyPrefab, enemySpawnPos, Quaternion.identity);
            enemyInstance.GetComponent<WheelAI>().StartChasing(); // chasing funtion from wheel Ai Script 
        }
        else
        {
            Debug.LogError(" Enemy Prefab / Player is missing Tile Script");
        }
    }
    private void FindPlayer()
{         
    if (playerTransform == null)
    {         Debug.Log("Playefound ");
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        else
        {
            Debug.LogError("Player not found ");
        }
    }
}


    private void DeleteTile()
    {
        activeTiles[0].SetActive(false);
        activeTiles.RemoveAt(0);
        PlayerManager.score += 3;
    }
}
