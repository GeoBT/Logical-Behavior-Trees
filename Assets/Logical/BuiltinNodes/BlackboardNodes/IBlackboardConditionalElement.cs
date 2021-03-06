#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Logical.BuiltInNodes
{
    public interface IBlackboardConditionalElement
    {
        bool Evaluate(BlackboardElement element);
#if UNITY_EDITOR
        string GetOutportLabel(SerializedProperty conditionalProp);
#endif
    }
}