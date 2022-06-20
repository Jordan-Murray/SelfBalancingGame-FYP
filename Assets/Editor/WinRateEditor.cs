using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System;
using Unity.EditorCoroutines.Editor;
using UnityEngine.Events;
using UnityEditor.AnimatedValues;

public class WinRateEditor : EditorWindow
{
    CsvHandler csvHandler = new CsvHandler();
    
    
    [SerializeField] GameObject source = null;
    SerializedObject serObj;

    //[SerializeField] string filePath = "Assets/Winner.txt";
    [SerializeField] string filePath;
    //[SerializeField] string statsFilePath = "Assets/Cars.csv";
    [SerializeField] string statsFilePath;
    [SerializeField] string previousWinRatesFilePath = "Assets/PreviousWinRates.txt";
    [SerializeField] Dictionary<string, decimal> winnerDic = new Dictionary<string, decimal>();
    [SerializeField] Dictionary<float,decimal> previousWinRates = new Dictionary<float,decimal>();

    float newStat;
    List<dynamic> expandoObjList = new List<dynamic>();
    List<string> fields = new List<string>();
    List<string> expandoObjName = new List<string>();
    bool showingLabels = false;
    bool showRest = true;
    bool updatedWinRates = false;
    bool testingStatLimit = true;
    
    [SerializeField] int charSelectIndex = 0;
    [SerializeField] int statSelectIndex = 0;
    [SerializeField] decimal winRateToBalanceTo;
    string winRateToBalanceToString;

    string minValue;
    string maxValue;
    float currentStat = 0f;
    
    [SerializeField] List<string> statStringList = new List<string>();
    [SerializeField] int statListIndex = 0;
    [SerializeField] List<Stat> statLimitList = new List<Stat>();
    List<Stat> changedStatList = new List<Stat>();
    IDictionary<string, object> dicobj;
    string dynamicField;
    
    [SerializeField] int scriptStringIndex = 0;
    [SerializeField] int scriptPropertyIndex = 0;
    int scriptPropertyIndexAtCurrentRunNumber = 0;
    int newStatIndex;

    SerializedProperty serProp;
    SerializedProperty serPropAllRunsFinished;
    List<string> propNameList = new List<string>();
    
    [SerializeField] int numberOfRuns;
    int currentRunNumber;
    bool gameFinishedAllRuns;
    
    [SerializeField] float balanceSlider;
    [SerializeField] float margingOfErrorSlider;
    Vector2 vSbarValue;

    bool forceBreak = false;
    [SerializeField] bool isBalanced = false;
    bool winRateTooHigh = false;
    bool winRateTooLow = false;
    decimal currentBalancingPlayerWinRate;

    [MenuItem("Game Balancing Framework/Balance to Win Rate")]
    public static void ShowWindow()
    {
        GetWindow<WinRateEditor>("Balance to Win Rate");
    }

    IEnumerator CountEditorUpdates()
    {
        var waitForOneSecond = new EditorWaitForSeconds(2.0f);
        while (true)
        {
            yield return waitForOneSecond;
            if(!forceBreak){
                EvaluateIsBalanced();
                if(!isBalanced)
                    Balance();
            }
        }
    }

    public WinRateEditor()
    {
        EditorGUIUtility.fieldWidth = 220;
        EditorGUIUtility.labelWidth = 220;
    }

    private void OnInspectorUpdate() {
        if(EditorApplication.isPaused == false && EditorApplication.isPlaying == true)
        {
            //Get current run number from game manager
            setGameRuns(source.GetComponents<MonoBehaviour>());
            if(currentRunNumber == numberOfRuns && !isBalanced)
            {
                EditorApplication.isPlaying = false;
                gameFinishedAllRuns = true;
                if (gameFinishedAllRuns && !forceBreak)
                {
                    updatedWinRates = false;
                    gameFinishedAllRuns = false;
                    showRest = true;
                    currentRunNumber = 0;
                    CalculateWinPercent();
                    if(!isBalanced)
                        EditorCoroutineUtility.StartCoroutine(CountEditorUpdates(), this);
                }
            }
        }
    }

    private void OnGUI()
    {
        GUILayout.BeginVertical();
        vSbarValue = EditorGUILayout.BeginScrollView(vSbarValue);
        EnterFileNames();
        if(!String.IsNullOrEmpty(filePath) && !String.IsNullOrEmpty(statsFilePath) && !updatedWinRates && GUILayout.Button("Fetch Win Rates and Stats"))
        {
            CalculateWinPercent();
        }
        if (showingLabels == true)
        {
            ShowText();
            EditorGUIUtility.labelWidth = 220;
            if(showRest)
            {
                SelectCharacterToBalance();
                BalanceToWinRate();
                if (!string.IsNullOrEmpty(winRateToBalanceToString)){
                    SetMinMaxCharStats();
                }
            }
            if(statLimitList.Count != 0)
            {
                SelectBalanceSlider();
                ConfigureRunNumber();
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Balance Game"))
                {
                    previousWinRates.Clear();
                    EvaluateIsBalanced();
                    if (scriptPropertyIndex != 0 && !isBalanced)
                    {
                        testingStatLimit = true;
                        Balance();
                    }
                }
            }
            if (GUILayout.Button("Refresh Window"))
            {
                showRest = true;
                updatedWinRates = false;
            }
            if(statLimitList.Count != 0)
            {
                EditorGUILayout.EndHorizontal();  
            }  
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    private void ConfigureRunNumber()
    {
        EditorGUILayout.LabelField("Configure Game Runs", EditorStyles.boldLabel);
        source = (GameObject)EditorGUILayout.ObjectField("Object that configures Game Runs: ", source, typeof(GameObject), true);
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Select the Object and Property that managed the number of Game Runs and Current Run Number", EditorStyles.wordWrappedLabel);
        if (source != null)
        {
            SelectPropertyForGameRuns();
        }
    }

    private void EnterFileNames()
    {
        EditorGUILayout.LabelField("Enter File Path to where Game Winners are stored: ", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        filePath = EditorGUILayout.TextField("File Path", filePath);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Enter File Path to where Character Stats are stored: ", EditorStyles.boldLabel);
        statsFilePath = EditorGUILayout.TextField("File Path", statsFilePath);
        EditorGUILayout.Space();
    }

    private void SelectPropertyForGameRuns()
    {
        var scriptStringArray = GetProptiesFromAllScripts(source);
        scriptStringIndex = EditorGUILayout.Popup(scriptStringIndex, scriptStringArray.ToArray());
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("GameRuns Property", EditorStyles.wordWrappedLabel);
        scriptPropertyIndex = EditorGUILayout.Popup(scriptPropertyIndex, propNameList.ToArray());
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Current Run Number Property", EditorStyles.wordWrappedLabel);
        scriptPropertyIndexAtCurrentRunNumber = EditorGUILayout.Popup(scriptPropertyIndexAtCurrentRunNumber, propNameList.ToArray());
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        setGameRuns(source.GetComponents<MonoBehaviour>());
    }

    private void ShowText()
    {
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Character Win Rates: ", EditorStyles.boldLabel);
        EditorGUIUtility.labelWidth = 10;
        //Titles
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Object Name");
        EditorGUILayout.LabelField("Object Win Percentage");
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        //Name and Win Rate
        for (int i = 0; i < winnerDic.Count(); i++)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(winnerDic.ElementAt(i).Key);
            EditorGUILayout.LabelField(winnerDic.ElementAt(i).Value.ToString());
            EditorGUILayout.EndHorizontal();
        }
    }

    private void BalanceToWinRate()
    {
        // if(winRateToBalanceTo == 0.0m){
            EditorGUILayout.LabelField("Enter your desired Win Rate Percentage", EditorStyles.boldLabel);
            winRateToBalanceToString = EditorGUILayout.TextField("Desired Win Rate: ", winRateToBalanceToString);
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            if (!string.IsNullOrEmpty(winRateToBalanceToString))
            {
                winRateToBalanceTo = Decimal.Parse(winRateToBalanceToString);
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Enter your desired margin of error", EditorStyles.boldLabel);
            margingOfErrorSlider = EditorGUILayout.Slider("Margin of Error",margingOfErrorSlider, 0.1f, 2.0f);
            EditorGUILayout.Space();
            EditorGUILayout.Space();
        // }
    }

    private void SelectBalanceSlider()
    {
        EditorGUILayout.LabelField("Select Below on the Slider Your Desired Balance Rigorousness", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Note - This effects the number of Game Runs your game will comeplete before selecting new stats to test", EditorStyles.wordWrappedLabel);
        balanceSlider = EditorGUILayout.Slider("Balance Rigorousness",balanceSlider, 25, 100);
        numberOfRuns = (int)balanceSlider * 2;
        EditorGUILayout.LabelField("Your game will comeplete " + numberOfRuns + " runs to detirmine win rates and balance every itteration", EditorStyles.wordWrappedLabel);
        EditorGUILayout.Space();
        EditorGUILayout.Space();
    }

    private void Balance()
    {
        forceBreak = false;
        //Get current stats
        var statString = dicobj.Single(s => s.Key == statLimitList[0].characterStatName).Value.ToString();
        currentStat = float.Parse(statString);
        var lowerStatLimit = statLimitList[0].lowerStatLimit;
        var upperStatLimit = statLimitList[0].upperStatLimit;

        previousWinRates = csvHandler.ReadPreviousWinRatesFromFile(previousWinRatesFilePath);

        if(previousWinRates.Count == 1 && testingStatLimit){
            if(currentBalancingPlayerWinRate < winRateToBalanceTo && newStat == upperStatLimit){
                Debug.Log("Character stat limits are currently too restricted to reach the desired win rate");
                forceBreak = true;
            }else if(currentBalancingPlayerWinRate > winRateToBalanceTo && newStat == lowerStatLimit){
                Debug.Log("Character stat limits are currently too restricted to reach the desired win rate");
                forceBreak = true;
            }
            testingStatLimit = false;
        }

        if(!previousWinRates.ContainsKey(currentStat))
        {
            csvHandler.WritePreviousWinRateToFile(previousWinRatesFilePath,new KeyValuePair<float, decimal>(currentStat,currentBalancingPlayerWinRate));
            previousWinRates[currentStat] = currentBalancingPlayerWinRate;
            // Debug.Log("Adding to Prev Win Rates: " + currentStat +" - " + currentBalancingPlayerWinRate);
        }
        
        //Work out new stat
        if(winRateTooHigh){
            newStat = (lowerStatLimit + currentStat) / 2;
        }
        else if(winRateTooLow){
            newStat = (upperStatLimit + currentStat) / 2;
        }

        //Get lowest win rate above target
        //e.g Target WR = 25% Previous WRs = 40%,30%,60%,10%,5%
        //Lowest WR > Target = 30%;

        var lowestWinRateAboveTarget = new KeyValuePair<float,decimal>(-1,currentBalancingPlayerWinRate);
        var highestWinRateBelowTarget = new KeyValuePair<float,decimal>(-1,currentBalancingPlayerWinRate);;

        if(previousWinRates.Any(wr => wr.Value > winRateToBalanceTo))
        {
            var lowestWinRateAboveTargets = previousWinRates.Where(wr => wr.Value > winRateToBalanceTo).OrderBy(wr => wr.Value);
            //Need to find if the lowest win rate above target has any duplicates, if so, pick the lowest stat
            //atm its just finding any duplicates and picking the lowest one, rather than seeing if the lowest win rate above has any duplicaes. Need to fix.
            // if(lowestWinRateAboveTargets.GroupBy(wr => wr.Value).Where(wr => wr.Count() > 1).Any()){
            //     // lowestWinRateAboveTargets.OrderBy(wr => wr.Key);
            //     lowestWinRateAboveTarget = lowestWinRateAboveTargets.GroupBy(wr => wr.Value).Where(wr => wr.Count() > 1).First().OrderBy(wr => wr.Key).First();
            //     Debug.Log("Duplicates found, chosing lowest stat: " + lowestWinRateAboveTarget.Key + " - " + lowestWinRateAboveTarget.Value);

            // }else{
                lowestWinRateAboveTarget = previousWinRates.Where(wr => wr.Value > winRateToBalanceTo).OrderBy(wr => wr.Value).First();
                if(previousWinRates
                    .Where(wr => wr.Value == lowestWinRateAboveTarget.Value && wr.Key != lowestWinRateAboveTarget.Key)
                    .Any()){
                    lowestWinRateAboveTarget = previousWinRates
                        .Where(wr => wr.Value == lowestWinRateAboveTarget.Value && wr.Key != lowestWinRateAboveTarget.Key)
                        .OrderBy(wr => wr.Key)
                        .First();    
                }
            // }
            // lowestWinRateAboveTarget = previousWinRates.Where(wr => wr.Value > winRateToBalanceTo).OrderBy(wr => wr.Value).First();
        }

        //Get highest win rate below target
        //e.g Target WR = 25% Previous WRs = 40%,30%,60%,10%,5%
        //Highest WR < Target = 10%;

        if(previousWinRates.Any(wr => wr.Value < winRateToBalanceTo))
        {
            var highestWinRateBelowTargets = previousWinRates.Where(wr => wr.Value < winRateToBalanceTo).OrderByDescending(wr => wr.Value);
            //Need to find if the highest win rate below target has any duplicates, if so, pick the highest stat
            //atm its just finding any duplicates and picking the highest one, rather than seeing if the highest win rate below has any duplicaes. Need to fix.
            // if(highestWinRateBelowTargets.GroupBy(wr => wr.Value).Where(wr => wr.Count() > 1).Any()){
            //     // highestWinRateBelowTargets.OrderByDescending(wr => wr.Key);
            //     highestWinRateBelowTarget = highestWinRateBelowTargets.GroupBy(wr => wr.Value).Where(wr => wr.Count() > 1).First().OrderByDescending(wr => wr.Key).First();
            //     Debug.Log("Duplicates found, chosing higest stat: " + highestWinRateBelowTarget.Key + " - " + highestWinRateBelowTarget.Value);
            // }else{
                highestWinRateBelowTarget = previousWinRates.Where(wr => wr.Value < winRateToBalanceTo).OrderByDescending(wr => wr.Value).First();
                if(previousWinRates
                    .Where(wr => wr.Value == highestWinRateBelowTarget.Value && wr.Key != highestWinRateBelowTarget.Key)
                    .Any()){
                    highestWinRateBelowTarget = previousWinRates
                        .Where(wr => wr.Value == highestWinRateBelowTarget.Value && wr.Key != highestWinRateBelowTarget.Key)
                        .OrderByDescending(wr => wr.Key)
                        .First();    
                }
            // }
            // highestWinRateBelowTarget = previousWinRates.Where(wr => wr.Value < winRateToBalanceTo).OrderByDescending(wr => wr.Value).First();
        }

        if(highestWinRateBelowTarget.Key != -1 && lowestWinRateAboveTarget.Key != -1){
            //New stat = ([Stat,30%] + [Stat,10%]) / 2;
            newStat = (lowestWinRateAboveTarget.Key + highestWinRateBelowTarget.Key) / 2;
            // Debug.Log("New Stat: " + lowestWinRateAboveTarget.Key + " + " + highestWinRateBelowTarget.Key + " / 2 = "+ newStat);
        }else if(highestWinRateBelowTarget.Key == -1 && lowestWinRateAboveTarget.Key != -1){
            newStat = (lowestWinRateAboveTarget.Key + lowerStatLimit) / 2;
            // Debug.Log("Used LowestWinRateAboveTarget: " + lowestWinRateAboveTarget.Key + " + " + lowerStatLimit + " / 2"+ " = " + newStat);
        }else if(highestWinRateBelowTarget.Key != -1 && lowestWinRateAboveTarget.Key == -1){
            newStat = (highestWinRateBelowTarget.Key + upperStatLimit) / 2;
            // Debug.Log("Used HighestWinRateBelowTarget: " + highestWinRateBelowTarget.Key + " + " + upperStatLimit + " / 2"+ " = " + newStat);
        }else{
            // Debug.Log("Used Upper and Lower Limits");
        }

        if(winRateTooHigh && testingStatLimit){
            newStat = lowerStatLimit;
        }else if(winRateTooLow && testingStatLimit){
            newStat = upperStatLimit;
        }

        newStat = (float)Math.Round(newStat, 2);

        var attempts = 1;
        if(!StatTestedBefore(lowestWinRateAboveTarget,highestWinRateBelowTarget,attempts)){
            //Write new stats
            newStatIndex = 1;
            for(int i = 0; i< fields.Count; i++)
            {
                if(fields[i] == statLimitList[0].characterStatName)
                {
                    newStatIndex = i;
                }
            }

            //Adding name back into list
            var newobj = expandoObjList.ElementAt(charSelectIndex);
            var dictionaryObj = (IDictionary<string, object>)newobj;
            if(!dicobj.ContainsKey(fields[0]))
                dictionaryObj.Add(fields[0],expandoObjName[charSelectIndex]);

            csvHandler.WriteNewStats(statsFilePath,newStatIndex,charSelectIndex,newStat, dictionaryObj);
            csvHandler.ReadWinnerCSV(filePath,true);

            showRest = false;
            Repaint();

            EditorApplication.isPlaying = true;
        }
    }

    private bool StatTestedBefore(KeyValuePair<float,decimal> lowestWinRateAboveTarget, KeyValuePair<float,decimal> highestWinRateBelowTarget, int attempts){
        if(attempts >= 10)
        {
            forceBreak = true;
            Debug.Log("A suitable new stat couldnt be found");
            return false;
        }
        if(previousWinRates.ContainsKey(newStat))
        {
            var winRateWithStat = previousWinRates[newStat];
            if(winRateWithStat < winRateToBalanceTo){
                newStat = (newStat + lowestWinRateAboveTarget.Key) / 2;
                newStat = (float)Math.Round(newStat, 2);

            }else if(winRateWithStat > winRateToBalanceTo){
                newStat = (newStat + highestWinRateBelowTarget.Key) / 2;
                newStat = (float)Math.Round(newStat, 2);
            }
            attempts++;
            StatTestedBefore(lowestWinRateAboveTarget,highestWinRateBelowTarget,attempts);
        }
        return false;
    }

    private void EvaluateIsBalanced()
    {
        //If we have any win rates
        // foreach (var kvp in winnerDic)
        // {
        //     Debug.Log(kvp.Key + ": " + kvp.Value);
        // }
        if(winnerDic.Any(s => s.Key ==  expandoObjName[charSelectIndex]))
        {
            var currentBalancingPlayer = winnerDic.Single(s => s.Key == expandoObjName[charSelectIndex]).Key;
            
            currentBalancingPlayerWinRate = winnerDic.Single(s => s.Key == expandoObjName[charSelectIndex]).Value;

            var minWinRateToBalanceTo = (float)winRateToBalanceTo - margingOfErrorSlider;
            var maxWinRateToBalanceTo = (float)winRateToBalanceTo + margingOfErrorSlider;
            
            if (currentBalancingPlayerWinRate == winRateToBalanceTo ||
            ((float)currentBalancingPlayerWinRate > minWinRateToBalanceTo && (float)currentBalancingPlayerWinRate < maxWinRateToBalanceTo))
            {
                isBalanced = true;
                Debug.Log("Balanced!");
                Debug.Log(currentBalancingPlayerWinRate + " > " + minWinRateToBalanceTo);
                Debug.Log(currentBalancingPlayerWinRate + " < " + maxWinRateToBalanceTo);

            }
            else{
                isBalanced = false;
                
                if((float)currentBalancingPlayerWinRate < minWinRateToBalanceTo)
                {
                    winRateTooLow = true;
                    winRateTooHigh = false;
                }else if((float)currentBalancingPlayerWinRate > maxWinRateToBalanceTo){
                    winRateTooHigh = true;
                    winRateTooLow = false;
                }
                
            }
        }else{
            Debug.Log("No winrate available");
        }
    }

    private void SetMinMaxCharStats()
    {
        EditorGUILayout.LabelField("Set Minimum and Maximum Value for Character Stats", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Note - Only stats with limits set will be altered");
        EditorGUILayout.BeginHorizontal();
        statSelectIndex = EditorGUILayout.Popup(statSelectIndex, fields.ToArray());
        dynamicField = fields[statSelectIndex];
        var newobj = expandoObjList.ElementAt(charSelectIndex);
        dicobj = (IDictionary<string, object>)newobj;

        EditorGUILayout.LabelField(dicobj[dynamicField].ToString(), EditorStyles.boldLabel);
        EditorGUILayout.EndHorizontal();

        if (float.TryParse(dicobj[dynamicField].ToString(), out float numProperty))
        {
            minValue = EditorGUILayout.TextField("Min Value: ", minValue);
            maxValue = EditorGUILayout.TextField("Max Value: ", maxValue);
        }
        if (!string.IsNullOrEmpty(minValue) && !string.IsNullOrEmpty(maxValue)
            && float.TryParse(minValue, out float minValueFloat) && float.TryParse(maxValue, out float maxValueFloat)
            && float.TryParse(dicobj[dynamicField].ToString(), out float currentStatNum))
        {
            var newStat = new Stat(currentStatNum, maxValueFloat, minValueFloat, dynamicField, expandoObjName[charSelectIndex]);
            if (GUILayout.Button("Add Limit"))
            {
                AddIfNoDupe(statLimitList, newStat);
            }
            if(statLimitList.Count != 0)
                statListIndex = EditorGUILayout.Popup(statListIndex, statStringList.ToArray());
            if(statLimitList.Count != 0 && statStringList.Count != 0)
            {
                if (GUILayout.Button("Remove Limit"))
                {
                    statStringList.RemoveAt(statListIndex);
                    statLimitList.RemoveAt(statListIndex);
                    statListIndex = 0;
                }
            }
        }
        EditorGUILayout.Space();
        EditorGUILayout.Space();
    }
    
    private void AddIfNoDupe(List<Stat> currentList, Stat newStat)
    {
        currentList.Add(newStat);
        List<Stat> distinctList = new List<Stat>(); 
        if (currentList.Count != 0)
        {
            distinctList = currentList
            .GroupBy(s => new { s.characterStatOwner, s.characterStatName })
            .Select(g => g.First())
            .ToList();
            var containsDupe = distinctList.Count != currentList.Count;
            if (containsDupe)
            {
                currentList.Remove(newStat);
                Debug.Log("You can't add two limits for one stat");
            }
            else{
                var statString = newStat.StatToString();
                statStringList.Add(statString);
            }
        }
    }

    private void SelectCharacterToBalance()
    {
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        expandoObjList = csvHandler.ReadStatsCSV(statsFilePath, true);
        fields = csvHandler.fields;
        foreach (var obj in expandoObjList)
        {
            foreach (KeyValuePair<string, object> kvp in ((IDictionary<string, object>)obj))
            {
                if (kvp.Key == "Name")
                {
                    string PropertyWithValue = kvp.Value.ToString();
                    expandoObjName.Add(PropertyWithValue);
                }
            }
        }

        EditorGUILayout.LabelField("Select Character to Balance", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        charSelectIndex = EditorGUILayout.Popup(charSelectIndex, expandoObjName.ToArray());
        EditorGUILayout.Space();
        EditorGUILayout.Space();
    }

    private void CalculateWinPercent()
    {
        winnerDic.Clear();
        winnerDic = csvHandler.ReadWinnerCSV(filePath,false);
        showingLabels = true;
        var totalWins = (Decimal)0;
        if(winnerDic.Count != 0)
        {
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
            for(int i = 0; i < expandoObjName.Count(); i++)
            {
                if(!winnerDic.ContainsKey(expandoObjName[i])){
                    winnerDic[expandoObjName[i]] = 0;
                }
            }
        }
        updatedWinRates = true;
    }

    public List<string> GetProptiesFromAllScripts(GameObject source) //move to game run handler
    {
        var scripts = source.GetComponents<MonoBehaviour>();
        var scriptStringArray = new List<string>();
        foreach (var item in scripts)
        {
            scriptStringArray.Add(item.ToString());
        }
        
        return scriptStringArray;
    }

    public void setGameRuns(MonoBehaviour[] scripts)
    {
        serObj = new SerializedObject(scripts[scriptStringIndex]);
        serObj.Update();
        EditorGUI.BeginChangeCheck();
        SerializedProperty prop = serObj.GetIterator();
        while (prop.NextVisible(true))
        {
            propNameList.Add(prop.name);
        }
        //GameRuns
        serProp = serObj.FindProperty(propNameList[scriptPropertyIndex]);
        if(serProp != null)
        {
            switch (serProp.propertyType)
            {
                case SerializedPropertyType.Boolean:
                    EditorGUILayout.LabelField(serProp.boolValue.ToString());
                    break;
                case SerializedPropertyType.Bounds:
                    EditorGUILayout.LabelField(serProp.boundsValue.ToString());
                    break;
                case SerializedPropertyType.Float:
                    EditorGUILayout.LabelField(serProp.floatValue.ToString());
                    break;
                case SerializedPropertyType.Integer:
                    serProp.intValue = numberOfRuns;
                    break;
                case SerializedPropertyType.Vector2:
                    EditorGUILayout.LabelField(serProp.vector2Value.ToString());
                    break;
                case SerializedPropertyType.Vector3:
                    EditorGUILayout.LabelField(serProp.vector3Value.ToString());
                    break;
                default:
                    EditorGUILayout.LabelField("Unknown variable selected, try another");
                    break;
            }
        }
        else{
            EditorGUILayout.LabelField("Unknown variable selected, try another");
        }
        serPropAllRunsFinished = serObj.FindProperty(propNameList[scriptPropertyIndexAtCurrentRunNumber]);
        if(serPropAllRunsFinished != null)
        {
            switch (serPropAllRunsFinished.propertyType)
            {
                case SerializedPropertyType.Boolean:
                    EditorGUILayout.LabelField(serPropAllRunsFinished.boolValue.ToString());
                    break;
                case SerializedPropertyType.Bounds:
                    EditorGUILayout.LabelField(serPropAllRunsFinished.boundsValue.ToString());
                    break;
                case SerializedPropertyType.Float:
                    EditorGUILayout.LabelField(serPropAllRunsFinished.floatValue.ToString());
                    break;
                case SerializedPropertyType.Integer:
                //Set current run number in extention to current run number in game, so we know when we've completed all the runs
                    currentRunNumber = serPropAllRunsFinished.intValue;
                    break;
                case SerializedPropertyType.Vector2:
                    EditorGUILayout.LabelField(serPropAllRunsFinished.vector2Value.ToString());
                    break;
                case SerializedPropertyType.Vector3:
                    EditorGUILayout.LabelField(serPropAllRunsFinished.vector3Value.ToString());
                    break;
                default:
                    EditorGUILayout.LabelField("Unknown variable selected, try another");
                    break;
            }
        }
        else{
            EditorGUILayout.LabelField("Unknown variable selected, try another");
        }
        EditorGUI.EndChangeCheck();
        serObj.ApplyModifiedProperties();
    }

    public float GetAveragePowerLevelOfOtherCharacters(){
        List<float> powerLevels = new List<float>();

        var newcharobj = expandoObjList.ElementAt(charSelectIndex);
        var purgedExpandoList = expandoObjList;

        for (int i = 0; i < expandoObjList.Count; i++)
        {
            var newobj = purgedExpandoList.ElementAt(i);
            var dictionaryObj = (IDictionary<string, object>)newobj;
           
            if(dictionaryObj.ContainsKey("Name")){
                if(dictionaryObj["Name"].ToString() != expandoObjName.ElementAt(charSelectIndex))
                {
                    dictionaryObj.Remove("Name");
                    var powerLevel = 0f;
                    foreach(KeyValuePair<string, object> entry in dictionaryObj)
                    {       
                        powerLevel+=float.Parse(entry.Value.ToString());
                    }
                    powerLevels.Add(powerLevel);
                }
            }
        }
        return powerLevels.Average();
    }

    public float GetAveragePowerLevelOfCurrentCharacter(){
        List<float> powerLevels = new List<float>();

        var newobj = expandoObjList.ElementAt(charSelectIndex);
        var dictionaryObj = (IDictionary<string, object>)newobj;
        var namePurgedList = dictionaryObj;
        if(namePurgedList.ContainsKey("Name")){
            namePurgedList.Remove("Name");
        }
        var powerLevel = 0f;
        foreach(KeyValuePair<string, object> entry in namePurgedList)
        {       
            powerLevel+=float.Parse(entry.Value.ToString());
        }
        powerLevels.Add(powerLevel);
        return powerLevels.Average();
    }
}