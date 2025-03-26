using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class ParticleManager : MonoBehaviour
{

    // enum���� ���� SoundManager�� ��

    public enum ParticleType
    {
        DamageExplosion,
        WeaponFire,
        WeaponSmoke,


    }
    public static ParticleManager Instance { get; private set; }

    private Dictionary<ParticleType, ParticleSystem> particleSystemDic = new Dictionary<ParticleType, ParticleSystem>();
    private Dictionary<ParticleType, Queue<GameObject>> particlePools = new Dictionary<ParticleType, Queue<GameObject>>();

    public ParticleSystem WeaponexplosionParticle;
    public ParticleSystem WeaponFireParticle;
    public ParticleSystem WeaponSmokeParticle;

    // Ǯ ������ ����
    public int poosize = 30;

    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        else
        {
            Destroy(gameObject);
        }

        // ���߿� �Լ��� ���� ����
        particleSystemDic.Add(ParticleType.DamageExplosion,WeaponexplosionParticle);
        particleSystemDic.Add(ParticleType.WeaponFire, WeaponFireParticle);
        particleSystemDic.Add(ParticleType.WeaponSmoke, WeaponSmokeParticle);

        // Ǯ���ý���

        foreach(var type in particleSystemDic.Keys)
        {
            Queue<GameObject> pool = new Queue<GameObject>();
            for (int i = 0; i < poosize; i++)
            {

                GameObject obj = Instantiate(particleSystemDic[type].gameObject);
                obj.SetActive(false);
                pool.Enqueue(obj);

            }

            // �ڵ� �߰�
        
        }
    }

    public void ParticlePlay(ParticleType type , Vector3 position, Vector3 scale)
    {

        //ParticleSystem particle = Instantiate(particleSystemDic[type], position, Quaternion.identity);
        //particle.gameObject.transform.localScale = scale;
        //Transform playerTransform = PlayerManager_Sample.Instance.transform;
        //Vector3 directionToPlayer = playerTransform.position - position;
        //Quaternion rotation = Quaternion.LookRotation(directionToPlayer);
        //particle.Play();
        //Destroy(particle.gameObject, particle.main.duration); // ��ƼŬ�� ������Ŀ� �����ϱ�

        if (particlePools.ContainsKey(type))
        {
            GameObject particleObj = particlePools[type].Dequeue();

            if(particleObj != null)
            {
                particleObj.transform.position = position;
                ParticleSystem particleSystem = particleObj.GetComponentInChildren<ParticleSystem>();

                if (particleSystem.isPlaying)
                {
                    particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }

                particleObj.transform.localScale = scale;
                particleObj.SetActive(true);
                particleSystem.Play();
                StartCoroutine(particleEnd(type, particleObj, particleSystem));


            }
        }
    }

    IEnumerator particleEnd(ParticleType type, GameObject particleObj, ParticleSystem particleSystem)
    {
        while (particleSystem.isPlaying)
        {
            yield return null;
        }

        particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        particleObj.SetActive(false);
        particlePools[type].Enqueue(particleObj);
    }

}
