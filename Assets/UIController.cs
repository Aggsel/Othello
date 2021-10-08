using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour {

    [SerializeField] private TextMeshPro whiteScoreText;
    [SerializeField] private TextMeshPro blackScoreText;

    [SerializeField] private GameObject sidepanel;
    [SerializeField] private GameObject sidepanelArrowButton;
    private bool sidepanelActive = true;
    Vector3 defaultPosition;

    [SerializeField] private MonoBehaviour[] disableOnSidepanel;
    
    private static UIController _instance;
    public static UIController Instance { get { return _instance; } }

    private void Awake(){
        if(_instance != null && _instance != this)
            Destroy(this.gameObject);
        else
            _instance = this;
    }

    private void Start(){
        defaultPosition = sidepanel.GetComponent<RectTransform>().localPosition;
        SetSidePanel(false);
    }

    private void SetSidePanel(bool active){
        if(!active){
            sidepanel.GetComponent<RectTransform>().localPosition = defaultPosition + new Vector3(sidepanel.GetComponent<RectTransform>().sizeDelta.x, 0, 0);
            sidepanelArrowButton.GetComponent<RectTransform>().localRotation = Quaternion.Euler(new Vector3(0,0,180));
        }
        else{
            sidepanel.GetComponent<RectTransform>().localPosition = defaultPosition;
            sidepanelArrowButton.GetComponent<RectTransform>().localRotation = Quaternion.Euler(new Vector3(0,0,0));
        }
        SetScriptsActive(!active);
    }

    public void ToggleSidepanel(){
        SetSidePanel(sidepanelActive);
        sidepanelActive = !sidepanelActive;
    }

    private void SetScriptsActive(bool active){
        for (int i = 0; i < disableOnSidepanel.Length; i++){
            disableOnSidepanel[i].enabled = active;
        }
    }

    public void SetScore(bool color, int newScore){
        if(blackScoreText == null){
            Debug.LogError("No score text for player black attached to the ui controller. Cannot update text.", this);
            return;
        }

        if(whiteScoreText == null){
            Debug.LogError("No score text for player white attached to the ui controller. Cannot update text.", this);
            return;
        }

        if(color)
            blackScoreText.text = newScore.ToString();
        else
            whiteScoreText.text = newScore.ToString();
    }
}