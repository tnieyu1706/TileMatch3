using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TnieYuPackage.Helpers
{
    /// <summary>
    /// Cung cấp tính năng kéo thả trong cửa sổ Project của Unity để quản lý sub-asset.
    /// Bao gồm các tùy chọn tự động quét và sửa lại reference (tham chiếu) bị thiếu
    /// khi đóng gói (pack) hoặc giải nén (unpack) asset.
    /// </summary>
    [InitializeOnLoad]
    public static class SubAssetManager
    {
        static SubAssetManager()
        {
            EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
        }

        private static void OnProjectWindowItemGUI(string guid, Rect selectionRect)
        {
            Event evt = Event.current;

            if (evt.type != EventType.DragUpdated && evt.type != EventType.DragPerform) return;
            if (!selectionRect.Contains(evt.mousePosition)) return;

            string targetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(targetPath)) return;

            Object targetMainAsset = AssetDatabase.LoadMainAssetAtPath(targetPath);
            bool isTargetFolder = AssetDatabase.IsValidFolder(targetPath);

            Object[] draggedObjects = DragAndDrop.objectReferences;
            if (draggedObjects == null || draggedObjects.Length == 0) return;

            bool canPackIn = !isTargetFolder && targetMainAsset != null && CanPackInto(draggedObjects, targetMainAsset);
            bool canPackOut = isTargetFolder && CanPackOut(draggedObjects);

            if (!canPackIn && !canPackOut) return;

            if (evt.type == EventType.DragUpdated)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                evt.Use();
            }
            else if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();

                if (canPackIn)
                {
                    string msg =
                        $"Bạn có muốn đóng gói {draggedObjects.Length} asset vào '{targetMainAsset.name}' không?\n\n" +
                        "Bạn có thể chọn tự động quét toàn bộ project để sửa lại các tham chiếu bị thiếu.";

                    // DisplayDialogComplex trả về: 0 = ok (btn 1), 1 = cancel (btn 2), 2 = alt (btn 3)
                    int choice = EditorUtility.DisplayDialogComplex("Đóng gói Asset", msg, "Đóng gói (Sửa Tham Chiếu)",
                        "Hủy",
                        "Đóng gói (Bỏ qua Tham Chiếu)");

                    if (choice == 0) PackAssets(draggedObjects, targetMainAsset, true);
                    else if (choice == 2) PackAssets(draggedObjects, targetMainAsset, false);
                }
                else if (canPackOut)
                {
                    string msg =
                        $"Bạn có muốn giải nén {draggedObjects.Length} sub-asset vào thư mục '{Path.GetFileName(targetPath)}' không?\n\n" +
                        "Bạn có thể chọn tự động quét toàn bộ project để sửa lại các tham chiếu bị thiếu.";

                    int choice = EditorUtility.DisplayDialogComplex("Giải nén Asset", msg, "Giải nén (Sửa Tham Chiếu)",
                        "Hủy", "Giải nén (Bỏ qua Tham Chiếu)");

                    if (choice == 0) UnpackAssets(draggedObjects, targetPath, true);
                    else if (choice == 2) UnpackAssets(draggedObjects, targetPath, false);
                }

                evt.Use();
            }
        }

        #region Validation Logic

        private static bool CanPackInto(Object[] draggedObjects, Object targetAsset)
        {
            foreach (var obj in draggedObjects)
            {
                // KIỂM TRA QUAN TRỌNG: Phải là một file vật lý trong Project, KHÔNG phải GameObject từ Hierarchy
                if (!EditorUtility.IsPersistent(obj)) return false;

                string path = AssetDatabase.GetAssetPath(obj);
                if (string.IsNullOrEmpty(path)) return false;

                if (obj is GameObject || obj is SceneAsset || AssetDatabase.IsValidFolder(path))
                    return false;

                if (path == AssetDatabase.GetAssetPath(targetAsset))
                    return false;
            }

            return true;
        }

        private static bool CanPackOut(Object[] draggedObjects)
        {
            foreach (var obj in draggedObjects)
            {
                // KIỂM TRA QUAN TRỌNG: Phải là một file vật lý trong Project, KHÔNG phải GameObject từ Hierarchy
                if (!EditorUtility.IsPersistent(obj)) return false;

                string path = AssetDatabase.GetAssetPath(obj);
                if (string.IsNullOrEmpty(path)) return false;

                // Không thể giải nén một Main Asset hoặc một thư mục
                if (AssetDatabase.IsMainAsset(obj) || AssetDatabase.IsValidFolder(path))
                    return false;
            }

            return true;
        }

        #endregion

        #region Drag & Drop Processing (Pack / Unpack)

        /// <summary>
        /// Thêm các asset nguồn vào làm sub-asset bên trong main asset mục tiêu.
        /// Có tùy chọn quét và thay thế các tham chiếu bị thiếu trên toàn project.
        /// </summary>
        private static void PackAssets(Object[] sources, Object targetMain, bool fixReferences)
        {
            Dictionary<Object, Object> referenceMap = new Dictionary<Object, Object>();
            List<string> mainAssetsToDelete = new List<string>();
            List<Object> subAssetsToDestroy = new List<Object>();

            foreach (var source in sources)
            {
                string oldPath = AssetDatabase.GetAssetPath(source);

                Object clone = Object.Instantiate(source);
                clone.name = source.name;

                AssetDatabase.AddObjectToAsset(clone, targetMain);
                referenceMap.Add(source, clone); // Lưu lại map để sửa tham chiếu

                if (AssetDatabase.IsMainAsset(source))
                {
                    mainAssetsToDelete.Add(oldPath);
                }
                else
                {
                    subAssetsToDestroy.Add(source);
                }
            }

            // Lưu asset để clone được đăng ký đầy đủ trước khi sửa tham chiếu
            AssetDatabase.SaveAssets();

            if (fixReferences)
            {
                ReplaceReferencesInProject(referenceMap);
            }

            // Xóa file cũ một cách an toàn
            foreach (var path in mainAssetsToDelete)
            {
                AssetDatabase.DeleteAsset(path);
            }

            foreach (var obj in subAssetsToDestroy)
            {
                Object.DestroyImmediate(obj, true);
            }

            SaveAndRefresh(targetMain);
            Debug.Log($"[SubAssetManager] Đã đóng gói thành công {sources.Length} asset!");
        }

        /// <summary>
        /// Giải nén sub-asset ra một thư mục cụ thể dưới dạng file độc lập.
        /// Có tùy chọn quét và thay thế các tham chiếu bị thiếu trên toàn project.
        /// </summary>
        private static void UnpackAssets(Object[] subAssets, string targetFolderPath, bool fixReferences)
        {
            Dictionary<Object, Object> referenceMap = new Dictionary<Object, Object>();
            List<Object> subAssetsToDestroy = new List<Object>();

            foreach (var subAsset in subAssets)
            {
                string extension = subAsset is AnimationClip ? ".anim" : ".asset";
                string newPath = Path.Combine(targetFolderPath, subAsset.name + extension).Replace("\\", "/");
                newPath = AssetDatabase.GenerateUniqueAssetPath(newPath);

                Object clone = Object.Instantiate(subAsset);
                clone.name = subAsset.name;

                AssetDatabase.CreateAsset(clone, newPath);

                referenceMap.Add(subAsset, clone); // Lưu lại map
                subAssetsToDestroy.Add(subAsset); // Trì hoãn việc xóa cho đến khi sửa xong tham chiếu
            }

            AssetDatabase.SaveAssets();

            if (fixReferences)
            {
                ReplaceReferencesInProject(referenceMap);
            }

            // Dọn dẹp các sub-asset gốc
            foreach (var obj in subAssetsToDestroy)
            {
                AssetDatabase.RemoveObjectFromAsset(obj);
                Object.DestroyImmediate(obj, true);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[SubAssetManager] Đã giải nén thành công {subAssets.Length} asset vào {targetFolderPath}");
        }

        #endregion

        #region Reference Fixing Logic

        /// <summary>
        /// Quét project và thay thế bất kỳ thuộc tính nào đang trỏ tới asset cũ bằng asset mới.
        /// </summary>
        private static void ReplaceReferencesInProject(Dictionary<Object, Object> referenceMap)
        {
            if (referenceMap == null || referenceMap.Count == 0) return;

            string[] allPaths = AssetDatabase.GetAllAssetPaths();
            float total = allPaths.Length;

            for (int i = 0; i < allPaths.Length; i++)
            {
                string path = allPaths[i];

                // Cập nhật thanh tiến trình
                if (i % 50 == 0)
                {
                    if (EditorUtility.DisplayCancelableProgressBar("Đang cập nhật tham chiếu",
                            $"Đang quét project: {path}",
                            i / total))
                        break;
                }

                // Lọc bỏ các file không thuộc project và các file không thể chứa reference của Unity để tăng tốc
                if (!path.StartsWith("Assets/")) continue;
                string ext = Path.GetExtension(path).ToLower();
                if (ext == ".cs" || ext == ".png" || ext == ".jpg" || ext == ".wav" ||
                    ext == ".mp3" || ext == ".fbx" || ext == ".obj" || ext == ".unity" || ext == ".txt" ||
                    ext == ".json")
                {
                    // Lưu ý: file .unity (Scene) bị bỏ qua vì LoadAllAssetsAtPath hoạt động không ổn định trên scene đang đóng.
                    continue;
                }

                Object[] assetsInPath = AssetDatabase.LoadAllAssetsAtPath(path);
                bool modified = false;

                foreach (var asset in assetsInPath)
                {
                    if (asset == null) continue;

                    SerializedObject so = new SerializedObject(asset);
                    SerializedProperty prop = so.GetIterator();
                    bool enterChildren = true;

                    // Lặp qua từng thuộc tính bên trong asset
                    while (prop.Next(enterChildren))
                    {
                        enterChildren = true;
                        if (prop.propertyType == SerializedPropertyType.ObjectReference)
                        {
                            Object refObj = prop.objectReferenceValue;
                            if (refObj != null && referenceMap.TryGetValue(refObj, out Object newObj))
                            {
                                // Ghi đè tham chiếu cũ bằng clone mới
                                prop.objectReferenceValue = newObj;
                                modified = true;
                            }
                        }
                    }

                    if (modified)
                    {
                        so.ApplyModifiedPropertiesWithoutUndo();
                        EditorUtility.SetDirty(asset);
                    }
                }
            }

            EditorUtility.ClearProgressBar();
        }

        #endregion

        #region Context Menu Actions

        [MenuItem("Assets/Sub-Asset Manager/Unpack All Sub-assets", true)]
        [MenuItem("Assets/Sub-Asset Manager/Delete All Sub-assets", true)]
        private static bool ValidateContextMenu()
        {
            return Selection.activeObject != null && AssetDatabase.IsMainAsset(Selection.activeObject);
        }

        [MenuItem("Assets/Sub-Asset Manager/Unpack All Sub-assets", false, 20)]
        private static void ContextMenu_UnpackAll()
        {
            Object mainAsset = Selection.activeObject;
            string assetPath = AssetDatabase.GetAssetPath(mainAsset);
            string folderPath = Path.GetDirectoryName(assetPath);

            Object[] allObjects = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            List<Object> subAssetsToUnpack = new List<Object>();

            foreach (var obj in allObjects)
            {
                if (obj == null || AssetDatabase.IsMainAsset(obj)) continue;
                subAssetsToUnpack.Add(obj);
            }

            if (subAssetsToUnpack.Count == 0)
            {
                Debug.Log("[SubAssetManager] Không tìm thấy sub-asset nào để giải nén.");
                return;
            }

            string msg = $"Tìm thấy {subAssetsToUnpack.Count} sub-asset bên trong '{mainAsset.name}'.\n" +
                         $"Bạn có muốn giải nén chúng ra thư mục '{Path.GetFileName(folderPath)}' không?\n\n" +
                         "Bạn có thể chọn tự động quét toàn bộ project để sửa lại các tham chiếu bị thiếu.";

            int choice = EditorUtility.DisplayDialogComplex("Giải nén tất cả Sub-asset", msg,
                "Giải nén (Sửa Tham Chiếu)", "Hủy",
                "Giải nén (Bỏ qua Tham Chiếu)");

            if (choice == 1) return; // Người dùng bấm Hủy

            // Chạy logic giải nén với danh sách sub-asset thu thập được
            UnpackAssets(subAssetsToUnpack.ToArray(), folderPath, choice == 0);
        }

        [MenuItem("Assets/Sub-Asset Manager/Delete All Sub-assets", false, 21)]
        private static void ContextMenu_DeleteAll()
        {
            Object mainAsset = Selection.activeObject;
            string assetPath = AssetDatabase.GetAssetPath(mainAsset);
            Object[] allObjects = AssetDatabase.LoadAllAssetsAtPath(assetPath);

            if (EditorUtility.DisplayDialog("Cảnh báo",
                    $"Bạn có chắc chắn muốn XÓA TOÀN BỘ sub-asset bên trong '{mainAsset.name}' không?",
                    "Xóa", "Hủy"))
            {
                int count = 0;
                foreach (var obj in allObjects)
                {
                    if (obj == null || AssetDatabase.IsMainAsset(obj)) continue;

                    Object.DestroyImmediate(obj, true);
                    count++;
                }

                if (count > 0)
                {
                    SaveAndRefresh(mainAsset);
                    Debug.Log($"[SubAssetManager] Đã xóa {count} sub-asset khỏi {mainAsset.name}");
                }
            }
        }

        #endregion

        #region Helper Methods

        private static void SaveAndRefresh(Object asset)
        {
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(asset));
            AssetDatabase.Refresh();
        }

        #endregion
    }
}