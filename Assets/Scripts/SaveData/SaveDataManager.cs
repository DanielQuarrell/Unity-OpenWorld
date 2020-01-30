using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SaveDataManager
{
    private SaveDataManager() { }

    static SaveDataManager()
    {
        SaveDataManager.Initialise();
    }

    public static void Initialise()
    {
        string saveDataRootPath = Application.persistentDataPath + "/saves";
        if (!Directory.Exists(saveDataRootPath))
        {
            Directory.CreateDirectory(saveDataRootPath);
        }
    }

    public static void Save(string saveData, string fileName)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/saves/" + fileName + ".dat");
        byte[] data = Encoding.UTF8.GetBytes(saveData);
        file.Write(data, 0, saveData.Length);
        file.Close();
    }

    public static bool Load(ref string saveData, string fileName)
    {
        Debug.Log(Application.persistentDataPath);
        if (File.Exists(Application.persistentDataPath + "/saves/" + fileName + ".dat"))
        {
            byte[] data = File.ReadAllBytes(Application.persistentDataPath + "/saves/" + fileName + ".dat");
            saveData = Encoding.UTF8.GetString(data);
        }
        else
        {
            return false;
        }

        return true;
    }
}
