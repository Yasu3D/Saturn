using System.Collections.Generic;
using UnityEngine;

// Big thanks to AllPoland for pointing me towards ArcViewer's implementation.
// This is nearly identical, as it's equally fitting for Saturn.
// For the original code, see
// https://github.com/AllPoland/ArcViewer/blob/main/Assets/__Scripts/Previewer/ObjectPools/ObjectPool.cs

namespace SaturnGame.RhythmGame
{
    public abstract class ObjectPool : MonoBehaviour
    {
        public List<GameObject> AvailableObjects = new();
        public List<GameObject> ActiveObjects = new();

        public int PoolSize { get; private set; }
        [SerializeField] private GameObject prefab;
        [SerializeField] private int startSize;
        [SerializeField] private Vector3 objectStartPosition = new Vector3 (0, 0, -6);

        public void SetPoolSize(int size)
        {
            PoolSize = size;
            MatchPoolSize();
        }

        private void MatchPoolSize()
        {
            int trueSize = AvailableObjects.Count + ActiveObjects.Count;
            int difference = trueSize - PoolSize;

            if (trueSize > PoolSize)
            {
                int deletedObjects = 0;
                for (int i = AvailableObjects.Count -1; i >= 0; i--)
                {
                    if (deletedObjects >= difference) break;

                    Destroy(AvailableObjects[i].gameObject);
                    AvailableObjects.RemoveAt(i);
                    deletedObjects++;
                }
            }
            else
            {
                for (int i = 0; i < Mathf.Abs(difference); i++)
                {
                    GameObject newObject = CreateNewObject();
                    AvailableObjects.Add(newObject);
                }
            }
        }

        private GameObject CreateNewObject()
        {
            GameObject newObject = Instantiate(prefab);
            newObject.transform.SetParent(transform);
            newObject.transform.position = objectStartPosition;
            newObject.transform.localScale = Vector3.zero;
            newObject.SetActive(false);

            return newObject;
        }

        public GameObject GetObject()
        {
            if (AvailableObjects.Count > 0)
            {
                GameObject collectedObject = AvailableObjects[0];

                AvailableObjects.RemoveAt(0);
                ActiveObjects.Add(collectedObject);

                return collectedObject;
            }

            GameObject newObject = CreateNewObject();

            ActiveObjects.Add(newObject);
            PoolSize++;

            return newObject;
        }

        public void ReleaseObject(GameObject target)
        {
            if (!ActiveObjects.Contains(target))
            {
                Destroy(target);
                return;
            }
            
            target.transform.SetParent(transform);
            target.transform.position = objectStartPosition;
            target.transform.localScale = Vector3.zero;
            target.SetActive(false);

            ActiveObjects.Remove(target);
            AvailableObjects.Add(target);
        }

        void Update()
        {
            int trueSize = AvailableObjects.Count + ActiveObjects.Count;

            if (trueSize != PoolSize)
                MatchPoolSize();
        }

        void Start()
        {
            SetPoolSize(startSize);
        }
    }
}