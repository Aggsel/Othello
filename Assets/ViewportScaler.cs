using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewportScaler : MonoBehaviour
{
    [SerializeField] Vector3 minPos;
    [SerializeField] private float minAspect = 0.44f;
    [SerializeField] Vector3 maxPos;
    [SerializeField] private float maxAspect = 1.5f;

    void Update(){
        float aspect = (float)Screen.width / Screen.height;
        aspect = (aspect - minAspect) / (maxAspect - minAspect);
        transform.position = Vector3.Lerp(minPos, maxPos, aspect);
    }
}
