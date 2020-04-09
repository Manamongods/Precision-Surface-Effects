using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Culling
{
    public class ScriptCullable : Cullable
    {
        //Fields
        public MonoBehaviour[] disableScripts;


        //Methods
        public override void SetCullEnabled(bool active)
        {
            for (int i = 0; i < disableScripts.Length; i++)
            {
                var ds = disableScripts[i];
                if (ds.enabled != active)
                    ds.enabled = active;
            }
        }
    }
}