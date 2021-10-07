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
    private Disk[,] disks;
    private Board board;

    private bool currentPlayer = true;
    private readonly bool playerColor = false;
    private readonly bool opponentColor = true;

    [SerializeField] private DiscPosition[] initialPositions;

    //DEBUG
    [SerializeField] private GameObject validMoveIndicator;

    void Start(){
        CheckSerializedReferences();

        boardSize = settings.GetBoardSize();
        cellSize = settings.GetCellSize();
        disks = new Disk[boardSize,boardSize];
        board = new Board(boardSize);
        mainCamera = Camera.main;

        StartCoroutine(PlaceInitialDisks());
    }

    void Update() {
        UpdateIndicatorPosition();

        Vector2Int index;
        //Place black if right mouse button, otherwise place white. If disk is already in that spot, flip it.
        // if(Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)){
        //     if(GetBoardIndexFromMousePosition(out index)){
        //         if(!TryPlaceDisk(index, Input.GetMouseButtonDown(1))){
        //             TryFlipDisk(index);
        //         }
        //     }
        // }

        if(Input.GetMouseButtonDown(0) && currentPlayer == playerColor){
            if(GetBoardIndexFromMousePosition(out index)){
                Move move;
                if(TryPlaceDisk(index, playerColor, out move)){
                    NextPlayer();
                }
            }
        }

        if(Input.GetMouseButtonDown(2))
            if(GetBoardIndexFromMousePosition(out index))
                if(IsWithinBoard(index) && disks[index.x, index.y] != null)
                    RemoveDisk(index);
    }

    private void NextPlayer(){
        currentPlayer = !currentPlayer;
        if(currentPlayer == playerColor){

        }

        if(currentPlayer == opponentColor){
            StartCoroutine(PlayAsOpponent());
        }
    }

    private IEnumerator PlayAsOpponent(){
        yield return new WaitForSeconds(1.0f);
        List<Move> legalMoves = board.GetLegalMoves(currentPlayer);
        Move move;
        TryPlaceDisk(legalMoves[Random.Range(0, legalMoves.Count)].position, currentPlayer, out move);
        NextPlayer();
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
            Move move;
            TryPlaceDisk(initialPositions[i].position, initialPositions[i].color, out move, true);
            yield return new WaitForSeconds(0.01f);
        }
        NextPlayer();
    }

    /// <summary>
    /// Get the board coordinates based on the onscreen mouse position.
    /// </summary>
    /// <param name="position"></param>
    /// <returns>True if mouse was over a cell on the board.</returns>
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
    /// <param name="force">Should this disk be placed regardless of the rules?</param>
    /// <returns>True if the new disk was successfully placed.</returns>
    private bool TryPlaceDisk(Vector2Int position, bool color, out Move move, bool force = false){
        if(!board.TryPlaceDisk(position, color, out move, force))
            return false;

        Vector3 spawnPoint = color ? blackSpawnPoint.position : whiteSpawnPoint.position;
        Disk newDisk = Instantiate(diskPrefab, spawnPoint, Quaternion.identity).GetComponent<Disk>();
        newDisk.Place(position, color);
        disks[position.x, position.y] = newDisk;

        if(!force)
            FlipAllDisksFromMove(move);

        return true;
    }

    private void FlipAllDisksFromMove(Move move){
        for (int i = 0; i < move.flips.Count; i++){
            TryFlipDisk(move.flips[i]);
        }
    }

    private bool TryFlipDisk(Vector2Int position){
        if(!board.TryFlipDisk(position))
            return false;

        disks[position.x, position.y].Flip();
        return true;
    }

    private void RemoveDisk(Vector2Int position){
        board.RemoveDisk(position);

        Destroy(disks[position.x, position.y].gameObject);
        disks[position.x, position.y] = null;
    }

    private bool IsWithinBoard(Vector2Int position){
        if(position.x >= boardSize || position.x < 0 || position.y >= boardSize || position.y < 0)
            return false;
        return true;
    }
}
