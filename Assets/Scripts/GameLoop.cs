using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoop : MonoBehaviour
{
    public int purifierScore = 0;
    public int terrodirtScore = 0;

    string lobbyName = "lobbyX";
    string lobbyWinner;
    int deadPurifierPlayers;
    int deadTerroDirtPlayers;

    void Start(){
        GameStart();
        // GameObject.Find("Reloading").SetActive(false);
        // GameObject.Find("PlantCircle").SetActive(false);
        // GameObject.Find("DefuseCircle").SetActive(false);
    }
    
    public void GameStart()
    {
        if(GetPurifierScore() == 0 && GetTerroDirtScore() == 0){
            //Add Credits
            foreach(PlayerData player in FindObjectsOfType<PlayerData>()){
                PlayerPrefs.SetInt(player.username + "_" + "Credits", Credits.startingCredits);
                PlayerPrefs.Save();
            }

            //Delete Current Weapon Data File
            if(File.Exists("")){

            }
            ResetKills();
            Debug.Log("Game Start!");
        }
        
    }

    //If bomb is defused || Purifier team is eliminated
    //Bomb explodes || TerroDirt team is eliminated while bomb is not planted || Time runs out and bomb is not planted
    public IEnumerator RestartRound()
    {
        yield return new WaitForSeconds(5f);
        
        //Save Player Data (e.g. current weapons)
        for (int i = 0; i < FindObjectsOfType<PlayerData>().Length; i++)
        {
            FindObjectsOfType<PlayerData>()[i].SaveWeaponData();
        }
        
        //Restart Scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        //Calculate Additional Credits Based On Performance
    }

    public void FindWinCondition()
    {
        foreach(PlayerStats player in FindObjectsOfType<PlayerStats>()){
            if(player.CompareTag("Purifier") && player.isDead){
                deadPurifierPlayers++;
            }else if(player.CompareTag("TerroDirt") && player.isDead){
                deadTerroDirtPlayers++;
            }
        }
        
        if(deadPurifierPlayers < GameObject.FindGameObjectsWithTag("Purifier").Length){
            deadPurifierPlayers = 0;
        }
        if(deadTerroDirtPlayers < GameObject.FindGameObjectsWithTag("TerroDirt").Length){
            deadTerroDirtPlayers = 0;
        }
        
        if(deadPurifierPlayers >= GameObject.FindGameObjectsWithTag("Purifier").Length
            || (GameObject.FindGameObjectWithTag("DirtBomb") != null && FindObjectOfType<DirtBomb>().hasExploded))
        {
            Debug.Log("TerroDirts Round Win");

            if(GetPurifierScore() < 3 || GetTerroDirtScore() < 3){
                AddTerroDirtScore();
                AllocateRoundEndCredits("TerroDirt", "Purifier");
                StartCoroutine(RestartRound());
            }
        }else if((deadTerroDirtPlayers >= GameObject.FindGameObjectsWithTag("TerroDirt").Length && !FindObjectOfType<DirtBomb>().isPlanted && FindObjectOfType<DirtBomb>() != null)
            || FindObjectOfType<DirtBomb>().defused
            || (!FindObjectOfType<DirtBomb>().isPlanted && FindObjectOfType<RoundTimer>().timeRanOut)){
            Debug.Log("Purifiers Round Win");

            if(GetPurifierScore() < 3 || GetTerroDirtScore() < 3){
                AddPurifierScore();
                AllocateRoundEndCredits("Purifier", "TerroDirt");
                StartCoroutine(RestartRound());
            }
        }

        if(GetPurifierScore() >= 3 || GetTerroDirtScore() >= 3){
            if(GetPurifierScore() >= 3){
                SetLobbyWinner("Purifiers");
            }else if(GetTerroDirtScore() >= 3){
                SetLobbyWinner("TerroDirts");
            }
            StartCoroutine(GameEnd());
        }
    }

    public void AddTerroDirtScore()
    {
        PlayerPrefs.SetInt(lobbyName + "_" + "TerroDirt Score", GetTerroDirtScore() + 1);
    }

    public void AddPurifierScore()
    {
        PlayerPrefs.SetInt(lobbyName + "_" + "Purifier Score",  GetPurifierScore() + 1);
        PlayerPrefs.Save();
    }

    public void AllocateRoundEndCredits(string winTeamTag, string loseTeamTag){
        FindAnyObjectByType<Credits>().AddTeamCredits(Credits.roundWinReward, winTeamTag);
        FindAnyObjectByType<Credits>().AddTeamCredits(Credits.roundLossReward, loseTeamTag);
    }

    //First to 3 Points Wins
    IEnumerator GameEnd()
    {
        //Show Game Summary GUI
        //Main Menu Button
        FindObjectOfType<AudioManager>().Play("MusicEnd");
        ResetScore();
        ResetKills();
        ResetCredits();
        DeletePlayerFiles();
        Debug.Log("Game End! " + GetLobbyWinner() + " win! Returning to Main Menu...");
        yield return new WaitForSeconds(3f);

        SceneManager.LoadSceneAsync("GameEnd", LoadSceneMode.Additive);

        yield return new WaitForSeconds(10f);

        //SceneManager.LoadScene("MainMenu");
    }

    public void ResetScore(){
        PlayerPrefs.SetInt(lobbyName + "_" + "Purifier Score", 0);
        PlayerPrefs.SetInt(lobbyName + "_" + "TerroDirt Score", 0);
        PlayerPrefs.Save();
    }
    public void ResetKills(){
        foreach(PlayerData player in FindObjectsOfType<PlayerData>()){
            PlayerPrefs.SetInt(player.gameObject.name.Replace("(Clone)", "") + "_" + "Kills", 0);
            PlayerPrefs.Save();
        }
    }
    public void ResetCredits(){
        foreach(PlayerData player in FindObjectsOfType<PlayerData>()){
            PlayerPrefs.SetInt(player.username + "_" + "Credits", 0);
            PlayerPrefs.Save();
        }
    }

    public void DeletePlayerFiles(){
        if(Directory.Exists(PlayerData.Player_Data_Folder)){
            foreach(string userFile in Directory.GetFiles(PlayerData.Player_Data_Folder)){
                foreach(string file in Directory.GetFiles(userFile.Replace(".meta", ""))){
                    File.Delete(file.Replace(".meta", ""));
                    Debug.Log(file.Replace(".meta", ""));
                }
                Directory.Delete(userFile.Replace(".meta", ""));
                Debug.Log("Deleted " + userFile.Replace(".meta", ""));
            }
        }
    }

    public int GetPurifierScore(){
        return PlayerPrefs.GetInt(lobbyName + "_" + "Purifier Score");
    }
    public int GetTerroDirtScore(){
        return PlayerPrefs.GetInt(lobbyName + "_" + "TerroDirt Score");
    }
    public string GetLobbyName(){
        return lobbyName;
    }
    public void SetLobbyWinner(string winnerTag){
        lobbyWinner = winnerTag;
    }
    public string GetLobbyWinner(){
        return lobbyWinner;
    }
}
