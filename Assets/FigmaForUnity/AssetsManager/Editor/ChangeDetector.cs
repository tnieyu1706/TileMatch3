using System.Collections.Generic;

namespace FigmaForUnity.Editor
{
    public static class ChangeDetector
    {
        public static ChangeReport Compare(List<FigmaImageNode> remoteNodes, FigmaManifest manifest)
        {
            var report = new ChangeReport();

            if (manifest?.Assets == null || manifest.Assets.Count == 0)
            {
                report.ToInstall.AddRange(remoteNodes);
                return report;
            }

            foreach (var node in remoteNodes)
            {
                if (manifest.Assets.TryGetValue(node.NodeId, out var existing))
                {
                    if (existing.ImageRef == node.ImageRef)
                    {
                        report.Unchanged.Add(node);
                    }
                    else
                    {
                        report.ToUpdate.Add(node);
                    }
                }
                else
                {
                    report.ToInstall.Add(node);
                }
            }

            return report;
        }
    }
}
