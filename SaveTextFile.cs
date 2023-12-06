using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using JetBrains.Annotations;

// based on: https://www.youtube.com/watch?v=iFJeg9AzN2Y (though is somewhat altered)
// and very loosely: https://www.youtube.com/watch?v=6bVcLSZWqK8
/// <summary>
/// Saves the time of the last time the game was played as well as what the keybinds are.
/// Will be manually updated if any change.
/// </summary>
public class SaveTextFile : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Directory.CreateDirectory(Application.streamingAssetsPath + "/Chat_Logs/");

        CreateTextFile();
    }

    /// <summary>
    /// Creates the text file if it isn't already created and then adds the time the game stopped running and the player's keybinds.
    /// </summary>
    void CreateTextFile()
    {
        string txtDocumentName = Application.streamingAssetsPath + "/Chat_Logs/" + "Chat" + ".txt";

        string content = "\n\nLogin date: " + System.DateTime.Now + "\n" +
            "Keybinds: \nsprintKey = KeyCode.LeftShift; \njumpKey = KeyCode.Space; \ncrouchKey = KeyCode.LeftControl; \n" +
            "zoomKey = KeyCode.Mouse1; \ninteractKey = KeyCode.E; \nflashlightKey = KeyCode.Mouse0; \nreloadKey = KeyCode.R;";


        if (!File.Exists(txtDocumentName))
        {
            File.WriteAllText(txtDocumentName, "Login Log \n\n");
        }

        File.AppendAllText(txtDocumentName, content);
    }
}
