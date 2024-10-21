using System;
using System.Collections;
using System.Collections.Generic;
using CZToolKit.GraphProcessor;
using LogicGraph.Runtime;
using Sirenix.Serialization;
using UnityEngine;
using Object = UnityEngine.Object;

[CreateAssetMenu]
public class LogicGraphAssets : ScriptableObject, IGraphAsset, IGraphAsset<LogicGraphModel>
{
    [HideInInspector] public byte[] serializedGraph;
    [HideInInspector] public List<Object> graphUnityReferences = new List<Object>();
    public Object UnityAsset => this;
    public Type GraphType => typeof(LogicGraphModel);

    public void SaveGraph(BaseGraph graph)
    {
        serializedGraph = SerializationUtility.SerializeValue(graph, DataFormat.Binary, out graphUnityReferences);
    }

    public BaseGraph DeserializeGraph()
    {
        return DeserializeTGraph();
    }

    public LogicGraphModel DeserializeTGraph()
    {
        LogicGraphModel graph = null;
        if (serializedGraph is { Length: > 0 })
        {
            graph = SerializationUtility.DeserializeValue<LogicGraphModel>(serializedGraph, DataFormat.Binary, graphUnityReferences);
        }
        return graph ?? new LogicGraphModel();
    }
}
