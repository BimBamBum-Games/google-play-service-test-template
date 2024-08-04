using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RouterManager : MonoBehaviour
{
    public void Redirect_To_MainMenu_Scene()
    {
        SceneManager.LoadScene(1);
    }
}
