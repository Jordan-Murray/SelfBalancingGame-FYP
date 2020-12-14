using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class RaceManager : MonoBehaviour
{
    public bool RaceStarted;
    public float StartTime;
    public Text countdownText;

    public GameObject FinishLine;

    public List<GameObject> StartPositions;
    public List<GameObject> prefabs;

    public ImportCarsCSV importCars;
    public List<GameObject> CarsRacing;

    [SerializeField]
    public int GameRuns = 1;
    public int currentRunNumber = 1;
    public float GameSpeed;
    [System.NonSerialized]
    public float TimerGameSpeed;

    int carsFinished = 0;
    bool RaceFinished;


    void Start()
    {
        CarsRacing = importCars.carsList;
        carsFinished = 0;
        StartPositions = GameObject.FindGameObjectsWithTag("StartPositionNode").ToList();
        RaceStarted = false;
        TimerGameSpeed = 1/GameSpeed;
        countdownText.text = StartTime.ToString();
        StartCoroutine("StartRace",StartTime);
    }

    private void Update() {
        if(RaceFinished && currentRunNumber <= GameRuns - 1)
        {
            RaceFinished = false;
            ResetCars();
            importCars.Start();
            Start();
            currentRunNumber+=1;
        }
    }

    private void ResetCars()
    {
        foreach(GameObject car in CarsRacing)
        {
            Destroy(car);
        }
        CarsRacing.Clear();
    }

    public void RecordPosition(string carName)
    {
        StreamWriter writer = new StreamWriter("Assets/Winner.txt", true);
        writer.Write(carName + '\n');
        carsFinished++;
        writer.Close();
        if(carsFinished == CarsRacing.Count())
        {
            RaceFinished = true;
        }
    }

    IEnumerator StartRace(float delay){
        while(delay > 0)
        {
            yield return new WaitForSeconds(1.0f * TimerGameSpeed);
            delay--;
            countdownText.text = delay.ToString();
        }
        RaceStarted = true;
        countdownText.text = "Go";
        yield return new WaitForSeconds(1.0f * TimerGameSpeed);
        countdownText.text = string.Empty;
    }
};