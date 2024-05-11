using System.Collections.Generic;

namespace BehaviourTree
{ 
	public enum NodeState { RUNNING, SUCCESS, FAILURE }

	public class Node {  
		protected List<Node> children;    
		
		internal Node(params Node[] children) {  
			this.children = new();  
			foreach (Node child in children)  
				this.children.Add(child);  
		}   
		  
		protected virtual NodeState Evaluate() {
		    return NodeState.Failure; //gets overridden
		} 
	    
		public NodeState PerformNode() => Evaluate(); 
	}
}
