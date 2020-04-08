using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Culling
{
    public class Cullable : MonoBehaviour
    {
        //Fields
        public float importance = 1;
        public CullType cullType;
        public MonoBehaviour[] disableScripts;


        //Methods
        public virtual void SetCullEnabled(bool active)
        {
            for (int i = 0; i < disableScripts.Length; i++)
            {
                var ds = disableScripts[i];
                if (ds.enabled != active)
                    ds.enabled = active;
            }
        }


        //Lifecycle
        protected virtual void OnEnable()
        {
            cullType.Add(this);
        }
        protected virtual void OnDisable()
        {
            cullType.Remove(this);
        }
    }
}

/*
 * 
        [System.NonSerialized]
        public float invImportance;
        #if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            Refresh();
        }

    
        public virtual void Refresh()
        {
            invImportance = 1 / importance;
        }
#endif
 */