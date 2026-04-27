using UnityEngine;
using Zenject;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;

public class BulletPool : MonoBehaviour
{
    [Title("Settings")]
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField, Min(1)] private int initializeSize = 30;

    private Queue<Bullet> _pool = new Queue<Bullet>();

    private void Awake()
    {

        for(int i = 0; i < initializeSize; i++)
        {
            CreateBullet();
        }
    }

    public Bullet Get(Vector3 position, Quaternion rotation)
    {
        Bullet bullet = _pool.Count> 0 ?_pool.Dequeue() : CreateBullet();
        bullet.transform.SetPositionAndRotation(position, rotation);
        
        return bullet;
    }

    public void Return(Bullet bullet)
    {
        bullet.gameObject.SetActive(false);
        _pool.Enqueue(bullet);
    }

    private Bullet CreateBullet()
    {
        Bullet b = Instantiate(bulletPrefab, transform);
        b.gameObject.SetActive(false);
        return b;
    }
}
