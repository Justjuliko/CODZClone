using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public GameObject prefab; // Prefab que será instanciado en el pool
    public int poolSize = 10; // Tamaño inicial del pool

    private Queue<GameObject> pool;

    private void Awake()
    {
        // Inicializar el pool
        pool = new Queue<GameObject>();

        for (int i = 0; i < poolSize; i++)
        {
            CreateNewObject();
        }
    }

    // Crear un nuevo objeto y añadirlo al pool
    private void CreateNewObject()
    {
        GameObject obj = Instantiate(prefab, transform); // Establecer este GameObject como padre
        obj.SetActive(false);
        pool.Enqueue(obj);
    }

    // Obtener un objeto del pool
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
            // Si el pool está vacío, instanciar un nuevo objeto
            GameObject obj = Instantiate(prefab, transform); // Establecer este GameObject como padre
            return obj;
        }
    }

    // Devolver un objeto al pool
    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.SetParent(transform); // Asegurar que el objeto vuelve al mismo padre
        pool.Enqueue(obj);
    }
}
