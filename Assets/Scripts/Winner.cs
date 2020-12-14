using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Winner : MonoBehaviour
{
    public RaceManager raceManager;
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
            raceManager.RecordPosition(col.name);
        }
    }
}
