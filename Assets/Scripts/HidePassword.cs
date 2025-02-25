using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TogglePassword : MonoBehaviour
{
    public TMP_InputField passwordField; // Campo da senha
    public Image buttonImage; // Referência à imagem do botão
    public Sprite eyeOpenSprite; // Ícone para mostrar senha
    public Sprite eyeClosedSprite; // Ícone para esconder senha

    private bool isPasswordVisible = false;

    public void TogglePasswordVisibility()
    {
        isPasswordVisible = !isPasswordVisible;

        // Altera o tipo de conteúdo entre Standard (visível) e Password (oculto)
        passwordField.contentType = isPasswordVisible ? TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.Password;

        // Atualiza o InputField para forçar a mudança
        passwordField.ForceLabelUpdate();

        // Altera a imagem do botão
        buttonImage.sprite = isPasswordVisible ? eyeOpenSprite : eyeClosedSprite;
    }
}