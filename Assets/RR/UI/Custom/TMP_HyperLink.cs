using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Color = UnityEngine.Color;

namespace RR.UI.Custom
{
	[RequireComponent(typeof(TextMeshProUGUI))]
	public class TMP_HyperLink : MonoBehaviour, IPointerClickHandler
	{
		[SerializeField] private Color linkColor = Color.blue;
		[SerializeField] private Color linkHoverColor = Color.blue * 0.9f;
		
		private TextMeshProUGUI pTextMeshPro;
		private int isHoveredIndex = -1;
		private int textCharacterCount;
		
		private void Awake()
		{
			pTextMeshPro = GetComponent<TextMeshProUGUI>();
		}
		
		private void Start()
		{
			ResetLinksColor();
		}
		
		private void Update()
		{
			var isTextChanged = textCharacterCount != pTextMeshPro.textInfo.characterCount;
			if (isTextChanged)
			{
				textCharacterCount = pTextMeshPro.textInfo.characterCount;
				ResetLinksColor();
			}
			var linkIndex = TMP_TextUtilities.FindIntersectingLink(
				pTextMeshPro,
				RectTransformUtility.WorldToScreenPoint(Camera.current, Input.mousePosition),
				Camera.current
			);
			if (isHoveredIndex == linkIndex)
				return;
			if (linkIndex == -1)
			{
				SetLinkToColor(isHoveredIndex, linkColor);
				isHoveredIndex = linkIndex;
				return;
			}
			SetLinkToColor(isHoveredIndex = linkIndex, linkHoverColor);
		}
		
		public void OnPointerClick(PointerEventData eventData)
		{
			var linkIndex = TMP_TextUtilities.FindIntersectingLink(
				pTextMeshPro,
				RectTransformUtility.WorldToScreenPoint(Camera.current, Input.mousePosition),
				Camera.current
			);
			if (linkIndex == -1)
				return;
			var linkInfo = pTextMeshPro.textInfo.linkInfo[linkIndex];
			Application.OpenURL(linkInfo.GetLinkID());
		}
		
		private void ResetLinksColor()
		{
			for (var i = 0; i < pTextMeshPro.textInfo.linkCount; i++)
				SetLinkToColor(i, linkColor);
		}
		
		private void SetLinkToColor(int linkIndex, Color32 color)
		{
			var linkInfo = pTextMeshPro.textInfo.linkInfo[linkIndex];
			for (var i = 0; i < linkInfo.linkTextLength; i++)
			{
				var characterIndex = linkInfo.linkTextfirstCharacterIndex + i;
				var charInfo = pTextMeshPro.textInfo.characterInfo[characterIndex];
				if (!charInfo.isVisible)
					continue;
				var meshIndex = charInfo.materialReferenceIndex;
				var vertexIndex = charInfo.vertexIndex;
				var vertexColors = pTextMeshPro.textInfo.meshInfo[meshIndex].colors32;
				vertexColors[vertexIndex + 0] =
					vertexColors[vertexIndex + 1] =
						vertexColors[vertexIndex + 2] =
							vertexColors[vertexIndex + 3] = color;
			}
			pTextMeshPro.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
		}
	}
}