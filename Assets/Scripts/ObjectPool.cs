using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public GameObject prefab; // Prefab that will be instantiated in the pool
    public int poolSize = 10; // Initial size of the pool

    private Queue<GameObject> pool;

    private void Awake()
    {
        // Initialize the pool
        pool = new Queue<GameObject>();

        for (int i = 0; i < poolSize; i++)
        {
            CreateNewObject(); // Create objects for the pool
        }
    }

    // Create a new object and add it to the pool
    private void CreateNewObject()
    {
        GameObject obj = Instantiate(prefab, transform); // Set this GameObject as the parent
        obj.SetActive(false);
        pool.Enqueue(obj);
    }

    // Get an object from the pool
    public GameObject GetObject()
    {
        if (pool.Count > 0)
        {
            GameObject obj = pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        else
        {
            // If the pool is empty, instantiate a new object
            GameObject obj = Instantiate(prefab, transform); // Set this GameObject as the parent
            return obj;
        }
    }

    // Return an object to the pool
    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.SetParent(transform); // Ensure the object returns to the same parent
        pool.Enqueue(obj);
    }
}
