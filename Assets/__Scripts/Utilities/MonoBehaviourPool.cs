using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

// Big thanks to AllPoland for pointing me towards ArcViewer's implementation.
// This is nearly identical, as it's equally fitting for Saturn.
// For the original code, see
// https://github.com/AllPoland/ArcViewer/blob/main/Assets/__Scripts/Previewer/ObjectPools/MonobehaviorPool.cs

namespace SaturnGame
{
    public abstract class MonoBehaviourPool<T> : MonoBehaviour where T : MonoBehaviour
    {
        public List<T> AvailableObjects = new();
        public List<T> ActiveObjects = new();

        private int PoolSize { get; set; }
        [SerializeField] private T prefab;
        [SerializeField] private int startSize;
        [SerializeField] private Vector3 objectStartPosition = new(0, 0, -6);

        private void SetPoolSize(int size)
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
                    T newObject = CreateNewObject();
                    AvailableObjects.Add(newObject);
                }
            }
        }

        [NotNull]
        private T CreateNewObject()
        {
            T newObject = Instantiate(prefab, transform, true);
            Transform newTransform = newObject.transform;
            newTransform.position = objectStartPosition;
            newTransform.localScale = Vector3.zero;
            newObject.gameObject.SetActive(false);

            return newObject;
        }

        public T GetObject()
        {
            if (AvailableObjects.Count > 0)
            {
                T collectedObject = AvailableObjects[0];

                AvailableObjects.RemoveAt(0);
                ActiveObjects.Add(collectedObject);

                return collectedObject;
            }

            T newObject = CreateNewObject();

            ActiveObjects.Add(newObject);
            PoolSize++;

            return newObject;
        }

        public void ReleaseObject([NotNull] T target)
        {
            if (!ActiveObjects.Contains(target))
            {
                Destroy(target.gameObject);
                return;
            }

            Transform targetTransform = target.transform;
            targetTransform.SetParent(transform);
            targetTransform.position = objectStartPosition;
            targetTransform.localScale = Vector3.zero;
            target.gameObject.SetActive(false);

            ActiveObjects.Remove(target);
            AvailableObjects.Add(target);
        }

        private void Update()
        {
            int trueSize = AvailableObjects.Count + ActiveObjects.Count;

            if (trueSize != PoolSize)
                MatchPoolSize();
        }

        private void Start()
        {
            SetPoolSize(startSize);
        }
    }
}