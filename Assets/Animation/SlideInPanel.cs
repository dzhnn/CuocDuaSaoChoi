using UnityEngine;

public class SlideInPanel : MonoBehaviour
{
    public Vector3 targetPositionIn;   // Vị trí đích khi panel xuất hiện (trong khung nhìn)
    public Vector3 targetPositionOut;  // Vị trí đích khi panel ẩn đi (ngoài khung nhìn bên phải)
    public float slideSpeed = 5f;      // Tốc độ di chuyển

    private RectTransform rectTransform;
    private bool isSliding = false;    // Biến để kiểm tra panel có đang trượt không
    private bool isVisible = false;    // Trạng thái của panel (đang hiển thị hay ẩn)

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        // Đặt panel ở vị trí ẩn ban đầu (ngoài khung nhìn bên phải)
        rectTransform.anchoredPosition = targetPositionOut;
    }

    // Hàm này sẽ được gọi khi button được click
    public void ToggleSlide()
    {
        isSliding = true;
        isVisible = !isVisible;  // Chuyển đổi trạng thái hiển thị
    }

    void Update()
    {
        if (isSliding)
        {
            // Xác định vị trí đích dựa trên trạng thái hiển thị
            Vector3 targetPosition = isVisible ? targetPositionIn : targetPositionOut;

            // Di chuyển panel tới vị trí đích
            rectTransform.anchoredPosition = Vector3.Lerp(
                rectTransform.anchoredPosition,
                targetPosition,
                Time.deltaTime * slideSpeed);

            // Kiểm tra nếu panel đã đến gần vị trí đích
            if (Vector3.Distance(rectTransform.anchoredPosition, targetPosition) < 0.1f)
            {
                rectTransform.anchoredPosition = targetPosition; // Đặt đúng vị trí đích
                isSliding = false; // Dừng trượt
            }
        }
    }
}
