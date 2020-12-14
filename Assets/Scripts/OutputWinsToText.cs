using UnityEngine;
using UnityEditor;
using System.IO;

public class OutputWinsToText : MonoBehaviour
{
    public static void WriteString(string raceOutput)
    {
        string path = "Assets/test.txt";

        //Write some text to the test.txt file
        StreamWriter writer = new StreamWriter(path, true);
        writer.WriteLine("");
        writer.Close();

        // //Re-import the file to update the reference in the editor
        // AssetDatabase.ImportAsset(path); 
        // TextAsset asset = (TextAsset)Resources.Load("test");

        // //Print the text from the file
        // Debug.Log(asset.text);
        // streamReader.Close();
    }
}