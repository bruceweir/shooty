using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetCursorVisibility : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        MakeCursorVisible(false);
    }

    public void MakeCursorVisible(bool visible)
    {
        Cursor.visible = visible;
    }
}
