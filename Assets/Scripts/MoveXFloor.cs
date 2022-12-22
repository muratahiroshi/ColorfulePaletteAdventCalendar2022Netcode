using Unity.Netcode;
using UnityEngine;

public class MoveXFloor : NetworkBehaviour
{
    public float initialDirection = 1.0f;
    public int inverseCounter = 1500;
    private int _counter = 0;
    public float move = 0.002f;
    private float _direction;

    private void Start()
    {
        _direction = initialDirection;
    }

    private void Update()
    {
        var p = new Vector3(move * _direction, 0, 0);
        transform.Translate(p);

        _counter++;
        if (_counter != inverseCounter) return;
        
        _counter = 0;
        _direction *= -1;
    }
}