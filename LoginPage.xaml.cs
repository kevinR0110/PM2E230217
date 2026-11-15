using System.Net;

namespace PM2E230217;

public partial class LoginPage : ContentPage
{
    private const int MaxIntentos = 3;
    private int intentosFallidos = 0;   // solo cuentan las respuestas 401

    public LoginPage()
    {
        InitializeComponent();
    }

    // CheckBox "Mostrar contraseña": si está marcado, IsPassword = false
    private void OnMostrarClaveChanged(object? sender, CheckedChangedEventArgs e)
    {
        ClaveEntry.IsPassword = !e.Value;
    }

    private async void OnLoginClicked(object? sender, EventArgs e)
    {
        string correo = CorreoEntry.Text?.Trim() ?? "";
        string clave = ClaveEntry.Text ?? "";

        // 1. Campos vacíos: avisar y NO llamar a la API
        if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(clave))
        {
            await DisplayAlert("Campos vacíos", "Escribe tu correo y tu contraseña.", "OK");
            return;
        }

        // 2. Bloquear el botón y mostrar el indicador de carga
        LoginBtn.IsEnabled = false;
        Cargando.IsVisible = true;
        Cargando.IsRunning = true;

        bool exito = false;

        try
        {
            await ApiService.LoginAsync(correo, clave);
            exito = true;
            intentosFallidos = 0;
            ActualizarIntentos();
        }
        catch (ApiException ex)
        {
            // La API respondió con error
            if (ex.Codigo == HttpStatusCode.Unauthorized)
            {
                intentosFallidos++;
                ActualizarIntentos();

                if (intentosFallidos >= MaxIntentos)
                {
                    await DisplayAlert("Botón bloqueado", "Fallaste 3 veces. El botón de iniciar sesión fue bloqueado.", "OK");
                }
                else
                {
                    await DisplayAlert("Acceso denegado", ex.Message, "OK");
                }
            }
            else
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
        catch (Exception)
        {
            // API apagada, sin red o timeout: NO cuenta como intento
            await DisplayAlert("Sin conexión", "No se pudo conectar con la API. Verifica que esté encendida.", "OK");
        }
        finally
        {
            // Siempre se apaga el indicador; el botón solo se reactiva si no está bloqueado
            Cargando.IsRunning = false;
            Cargando.IsVisible = false;
            LoginBtn.IsEnabled = intentosFallidos < MaxIntentos;
        }

        // 3. Login correcto: abrir el Perfil con PushAsync
        if (exito)
        {
            ClaveEntry.Text = "";
            await Navigation.PushAsync(new PerfilPage());
        }
    }

    private void ActualizarIntentos()
    {
        IntentosLabel.Text = $"Intentos fallidos: {intentosFallidos} de {MaxIntentos}";
    }
}