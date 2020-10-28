/**
*   Authored by Tomasz Piowczyk
*   MIT LICENSE: https://github.com/Prastiwar/UnitySerializedDictionary/blob/master/LICENSE
*   Repository: https://github.com/Prastiwar/UnitySerializedDictionary
*/

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class UDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [SerializeField] private List<TKey> m_keys;
    [SerializeField] private List<TValue> m_values;

    public UDictionary() : base() { }
    public UDictionary(int capacity) : base(capacity) { }
    public UDictionary(IEqualityComparer<TKey> comparer) : base(0, comparer) { }
    public UDictionary(int capacity, IEqualityComparer<TKey> comparer) : base(capacity, comparer) { }
    public UDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary, null) { }
    public UDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) : base(dictionary != null ? dictionary.Count : 0, comparer) { }

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
        if (m_keys == null)
        { 
            m_keys = Keys.ToList();
            m_values = Values.ToList();
        }
    }

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
        if (Count == 0 && m_keys != null && m_values != null)
        {
            int length = m_keys.Count;

            Clear();
            for (int i = 0; i < length; i++)
            {
                if(m_keys[i] != null && m_values[i] != null)
                {
                    this[m_keys[i]] = m_values[i];
                }
            }

            m_keys = null;
            m_values = null;
        }
    }
}