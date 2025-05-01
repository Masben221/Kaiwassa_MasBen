using UnityEngine;
using DG.Tweening;

public class PulsatingMarker : MonoBehaviour
{
    [SerializeField] private float alphaMin = 0.3f; // ����������� ������������
    [SerializeField] private float alphaMax = 1.0f; // ������������ ������������
    [SerializeField] private float pulseDuration = 0.5f; // ������������ ������ �����

    private Material material;

    private void Start()
    {
        // �������� �������� �������
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            material = renderer.material;
            // ���������, ��� �������� ������������ ������������
            material.SetFloat("_Mode", 3); // Fade mode
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;

            // ������������� ��������� ������������
            Color color = material.color;
            color.a = alphaMax;
            material.color = color;

            // ��������� �������� ������������
            DOTween.To(() => material.color.a, x => {
                Color newColor = material.color;
                newColor.a = x;
                material.color = newColor;
            }, alphaMin, pulseDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetTarget(gameObject); // ��������� ������� ������ ��� ��������
        }
    }

    private void OnDestroy()
    {
        // ������������� ��������
        DOTween.Kill(gameObject); // ���������� gameObject ��� ����
    }
}