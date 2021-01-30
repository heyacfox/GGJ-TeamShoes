using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

public static class Utilities
{
    public static readonly Vector3 FlipX = new Vector3(-1, 1, 1);

    public static int[] IntArr0 = new int[0];
    public static float[] FloatArr0 = new float[0];
    public static byte[] ByteArr0 = new byte[0];
    public static string[] StringArr0 = new string[0];

    #region Replacement SmoothStep

    //https://en.wikipedia.org/wiki/Smoothstep


    public static float SmoothStep(float edge0, float edge1, float x)
    {
        // Scale x to 0..1 range
        x = (x - edge0) / (edge1 - edge0);
        // Evaluate polynomial
        return x * x * (3 - 2 * x);
    }

    public static float SmootherStep(float edge0, float edge1, float x)
    {
        // Scale x to 0..1 range
        x = (x - edge0) / (edge1 - edge0);
        // Evaluate polynomial
        return x * x * x * (x * (x * 6 - 15) + 10);
    }

    #endregion


    #region GameObject instance helpers

    public static void DestroyObject(MonoBehaviour scriptOnGob) { GameObject.Destroy(scriptOnGob.gameObject); }
    public static void DestroyObject(Transform tformOnGob) { GameObject.Destroy(tformOnGob.gameObject); }

    public static T InstantiateObject<T>(T scriptOnObject) where T : MonoBehaviour
    {
        return (GameObject.Instantiate(scriptOnObject.gameObject) as GameObject).GetComponent<T>();
    }

    /// <summary>Used for scripts where only one instance should exist, if this object isn't the ref'd singleton we destroy it.</summary>
    public static bool IsSingletonInstance<T>(this T obj, ref T singletonInst) where T : MonoBehaviour
    {
        if (singletonInst != null && obj != singletonInst)
        {
            UnityEngine.Object.Destroy(obj.gameObject);
            return false;
        }
        singletonInst = obj;
        return true;
    }
    #endregion

    #region Transform hierarchy traversal and manipulation

    public static Transform GetRootParent(this Transform t)
    {
        while (t.parent != null) t = t.parent;
        return t;
    }

    public static Transform FindChildWithName(this Transform t, string name)
    {
        foreach (Transform tr in t)
            if (tr.name == name) return tr;
        return null;
    }

    public static List<Transform> GetLeafChildren(this Transform t) { return t.GetLeafChildren(new List<Transform>()); }
    public static List<Transform> GetLeafChildren(this Transform t, List<Transform> list)
    {
        bool child = false;
        foreach (Transform c in t)
        {
            child = true;
            c.GetLeafChildren(list);
        }
        if (!child) list.Add(t);
        return list;
    }

    #endregion

    #region Generic collection helpers (Print, Random, ToStringKeyedDict)

    public static string Print<T>(this IEnumerable<T> list) { return list.Print(", "); }
    public static string Print<T>(this IEnumerable<T> list, string separator)
    {
        sb.Length = 0;
        foreach (T itm in list)
            sb.Append(itm).Append(separator);
        sb.Length = Mathf.Max(0, sb.Length - (separator != null ? separator.Length : 0));
        return sb.ToString();
    }
    static System.Text.StringBuilder sb = new System.Text.StringBuilder();


    public static T Random<T>(this List<T> list)
    {
        return list[UnityEngine.Random.Range(0, list.Count)];
    }
    public static T Random<T>(this T[] arr)
    {
        return arr[UnityEngine.Random.Range(0, arr.Length)];
    }

    public static Dictionary<string, object> ToStringKeyedDict(this Dictionary<object, object> dict, bool recursive)
    {
        Dictionary<string, object> ret = new Dictionary<string, object>();
        foreach (var kvp in dict)
        {
            if (recursive && kvp.Value as Dictionary<object, object> != null)
                ret[kvp.Key.ToString()] = (kvp.Value as Dictionary<object, object>).ToStringKeyedDict(recursive);
            else ret[kvp.Key.ToString()] = kvp.Value;
        }
        return ret;
    }
    #endregion

    #region MD5 Encoding
    public static string MD5(string input)
    {
        //Convert to bytes
        byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(input);

        // encrypt bytes with overload method
        byte[] hashBytes = MD5(bytes);

        // Convert the encrypted bytes back to a string (base 16)
        string hashString = "";
        for (int i = 0; i < hashBytes.Length; i++)
            hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
        return hashString.PadLeft(32, '0');
    }

    public static byte[] MD5(byte[] input)
    {
        System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
        byte[] hashBytes = md5.ComputeHash(input);
        return hashBytes;
    }
    #endregion

    /// <summary>
    /// Removes newlines from a string, where a newline is replaced with a single space only when separating 2 non-whitespace chars.
    /// </summary>
    public static StringBuilder StripNewlines(this StringBuilder sb)
    {
        sb.Replace("\r\n", "\n").Replace("\r", "\n");

        for (int i = 0; i < sb.Length; i++)
            if (sb[i] != '\n') continue;
            else
            {
                if (i > 0 && i < sb.Length - 1 && !char.IsWhiteSpace(sb[i - 1]) && !char.IsWhiteSpace(sb[i + 1]))
                    sb[i] = ' '; //only replace a newline with a space when the newline is the ONLY char separating 2 non-whitespace chars
                else
                    sb.Remove(i, 1);
            }

        return sb;
    }


    //TODO: use this newfangled method to make your game FUCKING FOOLPROOF!!!!!!!
    public static System.Net.IPAddress GetDefaultGateway()
    {
        var card = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
            .Where(e => e.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up).FirstOrDefault();
        if (card == null) return null;
        var address = card.GetIPProperties().GatewayAddresses.FirstOrDefault();
        return address.Address;
    }


    /// <summary>An object that can be passed around and throw an exception if other scripts fail to assign the data.</summary>
    public class CheckedResultAssignment<T>
    {
        public CheckedResultAssignment() { ResultAssigned = false; }

        public bool ResultAssigned { get; private set; }

        public T Result
        {
            get { if (ResultAssigned) return res; else throw new UnityException("SaveOpResult.Success hasn't been set by implementor."); }
            set { res = value; ResultAssigned = true; }
        }

        public void Reset() { ResultAssigned = false; res = default(T); }

        public override string ToString() { return base.ToString() + "(assigned=" + ResultAssigned + ", result=" + res + ")"; }

        T res;
    }
}
