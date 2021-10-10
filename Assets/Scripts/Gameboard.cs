using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public struct DiscPosition{
    public Vector2Int position;
    public bool color;
}

public class Gameboard : MonoBehaviour
{
    int boardSize = 0;
    float cellSize = 0.0f;
    private Camera mainCamera;
    private Disk[,] disks;
    private Board board;
    private bool currentPlayer = true;
    private bool isPlayersTurn = false;
    private GameObject disksParent;
    private GameObject boardTextParent;
    private List<GameObject> helpTexts = new List<GameObject>();
    private UIController uiController;
    private int turnsWithoutLegalMove = 0;

    private int whiteWins = 0;
    private int blackWins = 0;

    [Header("Game Settings")]
    [SerializeField] private DiscPosition[] initialPositions;
    [SerializeField] private GameSettings settings;
    [SerializeField] private float aiTimeBetweenTurns;
    [SerializeField] private string whiteWinningText;
    [SerializeField] private string blackWinningText;
    [SerializeField] private string tieWinningText;

    [Header("Opponent AI References")]
    [SerializeField] private BaseOpponent mainOpponent;
    [SerializeField] private BaseOpponent secondaryOpponent;

    [Header("Game Object References")]
    [SerializeField] private Transform blackSpawnPoint;
    [SerializeField] private Transform whiteSpawnPoint;

    [Tooltip("Reference to a object with a 'PlacementIndicator' component.")]
    [SerializeField] private GameObject placementIndicator;

    [Header("Asset References")]
    [SerializeField] private GameObject boardTextPrefab;
    [SerializeField] private GameObject diskPrefab;

    void Start(){
        CheckSerializedReferences();

        ResetGame();
        mainCamera = Camera.main;

        boardSize = settings.GetBoardSize();
        cellSize = settings.GetCellSize();

        disksParent = new GameObject("Disks");
        disksParent.transform.SetParent(transform);
        boardTextParent = new GameObject("Board Texts");
        boardTextParent.transform.SetParent(transform);

        uiController = UIController.Instance;

        ResetGame();
        PlaceInitialDisks();
    }

    void OnValidate(){
        if(mainOpponent != null)
            mainOpponent.SetColor(true);
        if(secondaryOpponent != null)
            secondaryOpponent.SetColor(false);
    }

    void Update() {
        Vector2Int index;
        if(Input.GetMouseButtonDown(0) && IsPlayersTurn()){
            if(GetBoardIndexFromMousePosition(out index)){
                Move move;
                if(TryPlaceDisk(index, currentPlayer, out move)){
                    OnPlayerTurnEnd();
                    NextPlayer();
                }
            }
        }
    }

    private void NextPlayer(){
        if(turnsWithoutLegalMove == 2){
            OnGameEnd();
            return;
        }
            
        UpdateScoreText();
        currentPlayer = !currentPlayer;
        
        if(IsPlayersTurn()){
            OnPlayerTurnStart();
            return;
        }

        if(currentPlayer && mainOpponent != null)
            StartCoroutine(PlayAsOpponent(mainOpponent));
        else
            StartCoroutine(PlayAsOpponent(secondaryOpponent));
    }

    private void OnPlayerTurnStart(){
        List<Move> legalMoves = board.GetLegalMoves(currentPlayer);
        if(legalMoves.Count == 0){      //No moves are available. Next players turn.
            turnsWithoutLegalMove++;
            NextPlayer();
        }

        turnsWithoutLegalMove = 0;
        PlaceHelpTexts(legalMoves);
        placementIndicator.SetActive(true);
        isPlayersTurn = true;
    }

    private void OnGameEnd(){
        int white, black;
        board.GetScore(out white, out black);
        
        if(settings.disableWinnerPrompt){
            StartNewGame();
            return;
        }

        if(white < black){
            uiController.DisplayEndScreen(blackWinningText);
            blackWins++;
        }

        if(white > black){
            uiController.DisplayEndScreen(whiteWinningText);
            whiteWins++;
        }

        if(white == black)
            uiController.DisplayEndScreen(tieWinningText);
    }

    public void StartNewGame(){
        ResetGame();
        PlaceInitialDisks();
    }

    private void ResetGame(){
        if(disks != null){
            for (int y = 0; y < disks.GetLength(0); y++){
                for (int x = 0; x < disks.GetLength(1); x++){
                    if(disks[x,y] != null)
                        Destroy(disks[x,y].gameObject);
                }
            }
        }
        turnsWithoutLegalMove = 0;
        disks = new Disk[boardSize,boardSize];
        board = new Board(boardSize);
    }

    private void OnPlayerTurnEnd(){
        ClearHelpTexts();
        placementIndicator.SetActive(false);
        isPlayersTurn = false;
    }

    private bool IsPlayersTurn(){
        if(isPlayersTurn)
            return true;
        if(currentPlayer && mainOpponent == null)
            return true;
        if(!currentPlayer && secondaryOpponent == null)
            return true;
        return false;
    }

    private IEnumerator PlayAsOpponent(BaseOpponent opponent){
        Move move;
        float gameSpeed = settings.GetGameSpeed(); 
        if(opponent.GetMove(this.board, out move)){     //Only place move if possible.
            yield return new WaitForSeconds(aiTimeBetweenTurns / gameSpeed);      //But wait one second first.
            TryPlaceDisk(move.position, currentPlayer, out move);
            turnsWithoutLegalMove = 0;
            yield return new WaitForSeconds(aiTimeBetweenTurns / gameSpeed);
        }
        else
            turnsWithoutLegalMove++;

        NextPlayer();
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

        if(mainOpponent == null){
            Debug.LogError("No opponent was attached to the gameboard, aborting.", this);
        }

        //If a critical field was not referenced, disable the component.
        if(!isValid)
            this.enabled = false;
    }

    private void PlaceInitialDisks(){
        for (int i = 0; i < initialPositions.Length; i++){
            Move move;
            TryPlaceDisk(initialPositions[i].position, initialPositions[i].color, out move, true);
        }
        NextPlayer();
    }

    /// <summary>
    /// Get the board coordinates based on the onscreen mouse position.
    /// </summary>
    /// <param name="position"></param>
    /// <returns>True if mouse was over a cell on the board.</returns>
    public bool GetBoardIndexFromMousePosition(out Vector2Int position){
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
        Disk newDisk = Instantiate(diskPrefab, spawnPoint, Quaternion.identity, disksParent.transform).GetComponent<Disk>();
        newDisk.Place(position, color);
        disks[position.x, position.y] = newDisk;

        if(!force)
            FlipAllDisksFromMove(move);

        return true;
    }

    private void PlaceHelpTexts(List<Move> legalMoves){
        if(legalMoves.Count > 0){
            for (int i = 0; i < legalMoves.Count; i++){
                int flips = legalMoves[i].flips.Count;
                GameObject newText = Instantiate(   boardTextPrefab, new Vector3(legalMoves[i].position.x * cellSize, 0.1f, legalMoves[i].position.y * cellSize), 
                                                    boardTextPrefab.transform.rotation, boardTextParent.transform);

                newText.GetComponent<TextMeshPro>().text = flips.ToString();
                helpTexts.Add(newText);
            }
        }
    }

    private void ClearHelpTexts(){
        for (int i = 0; i < helpTexts.Count; i++){
            Destroy(helpTexts[i]);
        }
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

    public bool IsWithinBoard(Vector2Int position){
        if(position.x >= boardSize || position.x < 0 || position.y >= boardSize || position.y < 0)
            return false;
        return true;
    }

    private void UpdateScoreText(){
        int white, black;
        board.GetScore(out white, out black);
        uiController.SetScore(true, black);
        uiController.SetScore(false, white);
    }
}
