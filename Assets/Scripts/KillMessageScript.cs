﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class KillMessageScript : MonoBehaviour
{

    public Player owner;
    public Player target;

    void Start()
    {
        Destroy(gameObject, 3f);

        Text killMessage = gameObject.GetComponentInChildren<Text>();
        killMessage.text = owner.name + " has killed " + target.name;
    }
}