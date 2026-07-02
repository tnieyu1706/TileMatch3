#if UNITY_EDITOR
using TileMatch3.Tools;
using UnityEditor;
using UnityEngine;

namespace TileMatch3.Core.Shape
{
    [CustomEditor(typeof(TileShape))]
    public class TileShapeInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            TileShape runtime = (TileShape)target;
            runtime.ValidateData();

            EditorGUILayout.Space(10);
            
            // Nút mở Tool Window nổi bật
            GUI.backgroundColor = new Color(0.2f, 0.6f, 1f); 
            if (GUILayout.Button("Open Tile Shape Paint Tool", GUILayout.Height(40)))
            {
                TileShapePaintTool.ShowWindow(runtime);
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(10);
            
            // Hiển thị thông số (Read-only)
            EditorGUILayout.LabelField("Shape Info", EditorStyles.boldLabel);
            EditorGUI.BeginDisabledGroup(true); // Disable để không cho sửa ở đây
            EditorGUILayout.IntField("Width", runtime.width);
            EditorGUILayout.IntField("Height", runtime.height);
            EditorGUILayout.IntField("Active Tiles", GetActiveTileCount(runtime));
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(15);

            // Vẽ Preview
            DrawReadOnlyPreview(runtime);
        }

        private int GetActiveTileCount(TileShape shape)
        {
            int count = 0;
            if (shape.gridData != null)
            {
                foreach (bool isActive in shape.gridData)
                    if (isActive) count++;
            }
            return count;
        }

        private void DrawReadOnlyPreview(TileShape runtime)
        {
            EditorGUILayout.LabelField("Preview:", EditorStyles.boldLabel);
            
            // Tính toán kích thước ô preview sao cho vừa vặn với Inspector
            float inspectorWidth = EditorGUIUtility.currentViewWidth - 40f;
            float maxCellSize = 20f;
            float cellSize = Mathf.Min(maxCellSize, inspectorWidth / Mathf.Max(runtime.width, 1));
            
            Rect rect = GUILayoutUtility.GetRect(runtime.width * cellSize, runtime.height * cellSize);
            
            // Căn giữa preview
            rect.x += (inspectorWidth - (runtime.width * cellSize)) / 2f;

            for (int y = 0; y < runtime.height; y++)
            {
                for (int x = 0; x < runtime.width; x++)
                {
                    Rect cellRect = new Rect(rect.x + x * cellSize, rect.y + y * cellSize, cellSize, cellSize);
                    bool isActive = runtime.GetCell(x, y);
                    
                    // Ô xanh lá nếu active, xám đậm nếu inactive
                    EditorGUI.DrawRect(cellRect, isActive ? new Color(0.2f, 0.8f, 0.2f) : new Color(0.3f, 0.3f, 0.3f));
                    
                    // Vẽ viền caro mờ tạo cảm giác grid
                    GUI.color = new Color(0, 0, 0, 0.3f);
                    GUI.DrawTexture(cellRect, EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill);
                    GUI.color = Color.white;
                }
            }
        }
    }
}
#endif