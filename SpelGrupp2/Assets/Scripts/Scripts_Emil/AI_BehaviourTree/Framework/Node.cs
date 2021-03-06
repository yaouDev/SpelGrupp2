using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Node : ScriptableObject {
    private NodeState nodeState;
    protected AI_Controller agent;
    public NodeState NodeState {
        get { return nodeState; }
        set { nodeState = value; }
    }
    public abstract NodeState Evaluate();

    public void SetAgent(AI_Controller ai) {
        agent = ai;
    }
}

public enum NodeState {
    RUNNING, SUCCESS, FAILURE,
}


