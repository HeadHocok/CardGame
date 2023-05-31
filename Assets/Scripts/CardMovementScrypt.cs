using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardMovementScrypt : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public CardController CC;

    private Camera MainCamera;
    private GameObject TempCardGO;

    Vector3 offset;
    public Transform DefaultParent, DefaultTempCardParent;
    public bool IsDraggable;

    int startID;

    void Awake()
    {
        MainCamera = Camera.allCameras[0];
        TempCardGO = GameObject.Find("TempCardGO");
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        offset = transform.position - MainCamera.ScreenToWorldPoint(eventData.position); //вычитаем разницу отступа мыши от центра карты чтобы плавно брать карту

        DefaultParent = DefaultTempCardParent = transform.parent; //Дочерним hand

        IsDraggable = GameManagerScrypt.Instance.IsPlayerTurn &&
                      (
                      (DefaultParent.GetComponent<DropPlaceScrypt>().Type == FieldType.SELF_HAND &&
                      GameManagerScrypt.Instance.CurrentGame.Player.Mana >= CC.Card.Manacost) ||
                      (DefaultParent.GetComponent<DropPlaceScrypt>().Type == FieldType.SELF_FIELD &&
                      CC.Card.CanAttack)
                      ); //Истинно в наш ход когда в нашей руке + хватает маны, или на поле и может атаковать

        if (!IsDraggable)
            return;

        startID = transform.GetSiblingIndex(); //заносим индекс каждый раз когда игрок берет карту

        if (CC.Card.IsSpell || CC.Card.CanAttack)
            GameManagerScrypt.Instance.HighlightTargets(CC, true); //подсвечиваем карты противника только тогда когда держим карту

        TempCardGO.transform.SetParent(DefaultParent);  //ставим на место взятой карты
        TempCardGO.transform.SetSiblingIndex(transform.GetSiblingIndex());

        transform.SetParent(DefaultParent.parent); //Дочерним bg
        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!IsDraggable)
            return;

        Vector3 newPos = MainCamera.ScreenToWorldPoint(eventData.position); //мировые координаты мыши
        transform.position = newPos + offset;

        if (!CC.Card.IsSpell)
        {
            if (TempCardGO.transform.parent != DefaultTempCardParent)
                TempCardGO.transform.SetParent(DefaultTempCardParent);

            if (DefaultParent.GetComponent<DropPlaceScrypt>().Type != FieldType.SELF_FIELD)
                CheckPosition();
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!IsDraggable)
            return;

        GameManagerScrypt.Instance.HighlightTargets(CC, false);

        transform.SetParent(DefaultParent); //Дочерним hand
        GetComponent<CanvasGroup>().blocksRaycasts = true;

        transform.SetSiblingIndex(TempCardGO.transform.GetSiblingIndex());
        TempCardGO.transform.SetParent(GameObject.Find("Canvas").transform); //Перемещаем за пределы поля
        TempCardGO.transform.localPosition = new Vector3(2800, 0);
    }

    void CheckPosition()
    {
        int newIndex = DefaultTempCardParent.childCount; //кол-во карт

        for (int i = 0; i < DefaultTempCardParent.childCount; i++)
        {
            if (transform.position.x < DefaultTempCardParent.GetChild(i).position.x)
            {
                newIndex = i;

                if (TempCardGO.transform.GetSiblingIndex() < newIndex)  //не считаем временную карту
                    newIndex--;

                break;
            }
        }

        if (TempCardGO.transform.parent == DefaultParent) //нельзя перемещать карты между другими картами в руке
            newIndex = startID;

        TempCardGO.transform.SetSiblingIndex(newIndex);
    }

    public void MoveToField(Transform field)
    {
        transform.SetParent(GameObject.Find("Canvas").transform);

        float xOffset = Random.Range(-4.0f, 4.0f);
        float yOffset = Random.Range(-0.10f, 0.10f);
        Vector3 newPosition = field.position + new Vector3(xOffset, yOffset, 0);
        transform.DOMove(newPosition, 0.5f).SetEase(Ease.OutCirc);
    }

    public void MoveToTarget(Transform target)
    {
        StartCoroutine(MoveToTargetCore(target));
    }

    IEnumerator MoveToTargetCore(Transform target) //Плавная анимация удара
    {
        Vector3 pos = transform.position;
        Transform parent = transform.parent;
        int index = transform.GetSiblingIndex();

        if (transform.parent.GetComponent<HorizontalLayoutGroup>())
            transform.parent.GetComponent<HorizontalLayoutGroup>().enabled = false; //Отключаем смещение карт на поле

        transform.SetParent(GameObject.Find("Canvas").transform);

        transform.DOMove(target.position, 0.25f).SetEase(Ease.InOutQuart);

        yield return new WaitForSeconds(0.25f);

        transform.DOMove(pos, 0.25f).SetEase(Ease.InOutQuart);

        yield return new WaitForSeconds(0.25f);

        transform.SetParent(parent);
        transform.SetSiblingIndex(index); //Возвращаем позицию в иерархии

        if (transform.parent.GetComponent<HorizontalLayoutGroup>())
            transform.parent.GetComponent<HorizontalLayoutGroup>().enabled = true;
    }
}
