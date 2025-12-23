using UnityEngine;

public class MenuController : MonoBehaviour
{
    int activePlayers = 0;

    public void addActivePlayers()
    {
        activePlayers++;
        Debug.Log("Active Players: " + activePlayers);

        if (activePlayers >= 4)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
        }
    }
}
