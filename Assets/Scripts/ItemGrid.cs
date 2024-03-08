using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemGrid : MonoBehaviour
{
    const float TileSizeWidth = 32f;
    const float TileSizeHeight = 32f;

    RectTransform rectTransform;

    Vector2 mousePositionOnTheGrid; //�׸��� ���� ��ܿ��� ���콺�� ��ġ ��
    Vector2Int tileGridPosition = new Vector2Int(); //�׸��� �������� ��ǥ
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }
    public Vector2Int GetTileGridPosition(Vector2 mousePosition) //�׸��� ���� ����� 0,0 ������ �ϴ��� max,max���ϴ� �׸��� �������� ��ǥ��ȯ
    {
        mousePositionOnTheGrid.x = mousePosition.x - rectTransform.position.x;
        mousePositionOnTheGrid.y = rectTransform.position.y - mousePosition.y;

        Vector2Int tileGridPosition = new Vector2Int();
        tileGridPosition.x = (int)(mousePositionOnTheGrid.x / TileSizeWidth);
        tileGridPosition.y = (int)(mousePositionOnTheGrid.y / TileSizeHeight);

        return tileGridPosition;
    }
    private void Update()
    {
        tileGridPosition = GetTileGridPosition(Input.mousePosition);
        Debug.Log($"{tileGridPosition}");
    }
}
