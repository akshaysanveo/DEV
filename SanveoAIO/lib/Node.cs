using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SanveoAIO.lib
{
    public class Node
    {
        public string label { get; set; }
        public string id { get; set; }
        public string version { get; set; }
        public bool load_on_demand { get; set; }

        public Node(string label, string id,string version, bool loadOnDemand = true)
        {
            this.label = label;
            this.id = id;
            this.load_on_demand = loadOnDemand;
            this.version = version;
        }
    }
}
