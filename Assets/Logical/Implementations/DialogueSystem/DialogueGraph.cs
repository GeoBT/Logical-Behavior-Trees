﻿using Logical;
using System;
using UnityEngine;

namespace DialogueSystem
{
    public class DialogueGraph : NodeGraph
    {
        [GraphProperties(typeof(DialogueGraph))]
        public class DialogueGraphProperties : AGraphProperties
        {
            public bool hey;
            [SerializeField]
            private int heeee;
            public SerializedClass serializedclass;
        }

        [Serializable]
        public class SerializedClass
        {
            public bool yes;
            public string gah = " dsfsdfs d";
        }
    }
}