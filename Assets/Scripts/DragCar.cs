using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DragCar : MonoBehaviour
{
    public string Name;
    public int TopSpeed;
    public float Acceleration;
    private float currentSpeed;
    public float reactionTime;
    bool waitedForReaction = false;
    private RaceManager raceManager;
    public bool raceFinished;

    // Start is called before the first frame update
    void Start()
    {
        raceFinished = false;
        reactionTime = UnityEngine.Random.Range(0.5f,5f);
        currentSpeed = 0;   
        raceManager = FindObjectsOfType<RaceManager>().FirstOrDefault();
    }

    static public GameObject Create(string _Name, int _TopSpeed, float _Acceleration, int _CarNumber)
    {
        var raceManager = FindObjectsOfType<RaceManager>().FirstOrDefault();
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

    private void Drive()
    {
        while(currentSpeed <= TopSpeed && RaceStarted())
        {
            currentSpeed += Acceleration;
        }
        Vector3 playerMovement = new Vector3(currentSpeed,0f,0f) * Time.deltaTime * raceManager.GameSpeed;
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
