using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Culling
{
    public abstract class CullType : ScriptableObject
    {
        //Fields
        public Type type = Type.HashSet;
        public int listCountPerInterval = 1000;

        private readonly HashSet<Cullable> hashSet = new HashSet<Cullable>();
        private readonly List<Cullable> list = new List<Cullable>();
        private int listIndex;



        //Methods
        public abstract bool EnabledAt(Cullable c, Vector3 position);

        public void Add(Cullable cullable)
        {
            if (type == Type.HashSet)
                hashSet.Add(cullable);
            else
                list.Add(cullable);
        }
        public void Remove(Cullable cullable)
        {
            if (type == Type.HashSet)
                hashSet.Remove(cullable);
            else
                list.Remove(cullable);
        }

        private void Cull(Cullable c)
        {
            var pos = c.transform.position;

            c.SetCullEnabled(EnabledAt(c, pos));
        }
        public void Cull()
        {
            if (type == Type.HashSet)
            {
                foreach (var cullable in hashSet)
                {
                    Cull(cullable);
                }
            }
            else
            {
                var listCount = list.Count;
                int c = Mathf.Min(listCountPerInterval, listCount);
                for (int i = 0; i < c; i++)
                {
                    Cull(list[listIndex]);

                    listIndex++;
                    if (listIndex >= listCount)
                        listIndex -= listCount;
                }
            }
        }



        //Datatypes
        public enum Type { HashSet, List }
    }
}