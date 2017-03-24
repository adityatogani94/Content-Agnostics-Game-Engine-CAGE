using UnityEngine;
using System.Collections;

public class DestroyByContact : MonoBehaviour {

    public GameObject explosion;
    public GameObject playerExplosion;
    private char[] hazardsNums = { '1', '2', '3', '4' };
    public int scoreValue;
    private GameController gameController;
    


    void Start()
    { 
        GameObject gameControllerObject = GameObject.FindWithTag("GameController");
        if(gameControllerObject != null)
        {
            gameController = gameControllerObject.GetComponent<GameController>();
        }
        if(gameController == null)
        {
            Debug.Log("Cannot find 'GameController' script");
        }
    }
    void OnTriggerEnter(Collider other)
    {    
        if(other.tag == "Boundary")
        {
            return;
        }
        
        TextMesh temp = GetComponent<TextMesh>();
        //Debug.Log(temp.text);
        //MasteroidsMechanics temp1 = (MasteroidsMechanics)GameInfo.currentMechanics;
        //GameInfo.currentMechanics.sendHook(new CompareHook());
        GameInfo.currentMechanics.sendHook(new CompareHook(temp.text.ToCharArray()[0], hazardsNums)) ;
        Instantiate(explosion, transform.position, transform.rotation);
        
        if(FrameworkCore.currentContent.wasLastActionValid())
        {
            gameController.winGame();
        }
        if (other.tag == "Player")
        {
            Instantiate(playerExplosion, other.transform.position, other.transform.rotation);
            gameController.GameOver();
        }
        gameController.AddScore(scoreValue);
        Destroy(other.gameObject);
        Destroy(gameObject);
    }
}
