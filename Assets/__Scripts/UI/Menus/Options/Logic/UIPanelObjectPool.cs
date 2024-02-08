using System.Collections.Generic;
using UnityEngine;

// Big thanks to AllPoland for pointing me towards ArcViewer's implementation.
// This is nearly identical, as it's equally fitting for Saturn.
// For the original code, see
// https://github.com/AllPoland/ArcViewer/blob/main/Assets/__Scripts/Previewer/ObjectPools/MonobehaviorPool.cs

namespace SaturnGame.UI
{
    public class UIPanelObjectPool : MonoBehaviour
    {
        public List<OptionPanel> AvailableObjects = new();
        public List<OptionPanel> ActiveObjects = new();

        public int PoolSize { get; private set; }
        [SerializeField] private OptionPanel prefab;
        [SerializeField] private int startSize;

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

                    var test = AvailableObjects[i];

                    Destroy(test.gameObject);
                    AvailableObjects.RemoveAt(i);
                    deletedObjects++;
                }
            }
            else
            {
                for (int i = 0; i < Mathf.Abs(difference); i++)
                {
                    OptionPanel newObject = CreateNewObject();
                    AvailableObjects.Add(newObject);
                }
            }
        }

        private OptionPanel CreateNewObject()
        {
            OptionPanel newObject = Instantiate(prefab);
            newObject.rect.SetParent(transform);
            newObject.gameObject.SetActive(false);

            return newObject;
        }

        public OptionPanel GetObject()
        {
            if (AvailableObjects.Count > 0)
            {
                OptionPanel collectedObject = AvailableObjects[0];

                AvailableObjects.RemoveAt(0);
                ActiveObjects.Add(collectedObject);

                return collectedObject;
            }

            OptionPanel newObject = CreateNewObject();

            ActiveObjects.Add(newObject);
            PoolSize++;

            return newObject;
        }

        public void ReleaseObject(OptionPanel target)
        {
            if (!ActiveObjects.Contains(target))
            {
                Destroy(target.gameObject);
                return;
            }
            
            target.rect.SetParent(transform);
            target.gameObject.SetActive(false);

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