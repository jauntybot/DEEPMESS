using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PersistentDataManager : MonoBehaviour {
    
    [SerializeField] string userDataFileName;

    UserData userData;
    List<IUserDataPersistence> userDataPersistenceObjs;
    FileDataHandler dataHandler;

    public static PersistentDataManager instance {get; private set; }
    void Awake() {
        if (instance != null) {
            Debug.Log("More than one instance of PersistentDataManager found.");
            Destroy(this.gameObject);
            return;
        }
        instance = this;
    }



    void Start() {
        dataHandler = new FileDataHandler(Application.persistentDataPath, userDataFileName);
        userDataPersistenceObjs = FindAllUserDataObjs();
        LoadUser();
    }

    public void NewUser() {
        userData = new();
    }

    public void LoadUser() {

        userData = dataHandler.Load();

        if (userData == null) {
            Debug.Log("No data was found. Initializing default user data.");
            NewUser();
        }

        foreach (IUserDataPersistence userDataObj in userDataPersistenceObjs) {
            userDataObj.LoadUser(userData);
        }
    }

    public void SaveUser() {

        foreach (IUserDataPersistence userDataObj in userDataPersistenceObjs) {
            userDataObj.SaveUser(ref userData);
        }

        dataHandler.Save(userData);
    }

    public void NewRun() {}

    public void LoadRun() {}

    public void SaveRun() {}


    void OnApplicationQuit() {
        SaveUser();
    }

    List<IUserDataPersistence> FindAllUserDataObjs() {
        IEnumerable<IUserDataPersistence> userDataObjs = FindObjectsOfType<MonoBehaviour>().OfType<IUserDataPersistence>();

        return new List<IUserDataPersistence>(userDataObjs);
    }

    List<IUserDataPersistence> FindAllRunDataObjs() {
        IEnumerable<IUserDataPersistence> runDataObjs = FindObjectsOfType<MonoBehaviour>().OfType<IUserDataPersistence>();

        return new List<IUserDataPersistence>(runDataObjs);
    }

}
