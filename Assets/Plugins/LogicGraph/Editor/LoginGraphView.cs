using System;
using System.Collections.Generic;
using CZToolKit;
using CZToolKit.GraphProcessor;
using CZToolKit.GraphProcessor.Editors;

namespace LogicGraph.Runtime
{
    public class LoginGraphView : BaseGraphView
    {
        public LoginGraphView(BaseGraphProcessor graph, BaseGraphWindow window, CommandDispatcher commandDispatcher) : base(graph, window, commandDispatcher)
        {
        }

        protected override void BuildNodeMenu(NodeMenuWindow nodeMenu)
        {
            foreach (var nodeType in GetNodeTypes())
            {
                if (nodeType.IsAbstract)
                    continue;
                var nodeStaticInfo = GraphProcessorUtil.NodeStaticInfos[nodeType];
                if (nodeStaticInfo.hidden)
                    continue;

                var path = nodeStaticInfo.path;
                var menu = nodeStaticInfo.menu;
                nodeMenu.entries.Add(new NodeMenuWindow.NodeEntry(path, menu, nodeType));
            }
        }

        private IEnumerable<Type> GetNodeTypes()
        {
            yield return typeof(EntryNodeModel);
        }
    }
}
