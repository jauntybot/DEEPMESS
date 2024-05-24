using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRunDataPersistence {

    void LoadRun(RunData run);
    void SaveRun(ref RunData run);

}
