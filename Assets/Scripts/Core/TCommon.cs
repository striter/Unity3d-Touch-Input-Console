using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

public static class TCommon
{
    #region Transform
    public static bool SetActivate(this MonoBehaviour behaviour, bool active)=>SetActivate(behaviour.gameObject, active);
    public static bool SetActivate(this Transform tra, bool active)=> SetActivate(tra.gameObject, active);
    public static bool SetActivate(this GameObject go, bool active)
    {
        if (go.activeSelf == active)
            return false;

        go.SetActive(active);
        return true;
    }
    public static void DestroyChildren(this Transform trans)
    {
        int count = trans.childCount;
        if (count <= 0)
            return;
        for (int i = 0; i < count; i++)
            GameObject.Destroy(trans.GetChild(i).gameObject);
    }
    public static void SetParentResetTransform(this Transform source,Transform target)
    {
        source.SetParent(target);
        source.transform.localPosition = Vector3.zero;
        source.transform.localScale = Vector3.one;
        source.transform.localRotation = Quaternion.identity;
    }
    public static void SetChildLayer(this Transform trans, int layer)
    {
        foreach (Transform temp in trans.gameObject.GetComponentsInChildren<Transform>(true))
            temp.gameObject.layer = layer;
    }
    public static Transform FindInAllChild(this Transform trans, string name)
    {
        foreach (Transform temp in trans.gameObject.GetComponentsInChildren<Transform>(true))
            if (temp.name == name) return temp;
        Debug.LogWarning("Null Child Name:" + name + ",Find Of Parent:" + trans.name);
        return null;
    }

    public static T Find<T>(this T[,] array, Predicate<T> predicate)
    {
        int length0 = array.GetLength(0);
        int length1 = array.GetLength(1);
        for (int i = 0; i < length0; i++)
            for (int j = 0; j < length1; j++)
                if (predicate(array[i, j])) return array[i, j];
        return default(T);
    }


    public static void SortChildByNameIndex(Transform transform, bool higherUpper = true)
    {
        List<Transform> childList = new List<Transform>();
        List<int> childIndexList = new List<int>();

        for (int i = 0; i < transform.childCount; i++)
        {
            childList.Add(transform.GetChild(i));
            childIndexList.Add(int.Parse(childList[i].gameObject.name));
        }
        childIndexList.Sort((a, b) => { return a <= b ? (higherUpper ? 1 : -1) : (higherUpper ? -1 : 1); });

        for (int i = 0; i < childList.Count; i++)
        {
            childList[i].SetSiblingIndex(childIndexList.FindIndex(p => p == int.Parse(childList[i].name)));
        }
    }

    #endregion
    #region Collections/Array Traversal
    public static T GetIndexKey<T, Y>(this Dictionary<T, Y> dictionary, int index) => dictionary.ElementAt(index).Key;
    public static Y GetIndexValue<T, Y>(this Dictionary<T, Y> dictionary, int index) => dictionary.ElementAt(index).Value;

    public static List<T> DeepCopy<T>(this List<T> list) where T : struct
    {
        List<T> copyList = new List<T>();
        list.Traversal((T value) => { copyList.Add(value); });
        return copyList;
    }

    public static Dictionary<T,Y> DeepCopy<T,Y>(this Dictionary<T,Y> dictionary) where T:struct where Y: struct
    {
        Dictionary<T, Y> copyDic = new Dictionary<T, Y>();
        dictionary.Traversal((T key, Y value) => { copyDic.Add(key, value); });
        return copyDic;
    }

    public static Dictionary<T,List<Y>> DeepCopy<T,Y>(this Dictionary<T,List<Y>> dictionary) where T:struct where Y:struct
    {
        Dictionary<T, List<Y>> copyDic = new Dictionary<T, List<Y>>();
        dictionary.Traversal((T key, List<Y> value) => { copyDic.Add(key, value.DeepCopy()); });
        return copyDic;
    }

    public static void Traversal<T>(this List<T> list, Action<int, T> OnEachItem,bool shallowCopy=false)
    {
        List<T> tempList = shallowCopy ? new List<T>(list) : list;
        TraversalEnumerableIndex(0, list.Count, (int index) => { OnEachItem(index, tempList[index]); return false; });
    }
    public static void Traversal<T>(this List<T> list, Action<T> OnEachItem, bool shallowCopy = false)
    {
        List<T> tempList = shallowCopy ? new List<T>(list) : list;
        TraversalEnumerableIndex(0, list.Count, (int index) => { OnEachItem(tempList[index]); return false; });
    }
    public static void TraversalBreak<T>(this List<T> list, Func<T,bool> OnEachItem,bool shallowCopy=false)
    {
        List<T> tempList = shallowCopy ? new List<T>(list) : list;
        TraversalEnumerableIndex(0, list.Count, (int index) => {return OnEachItem(tempList[index]);});
    }
    public static void Traversal<T, Y>(this Dictionary<T, Y> dic, Action<T> OnEachKey,bool shallowCopy=false)
    {
        Dictionary<T, Y> tempDic = shallowCopy ? new Dictionary<T, Y>(dic) : dic;
        foreach (T temp in tempDic.Keys)
            OnEachKey(temp);
    }
    public static void Traversal<T, Y>(this Dictionary<T, Y> dic, Action<Y> OnEachValue,bool shallowCopy =false)
    {
        Dictionary<T, Y> tempDic = shallowCopy  ? new Dictionary<T, Y>(dic) : dic;
        foreach (T temp in tempDic.Keys)
            OnEachValue(tempDic[temp]);
    }
    public static void Traversal<T, Y>(this Dictionary<T, Y> dic, Action<T, Y> OnEachPair,bool shallowCopy =false)
    {
        Dictionary<T, Y> tempDic = shallowCopy  ? new Dictionary<T, Y>(dic) : dic;
        foreach (T temp in tempDic.Keys)
            OnEachPair(temp,tempDic[temp]);
    }
    public static void TraversalBreak<T, Y>(this Dictionary<T, Y> dic, Func<Y, bool> OnEachItem, bool shallowCopy = false)
    {
        Dictionary<T, Y> tempDic = shallowCopy ? new Dictionary<T, Y>(dic) : dic;
        foreach (T temp in tempDic.Keys)
            if (OnEachItem(tempDic[temp]))
                break;
    }
    public static void Traversal<T>(this T[] array, Action<T> OnEachItem)
    {
        int length = array.Length;
        for (int i = 0; i < length; i++)
            OnEachItem(array[i]);
    }
    public static void Traversal<T>(this T[] array, Action<int,T> OnEachItem)
    {
        int length = array.Length;
        for (int i = 0; i < length; i++)
            OnEachItem(i,array[i]);
    }
    public static void Traversal<T>(this T[,] array, Action<T> OnEachItem)
    {
        int length0 = array.GetLength(0);
        int length1 = array.GetLength(1);
        for (int i = 0; i < length0; i++)
            for (int j = 0; j < length1; j++)
                OnEachItem(array[i, j]);
    }
    public static void Traversal<T>(this T[,] array, Action<int,int, T> OnEachItem)
    {
        int length0 = array.GetLength(0);
        int length1 = array.GetLength(1);
        for (int i = 0; i < length0; i++)
            for (int j = 0; j < length1; j++)
                OnEachItem(i,j,array[i, j]);
    }


    static void TraversalEnumerableIndex(int index, int count,Func<int,bool> OnItemBreak )
    {
        if (count == 0)
            return;
        for (int i = 0; i < count; i++)
        {
            if (OnItemBreak != null && OnItemBreak(index))
                break;

            index++;
            if (index == count)
                index = 0;
        }
    }

    #region Enum

    public static void TraversalEnum<T>(Action<T> enumAction)    //Can't Constraint T to System.Enum?
    {
        if (!typeof(T).IsSubclassOf(typeof(Enum)))
        {
            Debug.LogError("Can't Traversal EnEnum Class!");
            return;
        }

        foreach (object temp in Enum.GetValues(typeof(T)))
        {
            if (temp.ToString() == "Invalid")
                continue;
            enumAction((T)temp);
        }
    }
    public static List<T> GetEnumList<T>()
    {
        if (!typeof(T).IsSubclassOf(typeof(Enum)))
        {
            Debug.LogError("Can't Traversal EnEnum Class!");
            return null;
        }

        List<T> list = new List<T>();
        Array allEnums = Enum.GetValues(typeof(T));
        for (int i = 0; i < allEnums.Length; i++)
        {
            if (allEnums.GetValue(i).ToString() == "Invalid")
                continue;
            list.Add((T)allEnums.GetValue(i));
        }
        return list;
    }
    #endregion
    #endregion

}