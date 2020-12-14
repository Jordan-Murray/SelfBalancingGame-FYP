using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Reflection;

public class WinRateEditor : EditorWindow
{
    public GameObject source = null;
    string filePath = "Assets/Winner.txt";
    public int lineNumber;
    Dictionary<string, decimal> winnerDic = new Dictionary<string, decimal>();
    bool showingLabels = false;
    public int popupIndex = 0;
    public int popupIndex2 = 0;
    public int popupIndex3 = 0;
    public int charSelectIndex = 0;
    public string winRateToBalanceToString;
    public decimal winRateToBalanceTo;
    public int numberOfRuns;
    public string numberOfRunsToString;
    public bool finishedProp = false;
    List<string> propNameList = new List<string>();
    public string[] balanceStyle = new string[]{
        "Aggressive",
        "Reserved"
    };
    public bool readyToBalance = false;
    SerializedProperty serProp;

    [MenuItem("Game Testing Framework/Win Rate")]
    public static void ShowWindow()
    {
        GetWindow<WinRateEditor>("Win Rate");
    }

    private void OnGUI()
    {
        EditorGUIUtility.fieldWidth = 220;
        EditorGUIUtility.labelWidth = 220;
        EditorGUILayout.LabelField("Enter File Path to where Game Winners are stored: ",EditorStyles.boldLabel);
        EditorGUILayout.Space();
        filePath = EditorGUILayout.TextField("File Path", filePath);
        if (GUILayout.Button("Calculate Win Rate"))
        {
            CalculateWinPercent();
        }
        if (showingLabels == true)
        {
            ShowText();
            EditorGUIUtility.labelWidth = 220;
            BalanceToWinRate();
            if (winRateToBalanceToString != null)
                SetMinMaxCharStats();
                EditorGUILayout.LabelField("Configure Game Runs",EditorStyles.boldLabel);
                source = (GameObject)EditorGUILayout.ObjectField("Object that configures Game Runs: ", source, typeof(GameObject), true);
            if (source != null)
            {
                ConfigureRunNumber();
                if (GUILayout.Button("Balance Game"))
                {
                    if(popupIndex2 != 0)
                        Balance();
                    else
                        Debug.Log("Select the correct Game Runs variable in the second dropdown to proceed");
                }
            }
        }

    }

    private void ConfigureRunNumber()
    {
        var scripts = source.GetComponents<MonoBehaviour>();
        var scriptStringArray = new List<string>();
        foreach (var item in scripts)
        {
            scriptStringArray.Add(item.ToString());
        }
        popupIndex = EditorGUILayout.Popup(popupIndex, scriptStringArray.ToArray());
        SerializedObject serObj = new SerializedObject(scripts[popupIndex]);
        SerializedProperty prop = serObj.GetIterator();
        while (prop.NextVisible(true))
        {
            propNameList.Add(prop.name);
        }
        EditorGUILayout.BeginHorizontal();
        popupIndex2 = EditorGUILayout.Popup(popupIndex2, propNameList.ToArray());
        serProp = serObj.FindProperty(propNameList[popupIndex2]);
        switch (serProp.propertyType)
        {
            case SerializedPropertyType.Boolean:
                EditorGUILayout.LabelField(serProp.boolValue.ToString());
                break;
            case SerializedPropertyType.Bounds:
                EditorGUILayout.LabelField(serProp.boundsValue.ToString());
                break;
            case SerializedPropertyType.Color:
                EditorGUILayout.LabelField(serProp.colorValue.ToString());
                break;
            case SerializedPropertyType.Float:
                EditorGUILayout.LabelField(serProp.floatValue.ToString());
                break;
            case SerializedPropertyType.Integer:
                EditorGUILayout.LabelField(serProp.intValue.ToString());
                serProp.intValue = numberOfRuns;
                break;
            case SerializedPropertyType.ObjectReference:
                EditorGUILayout.LabelField(serProp.objectReferenceValue.ToString());
                break;
            case SerializedPropertyType.Rect:
                EditorGUILayout.LabelField(serProp.rectValue.ToString());
                break;
            case SerializedPropertyType.String:
                EditorGUILayout.LabelField(serProp.stringValue.ToString());
                break;
            case SerializedPropertyType.Vector2:
                EditorGUILayout.LabelField(serProp.vector2Value.ToString());
                break;
            case SerializedPropertyType.Vector3:
                EditorGUILayout.LabelField(serProp.vector3Value.ToString());
                break;
        }
        EditorGUILayout.EndHorizontal();
        serObj.ApplyModifiedProperties();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
    }

    private void ShowText()
    {
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Character Win Rates: ",EditorStyles.boldLabel);
        EditorGUIUtility.labelWidth = 10;
        //Titles
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Object Name");
        EditorGUILayout.LabelField("Object Win Percentage");
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        // EditorGUILayout.LabelField("---------------------------------------------------------------------------------------------");
        //Name and Win Rate
        for (int i = 0; i < winnerDic.Count(); i++)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(winnerDic.ElementAt(i).Key);
            EditorGUILayout.LabelField(winnerDic.ElementAt(i).Value.ToString());
            EditorGUILayout.EndHorizontal();
            // EditorGUILayout.LabelField("---------------------------------------------------------------------------------------------");
        }
    }

    private void BalanceToWinRate()
    {
        //BalanceToWinRate
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Select a Game Balancing Style",EditorStyles.boldLabel);

        popupIndex3 = EditorGUILayout.Popup("Game Balancing Style: ", popupIndex3, balanceStyle);
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        if (balanceStyle[popupIndex3] == "Aggressive")
        {
            numberOfRuns = 10; //testing
        }
        else
        {
            numberOfRuns = 5; //testing
        }
        EditorGUILayout.LabelField("Enter your desired Win Rate Percentage",EditorStyles.boldLabel);
        winRateToBalanceToString = EditorGUILayout.TextField("Desired Win Rate: ", winRateToBalanceToString);
        EditorGUILayout.Space();
        EditorGUILayout.Space();EditorGUILayout.Space();
        EditorGUILayout.Space();
        if (winRateToBalanceToString != null)
        {
            winRateToBalanceTo = Decimal.Parse(winRateToBalanceToString);
        }
    }

    private void Balance()
    {
        
        Debug.Log("Balanced");
    }

    private void SetMinMaxCharStats()
    {
        EditorGUILayout.LabelField("Set Minimum and Maximum Value for Character Stats",EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        charSelectIndex = EditorGUILayout.Popup(charSelectIndex, winnerDic.Keys.ToArray());
        EditorGUILayout.LabelField(winnerDic.ElementAt(charSelectIndex).Value.ToString());
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        
        //GetAndShowCharacterStats
        //SetMinAndMaxValues
        //EditorGUILayout.LabelField("Min Val:", minVal.ToString());
        //EditorGUILayout.LabelField("Max Val:", maxVal.ToString());
        //EditorGUILayout.MinMaxSlider(ref minVal, ref maxVal, minLimit, maxLimit);
    }

    private void CalculateWinPercent()
    {
        winnerDic = ReadCSV();
        showingLabels = true;
        var totalWins = (Decimal)0;
        foreach (var item in winnerDic)
        {
            totalWins += item.Value;
        }
        for (int i = 0; i < winnerDic.Count(); i++)
        {
            var key = winnerDic.ElementAt(i).Key;
            var wins = winnerDic.ElementAt(i).Value;
            var percent = (wins / totalWins) * 100;
            winnerDic[key] = Math.Round(percent, 2, MidpointRounding.AwayFromZero);
        }
    }

    private Dictionary<string, decimal> ReadCSV()
    {
        StreamReader streamReader = new StreamReader(filePath);
        var dictionary = new Dictionary<string, decimal>();
        bool eof = false;
        while (!eof)
        {
            string dataString = streamReader.ReadLine();
            if (dataString == null)
            {
                eof = true;
                break;
            }
            if (dictionary.ContainsKey(dataString))
            {
                dictionary[dataString] += 1;
            }
            else
            {
                dictionary.Add(dataString, 0);
            }
        }
        streamReader.Close();
        return dictionary;
    }
}