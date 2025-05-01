using UnityEngine;
using DG.Tweening;

public class PulsatingMarker : MonoBehaviour
{
    [SerializeField] private float alphaMin = 0.3f; // Минимальная прозрачность
    [SerializeField] private float alphaMax = 1.0f; // Максимальная прозрачность
    [SerializeField] private float pulseDuration = 0.5f; // Длительность одного цикла

    private Material material;

    private void Start()
    {
        // Получаем материал маркера
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            material = renderer.material;
            // Убедитесь, что материал поддерживает прозрачность
            material.SetFloat("_Mode", 3); // Fade mode
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;

            // Устанавливаем начальную прозрачность
            Color color = material.color;
            color.a = alphaMax;
            material.color = color;

            // Запускаем анимацию прозрачности
            DOTween.To(() => material.color.a, x => {
                Color newColor = material.color;
                newColor.a = x;
                material.color = newColor;
            }, alphaMin, pulseDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetTarget(gameObject); // Указываем целевой объект для анимации
        }
    }

    private void OnDestroy()
    {
        // Останавливаем анимацию
        DOTween.Kill(gameObject); // Используем gameObject как цель
    }
}