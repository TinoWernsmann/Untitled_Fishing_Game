using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class CreditsScroller : MonoBehaviour
{
    public float waitingTime = 3f;
    public float scrollSpeed = 40f;

    public bool move = false;

    private RectTransform rt ;
   
   

   void Start()
    {
        // Sobald das Objekt geladen wird, startet der Timer
        StartCoroutine(WarteTimer());
        rt = GetComponent<RectTransform>();
    }

    IEnumerator WarteTimer()
    {
        // Das Skript pausiert hier für X Sekunden
        yield return new WaitForSeconds(waitingTime);

        move = true; 
    }
    
    void Update()
    {
        if(!move) return;
        rt.anchoredPosition += new Vector2(0,scrollSpeed*Time.deltaTime);
    }
}
