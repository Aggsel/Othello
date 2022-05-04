using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Disk : MonoBehaviour
{
    [SerializeField] private GameSettings settings;
    [SerializeField] private AnimationCurve flipAnimation;
    [SerializeField] private AnimationCurve placementAnimation;
    [SerializeField] private float verticalPlacementOffset;
    private Vector3 placementOffset;

    [Header("Audio")]
    [SerializeField] private AudioSource placementAudio;
    [SerializeField] private float randomizePlacementAudio = 0.3f;
    [SerializeField] private float placementAudioDelay = 0.0f;
    [SerializeField] private AudioClip[] audioClips;

    //true = black, false = white
    private bool color = true;

    //Rotation Animation
    private int flipCounter = 0;
    private bool animationIsPlaying = false;

    private Vector3 targetBoardPlacement;

    void Awake(){
        placementAudio = placementAudio != null ? placementAudio : GetComponent<AudioSource>();
    }

    void Start(){
        placementOffset = new Vector3(0, verticalPlacementOffset, 0);
    }

    public void Flip(){
        color = !color;

        flipCounter++;
        if(!animationIsPlaying)
            StartCoroutine(FlipAnimation());
    }

    public void Place(Vector2Int boardCoordinates, bool color){
        this.color = color;
        RotateToCorrectSide();

        float cellSize = settings.GetCellSize();
        targetBoardPlacement = new Vector3(boardCoordinates.x * cellSize, 0, boardCoordinates.y * cellSize);

        StartCoroutine(PlaceAnimation());
    }

    private void RotateToCorrectSide(){
        transform.rotation = Quaternion.Euler(new Vector3(color ? 0 : 180, 0,0));
    }

    private void Update() {
        if(Input.GetKeyDown(KeyCode.Space))
            Flip();
    }

    public bool GetColor(){
        return color;
    }

    private void PlayPlacementAudio(float delay = 0.0f){
        if(audioClips.Length == 0)
            return;
        placementAudio.clip = audioClips[Random.Range(0,audioClips.Length)];
        placementAudio.pitch = 1.0f + (Random.value-0.5f) * randomizePlacementAudio;
        placementAudio.PlayDelayed(delay);
    }

    private IEnumerator FlipAnimation(){
        animationIsPlaying = true;
        float gameSpeed = settings.GetGameSpeed();
        while(flipCounter > 0){
            //Move up
            float timer = 0.0f;
            float animationDuration = placementAnimation.keys[placementAnimation.keys.Length-1].time / gameSpeed;
            Vector3 flipPosition = targetBoardPlacement + placementOffset;
            while(timer < animationDuration){
                float t = timer/animationDuration;
                t = placementAnimation.Evaluate(t);
                transform.position = Vector3.Lerp(targetBoardPlacement, flipPosition, t);
                timer += Time.deltaTime;
                yield return null;
            }

            //Flip
            float startRotationAngle = transform.localEulerAngles.y;
            timer = 0.0f;
            animationDuration = flipAnimation.keys[flipAnimation.keys.Length-1].time / gameSpeed;
            while(timer < animationDuration){
                float t = timer/animationDuration;
                transform.rotation = Quaternion.Euler(flipAnimation.Evaluate(t)*179 + startRotationAngle, 0, 0);
                timer += Time.deltaTime;
                yield return null;
            }

            //Move down
            timer = 0.0f;
            animationDuration = placementAnimation.keys[placementAnimation.keys.Length-1].time / gameSpeed;
            while(timer < animationDuration){
                float t = timer/animationDuration;
                t = placementAnimation.Evaluate(t);
                transform.position = Vector3.Lerp(flipPosition, targetBoardPlacement, t);
                timer += Time.deltaTime;
                yield return null;
            }

            flipCounter--;
        }
        RotateToCorrectSide();
        transform.position = targetBoardPlacement;
        animationIsPlaying = false;
    }

    private IEnumerator PlaceAnimation(){
        float timer = 0.0f;
        float gameSpeed = settings.GetGameSpeed();
        float animationDuration = placementAnimation.keys[placementAnimation.keys.Length-1].time / gameSpeed;
        Vector3 sourcePosition = transform.position;
        PlayPlacementAudio(placementAudioDelay);
        while(timer < animationDuration){
            float t = timer/animationDuration;
            t = placementAnimation.Evaluate(t);
            transform.position = Vector3.Lerp(sourcePosition, targetBoardPlacement, t);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.position = targetBoardPlacement;
    }
}
