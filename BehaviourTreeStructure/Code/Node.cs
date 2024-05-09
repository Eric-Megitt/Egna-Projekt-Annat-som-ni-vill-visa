using System.Collections.Generic;

namespace BehaviourTree
{ 
	public enum NodeState { RUNNING, SUCCESS, FAILURE }

	public enum Result { NORMAL, INVERTED }

	public class Node
	{
		protected List<Node> children;

		private readonly Result result;


		#region Constructors
		internal Node(Result result = Result.NORMAL) => this.result = result;

		internal Node(params Node[] children)
		{
			this.children = new();
			foreach (Node child in children)
				this.children.Add(child);
		}
		#endregion Constructors

		protected virtual NodeState Evaluate() => NodeState.FAILURE; //gets overriden

		public NodeState PerformNode() => Evaluate();

		private static NodeState Invert(NodeState state) => (state == NodeState.RUNNING) ? state : (state == NodeState.SUCCESS) ? NodeState.FAILURE : NodeState.SUCCESS;
	}
}