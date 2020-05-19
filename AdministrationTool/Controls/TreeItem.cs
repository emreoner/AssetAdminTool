using iSIM.Core.Common.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdministrationTool.Controls
{
    public class TreeItem
    {
        public string Id { get; set; }
        public string ParentId { get; set; }
        public int UniqueId { get; set; }
        public Type ItemType { get; set; }
        public object Tag { get; set; }
        public object OriginalObject { get; set; }
        public string Text { get; set; }
        public string Name { get; set; }
        public int ValuableChildrenCount { get; set; }

        public AssetType ImageKey { get; set; }
    }
}
