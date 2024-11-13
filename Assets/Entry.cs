using Darkness;
using UnityEngine;

public class Entry : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Game.AddSingleton<ObjectPools>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            Debug.Log($"ObjectPool:{ObjectPools.Instance}");
        }
    }
}