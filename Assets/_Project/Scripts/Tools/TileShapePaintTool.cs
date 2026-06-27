#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TileMatch3.Core.Shape;

namespace TileMatch3.Tools
{
    public class TileShapePaintTool : EditorWindow
    {
        private TileShape activeShape;

        // Trạng thái tool
        private int tempWidth;
        private int tempHeight;
        private Vector2 scrollPos;
        private bool isErasing = false; // false = Paint, true = Erase

        private const float CELL_SIZE = 35f; // Ô to hơn để dễ vẽ

        public static void ShowWindow(TileShape runtime)
        {
            TileShapePaintTool window = GetWindow<TileShapePaintTool>("Tile Shape Paint Tool");
            window.minSize = new Vector2(500, 400);

            window.LoadShape(runtime);
            window.Show();
        }

        private void LoadShape(TileShape runtime)
        {
            activeShape = runtime;
            if (activeShape != null)
            {
                activeShape.ValidateData();
                tempWidth = activeShape.width;
                tempHeight = activeShape.height;
            }
        }

        private void OnGUI()
        {
            if (activeShape == null)
            {
                DrawNoShapeSelectedGUI();
                return;
            }

            DrawToolbar();
            DrawGridCanvas();
        }

        private void DrawNoShapeSelectedGUI()
        {
            EditorGUILayout.HelpBox(
                "Không có TileShape nào được chọn.\nHãy chọn một Shape từ cửa sổ Project hoặc Inspector.",
                MessageType.Warning);

            activeShape = (TileShape)EditorGUILayout.ObjectField("Select Shape", activeShape, typeof(TileShape), false);
            if (activeShape != null) LoadShape(activeShape);
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Editing: <b>{activeShape.name}</b>",
                new GUIStyle(EditorStyles.label) { richText = true }, GUILayout.Width(200));

            // 1. Tool Cọ / Tẩy
            GUILayout.FlexibleSpace();
            GUI.backgroundColor = !isErasing ? Color.green : Color.white;
            if (GUILayout.Button("🖌 Paint", EditorStyles.miniButtonLeft, GUILayout.Width(70), GUILayout.Height(25)))
                isErasing = false;

            GUI.backgroundColor = isErasing ? new Color(1f, 0.5f, 0.5f) : Color.white;
            if (GUILayout.Button("🧽 Erase", EditorStyles.miniButtonRight, GUILayout.Width(70), GUILayout.Height(25)))
                isErasing = true;
            GUI.backgroundColor = Color.white;

            GUILayout.Space(10);
            if (GUILayout.Button("🗑 Clear Full", EditorStyles.miniButton, GUILayout.Width(80), GUILayout.Height(25)))
            {
                if (EditorUtility.DisplayDialog("Xóa toàn bộ", "Bạn có chắc chắn muốn xóa sạch canvas này?", "Clear",
                        "Cancel"))
                {
                    Undo.RecordObject(activeShape, "Clear Shape");
                    activeShape.Clear();
                    EditorUtility.SetDirty(activeShape);
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // 2. Chỉnh kích thước & Apply
            EditorGUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 50;
            tempWidth = EditorGUILayout.IntField("Width", tempWidth, GUILayout.Width(100));
            tempHeight = EditorGUILayout.IntField("Height", tempHeight, GUILayout.Width(100));
            EditorGUIUtility.labelWidth = 0;

            bool isModified = (tempWidth != activeShape.width || tempHeight != activeShape.height);

            EditorGUI.BeginDisabledGroup(!isModified);
            GUI.backgroundColor = isModified ? Color.yellow : Color.white;
            if (GUILayout.Button(isModified ? "⚠️ Apply Resize" : "Size Applied", GUILayout.Width(120)))
            {
                Undo.RecordObject(activeShape, "Resize Tile Shape");
                activeShape.Resize(tempWidth, tempHeight);
                EditorUtility.SetDirty(activeShape);
                GUI.FocusControl(null);
            }

            GUI.backgroundColor = Color.white;
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawGridCanvas()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            float totalWidth = activeShape.width * CELL_SIZE;
            float totalHeight = activeShape.height * CELL_SIZE;

            // Tạo một vùng đệm xung quanh canvas (margin)
            GUILayoutUtility.GetRect(totalWidth + 40, totalHeight + 40);
            Rect gridRect = new Rect(20, GUILayoutUtility.GetLastRect().y + 20, totalWidth, totalHeight);

            // Nền cho toàn bộ grid
            EditorGUI.DrawRect(gridRect, new Color(0.15f, 0.15f, 0.15f));

            // Vẽ các ô vuông
            for (int y = 0; y < activeShape.height; y++)
            {
                for (int x = 0; x < activeShape.width; x++)
                {
                    Rect cellRect = new Rect(gridRect.x + x * CELL_SIZE, gridRect.y + y * CELL_SIZE, CELL_SIZE,
                        CELL_SIZE);
                    bool isActive = activeShape.GetCell(x, y);

                    // Màu ô
                    if (isActive)
                    {
                        EditorGUI.DrawRect(
                            new Rect(cellRect.x + 1, cellRect.y + 1, cellRect.width - 2, cellRect.height - 2),
                            Color.green);
                    }

                    // Viền ô
                    Handles.color = new Color(1, 1, 1, 0.1f);
                    Handles.DrawWireCube(cellRect.center, cellRect.size);
                }
            }

            Handles.color = Color.white;

            // Xử lý Input (Kéo chuột để vẽ liên tục)
            Event e = Event.current;
            if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && e.button == 0) // Nút trái
            {
                if (gridRect.Contains(e.mousePosition))
                {
                    int gridX = Mathf.FloorToInt((e.mousePosition.x - gridRect.x) / CELL_SIZE);
                    int gridY = Mathf.FloorToInt((e.mousePosition.y - gridRect.y) / CELL_SIZE);

                    if (gridX >= 0 && gridX < activeShape.width && gridY >= 0 && gridY < activeShape.height)
                    {
                        bool currentState = activeShape.GetCell(gridX, gridY);
                        bool targetState = !isErasing;

                        if (currentState != targetState)
                        {
                            Undo.RecordObject(activeShape, "Paint Shape Cell");
                            activeShape.SetCell(gridX, gridY, targetState);
                            EditorUtility.SetDirty(activeShape);
                        }
                    }

                    e.Use(); // Quan trọng: Bắt event để GUI update ngay lập tức khi di chuột
                }
            }

            EditorGUILayout.EndScrollView();
        }
    }
}
#endif