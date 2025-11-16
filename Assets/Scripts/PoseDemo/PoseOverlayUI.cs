using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PoseDemo
{
    /// <summary>
    /// Lightweight UI helper that places simple markers over MediaPipe landmarks.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class PoseOverlayUI : MonoBehaviour
    {
        [SerializeField] private Color markerColor = new(0.0f, 0.83f, 1.0f, 0.9f);
        [SerializeField] private float markerSize = 14f;
        [SerializeField, Range(0f, 1f)] private float visibilityThreshold = 0.4f;

        private readonly List<Image> markers = new();
        private RectTransform rectTransform;

        public RectTransform RectTransform
        {
            get
            {
                rectTransform ??= GetComponent<RectTransform>();
                return rectTransform;
            }
        }

        /// <summary>
        /// Allocates UI elements for the requested number of landmarks.
        /// </summary>
        public void Initialize(int landmarkCount)
        {
            ClearMarkers();

            for (int i = 0; i < landmarkCount; i++)
            {
                markers.Add(CreateMarker(i));
            }

            HideAll();
        }

        /// <summary>
        /// Render landmarks in normalized (0-1) viewport coordinates.
        /// </summary>
        public void Render(Vector4[] viewportLandmarks)
        {
            if (viewportLandmarks == null || viewportLandmarks.Length == 0)
            {
                HideAll();
                return;
            }

            int count = Mathf.Min(viewportLandmarks.Length, markers.Count);

            for (int i = 0; i < count; i++)
            {
                Vector4 joint = viewportLandmarks[i];
                bool isVisible = joint.w >= visibilityThreshold;
                Image marker = markers[i];
                marker.gameObject.SetActive(isVisible);

                if (!isVisible)
                {
                    continue;
                }

                Vector2 normalized = new(
                    Mathf.Clamp01(joint.x),
                    Mathf.Clamp01(joint.y)
                );

                RectTransform markerRect = marker.rectTransform;
                markerRect.anchorMin = normalized;
                markerRect.anchorMax = normalized;
                markerRect.anchoredPosition = Vector2.zero;
            }

            // Disable any remaining markers beyond the provided landmark array.
            for (int i = count; i < markers.Count; i++)
            {
                markers[i].gameObject.SetActive(false);
            }
        }

        public void HideAll()
        {
            foreach (Image marker in markers)
            {
                if (marker != null)
                {
                    marker.gameObject.SetActive(false);
                }
            }
        }

        private Image CreateMarker(int index)
        {
            GameObject go = new($"Landmark_{index}");
            go.transform.SetParent(RectTransform, false);

            Image image = go.AddComponent<Image>();
            image.color = markerColor;
            image.raycastTarget = false;
            RectTransform markerRect = image.rectTransform;
            markerRect.sizeDelta = Vector2.one * markerSize;

            return image;
        }

        private void ClearMarkers()
        {
            foreach (Image marker in markers)
            {
                if (marker == null)
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    Destroy(marker.gameObject);
                }
                else
                {
                    DestroyImmediate(marker.gameObject);
                }
            }

            markers.Clear();
        }
    }
}

