using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace BehaviourTree
{
	public abstract class Tree : MonoBehaviour
	{
		#region NonStorage
		private Node _root = null;

		protected virtual void Start() => _root = SetupTree();

		private void Update() => _root?.PerformNode();

		protected abstract Node SetupTree();
		#endregion NonStorage

		#region Storage

		private static List<Dictionary<string, object>> _dataContext = new();

		public static void SetData(string key, object value, int storageNumber)
		{
			while (_dataContext.Count - 1 < storageNumber) 
				_dataContext.Add(new Dictionary<string, object>());
			if (_dataContext[storageNumber].TryGetValue(key, out object _))
			{
				_dataContext[storageNumber][key] = value;
			}
			else
			{
				_dataContext[storageNumber].Add(key, value);
			}
		}

		public static object GetData(string key, int storageNumber = 0, object defaultValue = null)
		{
			try
            {
                return _dataContext[storageNumber][key];
            }
			catch(Exception)
			{
				return defaultValue;
			}
		}

		public static bool TryClearData(string key, int storageNumber)
		{
			if (!_dataContext[storageNumber].ContainsKey(key))
				return false;

			_dataContext[storageNumber].Remove(key);
			return true;
		}

		#endregion Storage
	

	}
}