using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

public class Arena : MonoBehaviour{
    public List<GameObject> treats; 

    public void ResetArea(){
        foreach(GameObject treat in treats)
            treat.SetActive(true);
    }

    public void Collect(GameObject gameObject){
        gameObject.SetActive(false);
    }
}
