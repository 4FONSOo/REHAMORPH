using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TogglePassword : MonoBehaviour
{
    public TMP_InputField passwordField; // Campo da senha
    public Image buttonImage; // Refer�ncia � imagem do bot�o
    public Sprite eyeOpenSprite; // �cone para mostrar senha
    public Sprite eyeClosedSprite; // �cone para esconder senha

    private bool isPasswordVisible = false;

    public void TogglePasswordVisibility()
    {
        isPasswordVisible = !isPasswordVisible;

        // Altera o tipo de conte�do entre Standard (vis�vel) e Password (oculto)
        passwordField.contentType = isPasswordVisible ? TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.Password;

        // Atualiza o InputField para for�ar a mudan�a
        passwordField.ForceLabelUpdate();

        // Altera a imagem do bot�o
        buttonImage.sprite = isPasswordVisible ? eyeOpenSprite : eyeClosedSprite;
    }
}