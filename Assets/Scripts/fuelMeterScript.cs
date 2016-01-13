using UnityEngine;
using System.Collections;

public class fuelMeterScript : MonoBehaviour
{

    [SerializeField]
    MeshRenderer[] display;

    [SerializeField]
    PlayerMotor motor;

    [SerializeField]
    Material[] materials;

    float oldFuel;

    // Use this for initialization
    void Start()
    {
        foreach (MeshRenderer r in display)
        {
            r.material = materials[1]; //On state
        }
        oldFuel = motor.fuel;
    }

    void Update()
    {
        float fuel = motor.fuel;
        if (fuel != oldFuel)
        {
            if (motor.jetpackRefueling) // Refueling
            {
                //Reset to off state
                foreach (MeshRenderer r in display)
                {
                    r.material = materials[0];
                }
                float fuelPerBar = motor.maxFuel / 5f;
                int barsOn = Mathf.CeilToInt(fuel / fuelPerBar);
                for (int i = 0; i < barsOn && i < 5; i++)
                {
                    display[i].material = materials[1]; //On state
                }
            }
            else
            {
                //Reset to off state
                foreach (MeshRenderer r in display)
                {
                    r.material = materials[0];
                }
                float fuelPerBar = motor.maxFuel / 5f;
                int barsOn = Mathf.FloorToInt(fuel / fuelPerBar);
                for (int i = 0; i < barsOn && i < 5; i++)
                {
                    display[i].material = materials[1]; //On state
                }
            }
        }
        oldFuel = fuel;
    }
}
