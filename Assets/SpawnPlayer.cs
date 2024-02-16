﻿using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPlayer : MonoBehaviour
{
    public GameObject player_drone;
    public CinemachineVirtualCamera virtualCamera;

    // Start is called before the first frame update
    void Start()
    {
        GameObject instantiated_player_drone = Instantiate(player_drone, transform.position + new Vector3(0, 0.75f, 0), transform.rotation);
        instantiated_player_drone.name = "Low_Poly_Drone01";
        virtualCamera.Follow = instantiated_player_drone.transform;
        virtualCamera.LookAt = instantiated_player_drone.transform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
