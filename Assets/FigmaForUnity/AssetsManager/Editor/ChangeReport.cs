using System.Collections.Generic;

namespace FigmaForUnity.Editor
{
    public class ChangeReport
    {
        public List<FigmaImageNode> ToInstall { get; set; } = new List<FigmaImageNode>();
        public List<FigmaImageNode> ToUpdate { get; set; } = new List<FigmaImageNode>();
        public List<FigmaImageNode> Unchanged { get; set; } = new List<FigmaImageNode>();

        public bool HasChanges => ToInstall.Count > 0 || ToUpdate.Count > 0;
    }
}
