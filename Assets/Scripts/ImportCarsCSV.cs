using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ImportCarsCSV : MonoBehaviour
{
    public List<GameObject> carsList;
    public string fileName;
    public int lineNumber;

    // Start is called before the first frame update
    public void Start()
    {
        ReadCSV();
    }

    private void ReadCSV()
    {
        StreamReader streamReader = new StreamReader("Assets/"+ fileName);
        bool eof = false;
        lineNumber = 1;
        while(!eof)
        {
            string dataString = streamReader.ReadLine();
            if (dataString == null)
            {
                eof = true;
                break;
            }

            var dataValues = dataString.Split(',');
            NewMethod(dataValues);
        }
        streamReader.Close();
    }

    private void NewMethod(string[] dataValues)
    {
        if (dataValues[0] != "Name")
        {
            GameObject carObj = DragCar.Create(dataValues[0], Int32.Parse(dataValues[1]), float.Parse(dataValues[2]), lineNumber - 1);
            carsList.Add(carObj);
            lineNumber++;
        }
    }
}
