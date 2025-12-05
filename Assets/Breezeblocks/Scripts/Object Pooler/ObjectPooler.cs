using System.Collections.Generic;
using UnityEngine;

namespace ObjectPool
{
    public class ObjectPooler : MonoBehaviour
    {
        #region Singleton
        //-- Object Pooler variables
        public static ObjectPooler instance;
        #endregion

        #region Variables and Properties
        // Dictionary
        private Dictionary<string, Queue<GameObject>> poolDictionary;

        // Objects
        [System.Serializable]
        public class Pool
        {
            public string tag;
            public GameObject prefab;
            public int size;
        }

        [SerializeField]
        private List<Pool> m_pools = null;
        public List<Pool> Pools { get { return m_pools; } set { m_pools = value; } }


        #endregion

        public void Awake()
        {
            poolDictionary = new Dictionary<string, Queue<GameObject>>();

            instance = this;

            // Spawn objects
            foreach (Pool _pool in Pools)
            {
                Queue<GameObject> objectPool = new Queue<GameObject>();

                for (int i = 0; i < _pool.size; i++)
                {
                    GameObject obj = Instantiate(_pool.prefab);

                    obj.SetActive(false);
                    objectPool.Enqueue(obj);
                }

                poolDictionary.Add(_pool.tag, objectPool);
            }
        }

        /// <summary>
        /// Spawn Prefab with Tag at new Position and new Rotation.
        /// </summary>
        /// <param name="Tag"></param>
        /// <param name="Position"></param>
        /// <param name="Rotation"></param>
        /// <returns></returns>
        public GameObject SpawnFromPool(string Tag, Vector3 Position, Quaternion Rotation)
        {
            if (!poolDictionary.ContainsKey(Tag))
            {
                Debug.LogWarning("Pool with name: \"" + Tag + "\" doesn't exist.");
                return null;
            }

            GameObject objectToSpawn = poolDictionary[Tag].Dequeue();

            objectToSpawn.SetActive(true);
            objectToSpawn.transform.position = Position;
            objectToSpawn.transform.rotation = Rotation;

            // Call object on spawn method
            IPooledObjects[] _pooledObj = objectToSpawn.GetComponents<IPooledObjects>();

            if (_pooledObj != null)
            {
                foreach (IPooledObjects pooledObj in _pooledObj)
                {
                    pooledObj.OnObjectSpawn();
                }
            }

            poolDictionary[Tag].Enqueue(objectToSpawn);

            return objectToSpawn;
        }
    }
}