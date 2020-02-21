using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuUI : MonoBehaviour
{
    public void NewGame()
    {
        BinaryFormatter bf = new BinaryFormatter();

        FileStream readWorldFile = File.Open("Assets/WorldData/worldData.dat", FileMode.Open);
        WorldData worldData = (WorldData)bf.Deserialize(readWorldFile);
        readWorldFile.Close();

        //Create a new enemy file
        FileStream worldFile = File.Create(Application.persistentDataPath + "/worldData.dat");
        bf.Serialize(worldFile, worldData);
        worldFile.Close();

        FileStream readEnemyFile = File.Open("Assets/WorldData/enemiesData.dat", FileMode.Open);
        WorldEnemyData worldEnemyData = (WorldEnemyData)bf.Deserialize(readEnemyFile);
        readEnemyFile.Close();

        //Create a new enemy file
        FileStream enemyFile = File.Create(Application.persistentDataPath + "/enemiesData.dat");
        bf.Serialize(enemyFile, worldEnemyData);
        enemyFile.Close();

        SceneManager.LoadScene("GameScene");
    }

    public void LoadGame()
    {
        if (File.Exists(Application.persistentDataPath + "/enemiesData.dat") && File.Exists(Application.persistentDataPath + "/worldData.dat"))
        {
            SceneManager.LoadScene("GameScene");
        }
        else
        {
            Debug.LogError("Save doesn't exist");
        }
    }
}