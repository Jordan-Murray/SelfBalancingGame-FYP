using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DragCar : MonoBehaviour
{
    public string Name;
    public float TopSpeed;
    public float Acceleration;
    private float currentSpeed;
    public float reactionTime;
    bool waitedForReaction = false;
    private RaceManager raceManager;
    public bool raceFinished;
    public static Text CarStats;

    // Start is called before the first frame update
    void Start()
    {
        raceFinished = false;
        reactionTime = UnityEngine.Random.Range(0.5f,10f);
        currentSpeed = 0;   
        raceManager = FindObjectsOfType<RaceManager>().FirstOrDefault();
    }

    static public GameObject Create(string _Name, float _TopSpeed, float _Acceleration, int _CarNumber)
    {
        var raceManager = FindObjectsOfType<RaceManager>().FirstOrDefault();
        CarStats = GameObject.Find("Car"+(_CarNumber+1).ToString()).GetComponent<Text>();
        SetCarStatsText(_Name,_TopSpeed,_Acceleration);
        GameObject car = Instantiate(raceManager.prefabs[_CarNumber], raceManager.StartPositions[_CarNumber].transform.transform.position, Quaternion.identity);
        DragCar carScript = car.AddComponent<DragCar>() as DragCar;
        car.tag = "Player";
        carScript.name = _Name;
        carScript.Name = _Name;
        carScript.TopSpeed = _TopSpeed;
        carScript.Acceleration = _Acceleration;
        carScript.raceFinished = false;
        return car; 
    }

    // Update is called once per frame
    void Update()
    {
        if(!raceFinished)
        {
            Drive();
        }
    }

    public static void SetCarStatsText(string _Name, float _TopSpeed, float _Acceleration)
    {
        CarStats.text = _Name + ": Top Speed - " + _TopSpeed + ", Acc - " + _Acceleration;
    }

    private void Drive()
    {
        Vector3 playerMovement = new Vector3();
        while(currentSpeed <= TopSpeed && RaceStarted())
        {
            currentSpeed += Acceleration/11;
            //Debug.Log("Current Speed = " + currentSpeed + ". Top Speed = " + TopSpeed);
            playerMovement = new Vector3(currentSpeed,0f,0f) * Time.fixedDeltaTime * raceManager.GameSpeed;
            transform.Translate(playerMovement, Space.Self);
            break;
        }
        playerMovement = new Vector3(currentSpeed,0f,0f) * Time.fixedDeltaTime * raceManager.GameSpeed;
        transform.Translate(playerMovement, Space.Self);

    }

    private bool RaceStarted()
    {
        StartCoroutine(waitForReactionCoroutine());
        if(raceManager.RaceStarted && waitedForReaction)
            return true;
        return false;
    }

    IEnumerator waitForReactionCoroutine()
    {
        yield return new WaitForSeconds(reactionTime * raceManager.TimerGameSpeed);
        waitedForReaction = true;
    }
}
