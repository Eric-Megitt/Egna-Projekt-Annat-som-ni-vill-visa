using UnityEngine;

namespace BehaviourTree
{
	public abstract class Tree : MonoBehaviour
	{
		private Node _root = null;

		protected virtual void Start() => _root = SetupTree();

		private void Update() => _root?.PerformNode();

		protected abstract Node SetupTree();
	}
}
