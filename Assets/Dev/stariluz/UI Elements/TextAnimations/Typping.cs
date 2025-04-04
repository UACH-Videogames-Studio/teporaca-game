using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class Typping : MonoBehaviour
{
    [SerializeField]
    float waitTime=0;
    
    [SerializeField]
    TextMeshProUGUI text;

    [SerializeField]
    bool activateOnStart=true;
    Coroutine coroutine;
    string textSave="";
    [SerializeField]
    UnityEvent endEvent;
    
    // Start is called before the first frame update
    void Start()
    {
        textSave=text.text;
        text.text="";
        if(activateOnStart){
            Activate();
        }
    }

    public void Activate(){
        coroutine=StartCoroutine(TypeLetter());
    }
    
    void OnDisable(){
        if(coroutine!=null){
            StopCoroutine(coroutine);
        }
    }

    IEnumerator TypeLetter(){
        text.text="";
        for(int i=0; i<textSave.Length; i++){
            if(text){
                text.text+=textSave[i];
                yield return new WaitForSeconds(waitTime);
            }
        }
        endEvent?.Invoke();
    }
}
