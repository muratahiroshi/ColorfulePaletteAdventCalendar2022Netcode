using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class PlayerFollowCamera : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float turnSpeed = 10.0f;

    [SerializeField] private float distance = 15.0f;
    [SerializeField] private float high = 3.0f;
    [SerializeField] private float lookDownAngle = 30.0f;
    [SerializeField] private Quaternion verticalRotation;
    [SerializeField] public Quaternion horizontalRotation;

    public Transform Player
    {
        get => player;
        set
        {
            player = value;
            Initialize();
        }
    }

    void Start()
    {
        if (player != null)
        {
            Initialize();
        }
    }

    private void Initialize()
    {
        verticalRotation = Quaternion.Euler(lookDownAngle, 0, 0);
        horizontalRotation = Quaternion.identity;

        UpdateRotationPosition();
    }


    void LateUpdate()
    {
        if (player == null) return;

        // if (Input.GetMouseButton(0))
        // {
        //     horizontalRotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * turnSpeed, 0);
        // }

        UpdateRotationPosition();
    }

    private void UpdateRotationPosition()
    {
        var transform1 = transform;
        transform1.rotation = horizontalRotation * verticalRotation;
        transform1.position =
            player.position + new Vector3(0, high, 0) - transform1.rotation * Vector3.forward * distance;
    }
}