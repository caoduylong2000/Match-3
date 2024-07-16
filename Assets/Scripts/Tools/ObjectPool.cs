using System;
using System.Collections.Generic;
using UnityEngine;

/*
 * Thí class will allow us to instantiate as many objects as we will need
 * while the scene is being set up so that we don't slow down the game by 
 * calling Instantiate during gameplay.
 * 
 * Is is preferred, but optional, to call PoolObjects dirst with a specified maount
 */
public abstract class ObjectPool<T> : Singleton<ObjectPool<T>> where T: MonoBehaviour
{
    [SerializeField] protected T prefab;

    private List<T> pooledObjects;
    private int amount;
    private bool isReady;
    //Create the pool, with a specified amount of object
    public void PoolObjects(int amount = 0)
    {
        if (amount < 0)
            throw new ArgumentException("Amount to pool must be non-negative,");

        this.amount = amount;

        //Initialize the list
        pooledObjects = new List<T>(amount);

        //Instantiate a bunch of T's
        GameObject newObject;

        for(int i = 0; i != amount; ++i)
        {
            newObject = Instantiate(prefab.gameObject, transform);
            newObject.SetActive(false);

            //Add each T to the list
            pooledObjects.Add(newObject.GetComponent<T>());
        }

        //flag the pool as ready
        isReady = true;
    }

    //get an object from the pool
    public T GetPooledObject()
    {
        //check if pool is ready, if not make it ready
        if (!isReady)
            PoolObjects(1);

        //search through list for something not in use and return it
        for (int i = 0; i != amount; ++i)
            if (!pooledObjects[i].isActiveAndEnabled)
                return pooledObjects[i];

        //if we didn't find anything, make a new one
        GameObject newObject = Instantiate(prefab.gameObject, transform);
        newObject.SetActive(false);
        pooledObjects.Add(newObject.GetComponent<T>());
        ++amount;

        return newObject.GetComponent<T>();
    }

    //return an object back to the pool
    public void ReturnObjectToPool(T toBeReturned)
    {
        //verify the argrument
        if (toBeReturned == null)
            return;

        //make sure that the pool is ready, if not, make it ready
        if (!isReady)
        {
            PoolObjects();
            pooledObjects.Add(toBeReturned);
        }

        //deactivate the game object
        toBeReturned.gameObject.SetActive(false);
    }
}
