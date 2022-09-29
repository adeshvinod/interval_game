using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonBehavior : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void LoadScene(string scene_name)
    {
        SceneManager.LoadScene(scene_name);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
