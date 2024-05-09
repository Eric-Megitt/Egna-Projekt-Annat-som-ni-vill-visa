/*
	Eric Scott Megitt, SU22C LBS Gothenburg
	eric.megitt@elev.ga.lbs.se
	Version 0.2.1

	See ChangeLog at Bottom
*/


using ScottsEssentials;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BehaviourTree
{
	public class Random : Node
	{
		private readonly Dictionary<Node, int> weightDict;

		public Random(Dictionary<Node, int> children) : base(children.Keys.ToArray())
		{
			weightDict = children;
		}

		int lastIndex = -1;
		NodeState lastReturnValue = NodeState.SUCCESS;

		protected override NodeState Evaluate()
		{

			if (lastReturnValue == NodeState.RUNNING)
			{
				lastReturnValue = weightDict.ElementAt(lastIndex).Key.PerformNode();
			}
			else
			{
				int rand = UnityEngine.Random.Range(0, weightDict.Values.Sum());
				int index = 0;
				foreach (KeyValuePair<Node, int> entry in weightDict)
				{
					rand -= entry.Value;
					if (rand < 0)
						break;
					index++;
				}
				lastIndex = index;
				lastReturnValue = weightDict.ElementAt(lastIndex).Key.PerformNode();
			}
			return lastReturnValue;
		}
	}

    public class Fallback : Node
    {
        public Fallback(params Node[] children) : base(children) { }

        int? runningIndex = null;
        protected override NodeState Evaluate()
        {
            if (runningIndex != null)
            {
                switch (children[(int)runningIndex].PerformNode())
                {
                    case NodeState.FAILURE:
                        for (int i = ((int)runningIndex) + 1; i < children.Count; i++)
                        {
                            Node child = children[i];
                            switch (child.PerformNode())
                            {
                                case NodeState.FAILURE:
                                    continue;
                                case NodeState.SUCCESS:
                                    return NodeState.SUCCESS;
                                case NodeState.RUNNING:
                                    runningIndex = i;
                                    return NodeState.RUNNING;
                                default:
                                    continue;
                            }
                        }
                        runningIndex = null;
                        return NodeState.FAILURE;

                    case NodeState.SUCCESS:
                        runningIndex = null;
                        return NodeState.SUCCESS;
                    case NodeState.RUNNING:
                        return NodeState.RUNNING;
                }
            }
            else
            {
                for (int i = 0; i < children.Count; i++)
                {
                    Node child = children[i];
                    switch (child.PerformNode())
                    {
                        case NodeState.FAILURE:
                            continue;
                        case NodeState.SUCCESS:
                            return NodeState.SUCCESS;
                        case NodeState.RUNNING:
                            runningIndex = i;
                            return NodeState.RUNNING;
                        default:
                            continue;
                    }
                }
            }
            return NodeState.FAILURE;
        }
    }

    public class Sequence : Node
	{
		public Sequence(params Node[] children) : base(children) { }


		int? runningIndex = null;

		protected override NodeState Evaluate()
		{

			if (runningIndex != null)
			{
				switch (children[(int)runningIndex].PerformNode())
				{
					case NodeState.SUCCESS:
						for (int i = (int)runningIndex + 1; i < children.Count; i++)
						{
							Node child = children[i];
							switch (child.PerformNode())
							{
								case NodeState.FAILURE:
									return NodeState.FAILURE;
								case NodeState.SUCCESS:
									continue;
								case NodeState.RUNNING:
									runningIndex = i;
									return NodeState.RUNNING;
								default:
									return NodeState.SUCCESS;
							}
						}
						runningIndex = null;
						return NodeState.SUCCESS;

					case NodeState.FAILURE:
						runningIndex = null;
						return NodeState.FAILURE;
					case NodeState.RUNNING:
						return NodeState.RUNNING;

				}
			}
			else
			{
				for (int i = 0; i < children.Count; i++)
				{
					Node child = children[i];
					switch (child.PerformNode())
					{
						case NodeState.FAILURE:
							return NodeState.FAILURE;
						case NodeState.SUCCESS:
							continue;
						case NodeState.RUNNING:
							runningIndex = i;
							return NodeState.RUNNING;
						default:
							return NodeState.SUCCESS;
					}
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

/*

--Versions-- 

0.0.0:
	Composites:
		+ Random
		+ Fallback
		+ Sequence
		+ Invert
0.0.1:
	Composites:
		Fixed Bug in Fallback; it didnt store runningIndex
0.1.0:
	Composites:
		+ Parallel
0.2.0:
	Composites:
		+ HierchricalParallel
0.3.0:
	Composites:
		+ SubTree

*/