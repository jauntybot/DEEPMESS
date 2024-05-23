using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class FileDataHandler {


    string dataDirPath = "";
    string dataFileName = "";

    public FileDataHandler(string _dataDirPath, string _dataFileName) {
        dataDirPath = _dataDirPath;
        dataFileName = _dataFileName;
    }

    public UserData Load() {
        string fullPath = Path.Combine(dataDirPath, dataFileName);
        UserData loadedData = null;
        if (File.Exists(fullPath)) {
            try  {
                string dataToLoad = "";
                using (FileStream stream = new FileStream(fullPath, FileMode.Open)) {
                    using (StreamReader reader = new StreamReader(stream)) {
                        dataToLoad = reader.ReadToEnd();
                    }
                }

                loadedData = JsonUtility.FromJson<UserData>(dataToLoad);
            } catch (Exception e) {
                Debug.LogError("Error occured when trying to load data from file: " + fullPath + "\n" + e);
            }
        }
        return loadedData;
    }

    public void Save(UserData data) {
        string fullPath = Path.Combine(dataDirPath, dataFileName);
        try {

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            string dataToStore = JsonUtility.ToJson(data, true);

            using (FileStream stream = new FileStream(fullPath, FileMode.Create)) {
                using (StreamWriter writer = new StreamWriter(stream)) {
                    writer.Write(dataToStore);
                }
            }

        } catch (Exception e) {
            Debug.LogError("Error occured when trying to save data to file: " + fullPath + "\n" + e);
        }
    }

}
