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

    private readonly Vector2Int[] directions = new Vector2Int[]{
        new Vector2Int(0, 1),
        new Vector2Int(1, 1),
        new Vector2Int(1, 0),
        new Vector2Int(1, -1),
        new Vector2Int(0, -1),
        new Vector2Int(-1, -1),
        new Vector2Int(-1, 0),
        new Vector2Int(-1, 1),
    };

    [SerializeField] private DiscPosition[] initialPositions;

    //DEBUG
    [SerializeField] private GameObject validMoveIndicator;

    void Start(){
        CheckSerializedReferences();

        boardSize = settings.GetBoardSize();
        cellSize = settings.GetCellSize();

        board = new Disk[boardSize,boardSize];
        mainCamera = Camera.main;

        StartCoroutine(PlaceInitialDisks());
    }

    void Update() {
        UpdateIndicatorPosition();

        Vector2Int index;
        //Place black if right mouse button, otherwise place white. If disk is already in that spot, flip it.
        if(Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)){
            if(GetBoardIndexFromMousePosition(out index)){
                if(!TryAddNewDisk(index, Input.GetMouseButtonDown(1))){
                    if(IsWithinBoard(index))
                    board[index.x, index.y].Flip();
                }
            }
        }

        if(Input.GetMouseButtonDown(2)){
            if(GetBoardIndexFromMousePosition(out index))
                if(IsWithinBoard(index) && board[index.x, index.y] != null){
                    Destroy(board[index.x, index.y].gameObject);
                    board[index.x, index.y] = null; 
                }
        }
    }

    private void UpdateIndicatorPosition(){
        Vector2Int index;
        if(GetBoardIndexFromMousePosition(out index))
            if(IsWithinBoard(index)){
                if(!placementIndicator.activeInHierarchy)
                    placementIndicator.SetActive(true);
                placementIndicator.transform.position = new Vector3(index.x * cellSize, 0, index.y * cellSize);
            }
            else
                placementIndicator.SetActive(false);
    }
    

    private List<Vector2Int> GetLegalMoves(bool color){
        List<Vector2Int> legalMoves = new List<Vector2Int>(); 
        for (int y = 0; y < boardSize; y++){
            for (int x = 0; x < boardSize; x++){
                if(board[x,y] != null)  //cell already contains disk.
                    continue;

                Vector2Int currentPos = new Vector2Int(x,y);
                for (int i = 0; i < directions.Length; i++){
                    Vector2Int previousPos = currentPos - directions[i];
                    if(EvaluateDirection(currentPos, previousPos, color, 0)){
                        legalMoves.Add(currentPos);
                        break;  //this is already a valid move, no need to check the other directions.
                    }
                }
            }
        }
        return legalMoves;
    }


    private bool EvaluateDirection(Vector2Int currentPos, Vector2Int previousPos, bool originalColor, int depth){
        Vector2Int nextPos = currentPos + (currentPos - previousPos); //new Vector2Int(0, -1);
        if(!IsWithinBoard(nextPos)){
            Debug.Log($"{nextPos} is not within board. Current: {currentPos}, Previous: {previousPos}, Next: {nextPos}");
            return false;
        }
        
        if(board[nextPos.x, nextPos.y] == null){
            Debug.Log("Reached a dead end.");
            return false;
        }

        if(board[nextPos.x, nextPos.y].GetColor() == originalColor && depth >= 1){
            Debug.Log("Color was same and depth was more than or equal 1!");
            return true;
        }

        if(board[nextPos.x, nextPos.y].GetColor() == originalColor && depth == 0){
            Debug.Log("Color was same and depth was more 0");
            return false;
        }

        Debug.Log("Going down");
        return EvaluateDirection(nextPos, currentPos, originalColor, depth + 1);
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
        // yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < initialPositions.Length; i++){
            TryAddNewDisk(initialPositions[i].position, initialPositions[i].color);
            yield return new WaitForSeconds(0.01f);
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

    void OnDrawGizmos(){
        if(board != null){
            for (int y = 0; y < boardSize; y++){
                for (int x = 0; x < boardSize; x++){
                if(board[x,y] != null)
                    Gizmos.DrawSphere(new Vector3(x * cellSize, 0, y * cellSize), 0.4f);
                }
            }
        }
    }
}
