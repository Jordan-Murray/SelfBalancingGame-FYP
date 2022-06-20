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

    public GameObject FinishLine;

    public List<GameObject> StartPositions;
    public List<GameObject> prefabs;

    public ImportCarsCSV importCars;
    public List<GameObject> CarsRacing;

    public int GameRuns = 10;
    public int currentRunNumber = 1;
    public float GameSpeed;
    [System.NonSerialized]
    public float TimerGameSpeed;

    public bool RaceFinished;

    private Text Car1Text;
    private Text Car2Text;
    private Text Car3Text;
    private Text Car4Text;


    void Start()
    {
        CarsRacing = importCars.carsList;
        StartPositions = GameObject.FindGameObjectsWithTag("StartPositionNode").ToList();
        RaceStarted = false;
        TimerGameSpeed = 1/GameSpeed;
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
        writer.Close();
    }

    public void FinishRace(){
        RaceFinished = true;
    }

    IEnumerator StartRace(float delay){
        while(delay > 0)
        {
            yield return new WaitForSeconds(1.0f * TimerGameSpeed);
            delay--;
        }
        RaceStarted = true;
        yield return new WaitForSeconds(1.0f * TimerGameSpeed);
    }
}