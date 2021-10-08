using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class PlacementIndicator : MonoBehaviour
{

    [SerializeField] private Gameboard gameboard;
    [SerializeField] private GameSettings settings;

    private MeshRenderer meshRenderer;
    private float cellSize;

    void OnDisable(){
        meshRenderer.enabled = false;
    }

    void OnEnable(){
        if(meshRenderer == null)
            meshRenderer = GetComponent<MeshRenderer>();
            
        meshRenderer.enabled = true;
    }

    void Start(){
        gameboard = FindObjectOfType<Gameboard>();

        if(gameboard == null){
            Debug.LogError("No gameboard found, why even bother with a placement indicator if no board? Aborting.", this);
            this.enabled = false;
        }
        cellSize = settings.GetCellSize();
    }

    void Update(){
        Vector2Int index;
        if(gameboard.GetBoardIndexFromMousePosition(out index))
            if(gameboard.IsWithinBoard(index)){
                meshRenderer.enabled = true;
                transform.position = new Vector3(index.x, 0, index.y) * cellSize;
            }
            else{
                meshRenderer.enabled = false;
            }
    }
}
