using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct DiscPosition{
    public Vector2Int position;
    public bool color;
}

public class Gameboard : MonoBehaviour
{
    [SerializeField] private GameSettings settings;
    [SerializeField] private GameObject diskPrefab;
    int boardSize = 0;
    float cellSize = 0.0f;

    [SerializeField] private Transform blackSpawnPoint;
    [SerializeField] private Transform whiteSpawnPoint;

    [SerializeField] private GameObject placementIndicator;

    private Camera mainCamera;
    private Disk[,] board;

    [SerializeField] private DiscPosition[] initialPositions;

    //DEBUGGING!
    [SerializeField] private Vector2Int placement;
    [SerializeField] private bool color;

    void Start(){
        CheckSerializedReferences();

        boardSize = settings.GetBoardSize();
        cellSize = settings.GetCellSize();

        board = new Disk[boardSize,boardSize];
        mainCamera = Camera.main;

        StartCoroutine(PlaceInitialDisks());
    }

    /// <summary>
    /// Validate all serialized references in the inspector. If any critical references are missing, alert user and disable component.
    /// </summary>
    private void CheckSerializedReferences(){
        bool isValid = true;

        if(settings == null){
            Debug.LogError("No game settings attached to the gameboard. Aborting.", this);
            isValid = false;
        }

        if(diskPrefab == null){
            Debug.LogError("No disk prefab was attached to the game board. Aborting.", this);
            isValid = false;
        }

        if(blackSpawnPoint == null){
            Debug.LogWarning("Black spawn point is null, will just use (0,0,0) as spawn point instead.", this);
            blackSpawnPoint = new GameObject("Black Spawn Point").transform;
        }
        if(whiteSpawnPoint == null){
            Debug.LogWarning("White spawn point is null, will just use (0,0,0) as spawn point instead.", this);
            whiteSpawnPoint = new GameObject("White Spawn Point").transform;
        }

        //If a critical field was not referenced, disable the component.
        if(!isValid)
            this.enabled = false;
    }

    private IEnumerator PlaceInitialDisks(){
        yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < initialPositions.Length; i++){
            TryAddNewDisk(initialPositions[i].position, initialPositions[i].color);
            yield return new WaitForSeconds(0.2f);
        }
    }

    void Update() {
        Vector2Int index;
        if(GetBoardIndexFromMousePosition(out index))
            placementIndicator.transform.position = new Vector3(index.x * cellSize, 0, index.y * cellSize);

        if(Input.GetMouseButtonDown(0)){
            if(GetBoardIndexFromMousePosition(out index)){
                TryAddNewDisk(index, this.color);
            }
        }

        if(Input.GetMouseButtonDown(1)){
            if(GetBoardIndexFromMousePosition(out index)){
                TryAddNewDisk(index, !this.color);
            }
        }
    }

    private bool GetBoardIndexFromMousePosition(out Vector2Int position){
        Ray ray = mainCamera.ViewportPointToRay(mainCamera.ScreenToViewportPoint(Input.mousePosition));
        Plane boardPlane = new Plane(transform.up, transform.position);
        float distance;
        if(!boardPlane.Raycast(ray, out distance)){
            position = new Vector2Int(-1, -1);
            return false;
        }

        Vector3 point = ray.GetPoint(distance)/cellSize;
        position = new Vector2Int(Mathf.RoundToInt(point.x), Mathf.RoundToInt(point.z));
        return true;
    }

    [ContextMenu("Add new disk")]
    private void AddDisk(){
        TryAddNewDisk(this.placement, this.color);
    }

    /// <summary>
    /// Try to add a new disk to the board.
    /// </summary>
    /// <param name="position">The gameboard coordinates on which to place the new disk.</param>
    /// <param name="color">What color the new disk should be. True = black, False = white</param>
    /// <returns>True if the new disk was successfully placed.</returns>
    private bool TryAddNewDisk(Vector2Int position, bool color){
        if(!IsWithinBoard(position))
            return false;
        if(board[position.x, position.y] != null)
            return false;
    
        Vector3 spawnPoint = color ? blackSpawnPoint.position : whiteSpawnPoint.position;
        Disk newDisk = Instantiate(diskPrefab, spawnPoint, Quaternion.identity).GetComponent<Disk>();
        newDisk.Place(position, color);
        board[position.x, position.y] = newDisk;

        return true;
    }

    private bool IsWithinBoard(Vector2Int position){
        if(position.x >= boardSize || position.x < 0 || position.y >= boardSize || position.y < 0)
            return false;
        return true;
    }
}
