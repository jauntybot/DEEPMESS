using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using Newtonsoft;
using Newtonsoft.Json;

public class FileDataHandler {


    string dataDirPath = "";
    string userDataFileName = "";
    string runDataFileName = "";

    public FileDataHandler(string _dataDirPath, string _userDataFileName, string _runDataFileName) {
        dataDirPath = _dataDirPath;
        userDataFileName = _userDataFileName;
        runDataFileName = _runDataFileName;
    }

    public UserData LoadUser() {
        string fullPath = Path.Combine(dataDirPath, userDataFileName);
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

    public RunData LoadRun() {
        string fullPath = Path.Combine(dataDirPath, runDataFileName);
        RunData loadedData = null;
        if (File.Exists(fullPath)) {
            try  {
                string dataToLoad = "";
                using (FileStream stream = new FileStream(fullPath, FileMode.Open)) {
                    using (StreamReader reader = new StreamReader(stream)) {
                        dataToLoad = reader.ReadToEnd();
                    }
                }

                loadedData = JsonUtility.FromJson<RunData>(dataToLoad);
            } catch (Exception e) {
                Debug.LogError("Error occured when trying to load data from file: " + fullPath + "\n" + e);
            }
        }
        return loadedData;
    }

    public void SaveUser(UserData data) {
        string fullPath = Path.Combine(dataDirPath, userDataFileName);
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

    public void SaveRun(RunData data) {
        string fullPath = Path.Combine(dataDirPath, runDataFileName);
        try {

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            string dataToStore = JsonConvert.SerializeObject(data, new JsonSerializerSettings() {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented
            });
            
            JsonSerializer serializer = new();

            using FileStream stream = new(fullPath, FileMode.Create);
            using StreamWriter writer = new (stream);
            using JsonWriter jWriter = new JsonTextWriter(writer);
            writer.Write(dataToStore);
            //JsonConvert.Serialize(jWriter, data);

        } catch (Exception e) {
            Debug.LogError("Error occured when trying to save data to file: " + fullPath + "\n" + e);
        }
    }

}
