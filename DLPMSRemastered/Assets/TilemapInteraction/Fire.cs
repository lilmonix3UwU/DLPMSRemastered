using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour
{

    private Vector3Int position;

    private VineTileData data;

    private FireManager fireManager;


    private float burnTimeCounter, spreadIntervallCounter;




    public void StartBurning(Vector3Int position, VineTileData data, FireManager fm)
    {
        this.position = position;
        this.data = data;
        fireManager = fm;

        burnTimeCounter = data.burnTime;
        spreadIntervallCounter = data.spreadIntervall;
    }



    private void Update()
    {
        if (data == null)
            return;

        burnTimeCounter -= Time.deltaTime;
        if(burnTimeCounter <= 0)
        {
            fireManager.FinishedBurning(position);
            Destroy(gameObject, 1.0f);
        }

        spreadIntervallCounter -= Time.deltaTime;
        if(spreadIntervallCounter <=0)
        {
            spreadIntervallCounter = data.spreadIntervall;
            fireManager.TryToSpread(position);
        }
        
    }







}
