using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movable : MonoBehaviour
{
    private Vector3 from,
                    to;

    private float howFar;
    [SerializeField]
    private float speed = 1;

    public float Speed
    {
        get
        {
            return speed;
        }
    }

    protected bool idle = true;

    public bool Idle
    {
        get
        {
            return idle;
        }
    }
    //coroutine move from current position to new position
    public IEnumerator MoveToPosition (Vector3 targetPosition)
    {
        if (speed <= 0)
            Debug.LogWarning("Speed must be a positive number");

        from = transform.position;
        to = targetPosition;
        howFar = 0;
        idle = false;
        do
        {
            howFar += speed * Time.deltaTime;
            if (howFar > 1)
                howFar = 1;

            transform.position = Vector3.LerpUnclamped(from, to, Easing(howFar));

            yield return null;
        }
        while (howFar != 1);

        idle = true;
    }

    //coroutine move from current position to transform, chasing it if it is moving
    public IEnumerator MoveToTransform(Transform target)
    {
        if (speed <= 0)
            Debug.LogWarning("Speed must be a positive number");

        from = transform.position;
        to = target.position;
        howFar = 0;
        idle = false;
        do
        {
            howFar += speed * Time.deltaTime;
            if (howFar > 1)
                howFar = 1;

            to = target.position;
            transform.position = Vector3.LerpUnclamped(from, to, Easing(howFar));

            yield return null;
        }
        while (howFar != 1);

        idle = true;
    }

    public static float InCubic(float t) => t * t * t;
    public static float Easing(float t) => 1 - InCubic(1 - t);
    
}
