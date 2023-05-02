using UnityEditor;
using UnityEngine;

public enum ExampleEnum
{
    ValueFirst = 1,
    ValueSecond = 2,
    ValueThird = 3,
}

public class EnumExample : ScriptableObject
{
    public ExampleEnum MyEnum = ExampleEnum.ValueSecond;

    [MenuItem("Example/SerializedProperty Enum API")]
    static void Example()
    {
        EnumExample obj = ScriptableObject.CreateInstance<EnumExample>();
        SerializedObject serializedObject = new SerializedObject(obj);
        SerializedProperty enumProperty = serializedObject.FindProperty("MyEnum");

        enumProperty.enumNames[enumProperty.enumValueIndex] = "NEW NAME";
        enumProperty.       
        serializedObject.ApplyModifiedProperties();
        //MyEnum value: 2
        //Name of current value: ValueSecond
        //DisplayName: Value Second
        Debug.Log("MyEnum value: " + enumProperty.intValue +
            "\nName of current value: " + enumProperty.enumNames[enumProperty.enumValueIndex] +
            "\nDisplayName: " + enumProperty.enumDisplayNames[enumProperty.enumValueIndex]);
    }
}