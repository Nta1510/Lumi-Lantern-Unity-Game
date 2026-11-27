using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LumiLantern
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        private Canvas mainCanvas;

        // Top Bar
        private Text textLevelTitle;
        private Text textChapterSubtitle;
        private Text textMirrorCount;
        private Image imgMirrorIcon;

        // Bottom Bar & Tutorial
        private Text textTutorial;
        private Text textStatus;
        private Button btnUndo;
        private Button btnReset;
        private Button btnHint;
        private Text textHintCount;

        // Tutorial Hand Pointer
        private GameObject handPointerObj;
        private RectTransform handPointerRect;
        private Coroutine handPointerRoutine;

        // Win Modal
        private GameObject winModalObj;
        private RectTransform winCardRect;
        private Image[] starImages = new Image[3];
        private Text textWinTitle;
        private Text textWinSubtitle;
        private Button btnNextLevel;
        private Button btnReplay;

        // Chapter Select Panel
        private GameObject chapterPanelObj;
        private Text textTotalStars;
        private Transform chapterCardsContainer;

        private void Awake()
        {
            Instance = this;
        }

        public void Initialize()
        {
            try
            {
                BuildUI();
                // Force hide overlays after build
                HideWinModal();
                HideChapterSelect();
                Debug.Log("[UIManager] BuildUI completed successfully.");
            }
            catch (System.Exception e)
            {
                Debug.LogError("[UIManager] BuildUI failed: " + e.Message + "\n" + e.StackTrace);
            }
        }

        public static Font GetSafeFont()
        {
            Font font = null;
            // Unity 2022+: Arial.ttf is deprecated, use LegacyRuntime.ttf
            try { font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch { }
            if (font == null)
            {
                try { font = Resources.GetBuiltinResource<Font>("Arial.ttf"); } catch { }
            }
            if (font == null)
            {
                Font[] all = Resources.FindObjectsOfTypeAll<Font>();
                if (all != null && all.Length > 0) font = all[0];
            }
            return font;
        }

        private void BuildUI()
        {
            // 1. Create Canvas
            GameObject canvasObj = new GameObject("MainCanvas");
            canvasObj.transform.SetParent(transform, false);
            mainCanvas = canvasObj.AddComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            mainCanvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = (Screen.width > Screen.height) ? 1.0f : 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            // Ensure EventSystem
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject esObj = new GameObject("EventSystem");
                esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            Font font = GetSafeFont();

            // Build Screens
            BuildTopBar(canvasObj.transform, font);
            BuildBottomControls(canvasObj.transform, font);
            BuildTutorialHand(canvasObj.transform);
            BuildWinModal(canvasObj.transform, font);
            BuildChapterSelect(canvasObj.transform, font);
        }

        private void BuildTopBar(Transform parent, Font font)
        {
            GameObject topBar = new GameObject("TopBar");
            topBar.transform.SetParent(parent, false);
            RectTransform rect = topBar.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0.5f, 1);
            rect.anchoredPosition = new Vector2(0, -60);
            rect.sizeDelta = new Vector2(0, 180);

            // Menu Button (Left)
            GameObject btnMenuObj = new GameObject("BtnMenu");
            btnMenuObj.transform.SetParent(topBar.transform, false);
            RectTransform btnMenuRect = btnMenuObj.AddComponent<RectTransform>();
            btnMenuRect.anchorMin = new Vector2(0, 0.5f);
            btnMenuRect.anchorMax = new Vector2(0, 0.5f);
            btnMenuRect.anchoredPosition = new Vector2(90, 0);
            btnMenuRect.sizeDelta = new Vector2(100, 100);

            Image btnMenuBg = btnMenuObj.AddComponent<Image>();
            btnMenuBg.sprite = LumiAssetLoader.GetSprite("btn_dark");
            btnMenuBg.type = Image.Type.Sliced;
            Button btnMenu = btnMenuObj.AddComponent<Button>();
            btnMenu.onClick.AddListener(() => GameManager.Instance?.OpenChapterSelect());

            GameObject iconMenuObj = new GameObject("Icon");
            iconMenuObj.transform.SetParent(btnMenuObj.transform, false);
            RectTransform iconMenuRect = iconMenuObj.AddComponent<RectTransform>();
            iconMenuRect.sizeDelta = new Vector2(50, 50);
            Image imgMenu = iconMenuObj.AddComponent<Image>();
            imgMenu.sprite = LumiAssetLoader.GetSprite("icon_menu");

            // Center Titles (Màn X, Chương Y)
            GameObject centerObj = new GameObject("CenterTitles");
            centerObj.transform.SetParent(topBar.transform, false);
            RectTransform centerRect = centerObj.AddComponent<RectTransform>();
            centerRect.anchorMin = new Vector2(0.5f, 0.5f);
            centerRect.anchorMax = new Vector2(0.5f, 0.5f);
            centerRect.anchoredPosition = Vector2.zero;
            centerRect.sizeDelta = new Vector2(600, 140);

            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(centerObj.transform, false);
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.45f);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.anchoredPosition = Vector2.zero;
            textLevelTitle = titleObj.AddComponent<Text>();
            textLevelTitle.font = font;
            textLevelTitle.fontSize = 52;
            textLevelTitle.fontStyle = FontStyle.Bold;
            textLevelTitle.alignment = TextAnchor.MiddleCenter;
            textLevelTitle.color = Color.white;
            textLevelTitle.text = "Màn 1";

            GameObject subtitleObj = new GameObject("Subtitle");
            subtitleObj.transform.SetParent(centerObj.transform, false);
            RectTransform subtitleRect = subtitleObj.AddComponent<RectTransform>();
            subtitleRect.anchorMin = new Vector2(0, 0);
            subtitleRect.anchorMax = new Vector2(1, 0.45f);
            subtitleRect.anchoredPosition = Vector2.zero;
            textChapterSubtitle = subtitleObj.AddComponent<Text>();
            textChapterSubtitle.font = font;
            textChapterSubtitle.fontSize = 28;
            textChapterSubtitle.alignment = TextAnchor.MiddleCenter;
            textChapterSubtitle.color = new Color(0.72f, 0.68f, 0.85f);
            textChapterSubtitle.text = "Chương 1 · Ánh sáng đầu tiên";

            // Mirror Counter Badge (Right)
            GameObject badgeObj = new GameObject("MirrorBadge");
            badgeObj.transform.SetParent(topBar.transform, false);
            RectTransform badgeRect = badgeObj.AddComponent<RectTransform>();
            badgeRect.anchorMin = new Vector2(1, 0.5f);
            badgeRect.anchorMax = new Vector2(1, 0.5f);
            badgeRect.anchoredPosition = new Vector2(-100, 0);
            badgeRect.sizeDelta = new Vector2(170, 75);

            Image badgeBg = badgeObj.AddComponent<Image>();
            badgeBg.sprite = LumiAssetLoader.GetSprite("btn_dark");
            badgeBg.type = Image.Type.Sliced;

            // Mirror icon
            GameObject mIconObj = new GameObject("MirrorIcon");
            mIconObj.transform.SetParent(badgeObj.transform, false);
            RectTransform mIconRect = mIconObj.AddComponent<RectTransform>();
            mIconRect.anchorMin = new Vector2(0, 0.5f);
            mIconRect.anchorMax = new Vector2(0, 0.5f);
            mIconRect.anchoredPosition = new Vector2(35, 0);
            mIconRect.sizeDelta = new Vector2(40, 40);
            imgMirrorIcon = mIconObj.AddComponent<Image>();
            imgMirrorIcon.sprite = LumiAssetLoader.GetSprite("icon_mirror");

            // Mirror count text
            GameObject mCountObj = new GameObject("MirrorCount");
            mCountObj.transform.SetParent(badgeObj.transform, false);
            RectTransform mCountRect = mCountObj.AddComponent<RectTransform>();
            mCountRect.anchorMin = new Vector2(0.4f, 0);
            mCountRect.anchorMax = new Vector2(1, 1);
            mCountRect.anchoredPosition = Vector2.zero;
            textMirrorCount = mCountObj.AddComponent<Text>();
            textMirrorCount.font = font;
            textMirrorCount.fontSize = 32;
            textMirrorCount.fontStyle = FontStyle.Bold;
            textMirrorCount.alignment = TextAnchor.MiddleCenter;
            textMirrorCount.color = Color.white;
            textMirrorCount.text = "0/1";
        }

        private void BuildBottomControls(Transform parent, Font font)
        {
            GameObject bottomObj = new GameObject("BottomControls");
            bottomObj.transform.SetParent(parent, false);
            RectTransform rect = bottomObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(1, 0);
            rect.pivot = new Vector2(0.5f, 0);
            rect.anchoredPosition = new Vector2(0, 60);
            rect.sizeDelta = new Vector2(0, 450);

            // Tutorial Hint Text
            GameObject tutObj = new GameObject("TutorialText");
            tutObj.transform.SetParent(bottomObj.transform, false);
            RectTransform tutRect = tutObj.AddComponent<RectTransform>();
            tutRect.anchorMin = new Vector2(0.08f, 0.72f);
            tutRect.anchorMax = new Vector2(0.92f, 1f);
            tutRect.anchoredPosition = Vector2.zero;
            textTutorial = tutObj.AddComponent<Text>();
            textTutorial.font = font;
            textTutorial.fontSize = 32;
            textTutorial.alignment = TextAnchor.MiddleCenter;
            textTutorial.color = new Color(0.85f, 0.82f, 0.95f);
            textTutorial.raycastTarget = false;
            textTutorial.text = "Chạm ô trống để đặt gương · chạm lần nữa để đổi chiều";

            // Status Text (Tia đã thắp được X đèn. Còn Y gương: ...)
            GameObject statObj = new GameObject("StatusText");
            statObj.transform.SetParent(bottomObj.transform, false);
            RectTransform statRect = statObj.AddComponent<RectTransform>();
            statRect.anchorMin = new Vector2(0.08f, 0.44f);
            statRect.anchorMax = new Vector2(0.92f, 0.72f);
            statRect.anchoredPosition = Vector2.zero;
            textStatus = statObj.AddComponent<Text>();
            textStatus.font = font;
            textStatus.fontSize = 30;
            textStatus.alignment = TextAnchor.MiddleCenter;
            textStatus.color = new Color(0.95f, 0.85f, 0.5f);
            textStatus.raycastTarget = false;
            textStatus.text = "Tia đã thắp được 0 đèn.";

            // 3 Thumb Buttons Row
            GameObject btnRow = new GameObject("ButtonRow");
            btnRow.transform.SetParent(bottomObj.transform, false);
            RectTransform rowRect = btnRow.AddComponent<RectTransform>();
            rowRect.anchorMin = new Vector2(0.08f, 0);
            rowRect.anchorMax = new Vector2(0.92f, 0.42f);
            rowRect.anchoredPosition = Vector2.zero;

            // 1. Undo Button (↩ Hoàn tác)
            btnUndo = CreateActionButton(btnRow.transform, font, "Hoàn tác", "icon_undo", "btn_dark", false, () => {
                GridManager.Instance?.Undo();
            });
            RectTransform undoR = btnUndo.GetComponent<RectTransform>();
            undoR.anchorMin = new Vector2(0, 0);
            undoR.anchorMax = new Vector2(0.30f, 1);
            undoR.anchoredPosition = Vector2.zero;

            // 2. Reset Button (🔄 Đặt lại)
            btnReset = CreateActionButton(btnRow.transform, font, "Đặt lại", "icon_reset", "btn_dark", false, () => {
                GridManager.Instance?.ResetBoard();
            });
            RectTransform resetR = btnReset.GetComponent<RectTransform>();
            resetR.anchorMin = new Vector2(0.35f, 0);
            resetR.anchorMax = new Vector2(0.65f, 1);
            resetR.anchoredPosition = Vector2.zero;

            // 3. Hint Button (💡 Gợi ý · 3)
            btnHint = CreateActionButton(btnRow.transform, font, "Gợi ý · 3", "icon_hint", "btn_gold", true, () => {
                GameManager.Instance?.RequestHint();
            });
            RectTransform hintR = btnHint.GetComponent<RectTransform>();
            hintR.anchorMin = new Vector2(0.70f, 0);
            hintR.anchorMax = new Vector2(1, 1);
            hintR.anchoredPosition = Vector2.zero;
            textHintCount = btnHint.GetComponentInChildren<Text>();
        }

        private Button CreateActionButton(Transform parent, Font font, string label, string iconName, string bgSprite, bool isGold, Action onClick)
        {
            GameObject btnObj = new GameObject($"Btn_{label}");
            btnObj.transform.SetParent(parent, false);

            Image img = btnObj.AddComponent<Image>();
            img.sprite = LumiAssetLoader.GetSprite(bgSprite);
            img.type = Image.Type.Sliced;

            Button btn = btnObj.AddComponent<Button>();
            btn.onClick.AddListener(() => {
                AudioManager.Instance?.PlayClick();
                onClick?.Invoke();
            });

            // Icon
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(btnObj.transform, false);
            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0.62f);
            iconRect.anchorMax = new Vector2(0.5f, 0.62f);
            iconRect.sizeDelta = new Vector2(56, 56);
            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.sprite = LumiAssetLoader.GetSprite(iconName);
            iconImg.raycastTarget = false;

            // Text
            GameObject txtObj = new GameObject("Text");
            txtObj.transform.SetParent(btnObj.transform, false);
            RectTransform txtRect = txtObj.AddComponent<RectTransform>();
            txtRect.anchorMin = new Vector2(0, 0.1f);
            txtRect.anchorMax = new Vector2(1, 0.42f);
            txtRect.anchoredPosition = Vector2.zero;
            Text txt = txtObj.AddComponent<Text>();
            txt.font = font;
            txt.fontSize = 28;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = isGold ? new Color(0.15f, 0.12f, 0.05f) : Color.white;
            txt.raycastTarget = false;
            txt.text = label;

            return btn;
        }

        private void BuildTutorialHand(Transform parent)
        {
            handPointerObj = new GameObject("TutorialHand");
            handPointerObj.transform.SetParent(parent, false);
            handPointerRect = handPointerObj.AddComponent<RectTransform>();
            handPointerRect.sizeDelta = new Vector2(120, 120);

            Image img = handPointerObj.AddComponent<Image>();
            img.sprite = LumiAssetLoader.GetSprite("hand_pointer");
            img.raycastTarget = false;

            handPointerObj.SetActive(false);
        }

        public void ShowTutorialHand(Vector2 screenPos)
        {
            if (handPointerObj == null) return;
            handPointerObj.SetActive(true);
            handPointerRect.position = screenPos + new Vector2(30, -30);
            if (handPointerRoutine != null) StopCoroutine(handPointerRoutine);
            handPointerRoutine = StartCoroutine(AnimateHand(screenPos));
        }

        public void HideTutorialHand()
        {
            if (handPointerRoutine != null) StopCoroutine(handPointerRoutine);
            if (handPointerObj != null) handPointerObj.SetActive(false);
        }

        private IEnumerator AnimateHand(Vector2 targetPos)
        {
            while (true)
            {
                float t = (Mathf.Sin(Time.time * 4f) + 1f) * 0.5f;
                Vector2 offset = new Vector2(30f + t * 25f, -30f - t * 25f);
                handPointerRect.position = targetPos + offset;
                yield return null;
            }
        }

        private void BuildWinModal(Transform parent, Font font)
        {
            winModalObj = new GameObject("WinModal");
            winModalObj.transform.SetParent(parent, false);
            RectTransform modalRect = winModalObj.AddComponent<RectTransform>();
            modalRect.anchorMin = Vector2.zero;
            modalRect.anchorMax = Vector2.one;
            modalRect.sizeDelta = Vector2.zero;

            // Semi-transparent backdrop
            Image backdrop = winModalObj.AddComponent<Image>();
            backdrop.color = new Color(0, 0, 0, 0.45f);

            // Win Card (Sliding panel from bottom)
            GameObject cardObj = new GameObject("Card");
            cardObj.transform.SetParent(winModalObj.transform, false);
            winCardRect = cardObj.AddComponent<RectTransform>();
            winCardRect.anchorMin = new Vector2(0.06f, 0.05f);
            winCardRect.anchorMax = new Vector2(0.94f, 0.50f);
            winCardRect.anchoredPosition = Vector2.zero;

            Image cardBg = cardObj.AddComponent<Image>();
            cardBg.sprite = LumiAssetLoader.GetSprite("card_bg");
            cardBg.type = Image.Type.Sliced;

            // Star container
            GameObject starBox = new GameObject("Stars");
            starBox.transform.SetParent(cardObj.transform, false);
            RectTransform starRect = starBox.AddComponent<RectTransform>();
            starRect.anchorMin = new Vector2(0.5f, 0.82f);
            starRect.anchorMax = new Vector2(0.5f, 0.82f);
            starRect.sizeDelta = new Vector2(400, 100);

            for (int i = 0; i < 3; i++)
            {
                GameObject sObj = new GameObject($"Star_{i + 1}");
                sObj.transform.SetParent(starBox.transform, false);
                RectTransform sRect = sObj.AddComponent<RectTransform>();
                sRect.anchorMin = new Vector2(0.5f, 0.5f);
                sRect.anchorMax = new Vector2(0.5f, 0.5f);
                sRect.anchoredPosition = new Vector2((i - 1) * 110f, 0);
                sRect.sizeDelta = new Vector2(90, 90);

                Image sImg = sObj.AddComponent<Image>();
                sImg.sprite = LumiAssetLoader.GetSprite("star_filled");
                starImages[i] = sImg;
            }

            // Win Title ("Tuyệt vời!")
            GameObject winTitleObj = new GameObject("WinTitle");
            winTitleObj.transform.SetParent(cardObj.transform, false);
            RectTransform wtRect = winTitleObj.AddComponent<RectTransform>();
            wtRect.anchorMin = new Vector2(0, 0.58f);
            wtRect.anchorMax = new Vector2(1, 0.74f);
            wtRect.anchoredPosition = Vector2.zero;
            textWinTitle = winTitleObj.AddComponent<Text>();
            textWinTitle.font = font;
            textWinTitle.fontSize = 58;
            textWinTitle.fontStyle = FontStyle.Bold;
            textWinTitle.alignment = TextAnchor.MiddleCenter;
            textWinTitle.color = Color.white;
            textWinTitle.text = "Tuyệt vời!";

            // Win Subtitle
            GameObject winSubObj = new GameObject("WinSubtitle");
            winSubObj.transform.SetParent(cardObj.transform, false);
            RectTransform wsRect = winSubObj.AddComponent<RectTransform>();
            wsRect.anchorMin = new Vector2(0, 0.44f);
            wsRect.anchorMax = new Vector2(1, 0.58f);
            wsRect.anchoredPosition = Vector2.zero;
            textWinSubtitle = winSubObj.AddComponent<Text>();
            textWinSubtitle.font = font;
            textWinSubtitle.fontSize = 30;
            textWinSubtitle.alignment = TextAnchor.MiddleCenter;
            textWinSubtitle.color = new Color(0.85f, 0.82f, 0.95f);
            textWinSubtitle.text = "Thắp sáng bằng 3 gương, số gương ít nhất";

            // Next Level Button (Golden)
            GameObject nextObj = new GameObject("BtnNext");
            nextObj.transform.SetParent(cardObj.transform, false);
            RectTransform nextRect = nextObj.AddComponent<RectTransform>();
            nextRect.anchorMin = new Vector2(0.08f, 0.22f);
            nextRect.anchorMax = new Vector2(0.92f, 0.40f);
            nextRect.anchoredPosition = Vector2.zero;

            Image nextBg = nextObj.AddComponent<Image>();
            nextBg.sprite = LumiAssetLoader.GetSprite("btn_gold");
            nextBg.type = Image.Type.Sliced;
            btnNextLevel = nextObj.AddComponent<Button>();
            btnNextLevel.onClick.AddListener(() => {
                AudioManager.Instance?.PlayClick();
                GameManager.Instance?.GoNextLevel();
            });

            GameObject nextTxtObj = new GameObject("Text");
            nextTxtObj.transform.SetParent(nextObj.transform, false);
            RectTransform ntRect = nextTxtObj.AddComponent<RectTransform>();
            ntRect.anchorMin = Vector2.zero;
            ntRect.anchorMax = Vector2.one;
            ntRect.sizeDelta = Vector2.zero;
            Text nextTxt = nextTxtObj.AddComponent<Text>();
            nextTxt.font = font;
            nextTxt.fontSize = 38;
            nextTxt.fontStyle = FontStyle.Bold;
            nextTxt.alignment = TextAnchor.MiddleCenter;
            nextTxt.color = new Color(0.15f, 0.12f, 0.05f);
            nextTxt.text = "Màn tiếp theo";

            // Replay Button (Dark)
            GameObject repObj = new GameObject("BtnReplay");
            repObj.transform.SetParent(cardObj.transform, false);
            RectTransform repRect = repObj.AddComponent<RectTransform>();
            repRect.anchorMin = new Vector2(0.08f, 0.04f);
            repRect.anchorMax = new Vector2(0.92f, 0.18f);
            repRect.anchoredPosition = Vector2.zero;

            Image repBg = repObj.AddComponent<Image>();
            repBg.sprite = LumiAssetLoader.GetSprite("btn_dark");
            repBg.type = Image.Type.Sliced;
            btnReplay = repObj.AddComponent<Button>();
            btnReplay.onClick.AddListener(() => {
                AudioManager.Instance?.PlayClick();
                GameManager.Instance?.ReplayLevel();
            });

            GameObject repTxtObj = new GameObject("Text");
            repTxtObj.transform.SetParent(repObj.transform, false);
            RectTransform rtRect = repTxtObj.AddComponent<RectTransform>();
            rtRect.anchorMin = Vector2.zero;
            rtRect.anchorMax = Vector2.one;
            rtRect.sizeDelta = Vector2.zero;
            Text repTxt = repTxtObj.AddComponent<Text>();
            repTxt.font = font;
            repTxt.fontSize = 32;
            repTxt.fontStyle = FontStyle.Bold;
            repTxt.alignment = TextAnchor.MiddleCenter;
            repTxt.color = Color.white;
            repTxt.text = "Chơi lại";

            winModalObj.SetActive(false);
        }

        private void BuildChapterSelect(Transform parent, Font font)
        {
            chapterPanelObj = new GameObject("ChapterPanel");
            chapterPanelObj.transform.SetParent(parent, false);
            RectTransform panelRect = chapterPanelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.sizeDelta = Vector2.zero;

            Image bg = chapterPanelObj.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.07f, 0.12f, 0.98f);

            // Total Stars Badge (Right)
            // Header
            GameObject headerObj = new GameObject("Header");
            headerObj.transform.SetParent(chapterPanelObj.transform, false);
            RectTransform hRect = headerObj.AddComponent<RectTransform>();
            hRect.anchorMin = new Vector2(0, 1);
            hRect.anchorMax = new Vector2(1, 1);
            hRect.pivot = new Vector2(0.5f, 1);
            hRect.anchoredPosition = new Vector2(0, -60);
            hRect.sizeDelta = new Vector2(0, 160);

            // Back button
            GameObject btnBackObj = new GameObject("BtnBack");
            btnBackObj.transform.SetParent(headerObj.transform, false);
            RectTransform backRect = btnBackObj.AddComponent<RectTransform>();
            backRect.anchorMin = new Vector2(0, 0.5f);
            backRect.anchorMax = new Vector2(0, 0.5f);
            backRect.anchoredPosition = new Vector2(90, 0);
            backRect.sizeDelta = new Vector2(90, 90);

            Image backBg = btnBackObj.AddComponent<Image>();
            backBg.sprite = LumiAssetLoader.GetSprite("btn_dark");
            backBg.type = Image.Type.Sliced;
            Button btnBack = btnBackObj.AddComponent<Button>();
            btnBack.onClick.AddListener(() => {
                AudioManager.Instance?.PlayClick();
                GameManager.Instance?.CloseChapterSelect();
            });

            GameObject backIconObj = new GameObject("Icon");
            backIconObj.transform.SetParent(btnBackObj.transform, false);
            RectTransform biRect = backIconObj.AddComponent<RectTransform>();
            biRect.sizeDelta = new Vector2(45, 45);
            Image imgBack = backIconObj.AddComponent<Image>();
            imgBack.sprite = LumiAssetLoader.GetSprite("icon_back");

            // Header Title ("Chọn chương")
            GameObject htObj = new GameObject("Title");
            htObj.transform.SetParent(headerObj.transform, false);
            RectTransform htRect = htObj.AddComponent<RectTransform>();
            htRect.anchorMin = new Vector2(0.5f, 0.5f);
            htRect.anchorMax = new Vector2(0.5f, 0.5f);
            htRect.anchoredPosition = new Vector2(0, 15);
            htRect.sizeDelta = new Vector2(500, 70);
            Text htTxt = htObj.AddComponent<Text>();
            htTxt.font = font;
            htTxt.fontSize = 48;
            htTxt.fontStyle = FontStyle.Bold;
            htTxt.alignment = TextAnchor.MiddleCenter;
            htTxt.color = Color.white;
            htTxt.text = "Chọn chương";

            // Header Subtitle
            GameObject hsObj = new GameObject("Subtitle");
            hsObj.transform.SetParent(headerObj.transform, false);
            RectTransform hsRect = hsObj.AddComponent<RectTransform>();
            hsRect.anchorMin = new Vector2(0.5f, 0.5f);
            hsRect.anchorMax = new Vector2(0.5f, 0.5f);
            hsRect.anchoredPosition = new Vector2(0, -35);
            hsRect.sizeDelta = new Vector2(600, 50);
            Text hsTxt = hsObj.AddComponent<Text>();
            hsTxt.font = font;
            hsTxt.fontSize = 26;
            hsTxt.alignment = TextAnchor.MiddleCenter;
            hsTxt.color = new Color(0.72f, 0.68f, 0.85f);
            hsTxt.text = "Mỗi chương mở một bộ phận mới";

            // Total Stars Badge (Right)
            GameObject sBadgeObj = new GameObject("StarsBadge");
            sBadgeObj.transform.SetParent(headerObj.transform, false);
            RectTransform sbRect = sBadgeObj.AddComponent<RectTransform>();
            sbRect.anchorMin = new Vector2(1, 0.5f);
            sbRect.anchorMax = new Vector2(1, 0.5f);
            sbRect.anchoredPosition = new Vector2(-100, 0);
            sbRect.sizeDelta = new Vector2(160, 75);

            Image sbBg = sBadgeObj.AddComponent<Image>();
            sbBg.sprite = LumiAssetLoader.GetSprite("btn_dark");
            sbBg.type = Image.Type.Sliced;

            // Text on child GameObject so there is NO graphic conflict on sBadgeObj
            GameObject sTextObj = new GameObject("Text");
            sTextObj.transform.SetParent(sBadgeObj.transform, false);
            RectTransform stRect = sTextObj.AddComponent<RectTransform>();
            stRect.anchorMin = Vector2.zero;
            stRect.anchorMax = Vector2.one;
            stRect.sizeDelta = Vector2.zero;

            textTotalStars = sTextObj.AddComponent<Text>();
            textTotalStars.font = font;
            textTotalStars.fontSize = 32;
            textTotalStars.fontStyle = FontStyle.Bold;
            textTotalStars.alignment = TextAnchor.MiddleCenter;
            textTotalStars.color = new Color(1f, 0.84f, 0.25f);
            textTotalStars.text = "★ 52";

            // Scroll container for Chapter Cards
            GameObject scrollObj = new GameObject("ScrollContainer");
            scrollObj.transform.SetParent(chapterPanelObj.transform, false);
            RectTransform sContainerRect = scrollObj.AddComponent<RectTransform>();
            sContainerRect.anchorMin = new Vector2(0.06f, 0.05f);
            sContainerRect.anchorMax = new Vector2(0.94f, 0.86f);
            sContainerRect.anchoredPosition = Vector2.zero;

            VerticalLayoutGroup vlg = scrollObj.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 30;
            vlg.childForceExpandHeight = false;
            vlg.childControlHeight = false;
            chapterCardsContainer = scrollObj.transform;

            chapterPanelObj.SetActive(false);
        }

        public void UpdateHeader(LevelData level, int placedMirrors)
        {
            if (textLevelTitle == null) return;
            textLevelTitle.text = $"Màn {level.levelNumber}";
            textChapterSubtitle.text = level.chapterName;
            textMirrorCount.text = $"{placedMirrors}/{level.maxMirrors}";
            textTutorial.text = level.tutorialText;
            if (textHintCount != null) textHintCount.text = $"Gợi ý · {SaveSystem.GetHintsRemaining()}";
        }

        public void UpdateSimulationStatus(SimulationResult result, LevelData level)
        {
            if (textMirrorCount == null) return;
            textMirrorCount.text = $"{result.placedMirrorsCount}/{level.maxMirrors}";

            int remaining = level.maxMirrors - result.placedMirrorsCount;
            int lit = result.litLanterns.Count;
            int total = result.totalLanterns.Count;

            if (result.isWin)
            {
                textStatus.text = "Tất cả đèn đã thắp sáng rực rỡ!";
            }
            else
            {
                textStatus.text = $"Tia đã thắp được {lit}/{total} đèn. Còn {remaining} gương.";
            }
        }

        public void ShowWinModal(int stars, int placedMirrors, int minMirrors)
        {
            winModalObj.SetActive(true);

            // Animate stars
            for (int i = 0; i < 3; i++)
            {
                bool earned = (i < stars);
                starImages[i].sprite = LumiAssetLoader.GetSprite(earned ? "star_filled" : "star_empty");
            }

            textWinTitle.text = "Tuyệt vời!";
            if (placedMirrors <= minMirrors)
            {
                textWinSubtitle.text = $"Thắp sáng bằng {placedMirrors} gương, số gương ít nhất";
            }
            else
            {
                textWinSubtitle.text = $"Thắp sáng thành công với {placedMirrors} gương!";
            }

            // Animate slide up
            StartCoroutine(SlideUpModal());
        }

        public void HideWinModal()
        {
            if (winModalObj != null) winModalObj.SetActive(false);
        }

        private IEnumerator SlideUpModal()
        {
            Vector2 target = winCardRect.anchoredPosition;
            winCardRect.anchoredPosition = target - new Vector2(0, 500);
            float elapsed = 0f;
            float duration = 0.35f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0, 1, elapsed / duration);
                winCardRect.anchoredPosition = Vector2.Lerp(target - new Vector2(0, 500), target, t);
                yield return null;
            }
            winCardRect.anchoredPosition = target;
        }

        public bool IsChapterSelectOpen()
        {
            return chapterPanelObj != null && chapterPanelObj.activeSelf;
        }

        public bool IsWinModalOpen()
        {
            return winModalObj != null && winModalObj.activeSelf;
        }

        private float lastAspect = -1f;

        private void Update()
        {
            if (mainCanvas != null)
            {
                float currentAspect = (float)Screen.width / Mathf.Max(1, Screen.height);
                if (Mathf.Abs(lastAspect - currentAspect) > 0.02f)
                {
                    lastAspect = currentAspect;
                    CanvasScaler scaler = mainCanvas.GetComponent<CanvasScaler>();
                    if (scaler != null)
                    {
                        scaler.matchWidthOrHeight = (Screen.width > Screen.height) ? 1.0f : 0.5f;
                    }
                }
            }
        }

        public void ShowChapterSelect()
        {
            if (chapterPanelObj == null) return;
            chapterPanelObj.SetActive(true);
            textTotalStars.text = $"★ {SaveSystem.GetTotalStars()}";
            RefreshChapterCards();
        }

        public void HideChapterSelect()
        {
            if (chapterPanelObj != null) chapterPanelObj.SetActive(false);
        }

        private void RefreshChapterCards()
        {
            // Clear existing
            for (int i = chapterCardsContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(chapterCardsContainer.GetChild(i).gameObject);
            }

            Font font = GetSafeFont();
            var chapters = LevelDatabase.GetChapters();
            int totalStars = SaveSystem.GetTotalStars();

            for (int i = 0; i < chapters.Count; i++)
            {
                var ch = chapters[i];
                bool isUnlocked = (totalStars >= ch.requiredStarsToUnlock);

                GameObject cardObj = new GameObject($"ChapterCard_{ch.chapterNumber}");
                cardObj.transform.SetParent(chapterCardsContainer, false);
                RectTransform cr = cardObj.AddComponent<RectTransform>();
                cr.sizeDelta = new Vector2(940, 210);

                Image bg = cardObj.AddComponent<Image>();
                bg.sprite = LumiAssetLoader.GetSprite("card_bg");
                bg.type = Image.Type.Sliced;
                bg.color = isUnlocked ? Color.white : new Color(0.5f, 0.5f, 0.6f, 0.7f);

                Button btn = cardObj.AddComponent<Button>();
                int targetLevel = (ch.chapterNumber == 1) ? 1 :
                                  (ch.chapterNumber == 2) ? 6 :
                                  (ch.chapterNumber == 3) ? 8 :
                                  (ch.chapterNumber == 4) ? 10 : 13;

                if (isUnlocked)
                {
                    btn.onClick.AddListener(() => {
                        AudioManager.Instance?.PlayClick();
                        GameManager.Instance?.LoadLevel(targetLevel);
                        HideChapterSelect();
                    });
                }

                // Title
                GameObject tObj = new GameObject("Title");
                tObj.transform.SetParent(cardObj.transform, false);
                RectTransform tr = tObj.AddComponent<RectTransform>();
                tr.anchorMin = new Vector2(0.08f, 0.55f);
                tr.anchorMax = new Vector2(0.75f, 0.90f);
                tr.anchoredPosition = Vector2.zero;
                Text txtTitle = tObj.AddComponent<Text>();
                txtTitle.font = font;
                txtTitle.fontSize = 36;
                txtTitle.fontStyle = FontStyle.Bold;
                txtTitle.color = Color.white;
                txtTitle.text = ch.title;

                // Subtitle
                GameObject sObj = new GameObject("Subtitle");
                sObj.transform.SetParent(cardObj.transform, false);
                RectTransform sr = sObj.AddComponent<RectTransform>();
                sr.anchorMin = new Vector2(0.08f, 0.18f);
                sr.anchorMax = new Vector2(0.75f, 0.52f);
                sr.anchoredPosition = Vector2.zero;
                Text txtSub = sObj.AddComponent<Text>();
                txtSub.font = font;
                txtSub.fontSize = 26;
                txtSub.color = new Color(0.75f, 0.72f, 0.88f);
                txtSub.text = $"{ch.subtitle} · {ch.totalLevels} màn";

                // Lock or Star indicator on right
                GameObject rightObj = new GameObject("RightBadge");
                rightObj.transform.SetParent(cardObj.transform, false);
                RectTransform rr = rightObj.AddComponent<RectTransform>();
                rr.anchorMin = new Vector2(0.78f, 0.2f);
                rr.anchorMax = new Vector2(0.95f, 0.8f);
                rr.anchoredPosition = Vector2.zero;
                Text rightTxt = rightObj.AddComponent<Text>();
                rightTxt.font = font;
                rightTxt.fontSize = 30;
                rightTxt.fontStyle = FontStyle.Bold;
                rightTxt.alignment = TextAnchor.MiddleCenter;
                rightTxt.color = isUnlocked ? new Color(1f, 0.84f, 0.25f) : new Color(0.6f, 0.55f, 0.7f);
                rightTxt.text = isUnlocked ? $"★ Vào" : $"🔒 Cần {ch.requiredStarsToUnlock}★";
            }

            // Daily Puzzle Card ("Lồng đèn hôm nay")
            GameObject dailyObj = new GameObject("DailyCard");
            dailyObj.transform.SetParent(chapterCardsContainer, false);
            RectTransform dRect = dailyObj.AddComponent<RectTransform>();
            dRect.sizeDelta = new Vector2(940, 180);

            Image dBg = dailyObj.AddComponent<Image>();
            dBg.sprite = LumiAssetLoader.GetSprite("card_bg");
            dBg.type = Image.Type.Sliced;

            // Title
            GameObject dtObj = new GameObject("Title");
            dtObj.transform.SetParent(dailyObj.transform, false);
            RectTransform dtr = dtObj.AddComponent<RectTransform>();
            dtr.anchorMin = new Vector2(0.08f, 0.52f);
            dtr.anchorMax = new Vector2(0.68f, 0.90f);
            dtr.anchoredPosition = Vector2.zero;
            Text dTitle = dtObj.AddComponent<Text>();
            dTitle.font = font;
            dTitle.fontSize = 36;
            dTitle.fontStyle = FontStyle.Bold;
            dTitle.color = Color.white;
            dTitle.text = "Lồng đèn hôm nay";

            // Subtitle
            GameObject dsObj = new GameObject("Subtitle");
            dsObj.transform.SetParent(dailyObj.transform, false);
            RectTransform dsr = dsObj.AddComponent<RectTransform>();
            dsr.anchorMin = new Vector2(0.08f, 0.15f);
            dsr.anchorMax = new Vector2(0.68f, 0.50f);
            dsr.anchoredPosition = Vector2.zero;
            Text dSub = dsObj.AddComponent<Text>();
            dSub.font = font;
            dSub.fontSize = 26;
            dSub.color = new Color(0.75f, 0.72f, 0.88f);
            dSub.text = "Màn mới mỗi ngày · 3 gợi ý miễn phí";

            // Button "Chơi ▶"
            GameObject dBtnObj = new GameObject("BtnPlayDaily");
            dBtnObj.transform.SetParent(dailyObj.transform, false);
            RectTransform dbr = dBtnObj.AddComponent<RectTransform>();
            dbr.anchorMin = new Vector2(0.72f, 0.25f);
            dbr.anchorMax = new Vector2(0.94f, 0.75f);
            dbr.anchoredPosition = Vector2.zero;

            Image dBtnBg = dBtnObj.AddComponent<Image>();
            dBtnBg.sprite = LumiAssetLoader.GetSprite("btn_gold");
            dBtnBg.type = Image.Type.Sliced;
            Button dBtn = dBtnObj.AddComponent<Button>();
            dBtn.onClick.AddListener(() => {
                AudioManager.Instance?.PlayClick();
                GameManager.Instance?.LoadDailyLevel();
                HideChapterSelect();
            });

            GameObject dbtObj = new GameObject("Text");
            dbtObj.transform.SetParent(dBtnObj.transform, false);
            RectTransform dbtr = dbtObj.AddComponent<RectTransform>();
            dbtr.anchorMin = Vector2.zero;
            dbtr.anchorMax = Vector2.one;
            dbtr.sizeDelta = Vector2.zero;
            Text dBtnTxt = dbtObj.AddComponent<Text>();
            dBtnTxt.font = font;
            dBtnTxt.fontSize = 32;
            dBtnTxt.fontStyle = FontStyle.Bold;
            dBtnTxt.alignment = TextAnchor.MiddleCenter;
            dBtnTxt.color = new Color(0.15f, 0.12f, 0.05f);
            dBtnTxt.text = "Chơi ▶";
        }
    }
}
