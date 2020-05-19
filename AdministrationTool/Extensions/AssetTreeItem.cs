using iSIM.Core.Entity.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.WinControls.UI;

namespace AdministrationTool.Extensions
{
    public class AssetTreeItem : RadTreeNode
    {
        public AssetDto AssetDto { get; set; }

        public long Count { get; set; }
    }

    public class UnifiedAssetTreeItem : RadTreeNode
    {
        public UnifiedAssetDto AssetDto { get; set; }

        public long Count { get; set; }
    }
}
