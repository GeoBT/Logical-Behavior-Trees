using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
#endif

[BlackboardElementType(typeof(BoolBlackboardElement))]
public class BlackboardConditionalBool : IBlackboardConditionalElement
{
    [Serializable]
    public enum BoolComparator
    {
        Equals,
        Does_Not_Equal
    }
    [SerializeField]
    private BoolComparator m_boolComparator = BoolComparator.Equals;
    [SerializeField]
    private bool m_comparedValue = false;

    public bool Evaluate()
    {
        return true;
    }
}
