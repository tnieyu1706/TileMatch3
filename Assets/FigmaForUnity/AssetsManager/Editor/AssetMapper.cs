using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace FigmaForUnity.Editor
{
    public static class AssetMapper
    {
        public static List<FigmaImageNode> ExtractImageNodes(JObject document)
        {
            var nodes = new List<FigmaImageNode>();
            var root = document["document"];
            if (root == null)
            {
                Debug.LogError("[FigmaSync] Invalid document: no 'document' node.");
                return nodes;
            }

            var pages = root["children"];
            if (pages == null) return nodes;

            foreach (var page in pages)
            {
                var pageName = page["name"]?.ToString() ?? "Unknown";
                TraverseNode(page, nodes, pageName);
            }

            return nodes;
        }

        private static void TraverseNode(JToken node, List<FigmaImageNode> result, string pageName)
        {
            if (node["type"]?.ToString() == "INSTANCE")
                return;

            if (node["fills"] != null)
            {
                foreach (var fill in node["fills"])
                {
                    var type = fill["type"]?.ToString();
                    if (type == "IMAGE")
                    {
                        var imageRef = fill["imageRef"]?.ToString();
                        var nodeId = node["id"]?.ToString();
                        if (!string.IsNullOrEmpty(imageRef) && !string.IsNullOrEmpty(nodeId))
                        {
                            result.Add(new FigmaImageNode
                            {
                                NodeId = nodeId,
                                Name = node["name"]?.ToString(),
                                ImageRef = imageRef,
                                PageName = pageName
                            });
                        }
                        break;
                    }
                }
            }

            if (node["children"] != null)
            {
                foreach (var child in node["children"])
                {
                    TraverseNode(child, result, pageName);
                }
            }
        }
    }
}
