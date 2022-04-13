using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfinderManager : MonoBehaviour {
    [SerializeField] SimpleGraph graph;
    [SerializeField] float proximityToUseSamePath, acceptableDistanceFromTarget;
    private List<Vector3> latestCalculatedPath = new List<Vector3>();
    private Vector3 targetBlocked, desiredTarget, activeTarget;
    public static PathfinderManager instance;
    private PriorityQueue<AI_Movement> pathQueue;

    void Awake() {
        instance ??= this;
        pathQueue = new PriorityQueue<AI_Movement>();
    }

    private bool isPathBlocked() {
        return graph.getBlockedNode(activeTarget).Length != 0;
    }

    public float getAcceptableDistanceFromTarget(){
        return acceptableDistanceFromTarget;
    }

    private bool shouldUpdatePath() {
        bool realTargetFree = activeTarget == targetBlocked && graph.getBlockedNode(desiredTarget).Length == 0;
        return latestCalculatedPath == null || latestCalculatedPath.Count == 0 || isPathBlocked() || realTargetFree;
    }

    private float heuristic(Vector3 currentPos, Vector3 endPos) {
        return Mathf.Abs(currentPos.x - endPos.x) + Mathf.Abs(currentPos.z - endPos.z);
    }

    private List<Vector3> getPath(Dictionary<Vector3, Vector3> via, Vector3 node, Vector3 end, Vector3 start) {
        List<Vector3> path = new List<Vector3>();
        if (node == end) {
            for (; node != start; node = via[node]) {
                path.Add(node);
            }
            path.Add(start);
        }
        path.Reverse();
        return path;
    }


    /*     public List<Vector3> requestPath(Vector3 currentPosition, Vector3 endPos) {
            if (latestCalculatedPath != null && latestCalculatedPath.Count != 0) {
                bool wrongDirectionCond = Vector3.Dot(latestCalculatedPath[0], currentPosition) > 0;
                if (Vector3.Distance(currentPosition, latestCalculatedPath[0]) <= proximityToUseSamePath && endPos == latestCalculatedPath[latestCalculatedPath.Count - 1] && wrongDirectionCond) {
                    List<Vector3> pathToStartOflatest = aStar(currentPosition, latestCalculatedPath[0], false);
                    pathToStartOflatest.AddRange(latestCalculatedPath);
                    return pathToStartOflatest;
                }
            }
            return aStar(currentPosition, endPos, true);
        } */

    public void requestPath(AI_Movement agent, Vector3 currentPosition, Vector3 endPos) {
        if (latestCalculatedPath != null && latestCalculatedPath.Count != 0) {
            bool wrongDirectionCond = Vector3.Dot(latestCalculatedPath[0], currentPosition) > 0;
            if (Vector3.Distance(currentPosition, latestCalculatedPath[0]) <= proximityToUseSamePath && endPos == latestCalculatedPath[latestCalculatedPath.Count - 1] && wrongDirectionCond) {
                List<Vector3> pathToStartOflatest = aStar(currentPosition, latestCalculatedPath[0], false);
                pathToStartOflatest.AddRange(latestCalculatedPath);
                agent.setPath(pathToStartOflatest);
            }
        }
        if (!pathQueue.contains(agent) && Vector3.Distance(currentPosition, endPos) >= acceptableDistanceFromTarget)
            pathQueue.insert(agent, Vector3.Distance(currentPosition, endPos));

    }

    void Update() {
        try {
            AI_Movement agentToUpdate = pathQueue.deleteMin();
            agentToUpdate.updateTarget();
            agentToUpdate.setPath(aStar(agentToUpdate.transform.position, agentToUpdate.getCurrentTarget(), true));
        } catch (System.Exception) {
            Debug.Log("No current requested paths");
        }


    }

    private List<Vector3> aStar(Vector3 startPos, Vector3 endPos, bool updateLatestPath) {
        PriorityQueue<Vector3> priorityQueue = new PriorityQueue<Vector3>();
        Vector3 node = Vector3.zero;
        int edgesTested = 0;
        HashSet<Vector3> explored = new HashSet<Vector3>();
        Dictionary<Vector3, Vector3> via = new Dictionary<Vector3, Vector3>();
        Dictionary<Vector3, float> cost = new Dictionary<Vector3, float>();
        Vector3 closestBox = graph.getClosestBox(startPos);
        priorityQueue.insert(closestBox, 0);
        via[closestBox] = closestBox;
        cost[closestBox] = 0;

        while (!priorityQueue.isEmpty()) {
            node = priorityQueue.deleteMin();
            explored.Add(node);
            if (node == endPos) break;
            Dictionary<Vector3, float> currEdges = graph.getNeighbors(node);
            foreach (Vector3 neighbor in currEdges.Keys) {
                edgesTested++;
                float tmpCost = cost[node] + graph.getCost(node, neighbor);
                if ((!cost.ContainsKey(neighbor) || tmpCost < cost[neighbor]) && graph.getBlockedNode(neighbor).Length == 0) {
                    cost[neighbor] = tmpCost;
                    float heurVal = tmpCost + heuristic(neighbor, endPos);
                    priorityQueue.insert(neighbor, heurVal);
                    via[neighbor] = node;
                }
            }
        }
        List<Vector3> path = getPath(via, node, endPos, closestBox);
        if (updateLatestPath) latestCalculatedPath = path;
        return path;
    }

    // Reused (but modified) heap from ALDA
    private class PriorityQueue<T> {
        private const int DEFAULT_CAPACITY = 50;
        private int numberOfChildren, currentSize;
        // coordinate of box and its priority
        private KeyValuePair<T, float>[] array;
        private HashSet<T> contentCheckSet;

        public PriorityQueue() {
            numberOfChildren = 2;
            array = new KeyValuePair<T, float>[DEFAULT_CAPACITY];
            contentCheckSet = new HashSet<T>();
        }

        public int getParentIndex(int childToCheck) {
            if (childToCheck > 1) {
                int parentIndex;
                if ((childToCheck - 1) % numberOfChildren != 0) {
                    parentIndex = childToCheck;
                    while (parentIndex % numberOfChildren != 0) parentIndex++;
                } else parentIndex = childToCheck - 1;
                return parentIndex / numberOfChildren;
            }
            throw new System.Exception("Illegal argument: Cannot check parent of root!");
        }

        public bool contains(T element) {
            return contentCheckSet.Contains(element);
        }

        public int getFirstChildIndex(int parent) {
            if (parent > 0) return parent * numberOfChildren - (numberOfChildren - 2);
            throw new System.Exception("Illegal argument: Cannot check child of index 0 or below!");
        }

        public int size() { return currentSize; }

        public void insert(T element, float priority) {
            if (currentSize == array.Length - 1) enlargeArray(array.Length * 2 + 1);
            int hole = ++currentSize;
            KeyValuePair<T, float> insertPair = new KeyValuePair<T, float>(element, priority);
            for (array[0] = insertPair; hole > 1 && insertPair.Value < array[getParentIndex(hole)].Value; hole = getParentIndex(hole)) {
                array[hole] = array[getParentIndex(hole)];
            }
            array[hole] = insertPair;
            contentCheckSet.Add(element);
        }

        private void enlargeArray(int newSize) {
            KeyValuePair<T, float>[] old = array;
            array = new KeyValuePair<T, float>[newSize];
            for (int i = 0; i < old.Length; i++) {
                array[i] = old[i];
            }
        }

        public T findMin() {
            if (isEmpty()) throw new System.Exception("Underflow, Queue was empty!");
            return array[1].Key;
        }

        public T deleteMin() {
            if (isEmpty()) throw new System.Exception("Underflow, Queue was empty!");
            T minItem = findMin();
            array[1] = array[currentSize--];
            percolateDown(1);
            contentCheckSet.Remove(minItem);
            return minItem;
        }

        public bool isEmpty() { return currentSize == 0; }

        public void makeEmpty() { currentSize = 0; }

        private void percolateDown(int hole) {
            int child = 0;
            KeyValuePair<T, float> tmp = array[hole];
            for (; getFirstChildIndex(hole) <= currentSize; hole = child) {
                child = findIndexOfMinChild(hole);
                if (child > -1 && array[child].Value < tmp.Value) array[hole] = array[child];
                else break;
            }
            array[hole] = tmp;
        }

        private int findIndexOfMinChild(int parent) {
            int tmpChild = getFirstChildIndex(parent);
            if (array.Length > tmpChild && !array[tmpChild].Equals(null)) {
                int child = tmpChild;
                for (int i = 0; i < numberOfChildren && i + tmpChild <= currentSize; i++) {
                    if (!array[tmpChild + 1].Equals(null) && array[tmpChild + i].Value < array[child].Value) {
                        child = tmpChild + i;
                    }
                }
                return child;
            }
            return -1;
        }
    }
}

