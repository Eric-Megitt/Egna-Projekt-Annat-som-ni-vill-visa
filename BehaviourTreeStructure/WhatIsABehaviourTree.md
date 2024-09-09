# The Tree
Behaviour Trees are a way of organizing complex systems of behaviour. They can for instance be used for decision-making in video game NPCs, robotics or simulation.

They're called behaviour "trees" since they're commonly represented as a graphs that resemble trees, with sprouting branches that derive from the root.


```mermaid
graph TD    

A[Fallback] --> B[Sequence] 
A[Fallback] --> C[Sequence]
B[Sequence] --> D[Is door open?]
B[Sequence] --> E[Move into room]
C[Sequence] --> F[Move into door]
C[Sequence] --> G[Open door]
C[Sequence] --> H[Move into room]

```

All the different components in the tree are called Nodes. There are different types of them, the ones on the bottom row which don't have children are called leafs or edges. The nodes **with** children are called composites.

# Nodes
Nodes can be evaluated (a.k.a. executed), a node being evaluated means that it's Evaluate() method is called, the method can contain any code you please but it must return one of three ("node")states: *Failure*, *Success* or *Running*. NodeStates are how nodes are able to communicate with the nodes above them in the tree-structure.

(The root-node is special since it gets evaluated on a preset intervall, in video-games it's commonly once per frame, this will be important when we look at our first example later on.)

```cpp  
public enum NodeState { RUNNING, SUCCESS, FAILURE }
```

```cpp
public class Node  
{  
    protected List<Node> children;    
	
    internal Node(params Node[] children) {  
		this.children = new();  
		foreach (Node child in children)  
	    this.children.Add(child);  
    }   
	  
    protected virtual NodeState Evaluate() {
	    return NodeState.FAILURE; //gets overridden
    } 
    
	public NodeState PerformNode() => Evaluate(); 
}
```

Composite nodes, the ones with children, evaluate their children and return a NodeState based upon their children's result.

The NodeStates *Success* and *Failure* don't have any implicit meaning but composite nodes interpret these in unique ways. While a returned *Running* says: "Hey, I'm performing an asynchronous task, please *evaluate* me again so that I can finish my task". *Running* also makes all of it's ancestors (parents) up to the root return *Running* as well.

## Common Composite Nodes
To move on to the next section you only need a grasp of the *Sequence* and *Fallback* composites.
#### Sequence
The sequence composite iterates through all it's children until one returns *Failure* or all return success. If one of it's children returns *Failure*, the sequence does the same, and only if all the children return *Success* the sequence does so too.

```cs
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
```

#### Fallback
Sometimes referred to as *Selectors*. The Fallback composite iterates through all it's children until one returns *Success* or all return *Failure*. If one of it's children returns *Success*, the Fallback does the same, but if all the children return *Failure* the Fallback does so too.

```cs
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
```

#### Random
This composite is not necessarily a feature of every behaviour trees system. My implementation of the random composite executes only one of it's children, a randomized one or the node from the last iteration if it returned *Running*. The random node's return-value mirrors the one of the selected child. The children also have weights attached to them which are used to make the children nodes more or less probable to get executed.

```cs
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
```

## Decorators
A decorator is a composite node with merely one child by definition. Common ones are
(mainly 1-4):
1. Invert
	Takes child's returned NodeState and returns the opposite: *Success* becomes *Failure* and vice versa, while *Running* gives *Running*.
2. Retry
	Takes a number as an additional argument, the number is the amount of times that the child would be reevaluated, until the child returns a *Success*. The retry-node mirrors the child's last response.
3. Repeat 
	Evaluates child x amount of times (evaluations returning running aren't counted). Returns mirror of child's last return value.
4. Timeout
	Constricts asynchronous tasks to a time-limit. Returns *Failure* if time-limit is reached otherwise mirrors child.
5. & 6. Force Success/Failure
	Evaluates child till it's not *Running*. Returns *Success*/*Failure*, respectively, regardless of what the child's return value is.
7. & 8. Keep Running Until *Success*/*Failure*
	Keeps child running until *Success* or *Failure* respectively

## Edges/Leafs
While composite nodes are used to control the flow and logic of the tree, edges are used control-statements and actions. 

An example statement could be "Is the door open?", which could be true, the door is open, or false, it's closed. *Success* symbolizes true, and *Failure* false. 

Actions are things that happen, for instance following up on "Is the door open?", a good response to that being true might be to "Go though the door!".
# A tree in practice
Combining our knowledge so far we'll be able to look at an example. Remember how the root node gets evaluated on a time intervall, now that will be important to keep in mind.

```mermaid
graph TD

A[Fallback] --> B[Sequence]
	B[Sequence] --> C[Is the door open?]
	B[Sequence] --> D[Go though the door!]
A[Fallback] --> E[Open door!]
```
We'll start of by defining our goal, which is to make our way through a door. We start at the Fallback which leads us down to the Sequence which evaluates "Is the door open?", if it is then the Sequence continues to execute it's children in order resulting in the action "Go though the door!". The second scenario where "Is the door open?" returns *Failure* is more tricky. In this scenario the Sequence also returns *Failure* and the Fallback goes on to "Open door!". So the next time the root gets evaluated the door will be open and we end up like we did in scenario 1. Yay, we've made our way through the door!

# Sources
- Auryn Robotics. (n.d.). *Decorators*. Retrieved 2024-05-09 from https://www.behaviortree.dev/docs/nodes-library/decoratornode/
- Robohub. (2021). *Introduction to behavior trees*. Retrieved 2024-05-09 from https://robohub.org/introduction-to-behavior-trees/
