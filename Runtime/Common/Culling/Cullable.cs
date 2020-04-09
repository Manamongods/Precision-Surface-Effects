using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Culling
{
    public abstract class Cullable : MonoBehaviour
    {
        //Fields
        public float importance = 1;
        public CullType cullType;


        //Methods
        public abstract void SetCullEnabled(bool active);


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