using System;

namespace TemplateEngine_v3.Models.CustomEventArgs
{
    public class NodeChangedEventArgs : EventArgs
    {
        public Node NewNode { get; }

        public NodeChangedEventArgs(Node newNode)
        {
            NewNode = newNode;
        }
    }

}
