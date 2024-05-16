using System.Collections.Generic;
using System.Linq;

namespace BehaviourTree
{
	public class Random : Node {  
		private readonly Dictionary<Node, int> weightDict;  
	
		public Random(Dictionary<Node, int> childrenWithWeights) : base(childrenWithWeights.Keys.ToArray()) {  
			weightDict = childrenWithWeights;  
		}  
	
		bool running = false;    
		int runningIndex = 0;  
	
		protected override NodeState Evaluate()  
		{  
			NodeState childReturnValue;  
			if (running) {  
				childReturnValue = weightDict.ElementAt(runningIndex).Key.PerformNode();  
				if (childReturnValue != NodeState.RUNNING) {  
					running = false;  
				}  
				return childReturnValue;  
			}  
      
			int rand = UnityEngine.Random.Range(0, weightDict.Values.Sum());  
			int index = 0;  
			foreach (KeyValuePair<Node, int> entry in weightDict)  
			{  
				rand -= entry.Value;  
				if (rand < 0) {  
					break;  
				}  
				index++;  
			}  
  
			childReturnValue = weightDict.ElementAt(index).Key.PerformNode();  
			if (childReturnValue == NodeState.RUNNING) {
				runningIndex = index;  
				running = true;  
			}  
			return childReturnValue;  
		}  
	}

	public class Fallback : Node {  
		public Fallback(params Node[] children) : base(children) { }  
    
		bool running = false;    
		int runningIndex = 0;   
	
		protected override NodeState Evaluate() {    
			for (int i = running ? runningIndex : 0; i < children.Count; i++) {    
				running = false;    
				switch (children[i].PerformNode())  
				{  
					case NodeState.SUCCESS:  
						return NodeState.SUCCESS;  
					case NodeState.FAILURE:  
						continue;  
					case NodeState.RUNNING:  
						running = true;    
						runningIndex = i;  
						return NodeState.RUNNING;  
					default:  
						continue;  
				}   
			}    
			return NodeState.SUCCESS;    
		}    
	}

	public class Sequence : Node {  
		public Sequence(params Node[] children) : base(children) { }  
	
		bool running = false;  
		int runningIndex = 0;  
	
		protected override NodeState Evaluate() {  
			for (int i = running ? runningIndex : 0; i < children.Count; i++) {  
				running = false;  
				switch (children[i].PerformNode()) {  
					case NodeState.SUCCESS:  
						continue;  
					case NodeState.FAILURE:  
						return NodeState.FAILURE;  
					case NodeState.RUNNING:  
						running = true;  
						runningIndex = i;  
						return NodeState.RUNNING;  
					default:  
						return NodeState.SUCCESS;  
				}  
			}  
			return NodeState.SUCCESS;  
		}  
	}



	public class Invert : Node
	{
		/// <returns>The opposite <see cref="NodeState"/> from the one sent in (does not affect <see cref="NodeState.RUNNING"/>)</returns>
		public Invert(Node child) : base(child) { }

		protected override NodeState Evaluate() => Opposite(children[0].PerformNode());

		NodeState Opposite(NodeState nodeState)
		{
			switch (nodeState)
			{
				case NodeState.RUNNING:
					return NodeState.RUNNING;
				case NodeState.SUCCESS:
					return NodeState.FAILURE;
				case NodeState.FAILURE:
					return NodeState.SUCCESS;
				default:
					return NodeState.SUCCESS;
			}
		}
	}

	public class Parallel : Node
	{
		NodeState[] childrenReturnValues;

		/// <summary>
		/// Executes all <paramref name="children"/> simultaneously and stops when all have finished.
		/// </summary>
		public Parallel(params Node[] children) : base(children)
		{
			childrenReturnValues = new NodeState[children.Length];
		}

		protected override NodeState Evaluate()
		{
			for (int i = 0; i < children.Count; i++)
				if (childrenReturnValues[i] == NodeState.RUNNING)
					childrenReturnValues[i] = children[i].PerformNode();

			if (childrenReturnValues.Contains(NodeState.FAILURE))
			{
				childrenReturnValues.ResetArray();
				return NodeState.FAILURE;
			}
			else if (childrenReturnValues.Contains(NodeState.RUNNING))
			{
				return NodeState.RUNNING;
			}
			else
			{
				childrenReturnValues.ResetArray();
				return NodeState.SUCCESS;
			}
		}
	}

	public class ParallelHierarchy : Node
	{
		NodeState[] childrenReturnValues;

		/// <summary>
		/// Executes all <paramref name="children"/> simultaneously and stops executing the subordinate <paramref name="children"/> if a member finishes.
		/// </summary>
		/// <param name="children">The children's hierarchy ranks are determined by their positions.</param>
		public ParallelHierarchy(params Node[] children) : base(children)
		{
			childrenReturnValues = new NodeState[children.Length];
		}

		protected override NodeState Evaluate()
		{
			for (int i = 0; i < children.Count; i++)
			{
				if (childrenReturnValues[i] == NodeState.RUNNING)
					childrenReturnValues[i] = children[i].PerformNode();
				else
					for (int j = i; j < childrenReturnValues.Length; j++)
						childrenReturnValues[j] = NodeState.SUCCESS;

			}

			if (childrenReturnValues.Contains(NodeState.FAILURE))
			{
				childrenReturnValues.ResetArray();
				return NodeState.FAILURE;
			}
			else if (childrenReturnValues.Contains(NodeState.RUNNING))
			{
				return NodeState.RUNNING;
			}
			else
			{
				childrenReturnValues.ResetArray();
				return NodeState.SUCCESS;
			}
		}
	}

	public class SubTrees : Node
	{
		/// <summary>
		/// All children are performed every time with no regard to their NodeState
		/// </summary>
		public SubTrees(params Node[] children) : base(children) { }

		protected override NodeState Evaluate()
		{
			foreach (Node tree in children)
				tree.PerformNode();
			return NodeState.SUCCESS;
		}
	}
}

public static class ArrayTools
{
	/// <summary>
	/// Every element of the array gets set to it's default value
	/// </summary>
	public static void ResetArray<T>(this T[] array)
	{
		for (int i = 0; i < array.Length; i++)
			array[i] = default(T);
	}
}
