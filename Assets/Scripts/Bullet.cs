using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    public float move = 0.1f;
    private float _direction;
    public Vector3 direction;
    private int count;

    private void Update()
    {
        transform.Translate(direction * move);
        count++;

        if (count > 100)
        {
            Destroy (this.gameObject);
        }
        //
        // _counter++;
        // if (_counter != inverseCounter) return;
        //
        // _counter = 0;
        // _direction *= -1;
    }
}