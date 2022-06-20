using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Winner : MonoBehaviour
{
    public RaceManager raceManager;
    bool firstPlaceFinished = false;
    int carsFinished;
    // Start is called before the first frame update
    void Start()
    {
        FindObjectsOfType<RaceManager>().FirstOrDefault();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider col) {
        if(col.tag == "Player")
        {
            DragCar car = col.gameObject.GetComponent<DragCar>();
            car.raceFinished = true;
            carsFinished++;
            if(!firstPlaceFinished){
                raceManager.RecordPosition(col.name);
                firstPlaceFinished = true;
            }
        }
        if(carsFinished == raceManager.CarsRacing.Count)
        {
            firstPlaceFinished = false;
            carsFinished = 0;
            raceManager.FinishRace();
        }
    }
}
