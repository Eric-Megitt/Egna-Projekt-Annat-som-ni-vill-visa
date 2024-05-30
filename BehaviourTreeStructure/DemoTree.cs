using BehaviourTree;

public class Demotree : Tree
{
	protected override Node SetupTree()
	{
		return
			
		new Fallback(
			new Sequence(
				new IsTheDoorOpen(),
				new GoThroughTheDoor()
			),
			new OpenTheDoor()
		);
	}
}

public class IsTheDoorOpen : Node {
	protected override NodeState Evaluate() {
		return Door.Instance.isOpen ? NodeState.SUCCESS : NodeState.FAILURE;
	}
}

public class GoThroughTheDoor : Node {
	protected override NodeState Evaluate() {
		Player.Instance.GoThroughTheDoor();
		return NodeState.SUCCESS;
	}
}

public class OpenTheDoor : Node {
	protected override NodeState Evaluate() {
		Player.Instance.OpenTheDoor();
		return NodeState.SUCCESS;
	}
}