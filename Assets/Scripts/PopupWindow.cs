using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

// 팝업창 하나하나에 붙는 스크립트
// 닫아야 하는 팝업인지, 닫으면 안 되는 중요 팝업인지 저장하고,
// 닫기 버튼 입력과 드래그 이동, 생성 애니메이션을 처리함.
public class PopupWindow : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    [Header("UI")]
    public TMP_Text popupText;
    public Button closeButton;
    public Image backgroundImage;

    [Header("Popup Data")]
    public bool shouldClose; // true면 닫아야 하는 팝업, false면 닫으면 안 되는 중요 팝업

    [Header("Spawn Animation")]
    public float spawnAnimationTime = 0.15f;
    public float startScale = 0.75f;

    private PopupCloseGame gameManager;
    private RectTransform rectTransform;
    private RectTransform parentRectTransform;
    private Vector2 dragOffset;

    private float animationTimer = 0f;
    private bool isAnimating = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Init(string message, bool shouldClose, PopupCloseGame manager)
    {
        this.shouldClose = shouldClose;
        this.gameManager = manager;

        popupText.text = message;

        // 테스트용 색상 구분
        // 나중에 최종 버전에서는 두 색을 비슷하게 만들면 난이도가 올라감
        if (backgroundImage != null)
        {
            if (shouldClose)
            {
                backgroundImage.color = new Color(0.85f, 0.85f, 0.85f);
            }
            else
            {
                backgroundImage.color = new Color(0.95f, 0.9f, 0.75f);
            }
        }

        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(OnCloseButtonClicked);

        StartSpawnAnimation();
    }

    void Update()
    {
        if (!isAnimating) return;

        animationTimer += Time.deltaTime;
        float t = Mathf.Clamp01(animationTimer / spawnAnimationTime);

        // 처음엔 작게 시작해서 원래 크기로 커지는 간단한 생성 애니메이션
        float scale = Mathf.Lerp(startScale, 1f, t);
        transform.localScale = new Vector3(scale, scale, 1f);

        if (t >= 1f)
        {
            isAnimating = false;
            transform.localScale = Vector3.one;
        }
    }

    void StartSpawnAnimation()
    {
        animationTimer = 0f;
        isAnimating = true;
        transform.localScale = new Vector3(startScale, startScale, 1f);
    }

    void OnCloseButtonClicked()
    {
        if (gameManager != null)
        {
            gameManager.OnPopupClosed(this);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 드래그를 시작한 팝업을 맨 앞으로 올림
        transform.SetAsLastSibling();

        parentRectTransform = transform.parent as RectTransform;

        Vector2 localPointerPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPointerPosition
        );

        dragOffset = rectTransform.anchoredPosition - localPointerPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (parentRectTransform == null) return;

        Vector2 localPointerPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPointerPosition
        );

        rectTransform.anchoredPosition = localPointerPosition + dragOffset;
    }
}