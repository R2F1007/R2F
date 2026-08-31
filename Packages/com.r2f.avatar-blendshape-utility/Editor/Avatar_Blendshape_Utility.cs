using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEditor;
using UnityEngine;

// R2F FIXED BUILD 2026-08-15 V8: NATIVE_TEXT_SELECTION + SMOOTH_SINGLE_SLIDER_UPDATE
public class VRCFaceModifierWindow : EditorWindow
{
    public enum LanguageOption
    {
        English,
        Japanese,
        Korean,
        Chinese
    }
    private LanguageOption currentLanguage = LanguageOption.English;

    public enum ThemeOption
    {
        Dark,
        Light
    }
    private ThemeOption currentTheme = ThemeOption.Dark;

    [MenuItem("R2F/Avatar Blendshape Utility")]
    public static void OpenWindow()
    {
        var window = GetWindow<VRCFaceModifierWindow>();
        window.UpdateWindowTitle();
        window.minSize = new Vector2(540, 360);
        window.Show();
    }

    public void UpdateWindowTitle()
    {
        string tooltipText = GetText("Tooltip");
        titleContent = new GUIContent("Avatar Blendshape Utility", tooltipText);
    }

    // ---------- 多语言字典 ----------
    private string GetText(string key)
    {
        if (translations.TryGetValue(key, out string[] texts))
        {
            int idx = (int)currentLanguage;
            if (idx >= 0 && idx < texts.Length) return texts[idx];
        }
        return key;
    }

    private static readonly Dictionary<string, string[]> translations = new Dictionary<string, string[]>()
    {
        { "Tooltip", new[] { "[Guide & Shortcuts]\n• Ctrl + Z : Undo\n• Ctrl + Y / Ctrl + Shift + Z : Redo", "【操作ガイドとショートカット】\n• Ctrl + Z : 元に戻す\n• Ctrl + Y / Ctrl + Shift + Z : やり直し", "[조작 가이드 및 단축키]\n• Ctrl + Z : 실행 취소\n• Ctrl + Y / Ctrl + Shift + Z : 다시 실행", "【操作指南与快捷键】\n• Ctrl + Z : 撤回\n• Ctrl + Y / Ctrl + Shift + Z : 重做" } },
        { "GuideLabel", new[] { "Operation Guide", "操作ガイド", "작업 가이드", "操作指南" } },
        { "ResetOne", new[] { "0", "0", "0", "0" } },
        { "CollapsePreview", new[] { "Collapse preview", "プレビューを折りたたむ", "미리보기 접기", "收起预览窗口" } },
        { "ExpandPreview", new[] { "Expand preview", "プレビューを展開", "미리보기 펼치기", "展开预览窗口" } },
        { "TargetModelRoot", new[] { "Target Model Root:", "ターゲットモデル:", "대상 모델 루트:", "目标模型根节点:" } },
        { "Presets", new[] { "Blendshape Presets", "プリセット", "블렌드쉐이프 프리셋", "捏脸存档预设" } },
        { "NoPresets", new[] { "No Presets", "プリセットなし", "프리셋 없음", "暂无预设" } },
        { "Load", new[] { "Load", "ロード", "불러오기", "加载预设" } },
        { "Delete", new[] { "Delete", "削除", "삭제", "删除" } },
        { "NewPreset", new[] { "New Preset", "新規プリセット", "새 프리셋", "新捏脸预设" } },
        { "SavePreset", new[] { "Save Preset", "プリセット保存", "프리셋 저장", "保存新预设" } },
        { "Search", new[] { "Search:", "検索:", "검색:", "搜索:" } },
        { "Category", new[] { "Category ▾", "カテゴリ ▾", "카테고리 ▾", "分类 ▾" } },
        { "DragModel", new[] { "Please drag and drop the model root above", "上部にモデルのルートをドラッグしてください", "위쪽에 모델 루트를 드래그해 넣으세요", "请在上方拖入模型根节点" } },
        { "ResetAll", new[] { "Reset All", "全てリセット", "모두 초기화", "全部归零" } },
        { "ExportAnim", new[] { "Export as .anim", ".animとしてエクスポート", ".anim으로 내보내기", "导出当前表情为 .anim 动画文件" } },
        { "No3DPreview", new[] { "No 3D Preview", "3Dプレビューなし", "3D 미리보기 없음", "无 3D 预览视口" } },
        { "Front", new[] { "Front", "正面", "정면", "正面" } },
        { "45Deg", new[] { "45°", "45°", "45°", "45°" } },
        { "Side", new[] { "Side", "側面", "측면", "侧面" } },
        { "Back", new[] { "Back", "背面", "후면", "背面" } },
        { "SelectAll", new[] { "Select All", "全て選択", "모두 선택", "全选" } },
        { "DeselectAll", new[] { "Deselect All", "全て選択解除", "모두 선택 해제", "全不选" } },
        { "Unnamed", new[] { "Unnamed Preset", "無名プリセット", "이름 없는 프리셋", "未命名预设" } },
        { "Saved", new[] { "Saved preset: ", "保存しました: ", "저장된 프리셋: ", "已保存预设: " } },
        { "Loaded", new[] { "Loaded preset: ", "ロードしました: ", "불러온 프리셋: ", "已加载预设: " } },
        { "Info", new[] { "Info", "情報", "정보", "提示" } },
        { "NoModel", new[] { "Valid model not found, cannot export!", "有効なモデルが見つかりません。エクスポートできません！", "유효한 모델을 찾을 수 없어 내보낼 수 없습니다!", "未找到有效的模型对象，无法导出动画！" } },
        { "OK", new[] { "OK", "OK", "확인", "确定" } },
        { "ExportTitle", new[] { "Export Animation", "アニメーションをエクスポート", "애니메이션 내보내기", "导出表情动画" } },
        { "SelectPath", new[] { "Select save path", "保存先を選択", "저장 경로 선택", "选择保存动画文件的路径" } },
        { "AllZero", new[] { "All blendshapes are 0, exported animation has no motion.", "全ての数値が0です。アニメーションに動きがありません。", "모든 블렌드쉐이프가 0이므로 내보낸 애니메이션에 움직임이 없습니다.", "当前形态键数值全为 0，导出的动画文件没有任何动作。" } },
        { "ExportSuccess", new[] { "Export successful!", "エクスポート成功！", "내보내기 성공!", "动画导出成功！" } },
        { "ResetZero", new[] { "Reset all to 0", "全て0にリセットしました", "모두 0으로 초기화됨", "已全部重置为 0" } },
        { "ThemeLabel", new[] { "Theme", "テーマ", "테마", "主题" } },
        { "LangLabel", new[] { "Language", "言語", "언어", "语言" } },
        { "ThemeDark", new[] { "Dark", "ダーク", "다크", "暗色" } },
        { "ThemeLight", new[] { "Light", "ライト", "라이트", "浅色" } },
        { "LangEn", new[] { "English", "英語", "영어", "英语" } },
        { "LangJp", new[] { "Japanese", "日本語", "일본어", "日语" } },
        { "LangKr", new[] { "Korean", "한국어", "한국어", "한국어" } },
        { "LangCn", new[] { "Chinese", "中国語", "중국어", "中文" } }
    };

    // ---------- 数据结构与存档 ----------
    [SerializeField] private GameObject targetModel;
    [SerializeField] private SkinnedMeshRenderer faceSMR;
    private Mesh faceMesh;

    [SerializeField] private float[] blendShapeValues;
    private string[] blendShapeNames;
    private int blendShapeCount = 0;

    private class BlendShapeGroup
    {
        public string groupName;
        public List<int> indices = new List<int>();
        public bool isVisible = true;
    }
    private List<BlendShapeGroup> categories = new List<BlendShapeGroup>();

    private string searchFilter = "";
    private bool showOnlyModified = false;

    [Serializable]
    private class PresetData
    {
        public string presetName;
        public List<string> keys = new List<string>();
        public List<float> values = new List<float>();
    }

    [SerializeField] private List<PresetData> savedPresets = new List<PresetData>();
    private string newPresetName = "New Preset";
    private int selectedPresetIndex = 0;

    // 预览与摄像机
    private PreviewRenderUtility previewUtility;
    private GameObject previewInstance;
    private SkinnedMeshRenderer previewFaceSMR;
    private RenderTexture sceneViewRenderTexture;

    private Vector2 camRotation = new Vector2(0, 180f);
    private Vector3 camTargetPivot = Vector3.zero;
    private float camDistance = 0.5f;
    private Bounds previewFramingBounds;
    private Vector3 previewDetectedFaceCenter = Vector3.zero;
    private bool hasPreviewFramingBounds = false;
    private bool hasPreviewDetectedFaceCenter = false;
    private bool previewCameraNeedsFraming = true;
    private const float PreviewCameraFieldOfView = 30f;

    private int currentlyHoveredIndex = -1;
    private Vector2 scrollPos;
    private Vector2 lastMousePos;
    private Vector2 currentMouseScreenPosition;
    private Rect blendshapeViewportScreenRect;
    private bool hasBlendshapeViewportRect = false;
    private int activeBlendshapeSliderIndex = -1;
    private int activeBlendshapeTextInputIndex = -1;
    private int hoverSuppressedBlendshapeIndex = -1;
    private string[] blendShapeInputTexts;
    private Rect activeBlendshapeTextInputScreenRect;
    private bool hasActiveBlendshapeTextInputRect = false;
    private int previewDragButton = -1;

    // 布局拖拽调节
    [SerializeField] private float leftWidthRatio = 0.4f;
    [SerializeField] private bool isPreviewCollapsed = false;
    private bool isResizingSplitter = false;

    // 现代扁平化 UI 样式
    private GUIStyle labelWhiteStyle;
    private GUIStyle secondaryLabelStyle;
    private GUIStyle headerWhiteStyle;
    private GUIStyle titleWhiteStyle;
    private GUIStyle boxStyle;

    private GUIStyle customTextFieldStyle;
    private GUIStyle customPopupStyle;
    private GUIStyle customObjectFieldStyle;
    private GUIStyle cleanButtonStyle;
    private GUIStyle blendshapeStepButtonStyle;
    private GUIStyle accentButtonStyle;
    private GUIStyle guideLabelStyle;
    private GUIStyle groupHeaderStyle;
    private GUIStyle rowHoverStyle;
    private GUIStyle previewToolbarStyle;
    private GUIStyle customScrollViewStyle;

    private Texture2D lightInputBg;
    private Texture2D darkInputBg;
    private Texture2D lightInputFocusBg;
    private Texture2D darkInputFocusBg;
    private Texture2D lightButtonBg;
    private Texture2D darkButtonBg;
    private Texture2D lightButtonHoverBg;
    private Texture2D darkButtonHoverBg;
    private Texture2D lightButtonActiveBg;
    private Texture2D darkButtonActiveBg;
    private Texture2D lightBoxBg;
    private Texture2D darkBoxBg;
    private Texture2D lightAccentBg;
    private Texture2D darkAccentBg;
    private Texture2D lightAccentHoverBg;
    private Texture2D darkAccentHoverBg;
    private Texture2D lightAccentActiveBg;
    private Texture2D darkAccentActiveBg;
    private Texture2D lightSliderThumbBg;
    private Texture2D darkSliderThumbBg;
    private Texture2D lightSliderInactiveThumbBg;
    private Texture2D darkSliderInactiveThumbBg;
    private Texture2D lightGroupHeaderBg;
    private Texture2D darkGroupHeaderBg;
    private Texture2D lightRowHoverBg;
    private Texture2D darkRowHoverBg;
    private Texture2D lightPreviewToolbarBg;
    private Texture2D darkPreviewToolbarBg;

    // ---------- 生命周期 ----------
    private void OnEnable()
    {
        wantsMouseMove = true;
        wantsMouseEnterLeaveWindow = true;

        if (EditorPrefs.HasKey("VRCFaceModifier_Lang"))
        {
            int savedLanguage = EditorPrefs.GetInt("VRCFaceModifier_Lang");
            currentLanguage = Enum.IsDefined(typeof(LanguageOption), savedLanguage)
                ? (LanguageOption)savedLanguage
                : LanguageOption.English;
        }

        if (EditorPrefs.HasKey("VRCFaceModifier_Theme"))
        {
            int savedTheme = EditorPrefs.GetInt("VRCFaceModifier_Theme");
            currentTheme = Enum.IsDefined(typeof(ThemeOption), savedTheme)
                ? (ThemeOption)savedTheme
                : ThemeOption.Dark;
        }

        isPreviewCollapsed = EditorPrefs.GetBool("VRCFaceModifier_PreviewCollapsed", false);
        UpdateWindowTitle();
        newPresetName = GetText("NewPreset");

        InitPreviewUtility();
        LoadPresetsFromEditorPrefs();
        Undo.undoRedoPerformed += OnUndoRedoPerformed;

        if (targetModel != null)
        {
            OnTargetModelChanged();
        }
    }

    private void OnDisable()
    {
        Undo.undoRedoPerformed -= OnUndoRedoPerformed;
        SavePresetsToEditorPrefs();
        CleanPreviewUtility();
        CleanPreviewInstance();
        CleanSceneViewRenderTexture();
        DestroyThemeTextures();
    }

    private void CleanSceneViewRenderTexture()
    {
        if (sceneViewRenderTexture == null) return;

        sceneViewRenderTexture.Release();
        DestroyImmediate(sceneViewRenderTexture);
        sceneViewRenderTexture = null;
    }

    private void EnsureSceneViewRenderTexture(int width, int height)
    {
        width = Mathf.Max(1, width);
        height = Mathf.Max(1, height);

        if (sceneViewRenderTexture != null &&
            sceneViewRenderTexture.width == width &&
            sceneViewRenderTexture.height == height)
        {
            return;
        }

        CleanSceneViewRenderTexture();

        sceneViewRenderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32)
        {
            name = "Avatar Blendshape Scene View Mirror",
            hideFlags = HideFlags.HideAndDontSave,
            antiAliasing = 1,
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };
        sceneViewRenderTexture.Create();
    }

    private Texture2D MakeBorderTex(Color fillColor, Color borderColor)
    {
        const int textureSize = 16;
        const float radius = 5.5f;

        Texture2D result = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false)
        {
            hideFlags = HideFlags.HideAndDontSave,
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float nearestX = Mathf.Clamp(x + 0.5f, radius, textureSize - radius);
                float nearestY = Mathf.Clamp(y + 0.5f, radius, textureSize - radius);
                float distance = Vector2.Distance(
                    new Vector2(x + 0.5f, y + 0.5f),
                    new Vector2(nearestX, nearestY)
                );

                float outerCoverage = Mathf.Clamp01(radius - distance + 0.5f);
                float innerCoverage = Mathf.Clamp01(radius - 1f - distance + 0.5f);
                Color pixelColor = Color.Lerp(borderColor, fillColor, innerCoverage);
                pixelColor.a *= outerCoverage;
                result.SetPixel(x, y, pixelColor);
            }
        }

        result.Apply();
        return result;
    }

    private Texture2D MakeCircleTex(Color fillColor, Color borderColor)
    {
        const int textureSize = 16;
        const float radius = 7f;
        Vector2 center = new Vector2(textureSize * 0.5f, textureSize * 0.5f);

        Texture2D result = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false)
        {
            hideFlags = HideFlags.HideAndDontSave,
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float distance = Vector2.Distance(
                    new Vector2(x + 0.5f, y + 0.5f),
                    center
                );

                float outerCoverage = Mathf.Clamp01(radius - distance + 0.5f);
                float innerCoverage = Mathf.Clamp01(radius - 1f - distance + 0.5f);
                Color pixelColor = Color.Lerp(borderColor, fillColor, innerCoverage);
                pixelColor.a *= outerCoverage;
                result.SetPixel(x, y, pixelColor);
            }
        }

        result.Apply();
        return result;
    }

    private void DestroyThemeTextures()
    {
        DestroyTexture(ref lightInputBg);
        DestroyTexture(ref darkInputBg);
        DestroyTexture(ref lightInputFocusBg);
        DestroyTexture(ref darkInputFocusBg);
        DestroyTexture(ref lightButtonBg);
        DestroyTexture(ref darkButtonBg);
        DestroyTexture(ref lightButtonHoverBg);
        DestroyTexture(ref darkButtonHoverBg);
        DestroyTexture(ref lightButtonActiveBg);
        DestroyTexture(ref darkButtonActiveBg);
        DestroyTexture(ref lightBoxBg);
        DestroyTexture(ref darkBoxBg);
        DestroyTexture(ref lightAccentBg);
        DestroyTexture(ref darkAccentBg);
        DestroyTexture(ref lightAccentHoverBg);
        DestroyTexture(ref darkAccentHoverBg);
        DestroyTexture(ref lightAccentActiveBg);
        DestroyTexture(ref darkAccentActiveBg);
        DestroyTexture(ref lightSliderThumbBg);
        DestroyTexture(ref darkSliderThumbBg);
        DestroyTexture(ref lightSliderInactiveThumbBg);
        DestroyTexture(ref darkSliderInactiveThumbBg);
        DestroyTexture(ref lightGroupHeaderBg);
        DestroyTexture(ref darkGroupHeaderBg);
        DestroyTexture(ref lightRowHoverBg);
        DestroyTexture(ref darkRowHoverBg);
        DestroyTexture(ref lightPreviewToolbarBg);
        DestroyTexture(ref darkPreviewToolbarBg);
    }

    private void DestroyTexture(ref Texture2D texture)
    {
        if (texture == null) return;

        DestroyImmediate(texture);
        texture = null;
    }

    private void InitStyles()
    {
        bool isDark = currentTheme == ThemeOption.Dark;

        Color textMainColor = isDark
            ? new Color(0.95f, 0.96f, 0.98f, 1f)
            : new Color(0.075f, 0.09f, 0.12f, 1f);

        Color textSecondaryColor = isDark
            ? new Color(0.58f, 0.62f, 0.70f, 1f)
            : new Color(0.38f, 0.42f, 0.49f, 1f);

        Color accentColor = isDark
            ? new Color(0.30f, 0.58f, 1f, 1f)
            : new Color(0.10f, 0.36f, 0.88f, 1f);

        Color lightBorder = new Color(0.82f, 0.85f, 0.90f, 1f);
        Color darkBorder = new Color(0.13f, 0.15f, 0.19f, 1f);
        Color lightControlBorder = new Color(0.75f, 0.79f, 0.85f, 1f);
        Color darkControlBorder = new Color(0.19f, 0.22f, 0.28f, 1f);

        labelWhiteStyle = new GUIStyle(EditorStyles.label)
        {
            fontSize = 12,
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.MiddleLeft,
            normal = { textColor = textMainColor },
            active = { textColor = textMainColor },
            focused = { textColor = textMainColor },
            hover = { textColor = textMainColor }
        };

        secondaryLabelStyle = new GUIStyle(labelWhiteStyle)
        {
            fontSize = 11,
            normal = { textColor = textSecondaryColor },
            active = { textColor = textSecondaryColor },
            focused = { textColor = textSecondaryColor },
            hover = { textColor = textSecondaryColor }
        };

        headerWhiteStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 11,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            normal = { textColor = accentColor },
            active = { textColor = accentColor },
            focused = { textColor = accentColor },
            hover = { textColor = accentColor }
        };

        titleWhiteStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            normal = { textColor = textMainColor },
            active = { textColor = textMainColor },
            focused = { textColor = textMainColor },
            hover = { textColor = textMainColor }
        };

        if (lightInputBg == null)
            lightInputBg = MakeBorderTex(new Color(0.985f, 0.99f, 1f, 1f), lightControlBorder);
        if (darkInputBg == null)
            darkInputBg = MakeBorderTex(new Color(0.055f, 0.065f, 0.085f, 1f), darkControlBorder);
        if (lightInputFocusBg == null)
            lightInputFocusBg = MakeBorderTex(new Color(1f, 1f, 1f, 1f), new Color(0.18f, 0.45f, 0.95f, 1f));
        if (darkInputFocusBg == null)
            darkInputFocusBg = MakeBorderTex(new Color(0.07f, 0.085f, 0.12f, 1f), new Color(0.28f, 0.57f, 1f, 1f));

        if (lightButtonBg == null)
            lightButtonBg = MakeBorderTex(new Color(0.965f, 0.975f, 0.99f, 1f), lightControlBorder);
        if (darkButtonBg == null)
            darkButtonBg = MakeBorderTex(new Color(0.075f, 0.085f, 0.11f, 1f), darkControlBorder);
        if (lightButtonHoverBg == null)
            lightButtonHoverBg = MakeBorderTex(new Color(0.91f, 0.94f, 0.98f, 1f), new Color(0.57f, 0.67f, 0.82f, 1f));
        if (darkButtonHoverBg == null)
            darkButtonHoverBg = MakeBorderTex(new Color(0.105f, 0.125f, 0.17f, 1f), new Color(0.27f, 0.34f, 0.47f, 1f));
        if (lightButtonActiveBg == null)
            lightButtonActiveBg = MakeBorderTex(new Color(0.84f, 0.90f, 0.99f, 1f), new Color(0.24f, 0.49f, 0.94f, 1f));
        if (darkButtonActiveBg == null)
            darkButtonActiveBg = MakeBorderTex(new Color(0.10f, 0.20f, 0.38f, 1f), new Color(0.31f, 0.59f, 1f, 1f));

        if (lightBoxBg == null)
            lightBoxBg = MakeBorderTex(new Color(1f, 1f, 1f, 1f), lightBorder);
        if (darkBoxBg == null)
            darkBoxBg = MakeBorderTex(new Color(0.035f, 0.04f, 0.052f, 1f), darkBorder);

        if (lightAccentBg == null)
            lightAccentBg = MakeBorderTex(new Color(0.10f, 0.36f, 0.88f, 1f), new Color(0.10f, 0.36f, 0.88f, 1f));
        if (darkAccentBg == null)
            darkAccentBg = MakeBorderTex(new Color(0.20f, 0.45f, 0.94f, 1f), new Color(0.28f, 0.56f, 1f, 1f));
        if (lightAccentHoverBg == null)
            lightAccentHoverBg = MakeBorderTex(new Color(0.075f, 0.30f, 0.78f, 1f), new Color(0.075f, 0.30f, 0.78f, 1f));
        if (darkAccentHoverBg == null)
            darkAccentHoverBg = MakeBorderTex(new Color(0.25f, 0.52f, 1f, 1f), new Color(0.35f, 0.64f, 1f, 1f));
        if (lightAccentActiveBg == null)
            lightAccentActiveBg = MakeBorderTex(new Color(0.055f, 0.23f, 0.64f, 1f), new Color(0.055f, 0.23f, 0.64f, 1f));
        if (darkAccentActiveBg == null)
            darkAccentActiveBg = MakeBorderTex(new Color(0.12f, 0.32f, 0.76f, 1f), new Color(0.22f, 0.48f, 0.95f, 1f));

        if (lightSliderThumbBg == null)
            lightSliderThumbBg = MakeCircleTex(new Color(0.97f, 0.98f, 1f, 1f), new Color(0.10f, 0.36f, 0.88f, 1f));
        if (darkSliderThumbBg == null)
            darkSliderThumbBg = MakeCircleTex(new Color(0.82f, 0.85f, 0.90f, 1f), new Color(0.30f, 0.58f, 1f, 1f));
        if (lightSliderInactiveThumbBg == null)
            lightSliderInactiveThumbBg = MakeCircleTex(new Color(0.66f, 0.69f, 0.74f, 1f), new Color(0.66f, 0.69f, 0.74f, 1f));
        if (darkSliderInactiveThumbBg == null)
            darkSliderInactiveThumbBg = MakeCircleTex(new Color(0.58f, 0.60f, 0.64f, 1f), new Color(0.58f, 0.60f, 0.64f, 1f));

        if (lightGroupHeaderBg == null)
            lightGroupHeaderBg = MakeBorderTex(new Color(0.94f, 0.955f, 0.98f, 1f), new Color(0.82f, 0.85f, 0.90f, 1f));
        if (darkGroupHeaderBg == null)
            darkGroupHeaderBg = MakeBorderTex(new Color(0.055f, 0.065f, 0.085f, 1f), new Color(0.13f, 0.15f, 0.19f, 1f));

        if (lightRowHoverBg == null)
            lightRowHoverBg = MakeBorderTex(new Color(0.10f, 0.36f, 0.88f, 0.08f), new Color(0.10f, 0.36f, 0.88f, 0.12f));
        if (darkRowHoverBg == null)
            darkRowHoverBg = MakeBorderTex(new Color(0.18f, 0.39f, 0.82f, 0.20f), new Color(0.30f, 0.58f, 1f, 0.28f));

        if (lightPreviewToolbarBg == null)
            lightPreviewToolbarBg = MakeBorderTex(new Color(1f, 1f, 1f, 0.92f), new Color(0.78f, 0.82f, 0.88f, 0.95f));
        if (darkPreviewToolbarBg == null)
            darkPreviewToolbarBg = MakeBorderTex(new Color(0.025f, 0.03f, 0.04f, 0.92f), new Color(0.13f, 0.15f, 0.19f, 0.95f));

        Texture2D currentInputBg = isDark ? darkInputBg : lightInputBg;
        Texture2D currentInputFocusBg = isDark ? darkInputFocusBg : lightInputFocusBg;
        Texture2D currentButtonBg = isDark ? darkButtonBg : lightButtonBg;
        Texture2D currentButtonHoverBg = isDark ? darkButtonHoverBg : lightButtonHoverBg;
        Texture2D currentButtonActiveBg = isDark ? darkButtonActiveBg : lightButtonActiveBg;
        Texture2D currentBoxBg = isDark ? darkBoxBg : lightBoxBg;
        Texture2D currentAccentBg = isDark ? darkAccentBg : lightAccentBg;
        Texture2D currentAccentHoverBg = isDark ? darkAccentHoverBg : lightAccentHoverBg;
        Texture2D currentAccentActiveBg = isDark ? darkAccentActiveBg : lightAccentActiveBg;
        Texture2D currentGroupHeaderBg = isDark ? darkGroupHeaderBg : lightGroupHeaderBg;
        Texture2D currentRowHoverBg = isDark ? darkRowHoverBg : lightRowHoverBg;
        Texture2D currentPreviewToolbarBg = isDark ? darkPreviewToolbarBg : lightPreviewToolbarBg;
        Color inputTextColor = textMainColor;

        boxStyle = new GUIStyle(GUI.skin.box)
        {
            border = new RectOffset(6, 6, 6, 6),
            padding = new RectOffset(12, 12, 10, 10),
            margin = new RectOffset(0, 0, 0, 8),
            normal = { background = currentBoxBg, textColor = textMainColor },
            focused = { background = currentBoxBg, textColor = textMainColor },
            active = { background = currentBoxBg, textColor = textMainColor },
            hover = { background = currentBoxBg, textColor = textMainColor }
        };

        customTextFieldStyle = new GUIStyle(EditorStyles.textField)
        {
            border = new RectOffset(6, 6, 6, 6),
            padding = new RectOffset(8, 8, 3, 3),
            alignment = TextAnchor.MiddleLeft,
            normal = { background = currentInputBg, textColor = inputTextColor },
            focused = { background = currentInputFocusBg, textColor = inputTextColor },
            active = { background = currentInputFocusBg, textColor = inputTextColor },
            hover = { background = currentInputBg, textColor = inputTextColor }
        };

        customPopupStyle = new GUIStyle(EditorStyles.popup)
        {
            border = new RectOffset(6, 6, 6, 6),
            padding = new RectOffset(9, 9, 3, 3),
            alignment = TextAnchor.MiddleCenter,
            fontSize = 11,
            normal = { background = currentInputBg, textColor = inputTextColor },
            focused = { background = currentInputFocusBg, textColor = inputTextColor },
            active = { background = currentButtonActiveBg, textColor = inputTextColor },
            hover = { background = currentButtonHoverBg, textColor = inputTextColor }
        };

        customObjectFieldStyle = new GUIStyle(EditorStyles.objectField)
        {
            border = new RectOffset(6, 6, 6, 6),
            padding = new RectOffset(7, 5, 2, 2),
            normal = { background = currentInputBg, textColor = inputTextColor },
            focused = { background = currentInputFocusBg, textColor = inputTextColor },
            active = { background = currentInputFocusBg, textColor = inputTextColor },
            hover = { background = currentInputBg, textColor = inputTextColor }
        };

        cleanButtonStyle = new GUIStyle(GUI.skin.button)
        {
            border = new RectOffset(6, 6, 6, 6),
            padding = new RectOffset(9, 9, 3, 3),
            margin = new RectOffset(0, 0, 0, 0),
            alignment = TextAnchor.MiddleCenter,
            fontSize = 11,
            fontStyle = FontStyle.Normal,
            normal = { background = currentButtonBg, textColor = inputTextColor },
            focused = { background = currentButtonHoverBg, textColor = inputTextColor },
            active = { background = currentButtonActiveBg, textColor = inputTextColor },
            hover = { background = currentButtonHoverBg, textColor = inputTextColor }
        };

        blendshapeStepButtonStyle = new GUIStyle(cleanButtonStyle)
        {
            padding = new RectOffset(0, 0, 0, 0),
            alignment = TextAnchor.MiddleCenter,
            fontSize = 12,
            fontStyle = FontStyle.Bold
        };

        accentButtonStyle = new GUIStyle(cleanButtonStyle)
        {
            fontStyle = FontStyle.Bold,
            normal = { background = currentAccentBg, textColor = Color.white },
            focused = { background = currentAccentHoverBg, textColor = Color.white },
            active = { background = currentAccentActiveBg, textColor = Color.white },
            hover = { background = currentAccentHoverBg, textColor = Color.white }
        };

        guideLabelStyle = new GUIStyle(cleanButtonStyle)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            normal = { background = currentButtonBg, textColor = accentColor },
            focused = { background = currentButtonHoverBg, textColor = accentColor },
            active = { background = currentButtonActiveBg, textColor = accentColor },
            hover = { background = currentButtonHoverBg, textColor = accentColor }
        };

        customScrollViewStyle = new GUIStyle(GUI.skin.scrollView)
        {
            border = new RectOffset(6, 6, 6, 6),
            padding = new RectOffset(10, 10, 8, 8),
            margin = new RectOffset(0, 0, 0, 8),
            normal = { background = currentBoxBg },
            focused = { background = currentBoxBg },
            active = { background = currentBoxBg },
            hover = { background = currentBoxBg }
        };

        groupHeaderStyle = new GUIStyle(GUI.skin.box)
        {
            border = new RectOffset(6, 6, 6, 6),
            padding = new RectOffset(10, 10, 3, 3),
            alignment = TextAnchor.MiddleLeft,
            normal = { background = currentGroupHeaderBg, textColor = accentColor },
            focused = { background = currentGroupHeaderBg, textColor = accentColor },
            active = { background = currentGroupHeaderBg, textColor = accentColor },
            hover = { background = currentGroupHeaderBg, textColor = accentColor }
        };

        rowHoverStyle = new GUIStyle(GUI.skin.box)
        {
            border = new RectOffset(6, 6, 6, 6),
            normal = { background = currentRowHoverBg },
            focused = { background = currentRowHoverBg },
            active = { background = currentRowHoverBg },
            hover = { background = currentRowHoverBg }
        };

        previewToolbarStyle = new GUIStyle(GUI.skin.box)
        {
            border = new RectOffset(6, 6, 6, 6),
            normal = { background = currentPreviewToolbarBg },
            focused = { background = currentPreviewToolbarBg },
            active = { background = currentPreviewToolbarBg },
            hover = { background = currentPreviewToolbarBg }
        };
    }

    // ---------- 核心绘制 ----------
    private void OnGUI()
    {
        InitStyles();
        HandleKeyboardEvents();

        Event currentEvent = Event.current;
        currentMouseScreenPosition = GUIUtility.GUIToScreenPoint(currentEvent.mousePosition);

        if (currentEvent.type == EventType.MouseDown &&
            activeBlendshapeTextInputIndex >= 0 &&
            hasActiveBlendshapeTextInputRect &&
            !activeBlendshapeTextInputScreenRect.Contains(
                currentMouseScreenPosition
            ))
        {
            FinalizeBlendshapeTextInput(
                activeBlendshapeTextInputIndex
            );
        }

        if (currentEvent.type == EventType.MouseLeaveWindow)
        {
            if (activeBlendshapeTextInputIndex >= 0)
            {
                FinalizeBlendshapeTextInput(
                    activeBlendshapeTextInputIndex
                );
            }

            activeBlendshapeSliderIndex = -1;
            activeBlendshapeTextInputIndex = -1;
            hasActiveBlendshapeTextInputRect = false;
            hoverSuppressedBlendshapeIndex = -1;
            previewDragButton = -1;
            GUIUtility.hotControl = 0;

            if (currentlyHoveredIndex != -1)
            {
                currentlyHoveredIndex = -1;
                ApplyWeightsToPreviewAndScene();
                Repaint();
            }
        }

        Color windowBg = currentTheme == ThemeOption.Dark
            ? Color.black
            : new Color(0.945f, 0.955f, 0.975f, 1f);

        EditorGUI.DrawRect(
            new Rect(0, 0, position.width, position.height),
            windowBg
        );

        float windowWidth = position.width;
        float windowHeight = position.height;

        float minLeftWidth = 200f;
        float minRightWidth = 320f;
        float currentLeftWidth = Mathf.Clamp(
            windowWidth * leftWidthRatio,
            minLeftWidth,
            Mathf.Max(minLeftWidth, windowWidth - minRightWidth)
        );

        EditorGUILayout.BeginHorizontal();

        Event e = Event.current;
        Rect previewAreaRect = Rect.zero;

        if (!isPreviewCollapsed)
        {
            previewAreaRect = GUILayoutUtility.GetRect(
                currentLeftWidth,
                windowHeight,
                GUILayout.ExpandWidth(false),
                GUILayout.ExpandHeight(true)
            );

            Draw3DPreviewViewport(previewAreaRect);
        }

        Rect previewToggleRailRect = GUILayoutUtility.GetRect(
            30f,
            windowHeight,
            GUILayout.ExpandWidth(false),
            GUILayout.ExpandHeight(true)
        );

        Color previewToggleRailColor = currentTheme == ThemeOption.Dark
            ? new Color(0.025f, 0.03f, 0.04f, 1f)
            : new Color(0.90f, 0.92f, 0.95f, 1f);

        EditorGUI.DrawRect(previewToggleRailRect, previewToggleRailColor);

        Rect previewToggleButtonRect = new Rect(
            previewToggleRailRect.x + 4f,
            previewToggleRailRect.y + 10f,
            22f,
            30f
        );

        string previewToggleTooltip = isPreviewCollapsed
            ? GetText("ExpandPreview")
            : GetText("CollapsePreview");

        if (GUI.Button(
            previewToggleButtonRect,
            new GUIContent(
                isPreviewCollapsed ? ">" : "<",
                previewToggleTooltip
            ),
            cleanButtonStyle
        ))
        {
            isPreviewCollapsed = !isPreviewCollapsed;
            isResizingSplitter = false;
            EditorPrefs.SetBool(
                "VRCFaceModifier_PreviewCollapsed",
                isPreviewCollapsed
            );

            Repaint();
            GUIUtility.ExitGUI();
        }

        if (!isPreviewCollapsed)
        {
            Rect splitterRect = new Rect(
                previewAreaRect.xMax,
                0,
                5,
                windowHeight
            );

            Color splitterColor = currentTheme == ThemeOption.Dark
                ? new Color(0.075f, 0.085f, 0.11f, 1f)
                : new Color(0.82f, 0.85f, 0.90f, 1f);

            EditorGUI.DrawRect(splitterRect, splitterColor);

            Color splitterAccent = currentTheme == ThemeOption.Dark
                ? new Color(0.24f, 0.49f, 0.96f, 0.70f)
                : new Color(0.10f, 0.36f, 0.88f, 0.45f);

            EditorGUI.DrawRect(
                new Rect(splitterRect.x + 2f, 0, 1f, windowHeight),
                splitterAccent
            );

            EditorGUIUtility.AddCursorRect(
                splitterRect,
                MouseCursor.ResizeHorizontal
            );

            if (e.type == EventType.MouseDown &&
                splitterRect.Contains(e.mousePosition))
            {
                isResizingSplitter = true;
                e.Use();
            }

            if (isResizingSplitter)
            {
                if (e.type == EventType.MouseDrag)
                {
                    leftWidthRatio = Mathf.Clamp(
                        e.mousePosition.x / windowWidth,
                        minLeftWidth / windowWidth,
                        (windowWidth - minRightWidth) / windowWidth
                    );

                    Repaint();
                    e.Use();
                }
                else if (e.type == EventType.MouseUp)
                {
                    isResizingSplitter = false;
                    e.Use();
                }
            }
        }

        EditorGUILayout.BeginVertical(
            GUILayout.ExpandWidth(true),
            GUILayout.ExpandHeight(true)
        );

        GUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(12);

        EditorGUILayout.BeginVertical(
            GUILayout.ExpandWidth(true),
            GUILayout.ExpandHeight(true)
        );

        EditorGUILayout.BeginHorizontal();

        GUILayout.Label(
            new GUIContent(
                GetText("GuideLabel"),
                GetText("Tooltip")
            ),
            guideLabelStyle,
            GUILayout.Width(104),
            GUILayout.Height(24)
        );

        GUILayout.FlexibleSpace();

        EditorGUI.BeginChangeCheck();

        string[] themeOptions =
        {
            GetText("ThemeDark"),
            GetText("ThemeLight")
        };

        currentTheme = (ThemeOption)EditorGUILayout.Popup(
            (int)currentTheme,
            themeOptions,
            customPopupStyle,
            GUILayout.Width(80),
            GUILayout.Height(24)
        );

        if (EditorGUI.EndChangeCheck())
        {
            EditorPrefs.SetInt(
                "VRCFaceModifier_Theme",
                (int)currentTheme
            );

            UpdatePreviewBackgroundColor();
            GUI.FocusControl(null);
        }

        GUILayout.Space(6);
        EditorGUI.BeginChangeCheck();

        string[] langOptions =
        {
            GetText("LangEn"),
            GetText("LangJp"),
            GetText("LangKr"),
            GetText("LangCn")
        };

        currentLanguage = (LanguageOption)EditorGUILayout.Popup(
            (int)currentLanguage,
            langOptions,
            customPopupStyle,
            GUILayout.Width(88),
            GUILayout.Height(24)
        );

        if (EditorGUI.EndChangeCheck())
        {
            EditorPrefs.SetInt(
                "VRCFaceModifier_Lang",
                (int)currentLanguage
            );

            UpdateWindowTitle();

            if (newPresetName == "新捏脸预设" ||
                newPresetName == "New Preset" ||
                newPresetName == "新規プリセット" ||
                newPresetName == "새 프리셋")
            {
                newPresetName = GetText("NewPreset");
            }

            GUI.FocusControl(null);
        }

        EditorGUILayout.EndHorizontal();
        GUILayout.Space(8);

        EditorGUILayout.BeginVertical(boxStyle);
        EditorGUILayout.BeginHorizontal();

        GUILayout.Label(
            GetText("TargetModelRoot"),
            titleWhiteStyle,
            GUILayout.Width(142),
            GUILayout.Height(22)
        );

        EditorGUI.BeginChangeCheck();

        Rect objectFieldRect = EditorGUILayout.GetControlRect(
            false,
            22f,
            GUILayout.ExpandWidth(true)
        );

        Color previousBackgroundColor = GUI.backgroundColor;
        Color previousContentColor = GUI.contentColor;

        GUI.Box(
            objectFieldRect,
            GUIContent.none,
            customObjectFieldStyle
        );

        GUI.backgroundColor = new Color(1f, 1f, 1f, 0f);
        GUI.contentColor = currentTheme == ThemeOption.Dark
            ? new Color(0.95f, 0.96f, 0.98f, 1f)
            : new Color(0.075f, 0.09f, 0.12f, 1f);

        GameObject newTarget = (GameObject)EditorGUI.ObjectField(
            objectFieldRect,
            targetModel,
            typeof(GameObject),
            true
        );

        GUI.backgroundColor = previousBackgroundColor;
        GUI.contentColor = previousContentColor;

        if (EditorGUI.EndChangeCheck())
        {
            targetModel = newTarget;
            OnTargetModelChanged();
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(boxStyle);

        GUILayout.Label(
            GetText("Presets"),
            titleWhiteStyle,
            GUILayout.Height(20)
        );

        GUILayout.Space(2);
        EditorGUILayout.BeginHorizontal();

        string[] presetArray = savedPresets
            .Select(p => p.presetName)
            .ToArray();

        if (presetArray.Length == 0)
        {
            presetArray = new[] { GetText("NoPresets") };
        }

        selectedPresetIndex = EditorGUILayout.Popup(
            selectedPresetIndex,
            presetArray,
            customPopupStyle,
            GUILayout.Height(24)
        );

        GUILayout.Space(6);

        if (GUILayout.Button(
            GetText("Load"),
            cleanButtonStyle,
            GUILayout.Width(70),
            GUILayout.Height(24)
        ))
        {
            LoadSelectedPreset();
        }

        GUILayout.Space(6);

        if (GUILayout.Button(
            GetText("Delete"),
            cleanButtonStyle,
            GUILayout.Width(60),
            GUILayout.Height(24)
        ))
        {
            DeleteSelectedPreset();
        }

        EditorGUILayout.EndHorizontal();
        GUILayout.Space(6);
        EditorGUILayout.BeginHorizontal();

        newPresetName = EditorGUILayout.TextField(
            newPresetName,
            customTextFieldStyle,
            GUILayout.Height(24)
        );

        GUILayout.Space(6);

        if (GUILayout.Button(
            GetText("SavePreset"),
            cleanButtonStyle,
            GUILayout.Width(104),
            GUILayout.Height(24)
        ))
        {
            SaveCurrentAsPreset(newPresetName);
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginHorizontal(boxStyle);

        GUILayout.Label(
            GetText("Search"),
            secondaryLabelStyle,
            GUILayout.Width(48),
            GUILayout.Height(24)
        );

        searchFilter = EditorGUILayout.TextField(
            searchFilter,
            customTextFieldStyle,
            GUILayout.Height(24)
        );

        GUILayout.Space(6);

        if (GUILayout.Button(
            GetText("Category"),
            customPopupStyle,
            GUILayout.Width(84),
            GUILayout.Height(24)
        ))
        {
            ShowCategoryMenu();
        }

        GUILayout.Space(6);

        GUIStyle modifiedFilterStyle = showOnlyModified
            ? accentButtonStyle
            : cleanButtonStyle;

        if (GUILayout.Button(
            "0",
            modifiedFilterStyle,
            GUILayout.Width(32),
            GUILayout.Height(24)
        ))
        {
            showOnlyModified = !showOnlyModified;
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginVertical(
            GUILayout.ExpandWidth(true),
            GUILayout.ExpandHeight(true)
        );

        scrollPos = EditorGUILayout.BeginScrollView(
            scrollPos,
            customScrollViewStyle,
            GUILayout.ExpandWidth(true),
            GUILayout.ExpandHeight(true)
        );

        if (faceSMR == null || blendShapeCount == 0)
        {
            GUILayout.Space(22);
            GUILayout.Label(
                GetText("DragModel"),
                secondaryLabelStyle
            );
        }
        else
        {
            DrawBlendshapeList();
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        if (Event.current.type == EventType.Repaint)
        {
            Rect blendshapeViewportRect =
                GUILayoutUtility.GetLastRect();

            blendshapeViewportScreenRect =
                GUIUtility.GUIToScreenRect(blendshapeViewportRect);

            hasBlendshapeViewportRect =
                blendshapeViewportRect.width > 1f &&
                blendshapeViewportRect.height > 1f;
        }

        EditorGUILayout.BeginHorizontal(boxStyle);

        if (GUILayout.Button(
            GetText("ResetAll"),
            cleanButtonStyle,
            GUILayout.Width(106),
            GUILayout.Height(30)
        ))
        {
            ResetAllBlendShapesToZero();
        }

        GUILayout.Space(8);

        if (GUILayout.Button(
            GetText("ExportAnim"),
            accentButtonStyle,
            GUILayout.Height(30)
        ))
        {
            ExportCurrentAsAnimationClip();
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        GUILayout.Space(12);
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(2);
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        if (previewUtility != null)
        {
            Repaint();
        }
    }

    private string GetBlendshapeInputControlName(int index)
    {
        return "AvatarBlendshapeValueInput_" +
               index.ToString(
                   CultureInfo.InvariantCulture
               );
    }

    private string FormatBlendshapeInputValue(float value)
    {
        return Mathf.Clamp(value, 0f, 100f)
            .ToString(
                "0.0",
                CultureInfo.InvariantCulture
            );
    }

    private bool TryParseBlendshapeInputValue(
        string text,
        out float value)
    {
        value = 0f;

        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        string normalizedText =
            text.Trim().Replace(',', '.');

        return float.TryParse(
            normalizedText,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out value
        );
    }

    private void EnsureBlendshapeInputTexts()
    {
        if (blendShapeCount <= 0 ||
            blendShapeValues == null)
        {
            blendShapeInputTexts = null;
            activeBlendshapeTextInputIndex = -1;
            return;
        }

        if (blendShapeInputTexts == null ||
            blendShapeInputTexts.Length !=
            blendShapeCount)
        {
            blendShapeInputTexts =
                new string[blendShapeCount];
        }

        for (int index = 0;
             index < blendShapeCount;
             index++)
        {
            if (string.IsNullOrEmpty(
                blendShapeInputTexts[index]
            ))
            {
                blendShapeInputTexts[index] =
                    FormatBlendshapeInputValue(
                        blendShapeValues[index]
                    );
            }
        }
    }

    private void RefreshBlendshapeInputTextsFromValues()
    {
        EnsureBlendshapeInputTexts();

        if (blendShapeInputTexts == null ||
            blendShapeValues == null)
        {
            return;
        }

        int valueCount = Mathf.Min(
            blendShapeInputTexts.Length,
            blendShapeValues.Length
        );

        for (int index = 0;
             index < valueCount;
             index++)
        {
            blendShapeInputTexts[index] =
                FormatBlendshapeInputValue(
                    blendShapeValues[index]
                );
        }
    }

    private void FinalizeBlendshapeTextInput(int index)
    {
        if (blendShapeInputTexts != null &&
            blendShapeValues != null &&
            index >= 0 &&
            index < blendShapeInputTexts.Length &&
            index < blendShapeValues.Length)
        {
            // Invalid or unfinished text is restored to the last valid value.
            // Valid input has already been applied in real time while typing.
            blendShapeInputTexts[index] =
                FormatBlendshapeInputValue(
                    blendShapeValues[index]
                );
        }

        GUI.FocusControl(null);
        activeBlendshapeTextInputIndex = -1;
        hasActiveBlendshapeTextInputRect = false;
    }

    private float StepBlendshapeValue(
        float value,
        float step)
    {
        return NormalizeBlendshapeValue(
            value + step
        );
    }

    private float NormalizeBlendshapeValue(float value)
    {
        return Mathf.Clamp(
            Mathf.Round(
                value * 10f
            ) / 10f,
            0f,
            100f
        );
    }

    private void DrawBlendshapeList()
    {
        EnsureBlendshapeInputTexts();

        EventType currentEventType = Event.current.type;

        bool shouldUpdateHover =
            currentEventType == EventType.MouseMove ||
            currentEventType == EventType.MouseDown ||
            currentEventType == EventType.MouseDrag ||
            currentEventType == EventType.MouseUp ||
            currentEventType == EventType.ScrollWheel;

        int newHoveredIndex = -1;

        foreach (var group in categories)
        {
            if (!group.isVisible) continue;
            if (showOnlyModified &&
                !GroupHasModifiedShapes(group)) continue;

            GUILayout.Space(8);

            Rect groupHeaderRect =
                EditorGUILayout.GetControlRect(false, 24f);

            GUI.Box(
                groupHeaderRect,
                GUIContent.none,
                groupHeaderStyle
            );

            EditorGUI.LabelField(
                new Rect(
                    groupHeaderRect.x + 10f,
                    groupHeaderRect.y,
                    groupHeaderRect.width - 10f,
                    groupHeaderRect.height
                ),
                $"▼  {group.groupName}",
                headerWhiteStyle
            );

            foreach (int index in group.indices)
            {
                string shapeName = blendShapeNames[index];
                float val = blendShapeValues[index];

                if (!string.IsNullOrEmpty(searchFilter) &&
                    !shapeName.ToLower().Contains(
                        searchFilter.ToLower()
                    ))
                {
                    continue;
                }

                if (showOnlyModified &&
                    Mathf.Approximately(val, 0f))
                {
                    continue;
                }

                EditorGUILayout.BeginHorizontal();

                Rect rowRect =
                    EditorGUILayout.GetControlRect(false, 26f);

                float numericFieldX =
                    rowRect.xMax - 84f;

                Rect numericFieldRect = new Rect(
                    numericFieldX,
                    rowRect.y + 2f,
                    48f,
                    22f
                );

                float sliderControlsRight =
                    numericFieldX - 6f;

                float stepButtonWidth =
                    rowRect.width < 280f
                        ? 18f
                        : 20f;

                // DrawBlendshapeSlider expands its mouse hit area by 4 px.
                // Keep the step buttons outside that hit area so one click
                // can never be interpreted as both a step and a drag.
                const float sliderControlGap = 4f;
                const float minimumSliderTrackWidth = 20f;

                float sliderControlsLeft = Mathf.Min(
                    rowRect.x + 142f,
                    sliderControlsRight -
                    stepButtonWidth * 2f -
                    sliderControlGap * 2f -
                    minimumSliderTrackWidth
                );

                sliderControlsLeft = Mathf.Max(
                    rowRect.x + 88f,
                    sliderControlsLeft
                );

                Rect decrementButtonRect = new Rect(
                    sliderControlsLeft,
                    rowRect.y + 3f,
                    stepButtonWidth,
                    20f
                );

                float sliderTrackX =
                    decrementButtonRect.xMax +
                    sliderControlGap;

                Rect incrementButtonRect = new Rect(
                    sliderControlsRight -
                    stepButtonWidth,
                    rowRect.y + 3f,
                    stepButtonWidth,
                    20f
                );

                Rect sliderRect = new Rect(
                    sliderTrackX,
                    rowRect.y + 5f,
                    Mathf.Max(
                        minimumSliderTrackWidth,
                        incrementButtonRect.x -
                        sliderControlGap -
                        sliderTrackX
                    ),
                    16f
                );

                Rect precisionControlsRect = new Rect(
                    decrementButtonRect.xMin - 2f,
                    rowRect.y,
                    rowRect.xMax -
                    decrementButtonRect.xMin + 2f,
                    rowRect.height
                );

                bool mouseIsOverPrecisionControls =
                    precisionControlsRect.Contains(
                        Event.current.mousePosition
                    );

                bool mouseIsInsideFullRow =
                    IsMouseOverBlendshapeRow(
                        rowRect,
                        Event.current
                    );

                bool mouseIsOverRow =
                    mouseIsInsideFullRow &&
                    !mouseIsOverPrecisionControls;

                if (shouldUpdateHover &&
                    hoverSuppressedBlendshapeIndex == index &&
                    activeBlendshapeSliderIndex < 0 &&
                    !mouseIsInsideFullRow)
                {
                    hoverSuppressedBlendshapeIndex = -1;
                }

                bool isHovered = activeBlendshapeSliderIndex >= 0
                    ? index == activeBlendshapeSliderIndex
                    : mouseIsOverRow;

                if (shouldUpdateHover &&
                    isHovered &&
                    activeBlendshapeSliderIndex < 0 &&
                    hoverSuppressedBlendshapeIndex != index)
                {
                    newHoveredIndex = index;
                }

                if (isHovered &&
                    Event.current.type == EventType.Repaint)
                {
                    // Draw directly through the style so the hover highlight
                    // does not create a conditional IMGUI control ID. A changing
                    // control order would break the slider's first drag.
                    rowHoverStyle.Draw(
                        rowRect,
                        GUIContent.none,
                        false,
                        false,
                        false,
                        false
                    );
                }

                Color rowDividerColor =
                    currentTheme == ThemeOption.Dark
                        ? new Color(0.11f, 0.12f, 0.15f, 1f)
                        : new Color(0.88f, 0.90f, 0.93f, 1f);

                EditorGUI.DrawRect(
                    new Rect(
                        rowRect.x,
                        rowRect.yMax - 1f,
                        rowRect.width,
                        1f
                    ),
                    rowDividerColor
                );

                EditorGUI.LabelField(
                    new Rect(
                        rowRect.x + 4f,
                        rowRect.y,
                        Mathf.Max(
                            72f,
                            sliderControlsLeft -
                            rowRect.x - 10f
                        ),
                        rowRect.height
                    ),
                    new GUIContent(shapeName, shapeName),
                    labelWhiteStyle
                );

                float newVal = val;
                bool valueChangedByStepButton = false;

                if (GUI.Button(
                    decrementButtonRect,
                    "<",
                    blendshapeStepButtonStyle
                ))
                {
                    newVal = StepBlendshapeValue(
                        val,
                        -0.1f
                    );

                    valueChangedByStepButton = true;
                }

                newVal = DrawBlendshapeSlider(
                    sliderRect,
                    newVal,
                    0f,
                    100f,
                    index
                );

                if (GUI.Button(
                    incrementButtonRect,
                    ">",
                    blendshapeStepButtonStyle
                ))
                {
                    newVal = StepBlendshapeValue(
                        newVal,
                        0.1f
                    );

                    valueChangedByStepButton = true;
                }

                if (valueChangedByStepButton)
                {
                    GUI.FocusControl(null);
                    activeBlendshapeTextInputIndex = -1;
                    hasActiveBlendshapeTextInputRect = false;
                    hoverSuppressedBlendshapeIndex = index;
                    currentlyHoveredIndex = -1;

                    blendShapeInputTexts[index] =
                        FormatBlendshapeInputValue(newVal);
                }

                if (activeBlendshapeSliderIndex == index)
                {
                    activeBlendshapeTextInputIndex = -1;
                    hasActiveBlendshapeTextInputRect = false;

                    blendShapeInputTexts[index] =
                        FormatBlendshapeInputValue(newVal);
                }

                string inputControlName =
                    GetBlendshapeInputControlName(index);

                bool inputClicked =
                    Event.current.type == EventType.MouseDown &&
                    Event.current.button == 0 &&
                    numericFieldRect.Contains(
                        Event.current.mousePosition
                    );

                if (inputClicked)
                {
                    activeBlendshapeTextInputIndex = index;
                    hoverSuppressedBlendshapeIndex = index;
                    currentlyHoveredIndex = -1;
                }

                GUI.SetNextControlName(inputControlName);

                string editedInputText =
                    GUI.TextField(
                        numericFieldRect,
                        blendShapeInputTexts[index],
                        customTextFieldStyle
                    );

                if (inputClicked)
                {
                    // Explicitly start text editing. This avoids Unity IMGUI
                    // dropping keyboard focus during the window's live repaint.
                    EditorGUI.FocusTextInControl(
                        inputControlName
                    );

                    if (GUIUtility.keyboardControl != 0)
                    {
                        TextEditor textEditor =
                            GUIUtility.GetStateObject(
                                typeof(TextEditor),
                                GUIUtility.keyboardControl
                            ) as TextEditor;

                        if (textEditor != null)
                        {
                            textEditor.SelectAll();
                        }
                    }
                }

                bool inputIsFocused =
                    GUI.GetNameOfFocusedControl() ==
                    inputControlName;

                if (inputIsFocused)
                {
                    activeBlendshapeTextInputIndex = index;
                    hoverSuppressedBlendshapeIndex = index;
                    currentlyHoveredIndex = -1;
                }

                if (activeBlendshapeTextInputIndex == index &&
                    Event.current.type == EventType.Repaint)
                {
                    activeBlendshapeTextInputScreenRect =
                        GUIUtility.GUIToScreenRect(
                            numericFieldRect
                        );

                    hasActiveBlendshapeTextInputRect = true;
                }

                if (editedInputText !=
                    blendShapeInputTexts[index])
                {
                    activeBlendshapeTextInputIndex = index;
                    hoverSuppressedBlendshapeIndex = index;
                    currentlyHoveredIndex = -1;

                    blendShapeInputTexts[index] =
                        editedInputText;

                    float parsedValue;

                    if (TryParseBlendshapeInputValue(
                        editedInputText,
                        out parsedValue
                    ))
                    {
                        newVal = Mathf.Clamp(
                            parsedValue,
                            0f,
                            100f
                        );
                    }
                }

                if (GUI.Button(
                    new Rect(
                        rowRect.xMax - 30f,
                        rowRect.y + 2f,
                        30f,
                        22f
                    ),
                    GetText("ResetOne"),
                    cleanButtonStyle
                ))
                {
                    newVal = 0f;
                    GUI.FocusControl(null);
                    activeBlendshapeTextInputIndex = -1;
                    hasActiveBlendshapeTextInputRect = false;
                    hoverSuppressedBlendshapeIndex = index;
                    currentlyHoveredIndex = -1;

                    blendShapeInputTexts[index] =
                        FormatBlendshapeInputValue(newVal);
                }

                bool valueChangeRequested =
                    Math.Abs(newVal - val) > 0.001f;

                if (valueChangeRequested)
                {
                    newVal = NormalizeBlendshapeValue(newVal);
                }

                if (Math.Abs(newVal - val) > 0.001f)
                {
                    bool isSliderDragUpdate =
                        activeBlendshapeSliderIndex == index;

                    if (!isSliderDragUpdate)
                    {
                        Undo.RecordObjects(
                            new UnityEngine.Object[]
                            {
                                this,
                                faceSMR
                            },
                            "Modify BlendShape Weight"
                        );
                    }

                    blendShapeValues[index] = newVal;
                    faceSMR.SetBlendShapeWeight(index, newVal);

                    if (activeBlendshapeTextInputIndex != index)
                    {
                        blendShapeInputTexts[index] =
                            FormatBlendshapeInputValue(newVal);
                    }

                    if (activeBlendshapeSliderIndex == index ||
                        activeBlendshapeTextInputIndex == index ||
                        hoverSuppressedBlendshapeIndex == index)
                    {
                        currentlyHoveredIndex = -1;
                    }

                    if (isSliderDragUpdate)
                    {
                        ApplySingleBlendshapeDuringSliderDrag(
                            index,
                            newVal
                        );
                    }
                    else
                    {
                        ApplyWeightsToPreviewAndScene();
                    }
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        if (shouldUpdateHover)
        {
            if (activeBlendshapeSliderIndex >= 0 ||
                activeBlendshapeTextInputIndex >= 0 ||
                newHoveredIndex == hoverSuppressedBlendshapeIndex)
            {
                newHoveredIndex = -1;
            }

            if (currentlyHoveredIndex != newHoveredIndex)
            {
                currentlyHoveredIndex = newHoveredIndex;
                ApplyWeightsToPreviewAndScene();
            }
        }
    }

    private float DrawBlendshapeSlider(
        Rect rect,
        float value,
        float minValue,
        float maxValue,
        int blendShapeIndex
    )
    {
        Event e = Event.current;

        int controlId = GUIUtility.GetControlID(
            0x6B310000 ^ blendShapeIndex,
            FocusType.Passive,
            rect
        );

        Rect interactionRect = new Rect(
            rect.x - 4f,
            rect.y,
            rect.width + 8f,
            rect.height
        );

        switch (e.GetTypeForControl(controlId))
        {
            case EventType.MouseDown:
                if (e.button == 0 &&
                    interactionRect.Contains(e.mousePosition))
                {
                    GUIUtility.hotControl = controlId;
                    activeBlendshapeSliderIndex = blendShapeIndex;
                    hoverSuppressedBlendshapeIndex = blendShapeIndex;

                    if (currentlyHoveredIndex != -1)
                    {
                        currentlyHoveredIndex = -1;
                        ApplyWeightsToPreviewAndScene();
                    }

                    if (faceSMR != null)
                    {
                        Undo.RecordObjects(
                            new UnityEngine.Object[]
                            {
                                this,
                                faceSMR
                            },
                            "Modify BlendShape Weight"
                        );
                    }

                    value = GetSliderValueFromMouse(
                        rect,
                        e.mousePosition.x,
                        minValue,
                        maxValue
                    );

                    GUI.changed = true;
                    e.Use();
                }
                break;

            case EventType.MouseDrag:
                if (GUIUtility.hotControl == controlId &&
                    activeBlendshapeSliderIndex == blendShapeIndex)
                {
                    value = GetSliderValueFromMouse(
                        rect,
                        e.mousePosition.x,
                        minValue,
                        maxValue
                    );

                    GUI.changed = true;
                    e.Use();
                }
                break;

            case EventType.MouseUp:
                if (GUIUtility.hotControl == controlId &&
                    activeBlendshapeSliderIndex == blendShapeIndex)
                {
                    GUIUtility.hotControl = 0;
                    activeBlendshapeSliderIndex = -1;
                    e.Use();
                }
                break;

            case EventType.Repaint:
                float normalizedValue = Mathf.InverseLerp(
                    minValue,
                    maxValue,
                    value
                );

                float handleCenterX = Mathf.Lerp(
                    rect.xMin,
                    rect.xMax,
                    normalizedValue
                );

                float trackCenterY = Mathf.Round(rect.center.y);

                Rect trackRect = new Rect(
                    rect.x,
                    trackCenterY - 1f,
                    rect.width,
                    2f
                );

                Color remainingTrackColor =
                    currentTheme == ThemeOption.Dark
                        ? new Color(0.28f, 0.30f, 0.34f, 1f)
                        : new Color(0.68f, 0.71f, 0.76f, 1f);

                Color filledTrackColor =
                    currentTheme == ThemeOption.Dark
                        ? new Color(0.30f, 0.58f, 1f, 1f)
                        : new Color(0.10f, 0.36f, 0.88f, 1f);

                EditorGUI.DrawRect(
                    trackRect,
                    remainingTrackColor
                );

                if (handleCenterX > trackRect.xMin)
                {
                    EditorGUI.DrawRect(
                        new Rect(
                            trackRect.xMin,
                            trackRect.y,
                            handleCenterX - trackRect.xMin,
                            trackRect.height
                        ),
                        filledTrackColor
                    );
                }

                const float handleSize = 12f;

                Rect handleRect = new Rect(
                    handleCenterX - handleSize * 0.5f,
                    trackCenterY - handleSize * 0.5f,
                    handleSize,
                    handleSize
                );

                bool sliderIsActive =
                    value > minValue + 0.001f ||
                    GUIUtility.hotControl == controlId ||
                    interactionRect.Contains(e.mousePosition);

                Texture2D handleTexture = sliderIsActive
                    ? currentTheme == ThemeOption.Dark
                        ? darkSliderThumbBg
                        : lightSliderThumbBg
                    : currentTheme == ThemeOption.Dark
                        ? darkSliderInactiveThumbBg
                        : lightSliderInactiveThumbBg;

                GUI.DrawTexture(
                    handleRect,
                    handleTexture,
                    ScaleMode.StretchToFill,
                    true
                );
                break;
        }

        return Mathf.Clamp(value, minValue, maxValue);
    }

    private float GetSliderValueFromMouse(
        Rect rect,
        float mouseX,
        float minValue,
        float maxValue
    )
    {
        float normalizedValue = Mathf.InverseLerp(
            rect.xMin,
            rect.xMax,
            mouseX
        );

        return Mathf.Lerp(
            minValue,
            maxValue,
            normalizedValue
        );
    }

    private bool IsMouseOverBlendshapeRow(
        Rect rowRect,
        Event e
    )
    {
        if (e == null || !hasBlendshapeViewportRect)
        {
            return false;
        }

        switch (e.type)
        {
            case EventType.MouseMove:
            case EventType.MouseDown:
            case EventType.MouseDrag:
            case EventType.MouseUp:
            case EventType.ScrollWheel:
            case EventType.Repaint:
                break;

            default:
                return false;
        }

        if (!blendshapeViewportScreenRect.Contains(
            currentMouseScreenPosition
        ))
        {
            return false;
        }

        Rect rowScreenRect =
            GUIUtility.GUIToScreenRect(rowRect);

        return rowScreenRect.Contains(
            currentMouseScreenPosition
        );
    }

    private void Draw3DPreviewViewport(Rect rect)
    {
        if (rect.width <= 1f || rect.height <= 1f)
        {
            return;
        }

        if (previewCameraNeedsFraming)
        {
            FramePreviewCamera(rect);
        }

        HandlePreviewCameraInput(rect);

        // 使用场景摄像机的渲染环境，但临时套用工具自己的独立镜头位置。
        // 渲染完成后会完整恢复 Scene View 摄像机，不会改变或跟随场景视图。
        if (DrawSceneViewMirror(rect))
        {
            return;
        }

        if (previewUtility == null || previewInstance == null)
        {
            Color emptyPreviewBg =
                currentTheme == ThemeOption.Dark
                    ? Color.black
                    : new Color(0.925f, 0.94f, 0.965f, 1f);

            EditorGUI.DrawRect(rect, emptyPreviewBg);

            GUI.Label(
                new Rect(
                    rect.x + 14f,
                    rect.y + 14f,
                    200f,
                    20f
                ),
                GetText("No3DPreview"),
                secondaryLabelStyle
            );

            return;
        }

        previewUtility.BeginPreview(
            rect,
            GUIStyle.none
        );

        Quaternion camRot = Quaternion.Euler(
            camRotation.x,
            camRotation.y,
            0f
        );

        Vector3 camPos =
            camTargetPivot -
            camRot * Vector3.forward * camDistance;

        previewUtility.camera.transform.position = camPos;
        previewUtility.camera.transform.rotation = camRot;
        previewUtility.camera.nearClipPlane = 0.0001f;
        previewUtility.camera.farClipPlane = 20f;
        previewUtility.camera.fieldOfView =
            PreviewCameraFieldOfView;

        previewUtility.camera.clearFlags =
            CameraClearFlags.Color;

        previewUtility.camera.backgroundColor =
            currentTheme == ThemeOption.Dark
                ? Color.black
                : new Color(0.925f, 0.94f, 0.965f, 1f);

        previewUtility.camera.Render();

        Texture resultRender =
            previewUtility.EndPreview();

        GUI.DrawTexture(
            rect,
            resultRender,
            ScaleMode.StretchToFill,
            false
        );

        DrawPreviewBorder(rect);
        DrawPreviewCameraToolbar(rect);
    }

    private bool DrawSceneViewMirror(Rect rect)
    {
        SceneView sceneView =
            SceneView.lastActiveSceneView;

        Camera sceneCamera =
            sceneView != null ? sceneView.camera : null;

        if (sceneCamera == null ||
            targetModel == null ||
            !targetModel.scene.IsValid())
        {
            return false;
        }

        if (Event.current.type == EventType.Repaint)
        {
            int textureWidth = Mathf.Max(
                1,
                Mathf.RoundToInt(rect.width)
            );

            int textureHeight = Mathf.Max(
                1,
                Mathf.RoundToInt(rect.height)
            );

            EnsureSceneViewRenderTexture(
                textureWidth,
                textureHeight
            );

            RenderTexture previousTargetTexture =
                sceneCamera.targetTexture;

            RenderTexture previousActiveTexture =
                RenderTexture.active;

            Vector3 previousCameraPosition =
                sceneCamera.transform.position;

            Quaternion previousCameraRotation =
                sceneCamera.transform.rotation;

            float previousAspect = sceneCamera.aspect;
            float previousFieldOfView =
                sceneCamera.fieldOfView;

            float previousNearClipPlane =
                sceneCamera.nearClipPlane;

            float previousFarClipPlane =
                sceneCamera.farClipPlane;

            bool previousOrthographic =
                sceneCamera.orthographic;

            float previousOrthographicSize =
                sceneCamera.orthographicSize;

            try
            {
                Vector3 rootLocalPivot =
                    previewInstance != null
                        ? previewInstance.transform
                            .InverseTransformPoint(camTargetPivot)
                        : camTargetPivot;

                Vector3 worldPivot =
                    targetModel.transform.TransformPoint(
                        rootLocalPivot
                    );

                Quaternion worldCameraRotation =
                    targetModel.transform.rotation *
                    Quaternion.Euler(
                        camRotation.x,
                        camRotation.y,
                        0f
                    );

                float previewScale =
                    previewInstance != null
                        ? GetLargestScaleComponent(
                            previewInstance.transform.lossyScale
                        )
                        : 1f;

                float targetScale =
                    GetLargestScaleComponent(
                        targetModel.transform.lossyScale
                    );

                float worldDistance =
                    camDistance *
                    targetScale /
                    Mathf.Max(0.0001f, previewScale);

                Vector3 worldCameraPosition =
                    worldPivot -
                    worldCameraRotation *
                    Vector3.forward *
                    worldDistance;

                sceneCamera.transform.SetPositionAndRotation(
                    worldCameraPosition,
                    worldCameraRotation
                );

                sceneCamera.targetTexture =
                    sceneViewRenderTexture;

                sceneCamera.aspect =
                    rect.width /
                    Mathf.Max(1f, rect.height);

                sceneCamera.fieldOfView =
                    PreviewCameraFieldOfView;

                sceneCamera.orthographic = false;

                sceneCamera.nearClipPlane =
                    Mathf.Max(
                        0.0001f,
                        worldDistance * 0.001f
                    );

                sceneCamera.farClipPlane =
                    Mathf.Max(
                        previousFarClipPlane,
                        worldDistance * 20f
                    );

                sceneCamera.Render();
            }
            finally
            {
                sceneCamera.transform.SetPositionAndRotation(
                    previousCameraPosition,
                    previousCameraRotation
                );

                sceneCamera.targetTexture =
                    previousTargetTexture;

                sceneCamera.aspect = previousAspect;
                sceneCamera.fieldOfView =
                    previousFieldOfView;

                sceneCamera.nearClipPlane =
                    previousNearClipPlane;

                sceneCamera.farClipPlane =
                    previousFarClipPlane;

                sceneCamera.orthographic =
                    previousOrthographic;

                sceneCamera.orthographicSize =
                    previousOrthographicSize;

                RenderTexture.active =
                    previousActiveTexture;
            }
        }

        if (sceneViewRenderTexture != null)
        {
            GUI.DrawTexture(
                rect,
                sceneViewRenderTexture,
                ScaleMode.StretchToFill,
                false
            );

            DrawPreviewBorder(rect);
        }

        DrawPreviewCameraToolbar(rect);
        return true;
    }

    private void DrawPreviewBorder(Rect rect)
    {
        Color previewBorder =
            currentTheme == ThemeOption.Dark
                ? new Color(0.13f, 0.15f, 0.19f, 1f)
                : new Color(0.78f, 0.82f, 0.88f, 1f);

        EditorGUI.DrawRect(
            new Rect(
                rect.x,
                rect.y,
                rect.width,
                1f
            ),
            previewBorder
        );

        EditorGUI.DrawRect(
            new Rect(
                rect.x,
                rect.yMax - 1f,
                rect.width,
                1f
            ),
            previewBorder
        );
    }

    private void HandlePreviewCameraInput(Rect rect)
    {
        Event e = Event.current;

        Rect cameraInteractionRect = new Rect(
            rect.x,
            rect.y,
            rect.width,
            Mathf.Max(0f, rect.height - 46f)
        );

        int controlId = GUIUtility.GetControlID(
            "AvatarBlendshapePreviewCamera".GetHashCode(),
            FocusType.Passive,
            rect
        );

        switch (e.GetTypeForControl(controlId))
        {
            case EventType.MouseDown:
                if (!isResizingSplitter &&
                    cameraInteractionRect.Contains(e.mousePosition) &&
                    e.button >= 0 &&
                    e.button <= 2)
                {
                    GUIUtility.hotControl = controlId;
                    previewDragButton = e.button;
                    lastMousePos = e.mousePosition;
                    e.Use();
                }
                break;

            case EventType.MouseDrag:
                if (GUIUtility.hotControl == controlId &&
                    previewDragButton >= 0)
                {
                    Vector2 delta =
                        e.mousePosition - lastMousePos;

                    lastMousePos = e.mousePosition;

                    if (previewDragButton == 0)
                    {
                        camRotation.y += delta.x * 0.5f;
                        camRotation.x = Mathf.Clamp(
                            camRotation.x + delta.y * 0.5f,
                            -80f,
                            80f
                        );
                    }
                    else
                    {
                        Quaternion cameraRotation =
                            Quaternion.Euler(
                                camRotation.x,
                                camRotation.y,
                                0f
                            );

                        float panScale =
                            Mathf.Max(0.01f, camDistance) *
                            0.002f;

                        camTargetPivot -=
                            cameraRotation *
                            Vector3.right *
                            delta.x *
                            panScale;

                        camTargetPivot +=
                            cameraRotation *
                            Vector3.up *
                            delta.y *
                            panScale;
                    }

                    Repaint();
                    e.Use();
                }
                break;

            case EventType.MouseUp:
                if (GUIUtility.hotControl == controlId)
                {
                    GUIUtility.hotControl = 0;
                    previewDragButton = -1;
                    e.Use();
                }
                break;

            case EventType.ScrollWheel:
                if (!isResizingSplitter &&
                    cameraInteractionRect.Contains(e.mousePosition))
                {
                    float zoomFactor = Mathf.Max(
                        0.05f,
                        1f + e.delta.y * 0.03f
                    );

                    camDistance = Mathf.Clamp(
                        camDistance * zoomFactor,
                        0.01f,
                        10000f
                    );

                    Repaint();
                    e.Use();
                }
                break;
        }
    }

    private void FramePreviewCamera(Rect rect)
    {
        if (!hasPreviewFramingBounds)
        {
            previewCameraNeedsFraming = false;
            return;
        }

        Vector3 framingCenter =
            previewFramingBounds.center;

        if (hasPreviewDetectedFaceCenter)
        {
            // 先以头部中心计算四个视角投影宽度；最终高度会在距离算出后
            // 再按头顶位置重新定位。
            camTargetPivot = new Vector3(
                previewDetectedFaceCenter.x,
                framingCenter.y,
                previewDetectedFaceCenter.z
            );
        }
        else
        {
            camTargetPivot =
                framingCenter -
                Vector3.up *
                previewFramingBounds.size.y *
                0.08f;
        }

        Quaternion cameraRotation =
            Quaternion.Euler(
                camRotation.x,
                camRotation.y,
                0f
            );

        Vector3 cameraRight =
            cameraRotation * Vector3.right;

        Vector3 cameraUp =
            cameraRotation * Vector3.up;

        Vector3 cameraForward =
            cameraRotation * Vector3.forward;

        Vector3 boundsMin = previewFramingBounds.min;
        Vector3 boundsMax = previewFramingBounds.max;

        float projectedHalfWidth = 0f;
        float projectedHalfHeight = 0f;
        float projectedHalfDepth = 0f;

        for (int corner = 0; corner < 8; corner++)
        {
            Vector3 point = new Vector3(
                (corner & 1) == 0
                    ? boundsMin.x
                    : boundsMax.x,
                (corner & 2) == 0
                    ? boundsMin.y
                    : boundsMax.y,
                (corner & 4) == 0
                    ? boundsMin.z
                    : boundsMax.z
            );

            Vector3 offset =
                point - camTargetPivot;

            projectedHalfWidth = Mathf.Max(
                projectedHalfWidth,
                Mathf.Abs(Vector3.Dot(
                    offset,
                    cameraRight
                ))
            );

            projectedHalfHeight = Mathf.Max(
                projectedHalfHeight,
                Mathf.Abs(Vector3.Dot(
                    offset,
                    cameraUp
                ))
            );

            projectedHalfDepth = Mathf.Max(
                projectedHalfDepth,
                Mathf.Abs(Vector3.Dot(
                    offset,
                    cameraForward
                ))
            );
        }

        float aspect = Mathf.Max(
            0.1f,
            rect.width / Mathf.Max(1f, rect.height)
        );

        float halfFovRadians =
            PreviewCameraFieldOfView *
            Mathf.Deg2Rad *
            0.5f;

        float tangent = Mathf.Max(
            0.0001f,
            Mathf.Tan(halfFovRadians)
        );

        float distanceForHeight =
            projectedHalfHeight / tangent;

        float distanceForWidth =
            projectedHalfWidth /
            (tangent * aspect);

        if (hasPreviewDetectedFaceCenter)
        {
            // 侧面视角已经是正确的近距离，因此四个预设视角统一使用
            // “侧面投影宽度 + 侧面投影深度”计算距离。正面、45°和背面
            // 不再因为头部正面较宽而自动后退。
            float sideReferenceHalfWidth = Mathf.Max(
                0.001f,
                previewFramingBounds.extents.z
            );

            float sideReferenceHalfDepth = Mathf.Max(
                0.001f,
                previewFramingBounds.extents.x +
                Mathf.Abs(
                    previewFramingBounds.center.x -
                    previewDetectedFaceCenter.x
                )
            );

            float sideDistanceForWidth =
                sideReferenceHalfWidth /
                (tangent * aspect);

            float distanceForFullHeadHeight =
                previewFramingBounds.size.y /
                (tangent * 1.84f);

            camDistance =
                Mathf.Max(
                    sideDistanceForWidth,
                    distanceForFullHeadHeight
                ) *
                1.015f +
                sideReferenceHalfDepth * 0.35f;

            // 将头顶固定在预览窗口上沿附近。因为窗口是纵向长画幅，
            // 这样头部会占满宽度，脸部自然位于画面上半区，下面由身体填充。
            float visibleHalfHeight = Mathf.Max(
                0.001f,
                (camDistance -
                 sideReferenceHalfDepth * 0.35f) *
                tangent
            );

            camTargetPivot = new Vector3(
                previewDetectedFaceCenter.x,
                previewFramingBounds.max.y -
                visibleHalfHeight * 0.93f,
                previewDetectedFaceCenter.z
            );
        }
        else
        {
            float portraitHalfWidthLimit =
                projectedHalfHeight * aspect * 1.15f;

            float fallbackDistanceForWidth =
                Mathf.Min(
                    projectedHalfWidth,
                    portraitHalfWidthLimit
                ) /
                (tangent * aspect);

            camDistance =
                Mathf.Max(
                    distanceForHeight * 0.92f,
                    Mathf.Min(
                        fallbackDistanceForWidth,
                        distanceForHeight * 0.98f
                    )
                ) +
                projectedHalfDepth * 0.42f;
        }

        camDistance = Mathf.Max(
            0.05f,
            camDistance
        );

        previewCameraNeedsFraming = false;
    }

    private float GetLargestScaleComponent(Vector3 scale)
    {
        return Mathf.Max(
            Mathf.Abs(scale.x),
            Mathf.Abs(scale.y),
            Mathf.Abs(scale.z)
        );
    }

    private bool TryCalculateBlendshapeFaceBounds(
        SkinnedMeshRenderer renderer,
        out Bounds faceBounds)
    {
        faceBounds = new Bounds();

        if (renderer == null ||
            renderer.sharedMesh == null)
        {
            return false;
        }

        Mesh sourceMesh = renderer.sharedMesh;
        int vertexCount = sourceMesh.vertexCount;

        if (vertexCount <= 0 ||
            sourceMesh.blendShapeCount <= 0)
        {
            return false;
        }

        Mesh bakedMesh = new Mesh
        {
            hideFlags = HideFlags.HideAndDontSave
        };

        try
        {
            renderer.BakeMesh(bakedMesh);

            Vector3[] bakedVertices =
                bakedMesh.vertices;

            if (bakedVertices == null ||
                bakedVertices.Length != vertexCount)
            {
                return false;
            }

            bool[] affectedVertices =
                new bool[vertexCount];

            Vector3[] deltaVertices =
                new Vector3[vertexCount];

            Vector3[] deltaNormals =
                new Vector3[vertexCount];

            Vector3[] deltaTangents =
                new Vector3[vertexCount];

            float deltaThreshold = Mathf.Max(
                0.0000000001f,
                sourceMesh.bounds.size.sqrMagnitude *
                0.000000000001f
            );

            for (int shapeIndex = 0;
                 shapeIndex < sourceMesh.blendShapeCount;
                 shapeIndex++)
            {
                int frameCount =
                    sourceMesh.GetBlendShapeFrameCount(
                        shapeIndex
                    );

                if (frameCount <= 0)
                {
                    continue;
                }

                Array.Clear(
                    deltaVertices,
                    0,
                    deltaVertices.Length
                );

                sourceMesh.GetBlendShapeFrameVertices(
                    shapeIndex,
                    frameCount - 1,
                    deltaVertices,
                    deltaNormals,
                    deltaTangents
                );

                for (int vertexIndex = 0;
                     vertexIndex < vertexCount;
                     vertexIndex++)
                {
                    if (!affectedVertices[vertexIndex] &&
                        deltaVertices[vertexIndex]
                            .sqrMagnitude > deltaThreshold)
                    {
                        affectedVertices[vertexIndex] = true;
                    }
                }
            }

            float minimumFaceY =
                float.NegativeInfinity;

            Animator animator =
                previewInstance != null
                    ? previewInstance
                        .GetComponentInChildren<Animator>(true)
                    : null;

            if (animator != null &&
                animator.avatar != null &&
                animator.avatar.isHuman)
            {
                Transform head =
                    animator.GetBoneTransform(
                        HumanBodyBones.Head
                    );

                Transform hips =
                    animator.GetBoneTransform(
                        HumanBodyBones.Hips
                    );

                if (head != null && hips != null &&
                    head.position.y > hips.position.y)
                {
                    minimumFaceY =
                        head.position.y -
                        (head.position.y - hips.position.y) *
                        0.22f;
                }
            }

            bool hasFaceVertex = false;
            int detectedVertexCount = 0;

            for (int vertexIndex = 0;
                 vertexIndex < vertexCount;
                 vertexIndex++)
            {
                if (!affectedVertices[vertexIndex])
                {
                    continue;
                }

                Vector3 worldVertex =
                    renderer.transform.TransformPoint(
                        bakedVertices[vertexIndex]
                    );

                if (worldVertex.y < minimumFaceY)
                {
                    continue;
                }

                if (!hasFaceVertex)
                {
                    faceBounds = new Bounds(
                        worldVertex,
                        Vector3.zero
                    );

                    hasFaceVertex = true;
                }
                else
                {
                    faceBounds.Encapsulate(worldVertex);
                }

                detectedVertexCount++;
            }

            return hasFaceVertex &&
                   detectedVertexCount >= 8 &&
                   faceBounds.size.sqrMagnitude >
                   0.00000001f;
        }
        finally
        {
            DestroyImmediate(bakedMesh);
        }
    }

    private bool TryCalculatePreviewHeadBounds(
        SkinnedMeshRenderer[] renderers,
        Bounds faceBounds,
        Bounds totalBounds,
        out Bounds headBounds)
    {
        headBounds = new Bounds();

        if (renderers == null ||
            renderers.Length == 0)
        {
            return false;
        }

        float totalHeight = Mathf.Max(
            0.001f,
            totalBounds.size.y
        );

        float faceReferenceSize = Mathf.Clamp(
            Mathf.Max(
                faceBounds.size.x,
                faceBounds.size.y * 1.15f
            ),
            totalHeight * 0.08f,
            totalHeight * 0.18f
        );

        Vector3 headRegionCenter =
            faceBounds.center;

        float minimumHeadY =
            faceBounds.center.y -
            Mathf.Max(
                faceReferenceSize * 0.95f,
                totalHeight * 0.12f
            );

        Animator animator =
            previewInstance != null
                ? previewInstance
                    .GetComponentInChildren<Animator>(true)
                : null;

        if (animator != null &&
            animator.avatar != null &&
            animator.avatar.isHuman)
        {
            Transform head =
                animator.GetBoneTransform(
                    HumanBodyBones.Head
                );

            Transform neck =
                animator.GetBoneTransform(
                    HumanBodyBones.Neck
                );

            if (head != null)
            {
                // 脸部决定画面高度，头骨决定四个视角旋转时的深度中心。
                headRegionCenter.x =
                    faceBounds.center.x;

                headRegionCenter.z =
                    head.position.z;
            }

            if (neck != null)
            {
                minimumHeadY =
                    neck.position.y -
                    totalHeight * 0.018f;
            }
        }

        float horizontalRadius = Mathf.Clamp(
            Mathf.Max(
                faceReferenceSize * 1.35f,
                totalHeight * 0.14f
            ),
            totalHeight * 0.12f,
            totalHeight * 0.20f
        );

        float maximumHeadY =
            totalBounds.max.y +
            totalHeight * 0.004f;

        bool hasHeadVertex = false;
        int detectedVertexCount = 0;

        foreach (SkinnedMeshRenderer renderer in renderers)
        {
            if (renderer == null ||
                renderer.sharedMesh == null ||
                renderer.sharedMesh.vertexCount <= 0)
            {
                continue;
            }

            Mesh bakedMesh = new Mesh
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            try
            {
                renderer.BakeMesh(bakedMesh);

                Vector3[] vertices =
                    bakedMesh.vertices;

                for (int vertexIndex = 0;
                     vertexIndex < vertices.Length;
                     vertexIndex++)
                {
                    Vector3 worldVertex =
                        renderer.transform.TransformPoint(
                            vertices[vertexIndex]
                        );

                    if (worldVertex.y < minimumHeadY ||
                        worldVertex.y > maximumHeadY ||
                        Mathf.Abs(
                            worldVertex.x -
                            headRegionCenter.x
                        ) > horizontalRadius ||
                        Mathf.Abs(
                            worldVertex.z -
                            headRegionCenter.z
                        ) > horizontalRadius)
                    {
                        continue;
                    }

                    if (!hasHeadVertex)
                    {
                        headBounds = new Bounds(
                            worldVertex,
                            Vector3.zero
                        );

                        hasHeadVertex = true;
                    }
                    else
                    {
                        headBounds.Encapsulate(
                            worldVertex
                        );
                    }

                    detectedVertexCount++;
                }
            }
            finally
            {
                DestroyImmediate(bakedMesh);
            }
        }

        if (!hasHeadVertex ||
            detectedVertexCount < 16)
        {
            float fallbackWidth = Mathf.Max(
                faceBounds.size.x * 1.85f,
                totalHeight * 0.18f
            );

            float fallbackHeight = Mathf.Max(
                fallbackWidth * 1.05f,
                faceBounds.size.y * 2.4f
            );

            float fallbackDepth = Mathf.Max(
                faceBounds.size.z * 2.8f,
                fallbackWidth * 0.72f
            );

            headBounds = new Bounds(
                new Vector3(
                    faceBounds.center.x,
                    faceBounds.center.y +
                    fallbackHeight * 0.12f,
                    headRegionCenter.z
                ),
                new Vector3(
                    fallbackWidth,
                    fallbackHeight,
                    fallbackDepth
                )
            );
        }

        headBounds.Expand(
            new Vector3(
                totalHeight * 0.012f,
                totalHeight * 0.010f,
                totalHeight * 0.012f
            )
        );

        return headBounds.size.sqrMagnitude >
               0.00000001f;
    }

    private bool TryGetHumanoidUpperBodyAnchors(
        out Vector3 headPosition,
        out float torsoBottomY)
    {
        headPosition = Vector3.zero;
        torsoBottomY = 0f;

        if (previewInstance == null)
        {
            return false;
        }

        Animator animator =
            previewInstance.GetComponentInChildren<Animator>(true);

        if (animator == null ||
            animator.avatar == null ||
            !animator.avatar.isHuman)
        {
            return false;
        }

        Transform head =
            animator.GetBoneTransform(HumanBodyBones.Head);

        if (head == null)
        {
            return false;
        }

        Transform hips =
            animator.GetBoneTransform(HumanBodyBones.Hips);

        Transform chest =
            animator.GetBoneTransform(HumanBodyBones.UpperChest);

        if (chest == null)
        {
            chest = animator.GetBoneTransform(
                HumanBodyBones.Chest
            );
        }

        if (chest == null)
        {
            chest = animator.GetBoneTransform(
                HumanBodyBones.Spine
            );
        }

        headPosition = head.position;

        if (hips != null &&
            hips.position.y < headPosition.y)
        {
            // 取到髋部稍上方：保留部分上半身，但绝不退回全身视角。
            torsoBottomY = Mathf.Lerp(
                headPosition.y,
                hips.position.y,
                0.82f
            );
        }
        else if (chest != null &&
                 chest.position.y < headPosition.y)
        {
            float headToChest =
                headPosition.y - chest.position.y;

            torsoBottomY =
                chest.position.y -
                headToChest * 0.9f;
        }
        else
        {
            return false;
        }

        return true;
    }

    private void DrawPreviewCameraToolbar(Rect rect)
    {
        float buttonBarHeight = 30f;

        Rect barRect = new Rect(
            rect.x + 8f,
            rect.yMax - buttonBarHeight - 8f,
            rect.width - 16f,
            buttonBarHeight
        );

        GUI.Box(
            barRect,
            GUIContent.none,
            previewToolbarStyle
        );

        float buttonWidth = Mathf.Max(
            30f,
            (barRect.width - 17f) / 4f
        );

        if (GUI.Button(
            new Rect(
                barRect.x + 3f,
                barRect.y + 3f,
                buttonWidth,
                24f
            ),
            GetText("Front"),
            cleanButtonStyle
        ))
        {
            SetCameraPresetAngle(180f);
        }

        if (GUI.Button(
            new Rect(
                barRect.x + 3f + buttonWidth + 4f,
                barRect.y + 3f,
                buttonWidth,
                24f
            ),
            GetText("45Deg"),
            cleanButtonStyle
        ))
        {
            SetCameraPresetAngle(225f);
        }

        if (GUI.Button(
            new Rect(
                barRect.x + 3f +
                (buttonWidth + 4f) * 2f,
                barRect.y + 3f,
                buttonWidth,
                24f
            ),
            GetText("Side"),
            cleanButtonStyle
        ))
        {
            SetCameraPresetAngle(270f);
        }

        if (GUI.Button(
            new Rect(
                barRect.x + 3f +
                (buttonWidth + 4f) * 3f,
                barRect.y + 3f,
                buttonWidth,
                24f
            ),
            GetText("Back"),
            cleanButtonStyle
        ))
        {
            SetCameraPresetAngle(0f);
        }
    }

    private void SetCameraPresetAngle(float yAngle)
    {
        camRotation = new Vector2(0f, yAngle);
        previewCameraNeedsFraming = true;
        Repaint();
    }

    private void ShowCategoryMenu()
    {
        GenericMenu menu = new GenericMenu();

        menu.AddItem(
            new GUIContent(GetText("SelectAll")),
            false,
            () => SetAllCategoriesVisible(true)
        );

        menu.AddItem(
            new GUIContent(GetText("DeselectAll")),
            false,
            () => SetAllCategoriesVisible(false)
        );

        menu.AddSeparator("");

        foreach (var group in categories)
        {
            menu.AddItem(
                new GUIContent(group.groupName),
                group.isVisible,
                () =>
                {
                    group.isVisible = !group.isVisible;
                }
            );
        }

        menu.ShowAsContext();
    }

    private void HandleKeyboardEvents()
    {
        Event e = Event.current;

        if (e.type == EventType.KeyDown &&
            (e.control || e.command))
        {
            if (e.keyCode == KeyCode.Z && !e.shift)
            {
                Undo.PerformUndo();
                e.Use();
            }
            else if (e.keyCode == KeyCode.Y ||
                     (e.shift &&
                      e.keyCode == KeyCode.Z))
            {
                Undo.PerformRedo();
                e.Use();
            }
        }
    }

    private void OnUndoRedoPerformed()
    {
        SyncValuesFromSMR();
        Repaint();
    }

    private void SyncValuesFromSMR()
    {
        if (faceSMR != null &&
            blendShapeCount > 0 &&
            blendShapeValues != null)
        {
            for (int i = 0; i < blendShapeCount; i++)
            {
                blendShapeValues[i] =
                    faceSMR.GetBlendShapeWeight(i);
            }

            RefreshBlendshapeInputTextsFromValues();
        }
    }

    private void OnTargetModelChanged()
    {
        CleanPreviewInstance();
        activeBlendshapeTextInputIndex = -1;
        hasActiveBlendshapeTextInputRect = false;
        blendShapeInputTexts = null;

        if (targetModel == null)
        {
            faceSMR = null;
            faceMesh = null;
            blendShapeCount = 0;
            return;
        }

        SkinnedMeshRenderer[] smrs =
            targetModel.GetComponentsInChildren
                <SkinnedMeshRenderer>(true);

        faceSMR = null;

        foreach (var smr in smrs)
        {
            if (smr.sharedMesh != null &&
                smr.sharedMesh.blendShapeCount > 0)
            {
                string nameLower = smr.name.ToLower();

                if (nameLower.Contains("body") ||
                    nameLower.Contains("face") ||
                    nameLower.Contains("head") ||
                    nameLower.Contains("脸"))
                {
                    faceSMR = smr;
                    break;
                }

                if (faceSMR == null)
                {
                    faceSMR = smr;
                }
            }
        }

        if (faceSMR == null ||
            faceSMR.sharedMesh == null)
        {
            blendShapeCount = 0;
            return;
        }

        faceMesh = faceSMR.sharedMesh;
        blendShapeCount = faceMesh.blendShapeCount;

        blendShapeNames =
            new string[blendShapeCount];

        blendShapeValues =
            new float[blendShapeCount];

        for (int i = 0; i < blendShapeCount; i++)
        {
            blendShapeNames[i] =
                faceMesh.GetBlendShapeName(i);

            blendShapeValues[i] =
                faceSMR.GetBlendShapeWeight(i);
        }

        RefreshBlendshapeInputTextsFromValues();

        ParseCategories();
        CreateFullPreviewInstance();
    }

    private void ParseCategories()
    {
        categories.Clear();

        BlendShapeGroup currentGroup =
            new BlendShapeGroup
            {
                groupName = "DEFAULT",
                isVisible = true
            };

        for (int i = 0; i < blendShapeCount; i++)
        {
            string name = blendShapeNames[i];

            if (IsSeparatorBlendShape(
                name,
                out string extractedCategoryName
            ))
            {
                if (currentGroup.indices.Count > 0)
                {
                    categories.Add(currentGroup);
                }

                currentGroup = new BlendShapeGroup
                {
                    groupName =
                        extractedCategoryName.ToUpper(),
                    isVisible = true
                };

                continue;
            }

            currentGroup.indices.Add(i);
        }

        if (currentGroup.indices.Count > 0)
        {
            categories.Add(currentGroup);
        }
    }

    private bool IsSeparatorBlendShape(
        string name,
        out string categoryName
    )
    {
        categoryName = name;
        string trimmed = name.Trim();

        bool isSeparator =
            trimmed.StartsWith("---") ||
            trimmed.StartsWith("===") ||
            trimmed.StartsWith("▼") ||
            trimmed.StartsWith("----------") ||
            (trimmed.StartsWith("[") &&
             trimmed.EndsWith("]")) ||
            (trimmed.StartsWith("【") &&
             trimmed.EndsWith("】"));

        if (isSeparator)
        {
            categoryName = trimmed.Trim(
                '-',
                '=',
                '▼',
                '[',
                ']',
                '【',
                '】',
                ' '
            );

            if (string.IsNullOrEmpty(categoryName))
            {
                categoryName = "DEFAULT";
            }

            return true;
        }

        return false;
    }

    private bool GroupHasModifiedShapes(
        BlendShapeGroup group
    )
    {
        foreach (int index in group.indices)
        {
            if (!Mathf.Approximately(
                blendShapeValues[index],
                0f
            ))
            {
                return true;
            }
        }

        return false;
    }

    private void SetAllCategoriesVisible(bool visible)
    {
        foreach (var group in categories)
        {
            group.isVisible = visible;
        }
    }

    // ---------- 预设与导出 ----------
    private void LoadSelectedPreset()
    {
        if (selectedPresetIndex >= 0 &&
            selectedPresetIndex < savedPresets.Count)
        {
            ApplyPreset(
                savedPresets[selectedPresetIndex]
            );
        }
    }

    private void DeleteSelectedPreset()
    {
        if (selectedPresetIndex >= 0 &&
            selectedPresetIndex < savedPresets.Count)
        {
            savedPresets.RemoveAt(
                selectedPresetIndex
            );

            selectedPresetIndex = 0;
            SavePresetsToEditorPrefs();
        }
    }

    private void SaveCurrentAsPreset(string presetName)
    {
        if (string.IsNullOrEmpty(presetName))
        {
            presetName = GetText("Unnamed");
        }

        PresetData newPreset = new PresetData
        {
            presetName = presetName
        };

        for (int i = 0; i < blendShapeCount; i++)
        {
            newPreset.keys.Add(blendShapeNames[i]);
            newPreset.values.Add(blendShapeValues[i]);
        }

        savedPresets.Add(newPreset);
        selectedPresetIndex =
            savedPresets.Count - 1;

        SavePresetsToEditorPrefs();

        ShowNotification(
            new GUIContent(
                $"{GetText("Saved")}{presetName}"
            )
        );
    }

    private void ApplyPreset(PresetData preset)
    {
        Undo.RecordObjects(
            new UnityEngine.Object[]
            {
                this,
                faceSMR
            },
            "Apply BlendShape Preset"
        );

        for (int i = 0; i < preset.keys.Count; i++)
        {
            string key = preset.keys[i];
            int index = Array.IndexOf(
                blendShapeNames,
                key
            );

            if (index >= 0)
            {
                blendShapeValues[index] =
                    preset.values[i];

                faceSMR.SetBlendShapeWeight(
                    index,
                    preset.values[i]
                );
            }
        }

        RefreshBlendshapeInputTextsFromValues();

        EditorUtility.SetDirty(faceSMR);
        ApplyWeightsToPreviewAndScene();

        ShowNotification(
            new GUIContent(
                $"{GetText("Loaded")}{preset.presetName}"
            )
        );
    }

    private void ExportCurrentAsAnimationClip()
    {
        if (faceSMR == null || targetModel == null)
        {
            EditorUtility.DisplayDialog(
                GetText("Info"),
                GetText("NoModel"),
                GetText("OK")
            );

            return;
        }

        string savePath =
            EditorUtility.SaveFilePanelInProject(
                GetText("ExportTitle"),
                "New_Face_Expression",
                "anim",
                GetText("SelectPath")
            );

        if (string.IsNullOrEmpty(savePath))
        {
            return;
        }

        AnimationClip clip = new AnimationClip();

        string relativePath = GetRelativePath(
            targetModel.transform,
            faceSMR.transform
        );

        bool hasAnyKeyframe = false;

        for (int i = 0; i < blendShapeCount; i++)
        {
            float value = blendShapeValues[i];

            if (Mathf.Abs(value) > 0.001f)
            {
                AnimationCurve curve =
                    AnimationCurve.Constant(
                        0f,
                        0f,
                        value
                    );

                string propertyName =
                    "blendShape." +
                    blendShapeNames[i];

                clip.SetCurve(
                    relativePath,
                    typeof(SkinnedMeshRenderer),
                    propertyName,
                    curve
                );

                hasAnyKeyframe = true;
            }
        }

        if (!hasAnyKeyframe)
        {
            EditorUtility.DisplayDialog(
                GetText("Info"),
                GetText("AllZero"),
                GetText("OK")
            );
        }

        AssetDatabase.CreateAsset(
            clip,
            savePath
        );

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorGUIUtility.PingObject(clip);

        ShowNotification(
            new GUIContent(
                GetText("ExportSuccess")
            )
        );
    }

    private string GetRelativePath(
        Transform root,
        Transform target
    )
    {
        if (root == target)
        {
            return "";
        }

        List<string> pathComponents =
            new List<string>();

        Transform current = target;

        while (current != null &&
               current != root)
        {
            pathComponents.Add(current.name);
            current = current.parent;
        }

        pathComponents.Reverse();

        return string.Join(
            "/",
            pathComponents
        );
    }

    private void ResetAllBlendShapesToZero()
    {
        if (faceSMR == null ||
            blendShapeCount == 0)
        {
            return;
        }

        Undo.RecordObjects(
            new UnityEngine.Object[]
            {
                this,
                faceSMR
            },
            "Reset All BlendShapes to 0"
        );

        for (int i = 0; i < blendShapeCount; i++)
        {
            blendShapeValues[i] = 0f;
            faceSMR.SetBlendShapeWeight(i, 0f);
        }

        RefreshBlendshapeInputTextsFromValues();

        EditorUtility.SetDirty(faceSMR);
        ApplyWeightsToPreviewAndScene();

        ShowNotification(
            new GUIContent(
                GetText("ResetZero")
            )
        );
    }

    private void ApplySingleBlendshapeDuringSliderDrag(
        int index,
        float weight)
    {
        if (index < 0 || index >= blendShapeCount)
        {
            return;
        }

        if (previewFaceSMR != null)
        {
            previewFaceSMR.SetBlendShapeWeight(
                index,
                weight
            );
        }

        // faceSMR was already updated by DrawBlendshapeList.
        // Avoid rewriting every other blendshape on each mouse movement.
        Repaint();
        SceneView.RepaintAll();
    }

    private void ApplyWeightsToPreviewAndScene()
    {
        if (blendShapeCount == 0)
        {
            return;
        }

        for (int i = 0; i < blendShapeCount; i++)
        {
            float targetWeight =
                activeBlendshapeSliderIndex < 0 &&
                activeBlendshapeTextInputIndex < 0 &&
                i == currentlyHoveredIndex
                    ? 100f
                    : blendShapeValues[i];

            if (previewFaceSMR != null)
            {
                previewFaceSMR.SetBlendShapeWeight(
                    i,
                    targetWeight
                );
            }

            if (faceSMR != null)
            {
                faceSMR.SetBlendShapeWeight(
                    i,
                    targetWeight
                );
            }
        }

        Repaint();
        SceneView.RepaintAll();
    }

    // ---------- EditorPrefs ----------
    private const string PREF_KEY =
        "VRCFaceModifier_Presets_Data";

    private void SavePresetsToEditorPrefs()
    {
        string json = JsonUtility.ToJson(
            new PresetListWrapper
            {
                presets = savedPresets
            }
        );

        EditorPrefs.SetString(
            PREF_KEY,
            json
        );
    }

    private void LoadPresetsFromEditorPrefs()
    {
        if (!EditorPrefs.HasKey(PREF_KEY))
        {
            return;
        }

        string json =
            EditorPrefs.GetString(PREF_KEY);

        PresetListWrapper wrapper =
            JsonUtility.FromJson
                <PresetListWrapper>(json);

        if (wrapper != null &&
            wrapper.presets != null)
        {
            savedPresets = wrapper.presets;
        }
    }

    [Serializable]
    private class PresetListWrapper
    {
        public List<PresetData> presets;
    }

    // ---------- 预览实例与自动取景 ----------
    private void InitPreviewUtility()
    {
        if (previewUtility != null)
        {
            CleanPreviewUtility();
        }

        previewUtility =
            new PreviewRenderUtility();
    }

    private void CleanPreviewUtility()
    {
        if (previewUtility == null)
        {
            return;
        }

        previewUtility.Cleanup();
        previewUtility = null;
    }

    private void CleanPreviewInstance()
    {
        if (previewInstance != null)
        {
            DestroyImmediate(previewInstance);
            previewInstance = null;
        }

        previewFaceSMR = null;
        previewDetectedFaceCenter = Vector3.zero;
        hasPreviewFramingBounds = false;
        hasPreviewDetectedFaceCenter = false;
        previewCameraNeedsFraming = true;
    }

    private void CreateFullPreviewInstance()
    {
        if (targetModel == null)
        {
            return;
        }

        CleanPreviewInstance();

        previewInstance = Instantiate(targetModel);
        previewInstance.hideFlags =
            HideFlags.HideAndDontSave;

        previewInstance.transform.position =
            Vector3.zero;

        previewInstance.transform.rotation =
            Quaternion.identity;

        SkinnedMeshRenderer[] skinnedRenderers =
            previewInstance.GetComponentsInChildren
                <SkinnedMeshRenderer>(true);

        previewFaceSMR = FindMatchingPreviewFaceRenderer();

        previewUtility.AddSingleGO(previewInstance);

        Bounds totalBounds = new Bounds(
            previewInstance.transform.position,
            Vector3.zero
        );

        bool hasBounds = false;

        foreach (var renderer in skinnedRenderers)
        {
            if (!hasBounds)
            {
                totalBounds = renderer.bounds;
                hasBounds = true;
            }
            else
            {
                totalBounds.Encapsulate(
                    renderer.bounds
                );
            }
        }

        Bounds detectedFaceBounds = new Bounds();

        bool hasDetectedFaceBounds =
            hasBounds &&
            previewFaceSMR != null &&
            TryCalculateBlendshapeFaceBounds(
                previewFaceSMR,
                out detectedFaceBounds
            );

        Bounds detectedHeadBounds = new Bounds();

        bool hasDetectedHeadBounds =
            hasDetectedFaceBounds &&
            TryCalculatePreviewHeadBounds(
                skinnedRenderers,
                detectedFaceBounds,
                totalBounds,
                out detectedHeadBounds
            );

        if (hasDetectedHeadBounds)
        {
            previewDetectedFaceCenter =
                new Vector3(
                    detectedFaceBounds.center.x,
                    detectedFaceBounds.center.y,
                    detectedHeadBounds.center.z
                );

            // 取景边界只包含自动识别到的完整头部，不再包含上半身。
            previewFramingBounds =
                detectedHeadBounds;

            hasPreviewFramingBounds = true;
            hasPreviewDetectedFaceCenter = true;

            camTargetPivot =
                previewDetectedFaceCenter;

            camRotation =
                new Vector2(0f, 180f);

            camDistance = Mathf.Max(
                0.05f,
                detectedHeadBounds.size.x
            );

            previewCameraNeedsFraming = true;
        }
        else if (hasBounds)
        {
            float totalHeight = Mathf.Max(
                0.001f,
                totalBounds.size.y
            );

            float estimatedFaceY = Mathf.Lerp(
                totalBounds.min.y,
                totalBounds.max.y,
                0.78f
            );

            Vector3 framingCenter =
                totalBounds.center;

            Vector3 humanoidHeadPosition;
            float humanoidTorsoBottomY;

            bool hasHumanoidUpperBodyAnchors =
                TryGetHumanoidUpperBodyAnchors(
                    out humanoidHeadPosition,
                    out humanoidTorsoBottomY
                );

            if (hasHumanoidUpperBodyAnchors)
            {
                estimatedFaceY =
                    humanoidHeadPosition.y;

                // 四个预设视角都围绕真实头骨旋转，避免使用整模中心。
                framingCenter.x =
                    humanoidHeadPosition.x;

                framingCenter.z =
                    humanoidHeadPosition.z;
            }

            if (previewFaceSMR != null)
            {
                Bounds faceBounds =
                    previewFaceSMR.bounds;

                bool faceBoundsLooksLocalized =
                    faceBounds.size.y <=
                    totalHeight * 0.65f &&
                    faceBounds.center.y >=
                    totalBounds.min.y +
                    totalHeight * 0.45f;

                if (faceBoundsLooksLocalized)
                {
                    estimatedFaceY =
                        faceBounds.center.y;
                }
            }

            float topMargin =
                totalHeight * 0.025f;

            float framingTop =
                totalBounds.max.y + topMargin;

            float framingBottomByHeight =
                framingTop -
                totalHeight * 0.48f;

            float framingBottomByFace =
                estimatedFaceY -
                totalHeight * 0.30f;

            float framingBottom = Mathf.Max(
                totalBounds.min.y,
                Mathf.Min(
                    framingBottomByHeight,
                    framingBottomByFace
                )
            );

            if (hasHumanoidUpperBodyAnchors)
            {
                framingBottom = Mathf.Max(
                    framingBottom,
                    humanoidTorsoBottomY
                );
            }

            float framingHeight = Mathf.Max(
                totalHeight * 0.35f,
                framingTop - framingBottom
            );

            float framingWidth = Mathf.Min(
                totalBounds.size.x,
                framingHeight * 0.72f
            );

            float framingDepth = Mathf.Min(
                totalBounds.size.z,
                framingHeight * 0.72f
            );

            framingWidth = Mathf.Max(
                framingWidth,
                totalHeight * 0.18f
            );

            framingDepth = Mathf.Max(
                framingDepth,
                totalHeight * 0.18f
            );

            previewFramingBounds = new Bounds(
                new Vector3(
                    framingCenter.x,
                    framingBottom +
                    framingHeight * 0.5f,
                    framingCenter.z
                ),
                new Vector3(
                    framingWidth,
                    framingHeight,
                    framingDepth
                )
            );

            hasPreviewFramingBounds = true;
            camTargetPivot =
                previewFramingBounds.center;

            camRotation =
                new Vector2(0f, 180f);

            camDistance = Mathf.Max(
                0.05f,
                framingHeight
            );

            previewCameraNeedsFraming = true;
        }
        else
        {
            camTargetPivot =
                previewInstance.transform.position +
                Vector3.up * 1.15f;

            camDistance = 0.75f;
            camRotation =
                new Vector2(0f, 180f);

            hasPreviewFramingBounds = false;
            previewCameraNeedsFraming = false;
        }
    }

    private SkinnedMeshRenderer FindMatchingPreviewFaceRenderer()
    {
        if (targetModel == null ||
            faceSMR == null ||
            previewInstance == null)
        {
            return null;
        }

        Transform sourceRoot = targetModel.transform;
        Transform sourceTransform = faceSMR.transform;
        List<int> siblingIndexPath = new List<int>();

        while (sourceTransform != sourceRoot)
        {
            if (sourceTransform == null ||
                sourceTransform.parent == null)
            {
                return null;
            }

            siblingIndexPath.Add(
                sourceTransform.GetSiblingIndex()
            );

            sourceTransform = sourceTransform.parent;
        }

        Transform previewTransform =
            previewInstance.transform;

        for (int pathIndex =
                 siblingIndexPath.Count - 1;
             pathIndex >= 0;
             pathIndex--)
        {
            int siblingIndex =
                siblingIndexPath[pathIndex];

            if (siblingIndex < 0 ||
                siblingIndex >= previewTransform.childCount)
            {
                return null;
            }

            previewTransform =
                previewTransform.GetChild(siblingIndex);
        }

        SkinnedMeshRenderer[] sourceRenderers =
            faceSMR.GetComponents<SkinnedMeshRenderer>();

        SkinnedMeshRenderer[] previewRenderers =
            previewTransform.GetComponents
                <SkinnedMeshRenderer>();

        int componentIndex =
            Array.IndexOf(sourceRenderers, faceSMR);

        if (componentIndex < 0 ||
            componentIndex >= previewRenderers.Length)
        {
            return null;
        }

        return previewRenderers[componentIndex];
    }

    private void UpdatePreviewBackgroundColor()
    {
        Repaint();
    }
}
