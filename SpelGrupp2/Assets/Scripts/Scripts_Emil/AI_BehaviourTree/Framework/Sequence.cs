using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "AIBehavior/Framework/Sequence")]

public class Sequence : Node {
    [SerializeField] protected List<Node> nodes = new List<Node>();
    public override NodeState Evaluate() {
        bool isAnyChildRunning = false;
        foreach (Node node in nodes) {
            switch (node.Evaluate()) {
                case NodeState.RUNNING:
                    isAnyChildRunning = true;
                    break;
                case NodeState.SUCCESS:
                    break;
                case NodeState.FAILURE:
                    NodeState = NodeState.FAILURE;
                    return NodeState;
                default:
                    break;
            }
        }
        NodeState = isAnyChildRunning ? NodeState.RUNNING : NodeState.SUCCESS;
        return NodeState;
    }

    public Sequence(List<Node> nodes) {
        this.nodes = nodes;
    }

    public List<Node> GetInnerNodes() {
        return nodes;
    }

    public void SetInnerNodes(List<Node> innerNodes) { nodes = innerNodes; }
}
