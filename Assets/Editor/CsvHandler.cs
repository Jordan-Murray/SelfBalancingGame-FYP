using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;

public class CsvHandler
{
    public List<string> fields = new List<string>();
    private int PatchNumber = 1;

    public Dictionary<string, decimal> ReadWinnerCSV(string filePath, bool generateNewPatch)
    {
        var dictionary = new Dictionary<string, decimal>();
        StreamReader streamReader = new StreamReader(filePath);
        bool eof = false;
        var csv = new StringBuilder(); 
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
                csv.AppendLine(dataString);
            }
            else
            {
                dictionary.Add(dataString, 1);
            }
        }
        streamReader.Close();
        if(generateNewPatch)
        {
            GenerateNewPatchWinRates(filePath,csv);
        }
        return dictionary;
    }

    public void GenerateNewPatchWinRates(string filePath, StringBuilder csv)
    {
        //Clear winrates for new patch
        File.WriteAllText(filePath, String.Empty);
        //Write winner csv to new patch winner
        filePath = GetNewFilePath(filePath);
        File.WriteAllText(filePath, csv.ToString());
    }

    string GetNewFilePath(string _filePath)
    {
        string newFilePath = _filePath.Substring(0, _filePath.Length - 4);
        newFilePath+="Patch"+PatchNumber+".txt";
        PatchNumber++;
        return newFilePath;
    }

    public void WritePreviousWinRateToFile(string filePath, KeyValuePair<float,decimal> statAndWinRate)
    {
        StreamWriter writer = new StreamWriter(filePath, true);
        writer.WriteLine(statAndWinRate.Key + "," + statAndWinRate.Value);
        writer.Close();
    }

    public Dictionary<float,decimal> ReadPreviousWinRatesFromFile(string filePath){
        Dictionary<float,decimal> PreviousWinRates = new Dictionary<float, decimal>();
        StreamReader streamReader = new StreamReader(filePath);
        bool eof = false;
        while (!eof)
        {
            string dataString = streamReader.ReadLine();
            if (dataString == null)
            {
                eof = true;
                break;
            }
            else
            {
                var dataValues = dataString.Split(',');
                PreviousWinRates.Add(float.Parse(dataValues[0]),decimal.Parse(dataValues[1]));
            }
        }
        streamReader.Close();
        return PreviousWinRates;
    }

    string GetLine(string fileName, int line)
    {
        using (var sr = new StreamReader(fileName))
        {
            for (int i = 1; i < line; i++)
                sr.ReadLine();
            return sr.ReadLine();
        }
    }

    public List<dynamic> ReadStatsCSV(string statsFilePath, bool getFields)
    {
        fields.Clear();
        StreamReader streamReader = new StreamReader(statsFilePath);
        bool eof = false;
        var lineNumber = 1;
        List<dynamic> expandoList = new List<dynamic>();
        while (!eof)
        {
            string dataString = streamReader.ReadLine();
            if (dataString == null)
            {
                eof = true;
                break;
            }
            var dataValues = dataString.Split(',');
            if (lineNumber == 1 && getFields)
            {
                foreach (var field in dataValues)
                {
                    fields.Add(field);
                }
            }
            else
            {
                dynamic newexpando = new ExpandoObject();
                for (int i = 0; i < dataValues.Length; i++)
                {
                    var dynamicField = fields.ElementAt(i);
                    newexpando = AddExpandoFeild(newexpando, dynamicField, dataValues[i]);
                }
                expandoList.Add(newexpando);
            }
            lineNumber++;
        }
        streamReader.Close();
        return expandoList;
    }

    private IDictionary<string, System.Object> AddExpandoFeild(ExpandoObject obj, string field, string propertyValue)
    {
        var expandoDict = obj as IDictionary<string, System.Object>;
        if (expandoDict.ContainsKey(field))
        {
            expandoDict[field] = propertyValue;
        }
        else
        {
            expandoDict.Add(field, propertyValue);
        }
        return expandoDict;
    }

    public void WriteNewStats(string filePath , int statSelectIndex, int charSelectIndex, float newStat, IDictionary<string, object> dicobj)
    {   
        
        var dynamicField = fields[statSelectIndex];
        var currentStat = dicobj[dynamicField];
        dicobj[dynamicField] = newStat;

        string writeToFileString = "";
        
        for(int i = 0; i < dicobj.Count; i++)
        {
            var tempStat = new object();   
            if(dicobj.ContainsKey(fields[i])){
                tempStat = dicobj[fields[i]].ToString();
                writeToFileString += tempStat;
                if(i != dicobj.Count -1)
                    writeToFileString += ",";
            }
        }

        var tempFile = Path.GetTempFileName();
        string line  = null;
        int lineNumber = 0;
        int lineNumberToEdit = charSelectIndex + 1;

        using (StreamReader reader = new StreamReader(filePath))
        using (StreamWriter writer = new StreamWriter(tempFile))
        {
            while ((line = reader.ReadLine()) != null)
            {
                if (lineNumber == lineNumberToEdit && writeToFileString != "")
                {
                    writer.WriteLine(writeToFileString);
                }
                else
                {
                    writer.WriteLine(line);
                }
                lineNumber++;
            }
        }

        File.Delete(filePath);
        File.Move(tempFile, filePath);
    }

    public DateTime GetLastChangedTime(string filePath)
    {
        return File.GetLastWriteTime(filePath);
    }
}
