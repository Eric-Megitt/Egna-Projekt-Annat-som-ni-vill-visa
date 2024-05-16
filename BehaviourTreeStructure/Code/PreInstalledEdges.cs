using System;
using UnityEngine;

namespace BehaviourTree
{
	public class DebugNode : Node
	{
		object[] objects;

		public DebugNode(params object[] objects) //eric, dont have it like this unless u can pass in lambdas
		{
			this.objects = objects;
		}

		protected override NodeState Evaluate()
		{
			string output = string.Empty;

			foreach (object i in objects)
			{
				output += i;
			}

			Debug.Log(output);

			return NodeState.SUCCESS;
		}
	}

	public class DebugVariableNode : Node
	{
		private Func<object> func;

		public DebugVariableNode(Func<object> func) => this.func = func;

		protected override NodeState Evaluate()
		{
			Debug.Log(func.Invoke().ToString());
			return NodeState.SUCCESS;
		}
	}

	public class LambdaNode : Node
	{
		private Func<bool> func;

		public LambdaNode(Func<bool> func) => this.func = func;

		protected override NodeState Evaluate() => func.Invoke() ? NodeState.SUCCESS : NodeState.FAILURE;
	}

    public class LambdaNodeState : Node
    {
        private Func<NodeState> func;

        public LambdaNodeState(Func<NodeState> func) => this.func = func;

        protected override NodeState Evaluate() => func.Invoke();
    }

    public class Timer : Node
	{
		float delay;

		float countDown;

		public Timer(float delay)
		{
			this.delay = delay;
			this.countDown = this.delay;
		}

		protected override NodeState Evaluate()
		{
			if (countDown <= 0)
			{
				countDown = delay;
				return NodeState.SUCCESS;
			}

			countDown -= Time.deltaTime;
			return NodeState.RUNNING;
		}
	}
}
