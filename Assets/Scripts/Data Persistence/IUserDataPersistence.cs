using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUserDataPersistence {

    void LoadUser(UserData user);
    void SaveUser(ref UserData user);

}
